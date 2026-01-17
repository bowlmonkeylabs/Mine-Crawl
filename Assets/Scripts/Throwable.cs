using System;
using BML.Scripts.Utils;
using UnityEngine;
using UnityEngine.Events;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine.Animations;
using UnityEngine.Serialization;

namespace BML.Scripts
{
    [RequireComponent(typeof(Rigidbody))]
    public class Throwable : MonoBehaviour
    {
	    #region Inspector
	    
        [SerializeField] private Rigidbody _rigidbody;

        [SerializeField] private bool _applyForceOnAwake = false;
        [SerializeField, ShowIf("_applyForceOnAwake")] private Vector3 _initialForce;
        [SerializeField, ShowIf("_applyForceOnAwake")] private ForceMode _initialForceMode;
        
        [FormerlySerializedAs("_stopOnGroundHit")] [SerializeField] private bool _stickOnFirstCollision = true;
        [SerializeField, ShowIf("_stickOnFirstCollision")] private bool _stickWithParentConstraint = false;
        [SerializeField, ShowIf("@_stickWithParentConstraint && _stickOnFirstCollision"), ReadOnly] private ParentConstraint _parentConstraint = null;
        [SerializeField, ShowIf("_stickOnFirstCollision")] private LayerMask _stickyLayerMask;
        [SerializeField, ShowIf("_stickOnFirstCollision")] private bool _ignoreTriggerColliders = true;
        [FormerlySerializedAs("_onGrounded")] [SerializeField, ShowIf("_stickOnFirstCollision")]private UnityEvent _onStick;

        #endregion
        
        private bool _stuck = false;

        #region Unity lifecycle

        #if UNITY_EDITOR
	    
        private void OnValidate()
        {
	        if (_stickWithParentConstraint)
	        {
		        _parentConstraint = GetComponent<ParentConstraint>();
		        if (_parentConstraint == null)
		        {
			        _parentConstraint = this.gameObject.AddComponent<ParentConstraint>();
			        ComponentUtil.MoveComponentBelow(_parentConstraint, this);
		        }
			    _parentConstraint.constraintActive = false;
	        }
	        else if (_parentConstraint != null)
	        {
		        DestroyImmediate(_parentConstraint);
		        _parentConstraint = null;
	        }
        }

		#endif

        private void Awake() 
        {
	        if (_parentConstraint != null)
	        {
		        _parentConstraint.constraintActive = false;
	        }
	        
            _rigidbody.isKinematic = false;
            if (_applyForceOnAwake)
            {
	            _rigidbody.AddRelativeForce(_initialForce, _initialForceMode);
            }
        }

        private void OnTriggerEnter (Collider collider)
		{
			if (!_ignoreTriggerColliders && _stickOnFirstCollision && !_stuck && _stickyLayerMask.MMContains(collider.gameObject))
			{
				// Debug.Log($"OnTriggerEnter: {this.name} hit {collider.name}");
				this.StickToCollider(collider);
			}
		}

        private void OnCollisionEnter (Collision collision)
		{
			if (_stickOnFirstCollision && !_stuck && _stickyLayerMask.MMContains(collision.gameObject))
			{
				// Debug.Log($"OnCollisionEnter: {this.name} hit {collision.collider.name}");
                this.StickToCollider(collision.collider);
			}
		}

        private void FixedUpdate()
        {
	        if (_stickOnFirstCollision && _stuck && _stickWithParentConstraint)
	        {
				bool sourceIsNull = false;
				if (_parentConstraint.sourceCount == 0)
				{
					sourceIsNull = true;
				}
				else
				{
					var constraintSource = _parentConstraint.GetSource(0);
					if (constraintSource.sourceTransform.SafeIsUnityNull())
					{
						sourceIsNull = true;
					}
				}

				if (sourceIsNull)
				{
					// the thing previously attached is gone, so we should unstick
					Unstick();
				}
	        } 
        }

        #endregion
        
        #region Public interface

        public void DoThrow(Vector3 directionalForce)
        {
	        _rigidbody.AddForce(directionalForce, ForceMode.Impulse);
        }

        private void StickToCollider(Collider collider)
        {
	        // Debug.Log($"Stuck! {this.name} to {collider.name}");
	        _stuck = true;
	        if (_stickWithParentConstraint)
	        {
				var colliderTransform = collider.transform;
		        var constraintSource = new ConstraintSource
		        {
			        sourceTransform = colliderTransform,
			        weight = 1f,
		        };
				var translationOffset = colliderTransform.InverseTransformPoint(this.transform.position);
				var rotationOffset = Quaternion.Inverse(colliderTransform.rotation) * this.transform.rotation;

				_parentConstraint.translationAtRest = this.transform.position;
		        _parentConstraint.AddSource(constraintSource);
		        _parentConstraint.SetTranslationOffset(0, translationOffset);
				_parentConstraint.SetRotationOffset(0, rotationOffset.eulerAngles);
		        _parentConstraint.constraintActive = true;
	        }
	        else
	        {
		        _rigidbody.isKinematic = true;
	        }
	        _onStick.Invoke();
        }

        private void Unstick()
        {
	        if (!_stuck) return;
	        // Debug.Log($"Unstuck! {this.name}");

	        if (_stickWithParentConstraint)
	        {
				_parentConstraint.constraintActive = false;
				if (_parentConstraint.sourceCount > 0)
				{
					_parentConstraint.RemoveSource(0);
				}
	        }
	        else
	        {
		        _rigidbody.isKinematic = false;
	        }

	        _stuck = false;
        }
        
        #endregion
        
    }
}