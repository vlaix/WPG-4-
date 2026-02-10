using UnityEngine;

public class PlayerLocomotion : MonoBehaviour
{
    private InputManagerPlayer1 inputManagerP1;
    private InputManagerPlayer2 inputManagerP2;

    Vector3 moveDirection;
    Transform cameraObject;
    Rigidbody playerRigidbody;

    public float movementSpeed = 7;
    public float rotationSpeed = 15;

    private float VerticalInput
    {
        get
        {
            if (inputManagerP1 != null) return inputManagerP1.verticalInput;
            if (inputManagerP2 != null) return inputManagerP2.verticalInput;
            return 0f;
        }
    }

    private float HorizontalInput
    {
        get
        {
            if (inputManagerP1 != null) return inputManagerP1.horizontalInput;
            if (inputManagerP2 != null) return inputManagerP2.horizontalInput;
            return 0f;
        }
    }

    private void Awake()
    {
        inputManagerP1 = GetComponent<InputManagerPlayer1>();
        inputManagerP2 = GetComponent<InputManagerPlayer2>();
        
        playerRigidbody = GetComponent<Rigidbody>();
        cameraObject = Camera.main.transform;
    }

    public void HandleAllMovement() 
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        moveDirection = cameraObject.forward * VerticalInput;
        moveDirection = moveDirection + cameraObject.right * HorizontalInput;
        moveDirection.Normalize();
        moveDirection.y = 0;
        moveDirection = moveDirection * movementSpeed;

        Vector3 movementVelocity = moveDirection;
        playerRigidbody.linearVelocity = movementVelocity;
    }

    private void HandleRotation()
    {
        Vector3 targetDirection = Vector3.zero;

        targetDirection = cameraObject.forward * VerticalInput;
        targetDirection = targetDirection + cameraObject.right * HorizontalInput;
        targetDirection.Normalize();
        targetDirection.y = 0;

        if (targetDirection == Vector3.zero)
            targetDirection = transform.forward;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        Quaternion playerRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        transform.rotation = playerRotation;
    }
}