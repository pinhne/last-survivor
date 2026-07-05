using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Chọn màn — chỉ 2 ô: Desert (luôn mở) và Warzone (mở sau khi thắng Desert).
/// Dùng PlayerPrefs key chung để Kiệt (SaveManager) đồng bộ unlock.
/// </summary>
public class LevelSelectUI : MonoBehaviour
{
    public const string PREF_WARZONE_UNLOCKED = "LS_WarzoneUnlocked";

    [Header("Panel")]
    [SerializeField] private GameObject _levelSelectPanel;

    [Header("Desert — Màn 1")]
    [SerializeField] private Button _btnDesert;
    [SerializeField] private TMP_Text _desertLabel;

    [Header("Warzone — Màn 2")]
    [SerializeField] private Button _btnWarzone;
    [SerializeField] private TMP_Text _warzoneLabel;
    [SerializeField] private GameObject _warzoneLockIcon;

    [Header("Navigation")]
    [SerializeField] private Button _btnBack;
    [SerializeField] private GameObject _mainMenuPanel;

    private void Awake()
    {
        UIRaycastHelper.FixScene();

        if (_btnDesert != null)
            _btnDesert.onClick.AddListener(() => LoadLevel(MenuController.SCENE_DESERT));

        if (_btnWarzone != null)
            _btnWarzone.onClick.AddListener(OnWarzoneClicked);

        if (_btnBack != null)
            _btnBack.onClick.AddListener(Hide);

        if (_levelSelectPanel != null)
            _levelSelectPanel.SetActive(false);
    }

    private void OnEnable()
    {
        RefreshLockState();
    }

    public void Show()
    {
        if (_levelSelectPanel != null)
            _levelSelectPanel.SetActive(true);
        RefreshLockState();
    }

    public void Hide()
    {
        if (_levelSelectPanel != null)
            _levelSelectPanel.SetActive(false);

        if (_mainMenuPanel != null)
            _mainMenuPanel.SetActive(true);
    }

    public void RefreshLockState()
    {
        bool warzoneUnlocked = IsWarzoneUnlocked();

        if (_btnWarzone != null)
            _btnWarzone.interactable = warzoneUnlocked;

        if (_warzoneLockIcon != null)
            _warzoneLockIcon.SetActive(!warzoneUnlocked);

        if (_desertLabel != null)
        {
            _desertLabel.text = SceneLoadHelper.IsInBuild(MenuController.SCENE_DESERT)
                ? "Màn 1 — Chiến trường Sa mạc"
                : "Màn 1 — Demo UI (map Quân chưa giao)";
        }

        if (_warzoneLabel != null)
        {
            _warzoneLabel.text = warzoneUnlocked
                ? "Màn 2 — Khu đô thị đổ nát"
                : "Màn 2 — Đánh bại Boss Màn 1 để mở khóa";
        }
    }

    private void OnWarzoneClicked()
    {
        if (!IsWarzoneUnlocked()) return;
        LoadLevel(MenuController.SCENE_WARZONE);
    }

    private void LoadLevel(string sceneName)
    {
        SceneLoadHelper.Load(sceneName);
    }

    public static bool IsWarzoneUnlocked()
    {
        return PlayerPrefs.GetInt(PREF_WARZONE_UNLOCKED, 0) == 1;
    }

    public static void UnlockWarzone()
    {
        PlayerPrefs.SetInt(PREF_WARZONE_UNLOCKED, 1);
        PlayerPrefs.Save();
    }
}

