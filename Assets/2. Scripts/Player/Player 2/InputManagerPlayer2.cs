using UnityEngine;

public class InputManagerPlayer2 : MonoBehaviour
{
    PlayerControl playerControls; // Ubah dari PlayerControls ke PlayerControl
    public Vector2 movementInput;
    public float verticalInput;
    public float horizontalInput;

    private void OnEnable()
    {
        if (playerControls == null) 
        {
            playerControls = new PlayerControl(); // Ubah dari PlayerControls ke PlayerControl
            playerControls.Player2Movement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.Player2Movement.Movement.canceled += i => movementInput = Vector2.zero;
        }
        playerControls.Player2Movement.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player2Movement.Disable();
    }

    public void HandleAllInput() 
    {
        HandleMovementInput();
    }

    private void HandleMovementInput() 
    {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
    }
}