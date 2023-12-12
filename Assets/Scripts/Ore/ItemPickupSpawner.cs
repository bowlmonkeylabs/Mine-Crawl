using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Player.Items;
using BML.Scripts.Utils;
using Mono.CSharp;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class ItemPickupSpawner : MonoBehaviour
    {
        [SerializeField] private UnityEvent<GameObject> _onSpawnPickup;
        
        [SerializeField] private GameObject _basePickupPrefab;
        [SerializeField] private TransformSceneReference _pickupContainer;

        public void SpawnItemPickup(PlayerItem item)
        {
            var transformCached = transform;
            var newPickupGameObject = GameObjectUtils.SafeInstantiate(true, _basePickupPrefab, _pickupContainer?.Value ?? transformCached);
            newPickupGameObject.transform.SetPositionAndRotation(transformCached.position, transformCached.rotation);
            var newPickupController = newPickupGameObject.GetComponent<ItemPickupController>();
            if (newPickupController != null)
            {
                newPickupController.SetItem(item);
            }
            _onSpawnPickup.Invoke(newPickupGameObject);
        }
    }
}