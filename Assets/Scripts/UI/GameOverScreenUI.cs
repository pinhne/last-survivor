using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene GameOver.unity — RULE §11: Retry, Back to Menu, Quit.
/// </summary>
public class GameOverScreenUI : MonoBehaviour
{
    [SerializeField] private Button _btnRetry;
    [SerializeField] private Button _btnMainMenu;
    [SerializeField] private Button _btnQuit;

    private void Awake()
    {
        UIRaycastHelper.FixScene();
        SceneScreenUILayout.ApplyGameOver(transform);

        if (_btnRetry != null)
            _btnRetry.onClick.AddListener(OnRetry);

        if (_btnMainMenu != null)
            _btnMainMenu.onClick.AddListener(OnMainMenu);

        if (_btnQuit != null)
            _btnQuit.onClick.AddListener(OnQuit);
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        string lastLevel = PlayerPrefs.GetString("LS_LastLevel", MenuController.SCENE_UI_TEST);
        SceneLoadHelper.Load(lastLevel);
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoadHelper.Load(MenuController.SCENE_MAIN_MENU);
    }

    private void OnQuit()
    {
        Time.timeScale = 1f;
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
