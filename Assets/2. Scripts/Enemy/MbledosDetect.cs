using System;
using UnityEngine;

public class MbledosDetect : MonoBehaviour
{
    [SerializeField] private EnemyBehavior EnemyBehavior;
    [SerializeField] private CapsuleCollider collidermbledos;
    [SerializeField] EnemyData datambledos;
    [SerializeField] private float rangembledos;

    void Awake()
    {
        collidermbledos = GetComponent<CapsuleCollider>();
    }
    void Start()
    {
        collidermbledos.radius = datambledos.stopDistance+1;
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("Player2")) {
            EnemyBehavior.StartTimer();
        }
    }

    // Ganti ke OnTriggerExit
    void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Player") || other.CompareTag("Player2")) {
            EnemyBehavior.StopTimer();
        }
    }
}
