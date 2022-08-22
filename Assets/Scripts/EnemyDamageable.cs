using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BML.Scripts;
using BML.Scripts.Player;

namespace BML.Scripts
{
    public class EnemyDamageable : Damageable
    {
        public void TakeDamage(EnemyHitInfo enemyHitInfo)
        {
            base.TakeDamage(enemyHitInfo.Damage);
        }

        public void TakeCritDamage(EnemyHitInfo enemyHitInfo)
        {
            base.TakeCritDamage(enemyHitInfo.Damage);
        }
    }
}
