using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Scene Victory.unity — RULE §11: Back to Menu, Quit.
/// </summary>
public class VictoryScreenUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _totalScoreText;
    [SerializeField] private TMP_Text _totalTimeText;
    [SerializeField] private TMP_Text _totalKillText;
    [SerializeField] private Button _btnMainMenu;
    [SerializeField] private Button _btnQuit;

    private void Awake()
    {
        UIRaycastHelper.FixScene();
        SceneScreenUILayout.ApplyVictory(transform);

        if (_btnMainMenu != null)
            _btnMainMenu.onClick.AddListener(OnMainMenu);

        if (_btnQuit != null)
            _btnQuit.onClick.AddListener(OnQuit);
    }

    private void Start()
    {
        if (_totalScoreText != null)
            _totalScoreText.text = $"Tổng điểm: {PlayerPrefs.GetInt("LS_TotalScore", 0)} xu";

        if (_totalTimeText != null)
        {
            float totalSeconds = PlayerPrefs.GetFloat("LS_TotalTime", 0f);
            int min = Mathf.FloorToInt(totalSeconds / 60f);
            int sec = Mathf.FloorToInt(totalSeconds % 60f);
            _totalTimeText.text = $"Tổng thời gian: {min:00}:{sec:00}";
        }

        if (_totalKillText != null)
            _totalKillText.text = $"Tổng kill: {PlayerPrefs.GetInt("LS_TotalKills", 0)}";
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
