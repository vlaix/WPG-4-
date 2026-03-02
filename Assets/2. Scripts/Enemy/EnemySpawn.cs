using System.Collections.Generic;
using UnityEngine;

public class EnemySpawn : MonoBehaviour
{
    [SerializeField] private Collider SpawnCollider;
    [SerializeField] private float SpawnTime;
    [SerializeField] private EnemyData[] data;

    [Header("Spawn Caps")]
    [SerializeField] private int Normal;
    [SerializeField] private int Tank;
    [SerializeField] private int Archer;
    private int MaxSpawn, SpawnCount, NormalCount, TankCount, ArcherCount;
    private float BufferTime;
    private List<int> AvailableEnemy = new List<int>();

    [Header("Centang jika ada")]
    [SerializeField]  private bool IsTrigger;
    private bool Activate;


    void Start()
    {
        MaxSpawn = Normal+Tank+Archer;
        SpawnCount = 0;
        NormalCount = 0;
        TankCount = 0;
        ArcherCount = 0;
        SetSpawnTime();
        if(!IsTrigger) { //cek apakah ada trigger
            Activate = true;
            Debug.Log("Spawner aktif diawal" + name);
        }
    }

    void Update()
    {
        if(Activate) { //buat trigger
            BufferTime -= Time.deltaTime;

            if (BufferTime <= 0f && SpawnCount < MaxSpawn)
            {   
                //                          spawn metode list
                AvailableEnemy.Clear(); // Kosongkan list yang lama, tanpa membuat objek baru

                if (NormalCount < Normal) AvailableEnemy.Add(0); // Index 0 = Normal
                if (TankCount < Tank)   AvailableEnemy.Add(1); // Index 1 = Tank
                if (ArcherCount < Archer) AvailableEnemy.Add(2); // Index 2 = Archer

                // 2. Jika masih ada tipe yang jatahnya tersedia
                if (AvailableEnemy.Count > 0)
                {
                    // Pilih secara acak dari tipe yang tersedia saja
                    int indexTerpilih = AvailableEnemy[Random.Range(0, AvailableEnemy.Count)];

                    if (indexTerpilih == 0) NormalCount++;
                    else if (indexTerpilih == 1) TankCount++;
                    else if (indexTerpilih == 2) ArcherCount++;

                    SpawnEnemy(indexTerpilih);
                }
            }
        }
        // float randomizer;            spawn metode if
        // if(BufferTime <= 0f) {
        //     if(SpawnCount < MaxSpawn) {
        //         randomizer = Random.Range(0f, 2f); //misal 50% normal 25% tank & archer

        //         if(randomizer <= 1 && NormalCount < Normal && Normal != 0) {
        //             SpawnEnemy(0); //normal
        //             NormalCount++;

        //         } else if(randomizer > 1 && randomizer <= 1.5 && TankCount < Tank && Tank != 0) {
        //             SpawnEnemy(1); //tank
        //             TankCount++;

        //         } else if(randomizer > 1.5 && ArcherCount < Archer && Archer != 0) {
        //             SpawnEnemy(2); //archer
        //             ArcherCount++;
        //         }
        //     }
        // }
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

        spawnPos.y = 1.00f; // Tetap jaga ketinggian agar tidak tenggelam
        return spawnPos;
    }

    private void SpawnEnemy(int index) {
        Instantiate(data[index].prefab, GetPosition(), Quaternion.identity);
        SpawnCount += 1;
        SetSpawnTime();
    }

    public void StartSpawn() {
        Activate = true;
    }
}