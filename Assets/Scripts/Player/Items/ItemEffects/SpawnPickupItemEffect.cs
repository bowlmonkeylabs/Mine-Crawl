using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class SpawnPickupItemEffect : ItemEffect
    {
        #region Inspector

        [AssetsOnly] public PlayerItem Item;
        [AssetsOnly] public GameObject BasePickupPrefab;
        public SafeFloatValueReference ThrowForce;
        public TransformSceneReference Container;
        
        [SerializeField, LabelText("Pickup Delay"), Tooltip("Delay before pickup will be able to be activated by the player. Set to <0 to use the default value defined on the pickup prefab.")]
        private float _pickupDelaySetting = -1;
        private float? _pickupDelay => (_pickupDelaySetting < 0 ? null : (float?)_pickupDelaySetting);
        
        [SerializeField, Tooltip("Prevents throwing another item while the last thrown still exists.")] 
        private bool _onlyOneAtATime = false;

        #endregion

        private ItemPickupController _lastThrown;

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
            
            // Instantiate pickup
            var newGameObject = GameObjectUtils.SafeInstantiate(true, BasePickupPrefab, Container?.Value);
            newGameObject.transform.SetPositionAndRotation(_firePoint.position, _firePoint.rotation);
            var itemPickupController = newGameObject.GetComponentInChildren<ItemPickupController>();
            if (itemPickupController != null)
            {
                itemPickupController.SetItem(Item);
                if (_pickupDelay != null)
                {
                    itemPickupController.SetPickupDelay(_pickupDelay.Value);
                }
            }
            itemPickupController.PickupRigidbody.AddForce(throwForce, ForceMode.Impulse);
            _lastThrown = itemPickupController;
            
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