using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Player.Items;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items.ItemEffects
{
    public class InstantiatePrefabItemEffect : ItemEffect
    {
        #region Inspector

        [AssetsOnly]
        public GameObject Prefab;
        
        public bool UsePickaxeHitPosition;
        [HideIf("UsePickaxeHitPosition")] public TransformSceneReference Target;
        
        public TransformSceneReference Container;

        #endregion

        private Vector3 _pickaxeHitPosition;
        private Transform _playerTransform;
        public void PrimeEffect(Vector3 pickaxeHitPosition, Transform playerTransform)
        {
            _pickaxeHitPosition = pickaxeHitPosition;
            _playerTransform = playerTransform;
        }
        
        protected override bool ApplyEffectInternal()
        {
            // TODO the original code had some waterfall logic for the position and rotation. Why was this needed?
            // position = Target?.Value.position ?? _pickaxeHitPosition ?? _playerPosition
            // rotation = Target?.Value.position ?? _playerRotation
            var newGameObject = GameObjectUtils.SafeInstantiate(true, Prefab, Container?.Value);
            if (UsePickaxeHitPosition)
            {
                newGameObject.transform.SetPositionAndRotation(_pickaxeHitPosition, _playerTransform.rotation);
            }
            else
            {
                newGameObject.transform.SetPositionAndRotation(Target.Value.position, Target.Value.rotation);
            }
            
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