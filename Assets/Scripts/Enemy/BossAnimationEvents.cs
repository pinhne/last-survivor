using UnityEngine;

public class BossAnimationEvents : MonoBehaviour
{
    private SandstormBrute _sandstormBrute;

    private void Awake()
    {
        _sandstormBrute =
            GetComponentInParent<SandstormBrute>();

        if (_sandstormBrute == null)
        {
            Debug.LogError(
                "[BossAnimationEvents] Không tìm thấy " +
                "SandstormBrute ở object cha."
            );
        }
    }

    /// <summary>
    /// Đặt event này tại frame tay/vũ khí
    /// của Boss chạm Player.
    /// </summary>
    public void AE_DealDamage()
    {
        Debug.Log("[BossAnimationEvents] AE_DealDamage đã được gọi.");
        _sandstormBrute
            ?.ApplyAttackDamageFromAnimation();
    }

    /// <summary>
    /// Đặt event này gần frame cuối animation Attack.
    /// </summary>
    public void AE_AttackFinished()
    {
        _sandstormBrute
            ?.FinishAttackFromAnimation();
    }
}