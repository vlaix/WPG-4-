using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private GameObject loot;
    [SerializeField] private EnemyData data;
    [SerializeField] Health Health;
    public float healthpoint;
    private float BufferShoot;
    private float BufferAttack;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        Health = Health.Instance;
        agent.speed = data.speed;
        agent.stoppingDistance = data.stopDistance;

        healthpoint = data.healthPoint;
        BufferAttack = 0f;
        float randomizer;

        GameObject playerObj1 = GameObject.FindWithTag("Player");
        GameObject playerObj2 = GameObject.FindWithTag("Player2");

        randomizer = Random.Range(0f, 2f);

        if(randomizer < 1) {

            Player = playerObj2.transform;
        } else {
            Player = playerObj1.transform;
        }        
    }

    void Update()
    {   
        if (healthpoint >= 0f) {
            agent.SetDestination(Player.position);
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
            if(collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Player2")) {
                if(Time.time >= BufferAttack) {
                    Health.Hurt(data.damage);
                    BufferAttack = Time.time + data.Cooldown;
                }
            }
        }
    }

    public void TakeDamage(float damage)
    {
        healthpoint -= damage;

        if(healthpoint <= 0) {
            Die();
        }
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

        Vector3 direction = (Player.position - transform.position).normalized;
        peluru.GetComponent<Rigidbody>().linearVelocity = direction * 10;
    }

    void FaceTarget()
    {
        Vector3 direction = (Player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
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