using UnityEngine;

// Script này gắn thêm vào prefab quái để override chỉ số đúng theo bảng mục 3
[RequireComponent(typeof(EnemyAI))]
[RequireComponent(typeof(EnemyHealth))]
public class NormalEnemy : MonoBehaviour
{
    // Chỉ số được set trong Inspector theo từng prefab:
    // Walker:  HP=60,  damage=10, speed=2.5, reward=15
    // Runner:  HP=40,  damage=8,  speed=5.0, reward=12
    // Tank:    HP=150, damage=20, speed=1.5, reward=25
}