using UnityEngine;

/// <summary>
/// Tự polish UI khi scene load — nền + hiệu ứng + layout.
/// </summary>
[DefaultExecutionOrder(-100)]
public class UILayoutBootstrap : MonoBehaviour
{
    private void Awake()
    {
        if (GetComponent<UIScenePolish>() == null)
            gameObject.AddComponent<UIScenePolish>();
    }
}
