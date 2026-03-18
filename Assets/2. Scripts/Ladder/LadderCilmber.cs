using UnityEngine;

/// <summary>
/// LadderClimber yang auto-detect movement script apapun!
/// Support: PlayerLocomotion, playermovtest, atau script lain
/// </summary>
public class LadderClimber : MonoBehaviour
{
    [Header("Climbing Settings")]
    public float climbSpeed = 5f;
    public float horizontalSpeed = 3f;
    public KeyCode exitKey = KeyCode.Space;

    [Header("Debug")]
    public bool showDebugLogs = true;

    // Components
    private MonoBehaviour movementScript; // ✅ Generic MonoBehaviour!
    private Rigidbody rb;
    private InputManagerPlayer1 inputP1;
    private InputManagerPlayer2 inputP2;
    private InputReader inputReader; // ✅ Support InputReader juga!

    // Status
    private bool isNearLadder = false;
    private bool isClimbing = false;

    // Input properties - support multiple input systems
    private float VerticalInput
    {
        get
        {
            // Try InputReader first (for playermovtest)
            if (inputReader != null)
            {
                if (showDebugLogs && inputReader.Vertical != 0 && Time.frameCount % 30 == 0)
                    Debug.Log($"[INPUT] InputReader Vertical: {inputReader.Vertical}");
                return inputReader.Vertical;
            }

            // Try InputManagerPlayer1
            if (inputP1 != null)
            {
                if (showDebugLogs && inputP1.verticalInput != 0 && Time.frameCount % 30 == 0)
                    Debug.Log($"[INPUT] InputP1 Vertical: {inputP1.verticalInput}");
                return inputP1.verticalInput;
            }

            // Try InputManagerPlayer2
            if (inputP2 != null)
            {
                if (showDebugLogs && inputP2.verticalInput != 0 && Time.frameCount % 30 == 0)
                    Debug.Log($"[INPUT] InputP2 Vertical: {inputP2.verticalInput}");
                return inputP2.verticalInput;
            }

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

        // ✅ Try find ANY movement script
        FindMovementScript();

        // ✅ Try find ANY input script
        FindInputScript();

        if (showDebugLogs)
        {
            Debug.Log($"[AWAKE] LadderClimber initialized");
            Debug.Log($"[AWAKE] Movement Script: {(movementScript != null ? movementScript.GetType().Name : "NULL")}");
            Debug.Log($"[AWAKE] Rigidbody: {rb != null}");
            Debug.Log($"[AWAKE] Input System: {GetInputSystemName()}");
        }
    }

    private void FindMovementScript()
    {
        // Try playermovtest first (most common now!)
        movementScript = GetComponent<playermovtest>();

        // If not found, try PlayerLocomotion
        if (movementScript == null)
        {
            movementScript = GetComponent<PlayerLocomotion>();
        }

        // If still not found, try finding ANY MonoBehaviour that might be movement
        if (movementScript == null)
        {
            var allScripts = GetComponents<MonoBehaviour>();
            foreach (var script in allScripts)
            {
                var typeName = script.GetType().Name.ToLower();
                if (typeName.Contains("movement") || typeName.Contains("locomotion") || typeName.Contains("move") || typeName.Contains("player"))
                {
                    // Skip input scripts
                    if (typeName.Contains("input")) continue;

                    movementScript = script;
                    if (showDebugLogs)
                        Debug.Log($"[AWAKE] Auto-detected movement script: {script.GetType().Name}");
                    break;
                }
            }
        }

        if (movementScript == null && showDebugLogs)
        {
            Debug.LogError("[AWAKE] ❌ No movement script found! Player won't stop moving during climb!");
        }
    }

    private void FindInputScript()
    {
        // Try InputReader first (for playermovtest)
        inputReader = GetComponent<InputReader>();

        // Also try InputManagers (backup)
        inputP1 = GetComponent<InputManagerPlayer1>();
        inputP2 = GetComponent<InputManagerPlayer2>();

        if (inputReader == null && inputP1 == null && inputP2 == null && showDebugLogs)
        {
            Debug.LogError("[AWAKE] ❌ No input script found! Climbing input won't work!");
        }
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
        if (isNearLadder && !isClimbing)
        {
            float vInput = VerticalInput;

            if (Mathf.Abs(vInput) > 0.1f)
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[UPDATE] ✅ Starting climb. Input: {vInput}");
                }
                StartClimbing();
            }
        }

        if (isClimbing && Input.GetKeyDown(exitKey))
        {
            if (showDebugLogs) Debug.Log($"[UPDATE] Exit key pressed");
            StopClimbing();
        }
    }

    private void FixedUpdate()
    {
        if (isClimbing)
        {
            float verticalMovement = VerticalInput * climbSpeed;
            float horizontalMovement = HorizontalInput * horizontalSpeed;

            Vector3 climbMovement = new Vector3(
                horizontalMovement,
                verticalMovement,
                0f
            );

            Vector3 worldMovement = transform.TransformDirection(climbMovement);
            rb.linearVelocity = worldMovement;

            if (showDebugLogs && Time.frameCount % 60 == 0)
            {
                Debug.Log($"[CLIMB] V:{verticalMovement:F2}, H:{horizontalMovement:F2}");
            }
        }
    }

    private void StartClimbing()
    {
        if (showDebugLogs)
        {
            Debug.Log($"[START] 🪜 StartClimbing()");
        }

        isClimbing = true;

        // ✅ SAFETY: Only disable if currently enabled!
        if (movementScript != null)
        {
            if (movementScript.enabled)
            {
                movementScript.enabled = false;
                if (showDebugLogs)
                {
                    Debug.Log($"[START] ✅ Disabled movement: {movementScript.GetType().Name}");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.LogWarning($"[START] ⚠️ Movement already disabled! (Shield active or other reason)");
                    Debug.LogWarning($"[START] Allowing climb anyway...");
                }
                // Still allow climbing even if movement already disabled
            }
        }
        else
        {
            if (showDebugLogs)
            {
                Debug.LogWarning($"[START] ⚠️ No movement script found!");
            }
        }

        // Disable gravity
        if (rb != null)
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            if (showDebugLogs) Debug.Log($"[START] ✅ Gravity disabled");
        }
        else
        {
            if (showDebugLogs) Debug.LogError($"[START] ❌ Rigidbody is NULL!");
        }

        if (showDebugLogs)
        {
            Debug.Log($"[START] 🎉 Climbing started!");
        }
    }

    private void StopClimbing()
    {
        if (showDebugLogs) Debug.Log($"[STOP] ✅ StopClimbing()");

        isClimbing = false;

        // ✅ ALWAYS try to re-enable movement (in case it was disabled by us or by shield)
        if (movementScript != null)
        {
            if (!movementScript.enabled)
            {
                movementScript.enabled = true;
                if (showDebugLogs)
                {
                    Debug.Log($"[STOP] Re-enabled movement: {movementScript.GetType().Name}");
                }
            }
            else
            {
                if (showDebugLogs)
                {
                    Debug.Log($"[STOP] Movement already enabled");
                }
            }
        }

        // Re-enable gravity
        if (rb != null)
        {
            rb.useGravity = true;
            if (showDebugLogs) Debug.Log($"[STOP] Gravity re-enabled");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[TRIGGER] Enter: {other.name}, Tag: '{other.tag}'");
        }

        if (other.CompareTag("Ladder"))
        {
            isNearLadder = true;
            if (showDebugLogs)
            {
                Debug.Log($"[TRIGGER] ✅ Near ladder! Press W to climb");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[TRIGGER] Exit: {other.name}");
        }

        if (other.CompareTag("Ladder"))
        {
            isNearLadder = false;
            if (showDebugLogs) Debug.Log($"[TRIGGER] Left ladder area");

            if (isClimbing)
            {
                if (showDebugLogs) Debug.Log($"[TRIGGER] Auto-exit climbing");
                StopClimbing();
            }
        }
    }

    // Manual test functions
    [ContextMenu("Test: Force Start Climbing")]
    private void TestStartClimbing()
    {
        Debug.Log("=== MANUAL TEST: Force Start Climbing ===");
        isNearLadder = true;
        StartClimbing();
    }

    [ContextMenu("Test: Force Stop Climbing")]
    private void TestStopClimbing()
    {
        Debug.Log("=== MANUAL TEST: Force Stop Climbing ===");
        StopClimbing();
    }

    [ContextMenu("Test: Show Current State")]
    private void TestShowState()
    {
        Debug.Log("=== CURRENT STATE ===");
        Debug.Log($"isNearLadder: {isNearLadder}");
        Debug.Log($"isClimbing: {isClimbing}");
        Debug.Log($"Movement Script: {(movementScript != null ? movementScript.GetType().Name : "NULL")}");
        Debug.Log($"Movement Enabled: {(movementScript != null ? movementScript.enabled : false)}");
        Debug.Log($"Input System: {GetInputSystemName()}");
        Debug.Log($"Vertical Input: {VerticalInput}");
        Debug.Log($"Horizontal Input: {HorizontalInput}");
    }
}