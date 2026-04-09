using System;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Transform[] players;
    [HideInInspector] public Transform closestPlayer;
    [SerializeField] private GameObject loot;
    [SerializeField] protected EnemyData data;
    [SerializeField] Health Health;
    public float healthpoint;
    protected float BufferShoot;
    private float BufferAttack;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Vector3 knockbackVelocity;

    [Header("Musuh Mbledos")]
    [SerializeField] private MaterialPropertyBlock propBlock;
    private float Buffermbledos;
    protected bool bomaktif;

    [Header("Shoot Settings")]
    [SerializeField] private float bulletSpawnHeight = 0.1f;

    [Header("Audio Settings")]
    [Tooltip("Suara saat musuh mati biasa")]
    [SerializeField] private AudioClip deathSFX;

    [Tooltip("Suara saat musuh meledak (Mbledos)")]
    [SerializeField] private AudioClip explosionSFX;

    [Range(0f, 1f)]
    [SerializeField] private float volume = 1f;

    // Penanda agar suara tidak tumpang tindih
    private bool hasExploded = false;

    // KUNCI PERBAIKAN: Penanda bahwa musuh sudah mati (agar tidak jalan/nyerang lagi saat delay)
    protected bool isDead = false;

    void Awake()
    {
        propBlock = new MaterialPropertyBlock();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) originalColor = enemyRenderer.material.color;
    }

    protected virtual void Start()
    {
        Health = Health.Instance;
        agent.speed = data.speed;
        agent.stoppingDistance = data.stopDistance;

        healthpoint = data.healthPoint;
        BufferAttack = 0f;

        // Cari semua player dengan tag "Player" dan "Player2"
        GameObject[] playerObjs1 = GameObject.FindGameObjectsWithTag("Player");
        GameObject[] playerObjs2 = GameObject.FindGameObjectsWithTag("Player2");
        players = new Transform[playerObjs1.Length + playerObjs2.Length];
        int idx = 0;
        foreach (var p in playerObjs1) players[idx++] = p.transform;
        foreach (var p in playerObjs2) players[idx++] = p.transform;

        UpdateClosestPlayer();

        Buffermbledos = data.Cooldown;
    }

    protected virtual void Update()
    {
        // Jika sedang dalam proses mati, hentikan semua aktivitas
        if (isDead) return;

        float progress;

        UpdateClosestPlayer();

        if (closestPlayer == null) return;

        if (knockbackVelocity.magnitude > 0.05f)
        {
            agent.isStopped = true;
            transform.position += knockbackVelocity * Time.deltaTime;
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, Time.deltaTime * 10f);
        }
        else if (healthpoint >= 0f)
        {
            if (isAttacking)
            {
                agent.isStopped = true;
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(closestPlayer.position);
            }
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", isAttacking ? 0f : agent.velocity.magnitude);
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            FaceTarget();
            if (Time.time >= BufferShoot && data.stopDistance != 0 && bomaktif == false)
            {
                Shoot();
                BufferShoot = Time.time + data.Cooldown;
            }
        }

        //mbledos
        if (bomaktif)
        {
            Buffermbledos -= Time.deltaTime;

            progress = 1.0f - (Buffermbledos / data.Cooldown);
            UpdateShaderProgress(progress);

            if (Buffermbledos <= 0)
            {
                IniSaatnyaMbledos();
            }
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        if (isDead) return; // Cegah menyerang jika sudah mati

        if (data.stopDistance == 0)
        { //melee attack
            if (collision.gameObject.CompareTag("Player"))
            {
                if (Time.time >= BufferAttack)
                {
                    Health.Hurt(data.damage);
                    BufferAttack = Time.time + data.Cooldown;
                    isAttacking = true;
                    if (animator != null) animator.SetTrigger("Attack");
                    StartCoroutine(ResetAttack());
                }
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (isDead) return;

        if (data.stopDistance == 0 && collision.gameObject.CompareTag("Player"))
        {
            isAttacking = false;
            agent.isStopped = false;
        }
    }

    private System.Collections.IEnumerator ResetAttack()
    {
        yield return new WaitForSeconds(data.Cooldown);
        isAttacking = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isDead) return;

        if (collision.gameObject.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition = default)
    {
        // Cegah menerima damage lagi saat sedang dalam proses jeda mati (bug double loot)
        if (isDead) return;

        healthpoint -= damage;

        Vector3 knockDir = (transform.position - attackerPosition).normalized;
        knockbackVelocity = knockDir * 9f;

        if (enemyRenderer != null)
            StartCoroutine(FlashRed());

        if (healthpoint <= 0)
        {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        enemyRenderer.material.color = originalColor;
    }

    public virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        // Panggil fungsi proses kematian yang menggunakan jeda waktu
        StartCoroutine(DeathRoutine());
    }

    private System.Collections.IEnumerator DeathRoutine()
    {
        // 1. Matikan Pergerakan agar musuh mematung
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;

        // 2. Matikan Collider agar peluru dan player bisa tembus badannya saat dia mematung
        foreach (Collider c in GetComponents<Collider>())
        {
            c.enabled = false;
        }

        // JEDA WAKTU. 
        yield return new WaitForSeconds(0.3f);

        // 4. Putar SFX Mati / Meledak
        if (hasExploded)
        {
            PlaySound2D(explosionSFX);
        }
        else
        {
            PlaySound2D(deathSFX);
        }

        // 5. Munculkan Loot
        SpawnLoot();

        // 6. Hancurkan Objek Musuh
        Destroy(gameObject);
    }

    void SpawnLoot()
    {
        if (loot != null)
        {
            Instantiate(loot, transform.position, Quaternion.identity);
        }
    }

    protected void Shoot()
    {
        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        Quaternion shootRotation = Quaternion.LookRotation(direction);
        Vector3 shootPosition = new Vector3(transform.position.x, transform.position.y + bulletSpawnHeight, transform.position.z);
        GameObject peluru = Instantiate(data.peluru, shootPosition, shootRotation);
        peluru.GetComponent<Rigidbody>().linearVelocity = direction * 10;
    }

    protected void FaceTarget()
    {
        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    protected void UpdateClosestPlayer()
    {
        if (players == null || players.Length == 0) return;

        float minDist = Mathf.Infinity;
        Transform nearest = null;

        foreach (Transform p in players)
        {
            if (p == null) continue;
            float dist = Vector3.Distance(transform.position, p.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = p;
            }
        }

        closestPlayer = nearest;
    }

    //mbledos
    public void StartTimer()
    {
        bomaktif = true;
    }

    public void StopTimer()
    {
        bomaktif = false;
        Buffermbledos = data.Cooldown;
        UpdateShaderProgress(0f);
    }

    private void IniSaatnyaMbledos()
    {
        Debug.Log("Mbledos");

        Health.Hurt(data.damage);

        // Tandai bahwa musuh ini mati karena meledak
        hasExploded = true;

        Die();
    }

    private void UpdateShaderProgress(float value)
    {
        if (enemyRenderer != null)
        {
            enemyRenderer.GetPropertyBlock(propBlock);
            propBlock.SetFloat("_MbledosProgress", Mathf.Clamp01(value));
            enemyRenderer.SetPropertyBlock(propBlock);
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (data != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.stopDistance);
        }
    }

    private void PlaySound2D(AudioClip clip)
    {
        if (clip == null) return;

        GameObject audioObj = new GameObject("TempAudio_" + clip.name);
        AudioSource source = audioObj.AddComponent<AudioSource>();

        source.clip = clip;
        source.volume = volume;
        source.spatialBlend = 0f;
        source.Play();

        Destroy(audioObj, clip.length);
    }
}