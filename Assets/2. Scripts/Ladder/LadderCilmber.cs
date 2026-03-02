using UnityEngine;

public class LadderClimber : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 5f;

    [Tooltip("Kecepatan gerak horizontal saat di tangga")]
    public float horizontalSpeed = 3f;

    [Tooltip("Tombol untuk keluar dari tangga (default: Space)")]
    public KeyCode exitKey = KeyCode.Space;

    // Referensi komponen
    private PlayerLocomotion locomotionScript;
    private Rigidbody rb;

    // Referensi input (Mendukung Player 1 & 2)
    private InputManagerPlayer1 inputP1;
    private InputManagerPlayer2 inputP2;

    // Status
    private bool isNearLadder = false;
    private bool isClimbing = false;

    // Input properties
    private float VerticalInput
    {
        get
        {
            if (inputP1 != null) return inputP1.verticalInput;
            if (inputP2 != null) return inputP2.verticalInput;
            return 0f;
        }
    }

    private float HorizontalInput
    {
        get
        {
            if (inputP1 != null) return inputP1.horizontalInput;
            if (inputP2 != null) return inputP2.horizontalInput;
            return 0f;
        }
    }

    private void Awake()
    {
        locomotionScript = GetComponent<PlayerLocomotion>();
        rb = GetComponent<Rigidbody>();

        inputP1 = GetComponent<InputManagerPlayer1>();
        inputP2 = GetComponent<InputManagerPlayer2>();
    }

    private void Update()
    {
        // Mulai climbing jika dekat tangga dan menekan W/S
        if (isNearLadder && !isClimbing && Mathf.Abs(VerticalInput) > 0.1f)
        {
            StartClimbing();
        }

        // Exit climbing dengan tombol Space
        if (isClimbing && Input.GetKeyDown(exitKey))
        {
            StopClimbing();
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            // PERBAIKAN: Player bisa gerak horizontal (A/D) DAN vertical (W/S)
            float verticalMovement = VerticalInput * climbSpeed;
            float horizontalMovement = HorizontalInput * horizontalSpeed;

            // Movement di tangga: bisa kiri-kanan DAN naik-turun
            Vector3 climbMovement = new Vector3(
                horizontalMovement,      // Kiri-kanan (A/D)
                verticalMovement,        // Naik-turun (W/S)
                0f                       // Tidak maju-mundur
            );

            // Apply movement relative to player's rotation
            Vector3 worldMovement = transform.TransformDirection(climbMovement);
            rb.linearVelocity = worldMovement;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;

        // Matikan script pergerakan normal
        if (locomotionScript != null) locomotionScript.enabled = false;

        // Matikan gravitasi
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;

        Debug.Log("🪜 Started climbing! Press Space to exit.");
    }

    private void StopClimbing()
    {
        isClimbing = false;

        // Nyalakan kembali script pergerakan normal
        if (locomotionScript != null) locomotionScript.enabled = true;

        // Nyalakan gravitasi kembali
        rb.useGravity = true;

        Debug.Log("✅ Stopped climbing!");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isNearLadder = true;
            Debug.Log("📍 Near ladder - Press W to climb");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isNearLadder = false;

            // Auto-exit jika keluar dari area tangga
            if (isClimbing)
            {
                StopClimbing();
            }
        }
    }
}