using UnityEngine;

public class EnemySpawnTrigger : MonoBehaviour
{
    [SerializeField] private EnemySpawn enemySpawn;

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("Player2")) {
            enemySpawn.StartSpawn();
            Debug.Log("Spawner Activated");
        }
    }
}
