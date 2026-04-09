using UnityEngine;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour
{
    public float Horizontal { get; private set; }
    public float Vertical { get; private set; }
    public bool IsJumpPressed { get; private set; }

    // Fungsi ini di-Invoke dari Player Input Component
    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        Horizontal = input.x; // Nilai -1 sampai 1
        Vertical = input.y;   // Nilai -1 sampai 1
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) IsJumpPressed = true;
        else if (context.canceled) IsJumpPressed = false;
    }

    public void OnPause() // Fungsi ini terpanggil dari PlayerInput
    {
        if (PauseManager.Instance != null)
        {
            PauseManager.Instance.TogglePause();
        }
    }
}