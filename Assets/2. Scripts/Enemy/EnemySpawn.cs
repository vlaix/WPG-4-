using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private GameObject enemy;
    [SerializeField] private Collider SpawnCollider;
    [SerializeField] private float SpawnTime;
    private float BufferTime;
    
    void Start()
    {
        SetSpawnTime();
    }

    void Update()
    {
        BufferTime -= Time.deltaTime;

        if(BufferTime <= 0f) {
            Vector3 SpawnPosition = GetPosition();
            Instantiate(enemy, SpawnPosition, Quaternion.identity);
            SetSpawnTime();
        }
    }

    private void SetSpawnTime() 
    {
        BufferTime = SpawnTime;
    }

    private Vector3 GetPosition()
    {
        Bounds bounds = SpawnCollider.bounds;
        Vector3 spawnPos = Vector3.zero;
        int side = Random.Range(0, 4);

        switch (side)
        {
            case 0: // Sisi Atas (Z Max)
                spawnPos.x = Random.Range(bounds.min.x, bounds.max.x);
                spawnPos.z = bounds.max.z;
                break;
            case 1: // Sisi Bawah (Z Min)
                spawnPos.x = Random.Range(bounds.min.x, bounds.max.x);
                spawnPos.z = bounds.min.z;
                break;
            case 2: // Sisi Kiri (X Min)
                spawnPos.x = bounds.min.x;
                spawnPos.z = Random.Range(bounds.min.z, bounds.max.z);
                break;
            case 3: // Sisi Kanan (X Max)
                spawnPos.x = bounds.max.x;
                spawnPos.z = Random.Range(bounds.min.z, bounds.max.z);
                break;
        }

        spawnPos.y = 1.04f; // Tetap jaga ketinggian agar tidak tenggelam
        return spawnPos;
    }
}
