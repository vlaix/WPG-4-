using UnityEngine;

public class FallDamage : MonoBehaviour
{
    [SerializeField] Health health;
    [SerializeField] Transform player1;
    [SerializeField] Transform player2;
    [SerializeField] Transform respawn;

    private void OnTriggerEnter(Collider collision)
    {
        if(collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Player2")){
            health.Hurt(2);
            if(collision.gameObject.CompareTag("Player")){
                Respawn(1);
            } else 
            {
                Respawn(2);
            }
        }   
    }

    private void Respawn(int player) 
    {
        if(player == 1) {
            player1.position = respawn.position;
        } else {
            player2.position = respawn.position;
        }
    }


}
