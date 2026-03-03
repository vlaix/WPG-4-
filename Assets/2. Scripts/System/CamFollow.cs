using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraFollow : MonoBehaviour
{
    [Header("Targets")]
    [Tooltip("Masukin player 1 dan player 2 ke sini")]
    public List<Transform> targets;

    [Header("Camera Movement")]
    public Vector3 offset; // Jarak dasar kamera dari player
    public float smoothTime = 0.5f; // Seberapa mulus pergerakan kamera
    private Vector3 velocity;

    [Header("Camera Zoom")]
    public float minZoom = 40f; // Zoom paling dekat (saat player nempel)
    public float maxZoom = 10f; // Zoom paling jauh (saat player berpencar)
    public float zoomLimiter = 50f; // Pembagi jarak untuk nentuin kecepatan zoom

    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void LateUpdate()
    {
        // Kalo list target kosong, kamera diem aja
        if (targets.Count == 0)
            return;

        Move();
        Zoom();
    }

    void Move()
    {
        // Cari titik tengah, tambahin offset, lalu gerakin kamera dengan mulus
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;
        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    void Zoom()
    {
        // Ngitung persentase zoom berdasarkan jarak player
        float newZoom = Mathf.Lerp(maxZoom, minZoom, GetGreatestDistance() / zoomLimiter);

        // CATATAN: Kalo game lo 3D (Perspective), pake fieldOfView
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, newZoom, Time.deltaTime);

        // CATATAN: Kalo game lo 2D (Orthographic), MATIKAN kode di atas, nyalain kode di bawah ini:
        // cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
    }

    float GetGreatestDistance()
    {
        // Bikin kotak (bounds) yang ngebungkus semua player, lalu ambil ukuran lebarnya
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        // Menggunakan jarak diagonal kotak biar aman buat atas-bawah & kiri-kanan
        return bounds.size.magnitude;
    }

    Vector3 GetCenterPoint()
    {
        // Kalo cuma ada 1 player yang hidup/aktif, langsung fokus ke dia
        if (targets.Count == 1)
        {
            return targets[0].position;
        }

        // Cari titik tengah dari kotak (bounds) yang ngebungkus semua player
        var bounds = new Bounds(targets[0].position, Vector3.zero);
        for (int i = 0; i < targets.Count; i++)
        {
            bounds.Encapsulate(targets[i].position);
        }

        return bounds.center;
    }
}