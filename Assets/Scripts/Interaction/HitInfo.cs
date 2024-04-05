using UnityEngine;

namespace BML.Scripts
{
    public class HitInfo
    {
        public DamageType DamageType;
        public int Damage;
        public Vector3? HitPositon;
        public Vector3? HitDirection;
        public float KnockbackTime = default;

        public HitInfo(DamageType DamageType, int Damage, Vector3 HitDirection) {
            this.DamageType = DamageType;
            this.Damage = Damage;
            this.HitDirection = HitDirection;
        }

        public HitInfo(DamageType DamageType, int Damage, Vector3 HitDirection, Vector3 HitPositon) {
            this.DamageType = DamageType;
            this.Damage = Damage;
            this.HitDirection = HitDirection;
            this.HitPositon = HitPositon;
        }
    }
}
