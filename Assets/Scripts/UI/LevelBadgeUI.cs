using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Badge tên màn góc trên giữa HUD.
/// </summary>
public class LevelBadgeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _levelTagText;
    [SerializeField] private TMP_Text _levelTitleText;

    private void Start()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (scene == MenuController.SCENE_WARZONE)
        {
            SetLevel("MÀN 2", "Khu đô thị đổ nát");
        }
        else if (scene == MenuController.SCENE_UI_TEST)
        {
            SetLevel("DEMO", "Xem trước UI in-game");
        }
        else
        {
            SetLevel("MÀN 1", "Chiến trường Sa mạc");
        }
    }

    public void SetLevel(string tag, string title)
    {
        if (_levelTagText != null) _levelTagText.text = tag;
        if (_levelTitleText != null) _levelTitleText.text = title;
    }
}
