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

        if (_btnRetry != null)
            _btnRetry.onClick.AddListener(OnRetry);

        if (_btnGameOverMenu != null)
            _btnGameOverMenu.onClick.AddListener(LoadMainMenu);
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
            cardRt.sizeDelta = new Vector2(560f, 430f);
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
        rt.sizeDelta = new Vector2(320f, 52f);
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

    private void ShowVictory()
    {
        if (_victoryPanel == null) return;

        _endOverlayOpen = true;
        _isPaused = false;

        FixVictoryOverlayLayout();
        UIRaycastHelper.FixScene();

        _victoryPanel.SetActive(true);
        _victoryPanel.transform.SetAsLastSibling();

        if (_btnVictoryContinue != null)
            _btnVictoryContinue.interactable = true;
        if (_btnVictoryMenu != null)
            _btnVictoryMenu.interactable = true;

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

        Time.timeScale = 0f;
        UnlockCursor();
        SetCrosshairVisible(false);
    }

    private void OnVictoryContinue()
    {
        Time.timeScale = 1f;
        _endOverlayOpen = false;

        if (_victoryPanel != null)
            _victoryPanel.SetActive(false);

        SetCrosshairVisible(true);

        if (LevelManager.Instance != null)
            LevelManager.Instance.ContinueAfterVictory();
        else
            SceneManager.LoadScene(SCENE_WARZONE);
    }

    private void ShowGameOver()
    {
        if (_gameOverPanel == null) return;

        _endOverlayOpen = true;
        _isPaused = false;

        UIRaycastHelper.FixScene();
        _gameOverPanel.SetActive(true);
        _gameOverPanel.transform.SetAsLastSibling();

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
