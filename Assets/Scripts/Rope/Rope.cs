using System;
using BML.Scripts.Utils;
using UnityEngine;
using MoreMountains.Tools;
using BML.ScriptableObjectCore.Scripts.Events;

namespace BML.Scripts
{
    public class Rope : MonoBehaviour
    {
        [SerializeField] private LayerMask _ceilingMask;
        [SerializeField] private int _maxDistance = 100;
        [SerializeField] private Collider _collider;
        [SerializeField] private Transform _colliderScaleTransform;
        [SerializeField] private GameObject _ladderPointPrefab;
        [SerializeField] private LayerMask _playerLayerMask;
        [SerializeField] private string _playerTag;
        [SerializeField] private GameEvent _playerEnteredRopeEvent;
        [SerializeField] private bool _skipDeploy = false;

        public bool IsDeployed {get; private set;}

        private bool _playerInRope;

        private void Start()
        {
            if (_skipDeploy)
            {
                IsDeployed = true;
            }
        }

        public void Deploy ()
        {
            if (IsDeployed)
            {
                return;
            }
            
            RaycastHit hit;
            
            var scalingTransform = _colliderScaleTransform;

            var margin = 0.1f;
            
            var colliderBottom = _collider.bounds.center - _collider.bounds.extents.oyo();
            if(Physics.Raycast(colliderBottom, Vector3.up, out hit, _maxDistance, _ceilingMask)) {
                float baseDistToHitPoint = Vector3.Distance(hit.point, colliderBottom);
                float distToHitPoint = baseDistToHitPoint * (1 + 2 * margin);
                float colliderScalingFactor = distToHitPoint / 2;

                var ropePointBottomPosition = colliderBottom;
                var colliderBottomPosition = colliderBottom - new Vector3(0, margin * baseDistToHitPoint, 0);
                var ropePointTopPosition = colliderBottom + baseDistToHitPoint * Vector3.up;
                // var colliderTopPosition = colliderBottomPosition + distToHitPoint * Vector3.up;

                _collider.transform.localScale = _collider.transform.localScale.xoz() + new Vector3(0, 1, 0); // Reset collider scale y to 1 before adjusting.
                scalingTransform.localScale = scalingTransform.localScale.xoz() + new Vector3(0, colliderScalingFactor, 0);
                this.transform.position = colliderBottomPosition;

                scalingTransform.rotation = Quaternion.identity;
                
                var topPoint = GameObjectUtils.SafeInstantiate(true, _ladderPointPrefab, transform.parent);
                topPoint.transform.SetPositionAndRotation(ropePointTopPosition, Quaternion.identity);
                topPoint.tag = "RopeTop";

                var bottomPoint = GameObjectUtils.SafeInstantiate(true, _ladderPointPrefab, transform.parent);
                bottomPoint.transform.SetPositionAndRotation(ropePointBottomPosition, Quaternion.identity);
                bottomPoint.tag = "RopeBottom";

                IsDeployed = true;
            }
        }

        void OnTriggerEnter(Collider collider) {
            if(_playerLayerMask.MMContains(collider.gameObject) && collider.gameObject.tag == _playerTag && IsDeployed && !_playerInRope) {
                _playerEnteredRopeEvent.Raise();
                _playerInRope = true;
            }
        }

        void OnTriggerExit(Collider collider) {
            if(_playerLayerMask.MMContains(collider.gameObject) && collider.gameObject.tag == _playerTag) {
                _playerInRope = false;
            }
        }
    }   
}
