using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Level Manager - Handles level progression and completion
/// Use this for in-game level transitions (next level, restart, etc.)
/// </summary>
public class LevelManager : MonoBehaviour
{
    [Header("Level Settings")]
    [Tooltip("Current level number (auto-detected from scene name)")]
    public int currentLevel = 1;

    [Tooltip("Total number of levels in the game")]
    public int totalLevels = 10;

    [Header("Scene Names")]
    [Tooltip("Name of the main menu scene")]
    public string mainMenuScene = "MainMenu";

    [Tooltip("Name of the level select scene")]
    public string levelSelectScene = "LevelSelector";

    [Header("Auto-Detect")]
    [Tooltip("Auto-detect current level from scene name?")]
    public bool autoDetectLevel = true;

    private void Start()
    {
        if (autoDetectLevel)
        {
            DetectCurrentLevel();
        }
    }

    /// <summary>
    /// Detects current level number from scene name
    /// Example: "LVL 1" → level 1, "LVL 15" → level 15
    /// </summary>
    private void DetectCurrentLevel()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // Try to extract number from scene name
        // Supports formats: "LVL 1", "Level 1", "Level_01", etc.
        string numberStr = "";

        for (int i = 0; i < sceneName.Length; i++)
        {
            if (char.IsDigit(sceneName[i]))
            {
                numberStr += sceneName[i];
            }
        }

        if (!string.IsNullOrEmpty(numberStr))
        {
            if (int.TryParse(numberStr, out int detectedLevel))
            {
                currentLevel = detectedLevel;
                Debug.Log($"Auto-detected level: {currentLevel}");
            }
        }
    }

    /// <summary>
    /// Complete current level and unlock next level
    /// Call this when player wins/completes the level
    /// </summary>
    public void CompleteLevel()
    {
        Debug.Log($"Level {currentLevel} completed!");

        // Unlock next level
        int levelReached = PlayerPrefs.GetInt("levelReached", 1);

        if (currentLevel >= levelReached)
        {
            // Unlock next level
            PlayerPrefs.SetInt("levelReached", currentLevel + 1);
            PlayerPrefs.Save();
            Debug.Log($"Level {currentLevel + 1} unlocked!");
        }
    }

    /// <summary>
    /// Load next level with loading screen
    /// </summary>
    public void LoadNextLevel()
    {
        if (currentLevel < totalLevels)
        {
            int nextLevel = currentLevel + 1;
            string nextSceneName = "LVL " + nextLevel;

            Debug.Log($"Loading next level: {nextSceneName}");
            LoadingScreen.LoadScene(nextSceneName);
        }
        else
        {
            Debug.Log("This is the last level! Returning to menu.");
            ReturnToMenu();
        }
    }

    /// <summary>
    /// Reload current level with loading screen
    /// </summary>
    public void RestartLevel()
    {
        Debug.Log($"Restarting level {currentLevel}");
        LoadingScreen.ReloadCurrentScene();
    }

    /// <summary>
    /// Return to main menu with loading screen
    /// </summary>
    public void ReturnToMenu()
    {
        Debug.Log("Returning to main menu");
        LoadingScreen.LoadScene(mainMenuScene);
    }

    /// <summary>
    /// Return to level select screen with loading screen
    /// </summary>
    public void ReturnToLevelSelect()
    {
        Debug.Log("Returning to level select");
        LoadingScreen.LoadScene(levelSelectScene);
    }

    /// <summary>
    /// Load specific level by number
    /// </summary>
    public void LoadLevel(int levelNumber)
    {
        if (levelNumber >= 1 && levelNumber <= totalLevels)
        {
            string sceneName = "LVL " + levelNumber;
            Debug.Log($"Loading level: {sceneName}");
            LoadingScreen.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError($"Invalid level number: {levelNumber}");
        }
    }

    /// <summary>
    /// Complete level and go to next level
    /// Convenience method that combines CompleteLevel + LoadNextLevel
    /// </summary>
    public void CompleteAndLoadNext()
    {
        CompleteLevel();
        LoadNextLevel();
    }

    // ===== PUBLIC METHODS FOR UI BUTTONS =====

    /// <summary>
    /// Call from "Next Level" button
    /// </summary>
    public void OnNextLevelButton()
    {
        CompleteAndLoadNext();
    }

    /// <summary>
    /// Call from "Restart" button
    /// </summary>
    public void OnRestartButton()
    {
        RestartLevel();
    }

    /// <summary>
    /// Call from "Main Menu" button
    /// </summary>
    public void OnMainMenuButton()
    {
        ReturnToMenu();
    }

    /// <summary>
    /// Call from "Level Select" button
    /// </summary>
    public void OnLevelSelectButton()
    {
        ReturnToLevelSelect();
    }

    /// <summary>
    /// Call from "Quit" button
    /// </summary>
    public void OnQuitButton()
    {
        Debug.Log("Quitting game...");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}