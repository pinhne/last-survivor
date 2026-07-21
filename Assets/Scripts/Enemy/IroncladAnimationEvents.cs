using UnityEngine;

public class IroncladAnimationEvents : MonoBehaviour
{
    private IroncladSpecter _ironcladSpecter;

    private void Awake()
    {
        _ironcladSpecter =
            GetComponentInParent<IroncladSpecter>();

        Debug.Log(
            $"[IroncladAnimationEvents] Tìm thấy IroncladSpecter: " +
            $"{_ironcladSpecter != null}"
        );
    }

    /// <summary>
    /// Gọi tại đúng frame Boss bắn hoặc tung đòn.
    /// </summary>
    public void AE_RangedHit()
    {
        Debug.Log(
            "[IroncladAnimationEvents] AE_RangedHit được gọi."
        );

        _ironcladSpecter?.ExecuteRangedAttackHit();
    }

    /// <summary>
    /// Gọi ở gần cuối animation Attack.
    /// </summary>
    public void AE_AttackFinished()
    {
        _ironcladSpecter
            ?.FinishRangedAttackFromAnimation();
    }
}