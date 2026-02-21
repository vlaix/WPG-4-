using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Stat")]
    public float speed;
    public float healthPoint;
    public float stopDistance;
    public float Cooldown;
    public int damage;

    [HeaderAttribute("Visual")]
    public GameObject prefab;
    public GameObject peluru;
}
