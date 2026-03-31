using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

/// <summary>
/// Loading Screen Manager - Handles scene transitions with loading screen
/// Attach this to a GameObject in the Loading scene
/// </summary>
public class LoadingScreen : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Progress bar fill image")]
    public Image progressBarFill;

    [Tooltip("Loading percentage text")]
    public TextMeshProUGUI progressText;

    [Tooltip("Loading tips text (optional)")]
    public TextMeshProUGUI tipsText;

    [Tooltip("Scene name text (optional)")]
    public TextMeshProUGUI sceneNameText;

    [Header("Settings")]
    [Tooltip("Minimum loading time in seconds")]
    [Range(0.5f, 5f)]
    public float minimumLoadTime = 1.5f;

    [Tooltip("Tips to show during loading")]
    public string[] loadingTips = new string[]
    {
        "Press Q to build structures",
        "Hold Shift to activate shield",
        "Collect resources to build bridges and ladders",
        "Press W/S to climb ladders",
        "Use WASD to move around",
        "Work together with your teammate!",
        "Shield blocks enemy attacks",
        "Build bridges to cross gaps",
        "Resources can be found around the map"
    };

    [Header("Animation")]
    [Tooltip("Smooth progress bar animation")]
    public bool smoothProgress = true;

    [Tooltip("Progress bar animation speed")]
    [Range(0.1f, 5f)]
    public float progressSpeed = 2f;

    // Private
    private static string sceneToLoad;
    private float targetProgress = 0f;
    private float currentProgress = 0f;

    private void Start()
    {
        // Start loading the target scene
        if (!string.IsNullOrEmpty(sceneToLoad))
        {
            StartCoroutine(LoadSceneAsync(sceneToLoad));
        }
        else
        {
            Debug.LogError("No scene to load! Use LoadingScreen.LoadScene(sceneName) to load a scene.");
        }

        // Show random tip
        if (tipsText != null && loadingTips.Length > 0)
        {
            tipsText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }

        // Show scene name
        if (sceneNameText != null && !string.IsNullOrEmpty(sceneToLoad))
        {
            sceneNameText.text = $"Loading {sceneToLoad}...";
        }
    }

    private void Update()
    {
        currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, progressSpeed * Time.unscaledDeltaTime);
        // Smooth progress bar animation
        if (smoothProgress && progressBarFill != null)
        {
            currentProgress = Mathf.MoveTowards(currentProgress, targetProgress, progressSpeed * Time.deltaTime);
            progressBarFill.fillAmount = currentProgress;

            if (progressText != null)
            {
                progressText.text = $"{Mathf.FloorToInt(currentProgress * 100)}%";
            }
        }
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        float startTime = Time.time;

        // Start loading the scene asynchronously
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // Loading loop
        while (!operation.isDone)
        {
            // Calculate progress (0 to 0.9 = loading, 0.9+ = done but waiting)
            float progress = Mathf.Clamp01(operation.progress / 0.9f);

            if (smoothProgress)
            {
                targetProgress = progress;
            }
            else
            {
                // Direct progress update
                if (progressBarFill != null)
                {
                    progressBarFill.fillAmount = progress;
                }

                if (progressText != null)
                {
                    progressText.text = $"{Mathf.FloorToInt(progress * 100)}%";
                }
            }

            // Check if loading is done
            if (operation.progress >= 0.9f)
            {
                // Wait for minimum load time
                float elapsedTime = Time.time - startTime;
                if (elapsedTime < minimumLoadTime)
                {
                    yield return new WaitForSeconds(minimumLoadTime - elapsedTime);
                }

                // Fill progress to 100%
                if (smoothProgress)
                {
                    targetProgress = 1f;
                    // Wait until progress bar is full
                    while (currentProgress < 0.99f)
                    {
                        yield return null;
                    }
                }
                else
                {
                    if (progressBarFill != null) progressBarFill.fillAmount = 1f;
                    if (progressText != null) progressText.text = "100%";
                }

                // Activate the scene
                operation.allowSceneActivation = true;
            }

            yield return null;
        }
    }

    /// <summary>
    /// Load a scene with loading screen
    /// Call this from any script: LoadingScreen.LoadScene("LevelName")
    /// </summary>
    public static void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        sceneToLoad = sceneName;
        SceneManager.LoadScene("Loading"); // Make sure you have a scene named "Loading"
    }

    /// <summary>
    /// Reload current scene with loading screen
    /// </summary>
    public static void ReloadCurrentScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        LoadScene(currentScene);
    }
}