using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Transform[] players;
    private Transform closestPlayer;
    [SerializeField] private GameObject loot;
    [SerializeField] private EnemyData data;
    [SerializeField] Health Health;
    public float healthpoint;
    private float BufferShoot;
    private float BufferAttack;
    private NavMeshAgent agent;
    private Animator animator;
    private bool isAttacking = false;
    private Renderer enemyRenderer;
    private Color originalColor;
    private Vector3 knockbackVelocity;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer != null) originalColor = enemyRenderer.material.color;
    }

    void Start()
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
    }

    void Update()
    {   
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
                FaceTarget(); // Fungsi untuk menghadap player saat berhenti
                if (Time.time >= BufferShoot && data.stopDistance != 0)
                {
                    Shoot();
                    BufferShoot = Time.time + data.Cooldown;
                }
            }
    }

    private void OnCollisionStay(Collision collision)
    {
        if(data.stopDistance == 0) { //melee attack
            if(collision.gameObject.CompareTag("Player")) {
                if(Time.time >= BufferAttack) {
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
        if(data.stopDistance == 0 && collision.gameObject.CompareTag("Player"))
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
        if(collision.gameObject.CompareTag("Obstacle")) {
            Destroy(gameObject);
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition = default)
    {
        healthpoint -= damage;

        Vector3 knockDir = (transform.position - attackerPosition).normalized;
        knockbackVelocity = knockDir * 9f;

        if (enemyRenderer != null)
            StartCoroutine(FlashRed());

        if(healthpoint <= 0) {
            Die();
        }
    }

    private System.Collections.IEnumerator FlashRed()
    {
        enemyRenderer.material.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        enemyRenderer.material.color = originalColor;
    }

    private void Die()
    {
        SpawnLoot();
        Destroy(gameObject);
    }

    void SpawnLoot()
    {
        Instantiate(loot, transform.position, Quaternion.identity);
    }

    void Shoot() 
    {
        GameObject peluru = Instantiate(data.peluru, transform.position, Quaternion.identity);

        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        peluru.GetComponent<Rigidbody>().linearVelocity = direction * 10;
    }

    void FaceTarget()
    {
        Vector3 direction = (closestPlayer.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    private void UpdateClosestPlayer()
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

    private void OnDrawGizmosSelected()
    {
        if (data != null) 
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, data.stopDistance);
        }
    }
}