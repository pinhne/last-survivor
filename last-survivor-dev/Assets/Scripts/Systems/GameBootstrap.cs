using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private LevelData levelData;

    private void Start()
    {
        SpawnManager.Instance.StartLevel(levelData);
    }
}