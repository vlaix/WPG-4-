using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float lifeTime = 3f;

    void Start()
    {
        Destroy(gameObject, lifeTime); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Tambahkan logika damage ke Player di sini
            Debug.Log("Player 1 terkena tembakan!");
            
            Destroy(gameObject); 
        }

        if (other.CompareTag("Player2"))
        {
            // Tambahkan logika damage ke Player di sini
            Debug.Log("Player 2 terkena tembakan!");
            
            Destroy(gameObject); 
        }

    }
}