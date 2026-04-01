using UnityEngine;
using UnityEngine.AI;

public class ArcherPasif : EnemyBehavior
{
    [Header("Static Archer Settings")]
    [SerializeField] private bool lookAtPlayer = true;

    protected override void Start()
    {
        // Memanggil fungsi Start dari EnemyBehavior untuk inisialisasi awal
        base.Start();

        // Mematikan NavMeshAgent agar musuh tidak berjalan
        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
            agent.enabled = false; // Mematikan total agar tidak memakan kalkulasi pathfinding
        }
    }
    
    protected override void Update()
    {
        // 1. Cari player terdekat (fungsi warisan dari induk)
        UpdateClosestPlayer();

        // Jika tidak ada player, jangan lakukan apa-apa
        if (closestPlayer == null) return;

        // 2. Logika Jarak (Menggunakan stopDistance dari EnemyData)
        float distanceToPlayer = Vector3.Distance(transform.position, closestPlayer.position);

        if (distanceToPlayer <= data.stopDistance)
        {
            // 3. Selalu menghadap ke arah player
            FaceTarget();

            // 4. Logika: SHOOT -> TIMER (Cooldown) -> SHOOT
            // Memastikan bom tidak aktif dan waktu sudah melewati buffer cooldown
            if (Time.time >= BufferShoot && !bomaktif) 
            {
                Shoot(); // Tembak!
                
                // Set Timer: Waktu sekarang + Cooldown dari EnemyData
                BufferShoot = Time.time + data.Cooldown; 
            }
        }
    }
}