using UnityEngine;

public class LaserSight : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [Header("Settings")]
    public float laserDistance = 50f; // Jarak maksimum laser

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        ShootLaser();
    }

    void ShootLaser()
    {
        // Titik awal laser adalah posisi objek ini
        lineRenderer.SetPosition(0, transform.position);

        RaycastHit hit;

        // Menembakkan Raycast ke arah depan (Forward)
        if (Physics.Raycast(transform.position, transform.forward, out hit, laserDistance))
        {
            // Jika terkena sesuatu, ujung Line Renderer berhenti di titik tabrakan
            lineRenderer.SetPosition(1, hit.point);
        }
        else
        {
            // Jika tidak kena apa-apa, laser lurus sepanjang laserDistance
            lineRenderer.SetPosition(1, transform.position + (transform.forward * laserDistance));
        }
    }
}