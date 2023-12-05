using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using MoreMountains.Feedbacks;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class WormController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Transform _burrowParticlePivot;
        [SerializeField] private Transform _emergeParticlePivot;
        [SerializeField] private MMF_Player _emergeFeedbacks;
        [SerializeField] private float _emergeParticlesTimeOffset = .2f;
        [SerializeField] private LayerMask _terrainMask;
        [SerializeField] private TransformSceneReference _mainCameraRef;

        private float speed;
        private Vector3 moveDirection;
        private Vector3 lastBurrowHitPoint;
        private Vector3 target;
        private bool emergeParticlesActivated;

        private void Start()
        {
            moveDirection = transform.forward;
        }

        private void Update()
        {
            HandleFeedbacks();
        }

        private void FixedUpdate()
        {
            _rb.rotation = Quaternion.LookRotation(moveDirection);
            _rb.MovePosition(_rb.position + moveDirection * (speed * Time.deltaTime));
        }

        private void HandleFeedbacks()
        {
            RaycastHit hit;

            Vector3 burrowParticlesDir = target - transform.position;
            float maxDist = (target - transform.position).magnitude;

            // Don't spawn particles if worm moving away from player
            if (Vector3.Dot(burrowParticlesDir, transform.forward) < 0)
            {
                emergeParticlesActivated = false;
                return;
            }
            
            if (!_mainCameraRef.SafeIsUnityNull() && !_mainCameraRef.Value.SafeIsUnityNull() 
                                                  && Physics.SphereCast(target, .5f, 
                                                      -burrowParticlesDir, out hit,
                                                      maxDist, _terrainMask))
            {
                _burrowParticlePivot.gameObject.SetActive(true);
                _burrowParticlePivot.position = hit.point;
                _burrowParticlePivot.rotation = Quaternion.LookRotation(transform.forward);

                // Reset particles if moved to new wall
                if (Vector3.SqrMagnitude(lastBurrowHitPoint - hit.point) > .15f)
                {
                    emergeParticlesActivated = false;
                }

                // Handle emerge particles
                if (!emergeParticlesActivated &&
                    (hit.point - transform.position).magnitude < speed * _emergeParticlesTimeOffset)
                {
                    _emergeParticlePivot.position = hit.point;
                    _emergeParticlePivot.rotation = Quaternion.LookRotation(transform.forward);
                    _emergeParticlePivot.gameObject.SetActive(true);
                    _emergeFeedbacks.PlayFeedbacks();
                    emergeParticlesActivated = true;
                }

                lastBurrowHitPoint = hit.point;
            }
        }
        
        public void Respawn(float newSpeed, Vector3 newTarget)
        {
            speed = newSpeed;
            moveDirection = transform.forward;
            target = newTarget;
        }
    }
}