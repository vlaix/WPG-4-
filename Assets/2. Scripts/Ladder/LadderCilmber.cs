using UnityEngine;

/// <summary>
/// LadderClimber dengan:
/// - Solid collision (tidak bisa tembus)
/// - Proper descending (S turun, tidak terbalik)
/// - Auto-detect movement script
/// </summary>
public class LadderClimber : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 5f;
    public float horizontalSpeed = 3f;
    public KeyCode exitKey = KeyCode.Space;

    [Header("Collision Settings")]
    [Tooltip("Enable solid collision (can't pass through ladder)?")]
    public bool solidCollision = true;

    [Tooltip("Layer for ladder solid collider")]
    public string ladderSolidLayer = "Default";

    [Header("Debug")]
    public bool showDebugLogs = false;

    // Components
    private MonoBehaviour movementScript;
    private Rigidbody rb;
    private InputReader inputReader;
    private InputManagerPlayer1 inputP1;
    private InputManagerPlayer2 inputP2;
    private CapsuleCollider playerCollider;

    // Ladder reference
    private GameObject currentLadder;
    private Collider ladderTrigger;
    private Collider ladderSolidCollider;

    // Status
    private bool isNearLadder = false;
    private bool isClimbing = false;

    // Input properties
    private float VerticalInput
    {
        get
        {
            if (inputReader != null) return inputReader.Vertical;
            if (inputP1 != null) return inputP1.verticalInput;
            if (inputP2 != null) return inputP2.verticalInput;
            return 0f;
        }
    }

    private float HorizontalInput
    {
        get
        {
            if (inputReader != null) return inputReader.Horizontal;
            if (inputP1 != null) return inputP1.horizontalInput;
            if (inputP2 != null) return inputP2.horizontalInput;
            return 0f;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerCollider = GetComponent<CapsuleCollider>();

        FindMovementScript();
        FindInputScript();

        if (showDebugLogs)
        {
            Debug.Log($"[LADDER] Initialized. Movement: {movementScript?.GetType().Name}, Input: {GetInputSystemName()}");
        }
    }

    private void FindMovementScript()
    {
        movementScript = GetComponent<playermovtest>();
        if (movementScript == null) movementScript = GetComponent<PlayerLocomotion>();

        if (movementScript == null)
        {
            var allScripts = GetComponents<MonoBehaviour>();
            foreach (var script in allScripts)
            {
                var typeName = script.GetType().Name.ToLower();
                if ((typeName.Contains("movement") || typeName.Contains("locomotion") || typeName.Contains("move"))
                    && !typeName.Contains("input"))
                {
                    movementScript = script;
                    break;
                }
            }
        }
    }

    private void FindInputScript()
    {
        inputReader = GetComponent<InputReader>();
        inputP1 = GetComponent<InputManagerPlayer1>();
        inputP2 = GetComponent<InputManagerPlayer2>();
    }

    private string GetInputSystemName()
    {
        if (inputReader != null) return "InputReader";
        if (inputP1 != null) return "InputManagerPlayer1";
        if (inputP2 != null) return "InputManagerPlayer2";
        return "NONE";
    }

    private void Update()
    {
        // Start climbing
        if (isNearLadder && !isClimbing)
        {
            float vInput = VerticalInput;

            // ✅ FIX: Accept both up (W) and down (S) to start climbing
            if (Mathf.Abs(vInput) > 0.1f)
            {
                if (showDebugLogs) Debug.Log($"[LADDER] Starting climb. Input: {vInput}");
                StartClimbing();
            }
        }

        // Exit climbing
        if (isClimbing && Input.GetKeyDown(exitKey))
        {
            if (showDebugLogs) Debug.Log($"[LADDER] Exit key pressed");
            StopClimbing();
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            // ✅ FIX: Proper vertical movement (W = up, S = down, NOT reversed!)
            float verticalMovement = VerticalInput * climbSpeed;
            float horizontalMovement = HorizontalInput * horizontalSpeed;

            Vector3 climbMovement = new Vector3(
                horizontalMovement,
                verticalMovement,  // Direct mapping: positive input = up, negative = down
                0f
            );

            Vector3 worldMovement = transform.TransformDirection(climbMovement);
            rb.linearVelocity = worldMovement;
        }
    }

    private void StartClimbing()
    {
        isClimbing = true;

        // Disable movement script
        if (movementScript != null && movementScript.enabled)
        {
            movementScript.enabled = false;
            if (showDebugLogs) Debug.Log($"[LADDER] Disabled movement: {movementScript.GetType().Name}");
        }

        // Disable gravity
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
        }

        // ✅ NEW: Disable player collider collision with ladder solid collider
        if (solidCollision && playerCollider != null && ladderSolidCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, ladderSolidCollider, true);
            if (showDebugLogs) Debug.Log($"[LADDER] Ignoring collision with solid collider");
        }

        if (showDebugLogs) Debug.Log($"[LADDER] 🪜 Climbing started!");
    }

    private void StopClimbing()
    {
        isClimbing = false;

        // Re-enable movement script
        if (movementScript != null && !movementScript.enabled)
        {
            movementScript.enabled = true;
            if (showDebugLogs) Debug.Log($"[LADDER] Re-enabled movement");
        }

        // Re-enable gravity
        if (rb != null)
        {
            rb.useGravity = true;
        }

        // ✅ NEW: Re-enable collision with ladder solid collider
        if (solidCollision && playerCollider != null && ladderSolidCollider != null)
        {
            Physics.IgnoreCollision(playerCollider, ladderSolidCollider, false);
            if (showDebugLogs) Debug.Log($"[LADDER] Re-enabled collision with solid collider");
        }

        if (showDebugLogs) Debug.Log($"[LADDER] Stopped climbing");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isNearLadder = true;
            currentLadder = other.gameObject;
            ladderTrigger = other;

            // ✅ NEW: Find solid collider in ladder
            if (solidCollision)
            {
                FindLadderSolidCollider();
            }

            if (showDebugLogs) Debug.Log($"[LADDER] Near ladder. Press W/S to climb");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Ladder"))
        {
            isNearLadder = false;
            currentLadder = null;
            ladderTrigger = null;
            ladderSolidCollider = null;

            if (isClimbing)
            {
                if (showDebugLogs) Debug.Log($"[LADDER] Auto-exit climbing");
                StopClimbing();
            }
        }
    }

    // ✅ NEW: Find solid collider on ladder
    private void FindLadderSolidCollider()
    {
        if (currentLadder == null) return;

        // Look for a non-trigger collider on the ladder or its children
        Collider[] colliders = currentLadder.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            if (!col.isTrigger && col != ladderTrigger)
            {
                ladderSolidCollider = col;
                if (showDebugLogs) Debug.Log($"[LADDER] Found solid collider: {col.name}");
                return;
            }
        }

        // If not found in children, check parent
        Transform parent = currentLadder.transform.parent;
        if (parent != null)
        {
            Collider[] parentColliders = parent.GetComponentsInChildren<Collider>();
            foreach (Collider col in parentColliders)
            {
                if (!col.isTrigger && col != ladderTrigger)
                {
                    ladderSolidCollider = col;
                    if (showDebugLogs) Debug.Log($"[LADDER] Found solid collider in parent: {col.name}");
                    return;
                }
            }
        }

        if (showDebugLogs) Debug.LogWarning($"[LADDER] No solid collider found on ladder!");
    }

    // Manual test functions
    [ContextMenu("Test: Show Current State")]
    private void TestShowState()
    {
        Debug.Log("=== LADDER CLIMBER STATE ===");
        Debug.Log($"Near Ladder: {isNearLadder}");
        Debug.Log($"Climbing: {isClimbing}");
        Debug.Log($"Movement Script: {movementScript?.GetType().Name ?? "NULL"}");
        Debug.Log($"Movement Enabled: {movementScript?.enabled ?? false}");
        Debug.Log($"Vertical Input: {VerticalInput}");
        Debug.Log($"Has Solid Collider: {ladderSolidCollider != null}");
    }
}