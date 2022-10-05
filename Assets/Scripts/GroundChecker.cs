using System;
using Pathfinding;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    public class GroundChecker : MonoBehaviour
    {
        [SerializeField] private CharacterController _charController;
        [SerializeField] private LayerMask _groundCheckLayer;
        [SerializeField] private float _groundCheckDistance = .25f;
        [SerializeField] private float _updateDelay = .1f;
        [SerializeField] private UnityEvent<bool> OnUpdateGroundingStatus;
        [SerializeField] private UnityEvent OnGrounded;
        [SerializeField] private UnityEvent OnUnGrounded;

        private float lastUpdateTime = Mathf.NegativeInfinity;
        private bool isGrounded;

        private void Update()
        {
            if (lastUpdateTime + _updateDelay > Time.time)
                return;

            RaycastHit hit;
            Vector3 charCenter = _charController.transform.position + _charController.center;
            //Vector3 p1 = charCenter + _charController.height * 0.5F * Vector3.down;
            Vector3 p1 = charCenter;
            Vector3 p2 = charCenter + _charController.height * 0.5F * Vector3.up;
            
            //Slight offset from bottom of char controller so collisions aren't missed
            Vector3 offset = Vector3.up * .05f;

            if (Physics.CapsuleCast(p1 + offset, p2, _charController.radius, Vector3.down, out hit, _groundCheckDistance + _charController.height * 0.5f, _groundCheckLayer))
            {
                OnUpdateGroundingStatus.Invoke(true);
                if (!isGrounded)
                {
                    isGrounded = true;
                    OnGrounded.Invoke();
                }
            }
            else
            {
                OnUpdateGroundingStatus.Invoke(false);
                if (isGrounded)
                {
                    isGrounded = false;
                    OnUnGrounded.Invoke();
                }
            }
        }
    }
}