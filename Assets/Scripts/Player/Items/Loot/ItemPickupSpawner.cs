using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Player.Items;
using BML.Scripts.Utils;
using Mono.CSharp;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts.Player.Items.Loot
{
    public class ItemPickupSpawner : MonoBehaviour
    {
        [SerializeField] private UnityEvent<GameObject> _onSpawnPickup;
        
        [SerializeField] private GameObject _basePickupPrefab;
        [SerializeField] private TransformSceneReference _pickupContainer;

        [SerializeField, LabelText("Pickup Delay"), Tooltip("Delay before pickup will be able to be activated by the player. Set to <0 to use the default value defined on the pickup prefab.")]
        private float _pickupDelaySetting = -1;
        private float? _pickupDelay => (_pickupDelaySetting < 0 ? null : (float?)_pickupDelaySetting);

        public void SpawnItemPickup(PlayerItem item)
        {
            var transformCached = transform;
            var newPickupGameObject = GameObjectUtils.SafeInstantiate(true, _basePickupPrefab, _pickupContainer?.Value ?? transformCached);
            newPickupGameObject.transform.SetPositionAndRotation(transformCached.position, transformCached.rotation);
            var newPickupController = newPickupGameObject.GetComponent<ItemPickupController>();
            if (newPickupController != null)
            {
                newPickupController.SetItem(item);
                if (_pickupDelay != null)
                {
                    newPickupController.SetPickupDelay(_pickupDelay.Value);
                }
            }
            _onSpawnPickup.Invoke(newPickupGameObject);
        }
    }
}