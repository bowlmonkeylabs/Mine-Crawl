using UnityEngine;
using UnityEngine.Events;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Throwable : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private Vector3 _initialForce = Vector3.zero;
        [SerializeField] private ForceMode _initialForceMode;
        [SerializeField] private bool _stopOnGroundHit = false;
        [FormerlySerializedAs("_terrainLayerMask")] [ShowIf("_stopOnGroundHit")] [SerializeField] private LayerMask _stickyLayerMask;
        [ShowIf("_stopOnGroundHit")] [SerializeField] private UnityEvent _onGrounded;

        private bool _grounded = false;

        void Awake() 
        {
            _rigidbody.isKinematic = false;
            if (_initialForce != Vector3.zero)
            {
				_rigidbody.AddRelativeForce(_initialForce, _initialForceMode);
            }
        }

        private void OnTriggerEnter (Collider collider)
		{
			if (_stopOnGroundHit && !_grounded && _stickyLayerMask.MMContains(collider.gameObject))
			{
                this.SetGrounded();
			}
		}

        private void OnCollisionEnter (Collision collision)
		{
			if (_stopOnGroundHit && !_grounded && _stickyLayerMask.MMContains(collision.gameObject))
			{
                this.SetGrounded();
			}
		}

        private void SetGrounded() {
            _grounded = true;
            _rigidbody.isKinematic = true;
            _onGrounded.Invoke();
        }

        public void DoThrow(Vector3 directionalForce) {
            _rigidbody.AddForce(directionalForce, ForceMode.Impulse);
        }
    }
}