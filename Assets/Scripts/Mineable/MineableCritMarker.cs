using BML.ScriptableObjectCore.Scripts.SceneReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;
using BML.Scripts.Utils;
using BML.Scripts.Player;
using Shapes;
using UnityEngine.Serialization;

namespace BML.Scripts.Mineable
{
    public class MineableCritMarker : MonoBehaviour
    {
        [FormerlySerializedAs("_oreCollider")] [SerializeField] private Collider _mineableCollider;
        [FormerlySerializedAs("_health")] [SerializeField] private Health _critMarkerHealth;
        [SerializeField] private GameObject _critMarker;
        [SerializeField] [MinMaxSlider(0, 90)] private Vector2 _critMarkerMinMaxAngle = new Vector2(15f, 30f);
        [SerializeField] private float _critMarkerSurfaceOffset = .05f;
        [SerializeField] private TransformSceneReference _playerCamRef;
        [SerializeField] private LayerMask _critMarkerObstructionLayerMask;

        private Vector3 lastHitPos;
        private Vector3 centerToPlayer;

        public void MoveCritMarker(HitInfo hitInfo) 
        {
            this.MoveCritMarker();
        }

        public void ActivateCritMarker()
        {
            if (_critMarker != null && _critMarker.activeSelf)
            {
                return;
            }
            
            MoveCritMarker();
        }

        public void MoveCritMarker()
        {
            if (_critMarker == null)
            {
                return;
            }
            
            if (_critMarkerHealth.IsDead)
            {
                _critMarker.SetActive(false);
                return;
            }
            
            if (!_critMarker.activeSelf) _critMarker.SetActive(true);

            var playerCamRefPosition = _playerCamRef.Value.position;
            centerToPlayer = playerCamRefPosition - _mineableCollider.bounds.center;

            Vector3 newCritPoint;
            bool isNewCritPointValid = false;
            int attempts = 0;
            const int MAX_ATTEMPTS = 3;
            do
            {
                attempts++;
                
                float randXRot = MathUtils.GetRandomInRangeReflected(_critMarkerMinMaxAngle.x, _critMarkerMinMaxAngle.y);
                float randYRot = MathUtils.GetRandomInRangeReflected(_critMarkerMinMaxAngle.x, _critMarkerMinMaxAngle.y);
                Quaternion randomRotation = Quaternion.Euler(randXRot, randYRot, 0f);

                newCritPoint = _mineableCollider.bounds.center;
                newCritPoint += randomRotation * centerToPlayer;
                newCritPoint = _mineableCollider.ClosestPoint(newCritPoint);
                newCritPoint += centerToPlayer.normalized * _critMarkerSurfaceOffset;

                var checkRay = new Ray(playerCamRefPosition, newCritPoint - playerCamRefPosition);
                bool didHit = Physics.Raycast(checkRay, out var hitInfo, 10f, _critMarkerObstructionLayerMask);
                if (!didHit)
                {
                    isNewCritPointValid = true;
                }
                else if (hitInfo.collider == _mineableCollider)
                {
                    isNewCritPointValid = true;
                    newCritPoint = _mineableCollider.ClosestPoint(hitInfo.point);
                    newCritPoint += centerToPlayer.normalized * _critMarkerSurfaceOffset;
                }
                else if (attempts == MAX_ATTEMPTS)
                {
                    // If this is the last attempt at re-calculating a crit point, just use the hit position as the crit marker position; hopefully it's at least not-inside whatever collider was hit.
                    newCritPoint = _mineableCollider.ClosestPoint(hitInfo.point);
                    newCritPoint += centerToPlayer.normalized * _critMarkerSurfaceOffset;
                }

                if (!isNewCritPointValid)
                {
                    Debug.Log($"Crit marker location invalid, retrying... ({attempts} attempt(s))");
                }
            } while (!isNewCritPointValid && attempts < MAX_ATTEMPTS);

            if (!isNewCritPointValid)
            {
                Debug.Log($"Unable to find a valid crit marker location after {attempts} attempt(s)");
            }
            
            _critMarker.transform.position = newCritPoint;
        }
        
        private void OnDrawGizmos()
        {
            if (_critMarker == null || !_critMarker.activeSelf)
            {
                return;
            }
            
            Draw.LineGeometry = LineGeometry.Volumetric3D;
            Draw.LineThicknessSpace = ThicknessSpace.Meters;
            Draw.LineThickness = .025f;
            float a = .25f;
            
            //Draw line from collider center to hit point
            Draw.Line(_mineableCollider.bounds.center, lastHitPos, new Color(1f, 1f, 0f, a));

            //Draw cones to represent the min and max angle ranges for random rotation
            float length = centerToPlayer.magnitude;
            float minRadius = length * Mathf.Tan(_critMarkerMinMaxAngle.x * Mathf.Deg2Rad);
            float maxRadius = length * Mathf.Tan(_critMarkerMinMaxAngle.y * Mathf.Deg2Rad);
            Draw.Cone(lastHitPos, -centerToPlayer.normalized, minRadius, 
                centerToPlayer.magnitude, false, new Color(1f, 0f, 0f, a));
            Draw.Cone(lastHitPos + centerToPlayer * .05f, -centerToPlayer.normalized, maxRadius, 
                centerToPlayer.magnitude, false, new Color(0f, 1f, 0f, a));
            
        }
    }

}
