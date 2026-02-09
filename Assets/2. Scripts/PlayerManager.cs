using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    MonoBehaviour inputManager; // Generic
    PlayerLocomotion playerLocomotion;

    private void Awake()
    {
        // Cari InputManager yang aktif (Player1 atau Player2)
        inputManager = GetComponent<InputManagerPlayer1>() as MonoBehaviour 
                    ?? GetComponent<InputManagerPlayer2>() as MonoBehaviour;
        playerLocomotion = GetComponent<PlayerLocomotion>();
    }

    private void Update()
    {
        if (inputManager is InputManagerPlayer1 player1)
            player1.HandleAllInput();
        else if (inputManager is InputManagerPlayer2 player2)
            player2.HandleAllInput();
    }

    private void FixedUpdate()
    {
        playerLocomotion.HandleAllMovement();
    }
}
