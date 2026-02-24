using UnityEngine;

public class LadderClimber : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 5f;

    // Referensi komponen
    private PlayerLocomotion locomotionScript;
    private Rigidbody rb;

    // Referensi input (Mendukung Player 1 & 2)
    private InputManagerPlayer1 inputP1;
    private InputManagerPlayer2 inputP2;

    // Status
    private bool isNearLadder = false;
    private bool isClimbing = false;

    // Properti untuk mengambil input vertikal (W/S atau Atas/Bawah)
    private float VerticalInput
    {
        get
        {
            if (inputP1 != null) return inputP1.verticalInput;
            if (inputP2 != null) return inputP2.verticalInput;
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
        // Jika sedang di dekat tangga, belum manjat, dan menekan tombol maju/mundur
        if (isNearLadder && !isClimbing && Mathf.Abs(VerticalInput) > 0.1f)
        {
            StartClimbing();
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            // KUNCI PERBAIKAN: 
            // Kita matikan total pergerakan X dan Z. Player HANYA bisa bergerak di sumbu Y.
            // Ini membuat karakter "melayang" lurus ke atas/bawah tanpa menabrak tembok sedikitpun.
            rb.linearVelocity = new Vector3(0f, VerticalInput * climbSpeed, 0f);
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;

        // Matikan script pergerakan normal agar player tidak lari ke depan
        if (locomotionScript != null) locomotionScript.enabled = false;

        // Matikan gravitasi agar player bisa menempel/berhenti di tengah tangga
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
    }

    private void StopClimbing()
    {
        isClimbing = false;

        // Nyalakan kembali script pergerakan normal
        if (locomotionScript != null) locomotionScript.enabled = true;

        // Nyalakan gravitasi kembali
        rb.useGravity = true;
    }

    // --- DETEKSI AREA TANGGA ---
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isNearLadder = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isNearLadder = false;

            // Jika keluar dari area tangga otomatis berhenti manjat
            if (isClimbing)
            {
                StopClimbing();
            }
        }
    }
}