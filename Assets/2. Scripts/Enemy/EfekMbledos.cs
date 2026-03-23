using UnityEngine;

public class EfekMbledos : MonoBehaviour
{
    [Header("Settings")]
    public float totalWaktuTimer = 3.0f;

    private float timerMbledos;
    private bool isCountingDown = false;
    [SerializeField] private Renderer enemyRenderer;
    [SerializeField] private MaterialPropertyBlock propBlock;

    void Awake()
    {
        enemyRenderer = GetComponent<Renderer>();
        propBlock = new MaterialPropertyBlock();
    }

    public void StartDeathTimer()
    {
        timerMbledos = totalWaktuTimer;
        isCountingDown = true;
    }

    void Update()
    {
        if (isCountingDown)
        {
            timerMbledos -= Time.deltaTime;
            float progress = 1.0f - (timerMbledos / totalWaktuTimer);

            // Pastikan nilai progress berada antara 0 dan 1
            progress = Mathf.Clamp01(progress);

            // Update nilai property float di shader
            // Ganti "_MbledosProgress" sesuai nama property yang Anda buat di Shader Graph
            SetShaderProgress(progress);

            if (timerMbledos <= 0)
            {
                isCountingDown = false;
            }
        }
    }

    public void SetShaderProgress(float progress)
    {
        enemyRenderer.GetPropertyBlock(propBlock);
        // Set nilai float. Nama property di shader biasanya diawali underscore.
        propBlock.SetFloat("_MbledosProgress", progress); 
        enemyRenderer.SetPropertyBlock(propBlock);
    }
}