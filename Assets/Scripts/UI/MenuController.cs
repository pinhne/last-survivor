using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Điều khiển Main Menu, Victory, GameOver và Pause screen.
/// Load scene bằng tên string theo quy tắc nhóm.
/// </summary>
public class MenuController : MonoBehaviour
{
    public const string SCENE_MAIN_MENU = "MainMenu";
    public const string SCENE_DESERT    = "Desert";
    public const string SCENE_WARZONE   = "Warzone";
    public const string SCENE_UI_TEST   = "UITest";
    public const string SCENE_VICTORY   = "Victory";
    public const string SCENE_GAME_OVER = "GameOver";

    [Header("Main Menu")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private Button _btnPlay;
    [SerializeField] private Button _btnQuit;

    [Header("Level Select")]
    [SerializeField] private LevelSelectUI _levelSelectUI;

    [Header("Pause")]
    [SerializeField] private GameObject _pausePanel;
    [SerializeField] private Button _btnResume;
    [SerializeField] private Button _btnPauseToMenu;

    [Header("Victory Screen")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private TMP_Text _victoryScoreText;
    [SerializeField] private TMP_Text _victoryTimeText;
    [SerializeField] private TMP_Text _victoryKillText;
    [SerializeField] private Button _btnVictoryContinue;
    [SerializeField] private Button _btnVictoryMenu;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private Button _btnRetry;
    [SerializeField] private Button _btnGameOverMenu;

    private bool _isPaused;

    private void Awake()
    {
        UIRaycastHelper.FixScene();

        if (_btnPlay != null)
            _btnPlay.onClick.AddListener(OnPlayClicked);

        if (_btnQuit != null)
            _btnQuit.onClick.AddListener(OnQuitClicked);

        if (_btnResume != null)
            _btnResume.onClick.AddListener(ResumeGame);

        if (_btnPauseToMenu != null)
            _btnPauseToMenu.onClick.AddListener(LoadMainMenu);

        if (_btnVictoryContinue != null)
            _btnVictoryContinue.onClick.AddListener(OnVictoryContinue);

        if (_btnVictoryMenu != null)
            _btnVictoryMenu.onClick.AddListener(LoadMainMenu);

        if (_btnRetry != null)
            _btnRetry.onClick.AddListener(OnRetry);

        if (_btnGameOverMenu != null)
            _btnGameOverMenu.onClick.AddListener(LoadMainMenu);

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_victoryPanel != null)
            _victoryPanel.SetActive(false);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);
    }

    private void OnEnable()
    {
        LevelManager.OnLevelVictory += ShowVictory;
        LevelManager.OnLevelFailed  += ShowGameOver;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelVictory -= ShowVictory;
        LevelManager.OnLevelFailed  -= ShowGameOver;
    }

    private void Update()
    {
        if (!IsGameplayScene()) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_isPaused) ResumeGame();
            else PauseGame();
        }
    }

    private bool IsGameplayScene()
    {
        string scene = SceneManager.GetActiveScene().name;
        return scene == SCENE_DESERT || scene == SCENE_WARZONE || scene == SCENE_UI_TEST;
    }

    private void OnPlayClicked()
    {
        if (_mainMenuPanel != null)
            _mainMenuPanel.SetActive(false);

        if (_levelSelectUI != null)
            _levelSelectUI.Show();
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void PauseGame()
    {
        _isPaused = true;
        if (_pausePanel != null)
            _pausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        _isPaused = false;
        if (_pausePanel != null)
            _pausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SCENE_MAIN_MENU);
    }

    private void ShowVictory()
    {
        if (_victoryPanel == null) return;

        _victoryPanel.SetActive(true);
        Time.timeScale = 0f;

        if (LevelManager.Instance != null)
        {
            int level = LevelManager.Instance.CurrentLevel;

            if (level == 1)
                LevelSelectUI.UnlockWarzone();

            if (_victoryScoreText != null && EconomyManager.Instance != null)
                _victoryScoreText.text = $"Điểm: {EconomyManager.Instance.CurrentMoney} xu";

            if (_victoryTimeText != null)
            {
                float remaining = LevelManager.Instance.TimeRemaining;
                float elapsed = LevelManager.LEVEL_TIME_LIMIT - remaining;
                int min = Mathf.FloorToInt(elapsed / 60f);
                int sec = Mathf.FloorToInt(elapsed % 60f);
                _victoryTimeText.text = $"Thời gian: {min:00}:{sec:00}";
            }
        }
    }

    private void OnVictoryContinue()
    {
        Time.timeScale = 1f;

        if (LevelManager.Instance != null && LevelManager.Instance.CurrentLevel == 1)
            SceneManager.LoadScene(SCENE_WARZONE);
        else
            SceneManager.LoadScene(SCENE_VICTORY);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null) return;

        _gameOverPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}

