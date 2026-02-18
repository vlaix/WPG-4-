using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    [SerializeField] private Transform Player;
    [SerializeField] private GameObject loot;
    [SerializeField] private EnemyData data;
    public float healthpoint;
    [SerializeField] private float BufferShoot;

    void Start()
    {
        healthpoint = data.healthPoint;
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
        Vector3 PlayerPosition = new Vector3(Player.position.x, transform.position.y, Player.position.z);
        transform.LookAt(new Vector3(Player.position.x, transform.position.y, Player.position.z));

        float distance = Vector3.Distance(transform.position, Player.position);

        if(distance <= data.stopDistance && data.stopDistance != 0) {
           if (Time.time >= BufferShoot) {
                isStop = true;
                Shoot();
                BufferShoot = Time.time + data.fireRate;
            } else {
                isStop = false;
            }
        }

        if (healthpoint != 0f) {
            if(isStop == false) {
                transform.position = Vector3.MoveTowards(transform.position, PlayerPosition, data.speed * Time.deltaTime);
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
}