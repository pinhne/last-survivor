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

        FixVictoryOverlayLayout();
    }

    private void FixVictoryOverlayLayout()
    {
        if (_victoryPanel == null) return;

        var card = _victoryPanel.transform.Find("OverlayCard");
        if (card == null) return;

        ReparentIfNeeded(_victoryScoreText, card);
        ReparentIfNeeded(_victoryTimeText, card);

        if (_victoryScoreText != null)
            SetOverlayStat(_victoryScoreText.rectTransform, 0.50f);
        if (_victoryTimeText != null)
            SetOverlayStat(_victoryTimeText.rectTransform, 0.38f);

        FixOverlayButton(card, "Btn_1", 0.22f);
        FixOverlayButton(card, "Btn_2", 0.08f);

        var cardRt = card.GetComponent<RectTransform>();
        if (cardRt != null)
            cardRt.sizeDelta = new Vector2(520f, 420f);
    }

    private static void FixOverlayButton(Transform card, string name, float anchorY)
    {
        var t = card.Find(name);
        if (t == null) return;
        var rt = t.GetComponent<RectTransform>();
        if (rt == null) return;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, anchorY);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
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
        rt.sizeDelta = new Vector2(480f, 32f);
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

        FixVictoryOverlayLayout();
        _victoryPanel.SetActive(true);
        _victoryPanel.transform.SetAsLastSibling();
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
        // RULE §13: LevelManager/SpawnManager xử lý load scene — UI chỉ đóng overlay.
        Time.timeScale = 1f;
        if (_victoryPanel != null)
            _victoryPanel.SetActive(false);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null) return;

        _gameOverPanel.SetActive(true);
        _gameOverPanel.transform.SetAsLastSibling();
        Time.timeScale = 0f;
    }

    private void OnRetry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
