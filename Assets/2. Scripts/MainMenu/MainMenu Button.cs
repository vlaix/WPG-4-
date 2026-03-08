using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour
{
    [SerializeField] private bool RestartProgress;

    void Start()
    {
        if(RestartProgress) {
            PlayerPrefs.DeleteAll();
        }
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LevelSelector()
    {
        SceneManager.LoadScene("LevelSelector");
    }
}
