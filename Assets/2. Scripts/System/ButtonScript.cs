using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{   
    [SerializeField] private WinCondition WinCondition;
    [SerializeField] private int currentLevel;

    public void Restart(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1.0f;
    }

    public void LevelSelect(){
        WinCondition.UnlockNextLevel(currentLevel);
        SceneManager.LoadScene("LevelSelector");
    }
}
