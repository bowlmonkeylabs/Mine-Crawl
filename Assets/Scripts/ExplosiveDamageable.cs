using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BML.Scripts;
using BML.Scripts.Player;

namespace BML.Scripts
{
    public class ExplosiveDamageable : Damageable
    {
        public void TakeDamage(ExplosionHitInfo explosionHitInfo)
        {
            base.TakeDamage(explosionHitInfo.Damage);
        }

        public void TakeCritDamage(ExplosionHitInfo explosionHitInfo)
        {
            base.TakeCritDamage(explosionHitInfo.Damage);
        }
    }
}
