using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUI : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string retrySceneName = "Desert";
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(retrySceneName);
    }

    public void BackToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}