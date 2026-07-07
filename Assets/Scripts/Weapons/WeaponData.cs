using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// ScriptableObject chứa toàn bộ thông số của 1 loại súng.
/// Tạo asset: chuột phải trong Project → Create → LastSurvivor → WeaponData
/// </summary>
[CreateAssetMenu(fileName = "Weapon_New", menuName = "LastSurvivor/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Info")]
    [FormerlySerializedAs("displayName")]
    public string weaponName = "Pistol";
    public Sprite weaponIcon;

    // Alias tương thích với UI/asset mới nếu đang gọi WeaponData.displayName.
    public string displayName
    {
        get => string.IsNullOrWhiteSpace(weaponName) ? name : weaponName;
        set => weaponName = value;
    }

    [Header("Stats")]
    public float damage = 25f;
    public float fireRate = 2f;
    public float range = 100f;
    public bool isAutoFire = false;

    [Header("Ammo")]
    public int maxAmmo = 12;
    public int maxReserve = 60;
    public float reloadTime = 1.5f;

    [Header("Camera Recoil")]
    [FormerlySerializedAs("recoilAmount")]
    public float verticalRecoil = 2f;

    public float horizontalRecoil = 0.25f;

    [FormerlySerializedAs("recoilRecover")]
    public float recoilRecoverSpeed = 8f;

    [Header("Weapon Visual Recoil")]
    public float weaponKickback = 0.05f;
    public float weaponKickUp = 2f;
    public float weaponKickSide = 0.5f;
    public float weaponReturnSpeed = 12f;

    [Header("Shotgun")]
    public bool isShotgun = false;
    public int pelletCount = 6;
    public float spreadAngle = 5f;

    [Header("Soft Aim Weapon Offset")]
    public bool moveWeaponOnAim = true;

    // Offset khi giữ chuột phải.
    // Z dương = đẩy súng ra xa camera hơn.
    // Y âm = hạ súng xuống.
    // X có thể chỉnh trái/phải.
    public Vector3 aimPositionOffset = new Vector3(0f, -0.03f, 0.1f);

    public Vector3 aimEulerOffset = new Vector3(-1f, 0f, 0f);


    [Header("Aiming / Zoom")]
    public bool isSniper = false;
    public float aimFOV = 50f;
    public float sniperFOV = 20f;

    [Header("Aim Position")]
    public Vector3 hipLocalPosition = Vector3.zero;
    public Vector3 hipLocalEuler = Vector3.zero;

    public Vector3 aimLocalPosition = Vector3.zero;
    public Vector3 aimLocalEuler = Vector3.zero;

    public float aimMoveSpeed = 12f;

    [Header("Animation")]
    public int weaponAnimationIndex = 0;

    [Header("Prefab")]
    public GameObject weaponPrefab;

    [Header("Audio")]
    public AudioClip shootSound;
    public AudioClip reloadSound;
    public AudioClip emptySound;
}