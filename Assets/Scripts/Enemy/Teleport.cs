using BML.ScriptableObjectCore.Scripts.SceneReferences;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class Teleport : MonoBehaviour
    {
        [SerializeField] private Transform _teleportTransform;
        [SerializeField] private TransformSceneReference _targetTransform;
        [SerializeField] private LayerMask _validTeleportMask;
        [SerializeField, MinMaxSlider(0f, 30f)] private Vector2 _teleportDistance;

        public void DoTeleport()
        {
            //TODO: Use stepped seed
            Debug.Log($"Teleport Starting");
            int retry = 0;
            while (retry < 20)
            {
                float verticalOffset = 5f;
                Vector2 horizontalOffset = _teleportDistance * Random.insideUnitCircle;
                Vector3 raycastOrigin = _targetTransform.Value.position +
                                        new Vector3(horizontalOffset.x, verticalOffset, horizontalOffset.y);
                RaycastHit hit;
                if (Physics.Raycast(raycastOrigin, Vector3.down, out hit, 10f, _validTeleportMask))
                {
                    Vector3? destination = hit.point;
                    _teleportTransform.position = destination.Value;
                    Debug.Log($"Teleporting: {destination.Value}");
                    return;
                }
                
                retry++;
                Debug.Log($"Teleport Failed!");
            }

            
        }
    }
}