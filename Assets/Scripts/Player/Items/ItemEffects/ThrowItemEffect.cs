using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class ThrowItemEffect : ItemEffect
    {
        #region Inspector

        [AssetsOnly]
        public GameObject Prefab;
        public SafeFloatValueReference ThrowForce;
        public TransformSceneReference Container;

        #endregion

        private Transform _firePoint;
        public void PrimeEffect(Transform firePoint)
        {
            _firePoint = firePoint;
        }
        
        protected override bool ApplyEffectInternal()
        {
            // Calculate throw
            var throwDir = _firePoint.forward;
            // TODO make throw juicier. have throw angle affect the resulting object rotation/spin?
            var throwForce = throwDir * ThrowForce.Value;
            
            // Instantiate throwable
            var newGameObject = GameObjectUtils.SafeInstantiate(true, Prefab.gameObject, Container?.Value);
            newGameObject.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);
            var throwable = newGameObject.GetComponentInChildren<Throwable>();
            throwable.DoThrow(throwForce);
            
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