using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private GameObject loot;
    [SerializeField] private EnemyData data;
    [SerializeField] Health Health;
    public float healthpoint;
    private float BufferShoot;
    private float BufferAttack;

    void Start()
    {
        Health = Health.Instance;

        healthpoint = data.healthPoint;
        BufferAttack = 0f;
        float randomizer;

        GameObject playerObj1 = GameObject.FindWithTag("Player");
        GameObject playerObj2 = GameObject.FindWithTag("Player2");

        randomizer = Random.Range(0f, 2f);
        Debug.Log("Randomizer = " + randomizer);

        if(randomizer < 1) {

            Player = playerObj2.transform;
        } else {
            Player = playerObj1.transform;
        }        
    }

    void Update()
    {   
        bool isStop = false;
        float distance;

        Vector3 PlayerPosition = new Vector3(Player.position.x, transform.position.y, Player.position.z);
        transform.LookAt(new Vector3(Player.position.x, transform.position.y, Player.position.z));
        distance = Vector3.Distance(transform.position, Player.position);

        if (distance <= data.stopDistance && data.stopDistance != 0) {
            isStop = true; 
        } else {
            isStop = false;
        }

        if(isStop) { //ranged attack
           if (Time.time >= BufferShoot) {
                Shoot();
                BufferShoot = Time.time + data.Cooldown;
            }
        }

        if (healthpoint >= 0f) {
            if(isStop == false) {
                transform.position = Vector3.MoveTowards(transform.position, PlayerPosition, data.speed * Time.deltaTime);
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

    void Shoot() {
        GameObject peluru = Instantiate(data.peluru, transform.position, Quaternion.identity);

        Vector3 direction = (Player.position - transform.position).normalized;
        peluru.GetComponent<Rigidbody>().linearVelocity = direction * 20;
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