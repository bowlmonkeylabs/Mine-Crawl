using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class WormController : MonoBehaviour
    {
        [SerializeField] private float _maxDeltaAngle = 3f;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Transform _burrowParticlePivot;
        [SerializeField] private LayerMask _terrainMask;
        [SerializeField] private TransformSceneReference _playerRef;
        [SerializeField] private TransformSceneReference _mainCameraRef;

        private float speed;
        private Vector3 moveDirection, wormToPlayerDir;

        private void Start()
        {
            moveDirection = transform.forward;
        }

        private void Update()
        {
            if (!_playerRef.SafeIsUnityNull() && !transform.SafeIsUnityNull())
            {
                wormToPlayerDir = (_playerRef.Value.position - transform.position).normalized;
            }
            moveDirection = Vector3.RotateTowards(
                moveDirection,
                wormToPlayerDir,
                _maxDeltaAngle * Mathf.Deg2Rad * Time.deltaTime,
                Mathf.Infinity).normalized;

            HandleBurrowParticles();
        }

        private void FixedUpdate()
        {
            _rb.rotation = Quaternion.LookRotation(moveDirection);
            _rb.velocity = moveDirection * speed;
        }

        private void HandleBurrowParticles()
        {
            RaycastHit hit;
            if (Physics.Raycast(_mainCameraRef.Value.position, -wormToPlayerDir, out hit,
                Mathf.Infinity, _terrainMask))
            {
                _burrowParticlePivot.gameObject.SetActive(true);
                _burrowParticlePivot.position = hit.point;
                _burrowParticlePivot.rotation = Quaternion.LookRotation(-wormToPlayerDir);
            }
            else
            {
                _burrowParticlePivot.gameObject.SetActive(false);
            }
        }
        
        public void Respawn(float newSpeed)
        {
            speed = newSpeed;
            moveDirection = transform.forward;
        }
    }
}