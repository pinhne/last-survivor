using UnityEngine;

/// <summary>
/// ScriptableObject chứa toàn bộ thông số của 1 loại súng.
/// Tạo asset: chuột phải trong Project → Create → LastSurvivor → WeaponData
/// </summary>
[CreateAssetMenu(fileName = "Weapon_New", menuName = "LastSurvivor/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    public string weaponName = "Pistol";
    public Sprite weaponIcon;               // Thu Hà dùng cho UI

    [Header("Stats")]
    public float damage = 25f;
    public float fireRate = 2f;        // số phát / giây
    public float range = 100f;      // tầm bắn (raycast distance)
    public bool isAutoFire = false;     // true = giữ chuột bắn liên tục

    [Header("Ammo")]
    public int maxAmmo = 12;        // đạn trong băng
    public int maxReserve = 60;        // đạn dự phòng
    public float reloadTime = 1.5f;      // giây để reload

    [Header("Recoil")]
    public float recoilAmount = 2f;        // độ giật camera lên (degrees)
    public float recoilRecover = 5f;        // tốc độ camera hồi về

    [Header("Shotgun")]
    public bool isShotgun = false;     // true chỉ dùng cho Shotgun
    public int pelletCount = 6;         // số viên đạn mỗi phát
    public float spreadAngle = 5f;        // góc tản (degrees)

    [Header("Sniper")]
    public bool isSniper = false;     // true chỉ dùng cho Sniper
    public float sniperFOV = 20f;       // FOV khi zoom scope

    [Header("Prefab")]
    public GameObject weaponPrefab;         // model súng hiển thị trong tay
    public AudioClip shootSound;           // Vy điền vào
    public AudioClip reloadSound;          // Vy điền vào
    public AudioClip emptySound;           // âm thanh khi hết đạn
}