using UnityEngine;
using UnityEngine.Events;
using MoreMountains.Tools;
using Sirenix.OdinInspector;

namespace BML.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Throwable : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private bool _stopOnGroundHit = false;
        [ShowIf("_stopOnGroundHit")] [SerializeField] private LayerMask _terrainLayerMask;
        [ShowIf("_stopOnGroundHit")] [SerializeField] private UnityEvent _onGrounded;

        private bool _grounded = false;

        void Awake() {
            _rigidbody.isKinematic = false;
        }

        private void OnTriggerEnter (Collider collider)
		{
			if (_stopOnGroundHit && !_grounded && _terrainLayerMask.MMContains(collider.gameObject))
			{
                this.SetGrounded();
			}
		}

        private void OnColliderEnter (Collision collision)
		{
			if (_stopOnGroundHit && !_grounded && _terrainLayerMask.MMContains(collision.gameObject))
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