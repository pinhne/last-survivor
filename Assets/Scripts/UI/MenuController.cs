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
    public const string SCENE_DESERT = "Desert";
    public const string SCENE_WARZONE = "Warzone";
    public const string SCENE_UI_TEST = "UITest";
    public const string SCENE_VICTORY = "Victory";
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
    [SerializeField] private Button _btnRestartCurrentLevel;
    [SerializeField] private Button _btnPauseToMenu;

    [Header("Victory Screen")]
    [SerializeField] private GameObject _victoryPanel;
    [SerializeField] private TMP_Text _victoryScoreText;
    [SerializeField] private TMP_Text _victoryTimeText;
    [SerializeField] private TMP_Text _victoryKillText;
    [SerializeField] private Button _btnVictoryContinue;
    [SerializeField] private Button _btnVictoryReplay;
    [SerializeField] private Button _btnVictoryMenu;

    [Header("Game Over Screen")]
    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private TMP_Text _gameOverScoreText;
    [SerializeField] private TMP_Text _gameOverKillText;
    [SerializeField] private Button _btnRetry;
    [SerializeField] private Button _btnGameOverMenu;

    [Header("Gameplay HUD")]
    [SerializeField] private GameObject _crosshairRoot;

    private bool _isPaused;
    private bool _endOverlayOpen;

    private void Awake()
    {
        UIRaycastHelper.FixScene();
        ResolveCrosshairReference();
        BindButtons();

        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        if (_victoryPanel != null)
            _victoryPanel.SetActive(false);

        if (_gameOverPanel != null)
            _gameOverPanel.SetActive(false);

        FixVictoryOverlayLayout();

        if (IsGameplayScene())
        {
            LockGameplayCursor();
            SetCrosshairVisible(true);
        }
        else
        {
            UnlockCursor();
            SetCrosshairVisible(false);
        }
    }

    private static string FormatTime(
    float totalSeconds
    )
    {
        totalSeconds =
            Mathf.Max(0f, totalSeconds);

        int minutes =
            Mathf.FloorToInt(
                totalSeconds / 60f
            );

        int seconds =
            Mathf.FloorToInt(
                totalSeconds % 60f
            );

        return $"{minutes:00}:{seconds:00}";
    }

    private void UpdateVictoryStatistics()
    {
        LevelManager levelManager =
            LevelManager.Instance;

        if (levelManager == null)
            return;

        if (_victoryScoreText != null)
        {
            _victoryScoreText.text =
                $"Điểm: " +
                $"{levelManager.ScoreThisLevel}";
        }

        if (_victoryTimeText != null)
        {
            _victoryTimeText.text =
                $"Thời gian: " +
                $"{FormatTime(levelManager.ElapsedTimeThisLevel)}";
        }

        if (_victoryKillText != null)
        {
            _victoryKillText.text =
                $"Số quái đã tiêu diệt: " +
                $"{levelManager.KillsThisLevel}";
        }
    }
    private void ResolveCrosshairReference()
    {
        if (_crosshairRoot != null)
            return;

        Transform root = transform.root;
        Transform found = root.Find("Crosshair");

        if (found == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
                found = canvas.transform.Find("Crosshair");
        }

        if (found == null)
        {
            GameObject go = GameObject.Find("Crosshair");
            if (go != null)
                found = go.transform;
        }

        if (found != null)
            _crosshairRoot = found.gameObject;
    }

    private void SetCrosshairVisible(bool visible)
    {
        ResolveCrosshairReference();

        if (_crosshairRoot != null)
            _crosshairRoot.SetActive(visible);
    }

    private void BindButtons()
    {
        if (_btnVictoryReplay != null)
        {
            _btnVictoryReplay.onClick.AddListener(
                ReplayCurrentLevel
            );
        }
        if (_btnPlay != null)
            _btnPlay.onClick.AddListener(OnPlayClicked);

        if (_btnQuit != null)
            _btnQuit.onClick.AddListener(OnQuitClicked);

        if (_btnResume != null)
            _btnResume.onClick.AddListener(ResumeGame);

        if (_btnPauseToMenu != null)
            _btnPauseToMenu.onClick.AddListener(LoadMainMenu);

        if (_btnVictoryContinue != null)
        {
            _btnVictoryContinue.onClick.RemoveListener(OnVictoryContinue);
            _btnVictoryContinue.onClick.AddListener(OnVictoryContinue);
            _btnVictoryContinue.interactable = true;
        }

        if (_btnVictoryMenu != null)
        {
            _btnVictoryMenu.onClick.RemoveListener(LoadMainMenu);
            _btnVictoryMenu.onClick.AddListener(LoadMainMenu);
            _btnVictoryMenu.interactable = true;
        }

        if (_btnRestartCurrentLevel != null)
        {
            _btnRestartCurrentLevel.onClick.AddListener(
                RestartCurrentLevel
            );
        }

        if (_btnRetry != null)
            _btnRetry.onClick.AddListener(OnRetry);

        if (_btnGameOverMenu != null)
            _btnGameOverMenu.onClick.AddListener(LoadMainMenu);
    }

    private void ReplayCurrentLevel()
    {
        Time.timeScale = 1f;

        _endOverlayOpen = false;
        _isPaused = false;

        if (_victoryPanel != null)
            _victoryPanel.SetActive(false);

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void RestartCurrentLevel()
    {
        Time.timeScale = 1f;

        _isPaused = false;
        _endOverlayOpen = false;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }
    private void FixVictoryOverlayLayout()
    {
        if (_victoryPanel == null)
            return;

        Transform card =
            _victoryPanel.transform.Find(
                "OverlayCard"
            );

        if (card == null)
            return;

        ReparentIfNeeded(
            _victoryScoreText,
            card
        );

        ReparentIfNeeded(
            _victoryTimeText,
            card
        );

        ReparentIfNeeded(
            _victoryKillText,
            card
        );

        if (_victoryScoreText != null)
        {
            SetOverlayStat(
                _victoryScoreText.rectTransform,
                0.62f
            );
        }

        if (_victoryTimeText != null)
        {
            SetOverlayStat(
                _victoryTimeText.rectTransform,
                0.51f
            );
        }

        if (_victoryKillText != null)
        {
            SetOverlayStat(
                _victoryKillText.rectTransform,
                0.40f
            );
        }

        FixOverlayButton(
            _btnVictoryContinue,
            card,
            0.24f
        );

        FixOverlayButton(
            _btnVictoryReplay,
            card,
            0.14f
        );

        FixOverlayButton(
            _btnVictoryMenu,
            card,
            0.04f
        );

        RectTransform cardRect =
            card.GetComponent<RectTransform>();

        if (cardRect != null)
        {
            cardRect.sizeDelta =
                new Vector2(560f, 500f);
        }
    }

    private static void FixOverlayButton(
    Button button,
    Transform card,
    float anchorY
)
    {
        if (button == null || card == null)
            return;

        if (button.transform.parent != card)
        {
            button.transform.SetParent(
                card,
                false
            );
        }

        RectTransform rectTransform =
            button.GetComponent<RectTransform>();

        if (rectTransform == null)
            return;

        rectTransform.anchorMin =
            new Vector2(0.5f, anchorY);

        rectTransform.anchorMax =
            new Vector2(0.5f, anchorY);

        rectTransform.pivot =
            new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition =
            Vector2.zero;

        rectTransform.sizeDelta =
            new Vector2(320f, 52f);
    }
    private static void ReparentIfNeeded(TMP_Text text, Transform card)
    {
        if (text == null || text.transform.parent == card) return;
        text.transform.SetParent(card, false);
    }

    private static void SetOverlayStat(RectTransform rt, float anchorY)
    {
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(500f, 36f);
    }

    private void OnEnable()
    {
        LevelManager.OnLevelVictory += ShowVictory;
        LevelManager.OnLevelFailed += ShowGameOver;
    }

    private void OnDisable()
    {
        LevelManager.OnLevelVictory -= ShowVictory;
        LevelManager.OnLevelFailed -= ShowGameOver;
    }

    private void Update()
    {
        if (!IsGameplayScene()) return;
        if (_endOverlayOpen) return;

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
        if (_endOverlayOpen) return;

        _isPaused = true;
        if (_pausePanel != null)
        {
            _pausePanel.SetActive(true);
            _pausePanel.transform.SetAsLastSibling();
        }

        Time.timeScale = 0f;
        UnlockCursor();
        SetCrosshairVisible(false);
    }

    public void ResumeGame()
    {
        _isPaused = false;
        if (_pausePanel != null)
            _pausePanel.SetActive(false);

        Time.timeScale = 1f;

        if (IsGameplayScene())
        {
            LockGameplayCursor();
            SetCrosshairVisible(true);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SCENE_MAIN_MENU);
    }

    private void UpdateGameOverStatistics()
    {
        LevelManager levelManager =
            LevelManager.Instance;

        if (levelManager == null)
            return;

        if (_gameOverScoreText != null)
        {
            _gameOverScoreText.text =
                $"Điểm: " +
                $"{levelManager.ScoreThisLevel}";
        }

        if (_gameOverKillText != null)
        {
            _gameOverKillText.text =
                $"Số quái đã tiêu diệt: " +
                $"{levelManager.KillsThisLevel}";
        }
    }

    private void ShowVictory()
    {
        if (_victoryPanel == null)
            return;

        _endOverlayOpen = true;
        _isPaused = false;

        UIRaycastHelper.FixScene();

        _victoryPanel.SetActive(true);
        _victoryPanel.transform.SetAsLastSibling();

        UpdateVictoryStatistics();

        if (_btnVictoryContinue != null)
            _btnVictoryContinue.interactable = true;

        if (_btnVictoryReplay != null)
            _btnVictoryReplay.interactable = true;

        if (_btnVictoryMenu != null)
            _btnVictoryMenu.interactable = true;

        Time.timeScale = 0f;

        UnlockCursor();
        SetCrosshairVisible(false);
    }

    private void OnVictoryContinue()
    {
        Time.timeScale = 1f;
        _endOverlayOpen = false;
        _isPaused = false;

        if (_victoryPanel != null)
            _victoryPanel.SetActive(false);

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.ContinueAfterVictory();
            return;
        }

        // Fallback chỉ dùng khi scene gameplay bị thiếu LevelManager.
        string currentScene =
            SceneManager.GetActiveScene().name;

        Debug.LogWarning(
            $"[MenuController] Không tìm thấy LevelManager trong scene {currentScene}."
        );

        if (currentScene == SCENE_DESERT)
        {
            SceneManager.LoadScene(SCENE_WARZONE);
        }
        else if (currentScene == SCENE_WARZONE)
        {
            SceneManager.LoadScene(SCENE_VICTORY);
        }
        else
        {
            SceneManager.LoadScene(SCENE_MAIN_MENU);
        }
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null)
            return;

        _endOverlayOpen = true;
        _isPaused = false;

        UIRaycastHelper.FixScene();

        _gameOverPanel.SetActive(true);
        _gameOverPanel.transform.SetAsLastSibling();

        UpdateGameOverStatistics();

        if (_btnRetry != null)
            _btnRetry.interactable = true;

        if (_btnGameOverMenu != null)
            _btnGameOverMenu.interactable = true;

        Time.timeScale = 0f;

        UnlockCursor();
        SetCrosshairVisible(false);
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        _endOverlayOpen = false;

        if (LevelManager.Instance != null)
            LevelManager.Instance.RetryCurrentLevel();
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private static void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private static void LockGameplayCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
