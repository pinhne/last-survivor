using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Level_XX", menuName = "LastSurvivor/LevelData")]
public class LevelData : ScriptableObject
{
    public int levelIndex;           // 1 hoặc 2
    public string mapScene;          // "Desert" hoặc "Warzone"
    public List<WaveData> waves;     // các wave quái thường
    public GameObject eliteBossPrefab; // Boss_Desert hoặc Boss_Warzone
}

[System.Serializable]
public class WaveData
{
    public GameObject enemyPrefab;
    public int enemyCount;
    public float spawnInterval;
    public float delayBeforeWave;
}