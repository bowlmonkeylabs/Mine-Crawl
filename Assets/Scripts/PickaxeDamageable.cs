using UnityEngine;
using UnityEngine.Events;
using BML.Scripts;
using BML.Scripts.Player;

public class PickaxeDamageable : Damageable
{
    public void TakeDamage(PickaxeHitInfo pickaxeHitInfo)
    {
        base.TakeDamage(pickaxeHitInfo.Damage);
    }

    public void TakeCritDamage(PickaxeHitInfo pickaxeHitInfo)
    {
        base.TakeCritDamage(pickaxeHitInfo.Damage);
    }
}
