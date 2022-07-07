using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
