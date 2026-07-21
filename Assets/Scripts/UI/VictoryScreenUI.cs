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

    [SerializeField] private Button _btnReplayGame;

    private void Awake()
    {
        UIRaycastHelper.FixScene();
        SceneScreenUILayout.ApplyVictory(transform);

        if (_btnMainMenu != null)
            _btnMainMenu.onClick.AddListener(OnMainMenu);

        if (_btnReplayGame != null)
        {
            _btnReplayGame.onClick.AddListener(
                OnReplayGame
            );
        }
    }

    private void Start()
    {
        if (_totalScoreText != null)
        {
            int totalScore =
                PlayerPrefs.GetInt(
                    "LS_TotalScore",
                    0
                );

            _totalScoreText.text =
                $"Tổng điểm: {totalScore}";
        }

        if (_totalTimeText != null)
        {
            float totalSeconds =
                PlayerPrefs.GetFloat(
                    "LS_TotalTime",
                    0f
                );

            int minutes =
                Mathf.FloorToInt(
                    totalSeconds / 60f
                );

            int seconds =
                Mathf.FloorToInt(
                    totalSeconds % 60f
                );

            _totalTimeText.text =
                $"Tổng thời gian: " +
                $"{minutes:00}:{seconds:00}";
        }

        if (_totalKillText != null)
        {
            int totalKills =
                PlayerPrefs.GetInt(
                    "LS_TotalKills",
                    0
                );

            _totalKillText.text =
                $"Tổng số quái: {totalKills}";
        }
    }

    private void OnMainMenu()
    {
        Time.timeScale = 1f;
        SceneLoadHelper.Load(MenuController.SCENE_MAIN_MENU);
    }

    private void OnReplayGame()
    {
        Time.timeScale = 1f;

        SceneLoadHelper.Load(
            MenuController.SCENE_DESERT
        );
    }
}
