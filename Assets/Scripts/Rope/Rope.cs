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
        [SerializeField] private GameObject _ladderPointPrefab;
        [SerializeField] private LayerMask _playerLayerMask;
        [SerializeField] private string _playerTag;
        [SerializeField] private GameEvent _playerEnteredRopeEvent;

        public bool IsDeployed {get; private set;}

        private bool _playerInRope;

        public void Deploy () {
            RaycastHit hit;
            
            var scalingTransform = _collider.transform;
            
            var colliderBottom = _collider.bounds.center - _collider.bounds.extents.oyo();
            if(Physics.Raycast(colliderBottom, Vector3.up, out hit, _maxDistance, _ceilingMask)) {
                float distToHitPoint = Vector3.Distance(hit.point, colliderBottom);
                float scalingFactor = distToHitPoint / 2;

                scalingTransform.localScale = scalingTransform.localScale.xoz() + new Vector3(0, scalingFactor, 0);
                transform.position = colliderBottom + new Vector3(0, scalingFactor, 0);

                scalingTransform.rotation = Quaternion.identity;
                
                var topPoint = GameObjectUtils.SafeInstantiate(true, _ladderPointPrefab, transform.parent);
                topPoint.transform.SetPositionAndRotation(transform.position + new Vector3(0, scalingFactor, 0), Quaternion.identity);
                topPoint.tag = "RopeTop";
                var bottomPoint = GameObjectUtils.SafeInstantiate(true, _ladderPointPrefab, transform.parent);
                bottomPoint.transform.SetPositionAndRotation(transform.position - new Vector3(0, scalingFactor, 0), Quaternion.identity);
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
