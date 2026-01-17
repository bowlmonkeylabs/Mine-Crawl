using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
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
        
        [SerializeField, Tooltip("Prevents throwing another item while the last thrown still exists.")] 
        private bool _onlyOneAtATime = false;

        #endregion

        private Throwable _lastThrown;

        private Transform _firePoint;
        public void PrimeEffect(Transform firePoint)
        {
            _firePoint = firePoint;
        }

        protected override bool ApplyEffectInternal()
        {
            if (_onlyOneAtATime && !_lastThrown.SafeIsUnityNull())
            {
                return false;
            }
            
            // Calculate throw
            var throwDir = _firePoint.forward;
            // TODO make throw juicier. have throw angle affect the resulting object rotation/spin? potentially you place when the ground is in range, but throw when it's further away?
            var throwForce = throwDir * ThrowForce.Value;
            
            // Instantiate throwable
            var newGameObject = GameObjectUtils.SafeInstantiate(true, Prefab.gameObject, Container?.Value);
            // newGameObject.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);
            newGameObject.transform.position = _firePoint.position;
            var throwable = newGameObject.GetComponentInChildren<Throwable>();
            throwable.DoThrow(throwForce);
            _lastThrown = throwable;
            
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