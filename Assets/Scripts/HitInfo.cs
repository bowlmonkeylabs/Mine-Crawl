using UnityEngine;

namespace BML.Scripts
{
    public class HitInfo
    {
        public int Damage;
        public Vector3 HitPositon;
        public Vector3 HitDirection;

        public HitInfo(int Damage, Vector3 HitDirection) {
            this.Damage = Damage;
            this.HitDirection = HitDirection;
        }

        public HitInfo(int Damage, Vector3 HitDirection, Vector3 HitPositon) {
            this.Damage = Damage;
            this.HitDirection = HitDirection;
            this.HitPositon = HitPositon;
        }
    }
}
