using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using Sirenix.Utilities;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class WormController : MonoBehaviour
    {
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private Transform _burrowParticlePivot;
        [SerializeField] private LayerMask _terrainMask;
        [SerializeField] private TransformSceneReference _playerRef;
        [SerializeField] private TransformSceneReference _mainCameraRef;

        private float speed;
        private Vector3 moveDirection, wormToPlayerDir;
        private Vector3 playerPosOnSpawn;

        private void Start()
        {
            moveDirection = transform.forward;
            playerPosOnSpawn = _mainCameraRef.Value.transform.position;
        }

        private void Update()
        {
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

            Vector3 burrowParticlesDir = playerPosOnSpawn - transform.position;
            float maxDist = (playerPosOnSpawn - transform.position).magnitude;

            if (!_mainCameraRef.SafeIsUnityNull() && !_mainCameraRef.Value.SafeIsUnityNull() 
                                                  && Physics.Raycast(playerPosOnSpawn, -burrowParticlesDir
                                                      , out hit, maxDist, _terrainMask))
            {
                _burrowParticlePivot.gameObject.SetActive(true);
                _burrowParticlePivot.position = hit.point;
                _burrowParticlePivot.rotation = Quaternion.LookRotation(transform.forward);
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
            playerPosOnSpawn = _mainCameraRef.Value.transform.position;
        }
    }
}