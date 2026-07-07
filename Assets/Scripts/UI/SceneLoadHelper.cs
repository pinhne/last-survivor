using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Load scene an toàn — fallback UIDemo khi Desert/Warzone chưa có trong build.
/// </summary>
public static class SceneLoadHelper
{
    public static bool IsInBuild(string sceneName)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string path = SceneUtility.GetScenePathByBuildIndex(i);
            if (System.IO.Path.GetFileNameWithoutExtension(path) == sceneName)
                return true;
        }
        return false;
    }

    public static void Load(string sceneName)
    {
        Time.timeScale = 1f;

        if (!IsInBuild(sceneName))
        {
            if (sceneName == MenuController.SCENE_DESERT || sceneName == MenuController.SCENE_WARZONE)
            {
                Debug.LogWarning(
                    $"[UI] Scene '{sceneName}' chưa có — mở '{MenuController.SCENE_UI_TEST}' để xem HUD.");
                sceneName = MenuController.SCENE_UI_TEST;
            }
        }

        if (!IsInBuild(sceneName))
        {
            Debug.LogError($"[UI] Scene '{sceneName}' không có trong Build Settings.");
            return;
        }

        PlayerPrefs.SetString("LS_LastLevel", sceneName);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }
}
