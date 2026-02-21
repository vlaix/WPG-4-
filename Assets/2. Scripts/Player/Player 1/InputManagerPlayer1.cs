using UnityEngine;

public class InputManagerPlayer1 : MonoBehaviour
{
    PlayerInput playerControls; // Ubah dari PlayerControls ke PlayerControl
    public Vector2 movementInput;
    public float verticalInput;
    public float horizontalInput;

    private void OnEnable()
    {
        if (playerControls == null) 
        {
            playerControls = new PlayerInput(); // Ubah dari PlayerControls ke PlayerControl
            playerControls.Player1Movement.Movement.performed += i => movementInput = i.ReadValue<Vector2>();
            playerControls.Player1Movement.Movement.canceled += i => movementInput = Vector2.zero;
        }
        playerControls.Player1Movement.Enable();
    }

    private void OnDisable()
    {
        playerControls.Player1Movement.Disable();
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