using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(InputReader), typeof(Animator))]
public class playermovtest : MonoBehaviour
{
    private Rigidbody rb;
    private InputReader inputReader;
    private Animator animator;

    [Header("Movement Settings")]
    public float moveSpeed = 7f;
    public float rotationSpeed = 10f;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        inputReader = GetComponent<InputReader>();
        animator = GetComponent<Animator>();

        rb.freezeRotation = true; // Penting agar karakter tidak guling
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        // Ambil input dari script InputReader
        Vector3 moveDir = new Vector3(inputReader.Horizontal, 0, inputReader.Vertical).normalized;

        // Update animator Speed parameter
        animator.SetFloat("Speed", moveDir.magnitude);

        if (moveDir.magnitude >= 0.1f)
        {
            // 1. Rotasi (Menghadap arah jalan)
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);

            // 2. Perpindahan Posisi
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }
    }
}