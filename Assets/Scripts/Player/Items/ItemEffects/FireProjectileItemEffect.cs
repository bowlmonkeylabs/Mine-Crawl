using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class FireProjectileItemEffect : ItemEffect
    {
        #region Inspector

        [AssetsOnly]
        public GameObject Prefab;
        
        public TransformSceneReference Target;
        public TransformSceneReference Container;

        #endregion
        
        private Transform _firePoint;
        public void PrimeEffect(Transform firePoint)
        {
            _firePoint = firePoint;
        }

        protected override bool ApplyEffectInternal()
        {
            var newGameObject = GameObjectUtils.SafeInstantiate(true, Prefab, Container?.Value);
            newGameObject.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);
            
            return true;
        }

        protected override bool UnapplyEffectInternal()
        {
            return true;
        }

        public override void Reset()
        {
            
        }

    }
}