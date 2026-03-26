using UnityEngine;

public class LevelGoal : MonoBehaviour
{
    public GameObject levelCompleteUI;
    private LevelManager levelManager;

    private void Start()
    {
        levelManager = FindObjectOfType<LevelManager>();

        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.CompareTag("Player2"))
        {
            OnLevelComplete();
        }
    }

    private void OnLevelComplete()
    {
        Debug.Log("Level Complete!");

        // Mark level as complete
        if (levelManager != null)
        {
            levelManager.CompleteLevel();
        }

        // Show complete UI
        if (levelCompleteUI != null)
        {
            levelCompleteUI.SetActive(true);
        }

        // Pause game (optional)
        Time.timeScale = 0f;
    }
}