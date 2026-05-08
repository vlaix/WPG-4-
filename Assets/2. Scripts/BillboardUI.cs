using UnityEngine;

public class BillboardUI : MonoBehaviour
{
    private Transform camTransform;

    void Start()
    {
        // Cari kamera utama
        camTransform = Camera.main.transform;
    }

    void LateUpdate()
    {
        // Paksa canvas selalu menghadap ke arah yang sama dengan kamera
        transform.LookAt(transform.position + camTransform.forward);
    }
}