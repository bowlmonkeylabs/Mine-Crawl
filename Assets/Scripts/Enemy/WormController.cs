using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using UnityEngine;

namespace BML.Scripts.Enemy
{
    public class WormController : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private TransformSceneReference _playerRef;
        [SerializeField] private float _maxDeltaAngle = 3f;

        private float speed;
        private Vector3 moveDirection, targetDirection;

        private void Start()
        {
            moveDirection = transform.forward;
        }

        private void Update()
        {
            targetDirection = (_playerRef.Value.position - transform.position).normalized;
            moveDirection = Vector3.RotateTowards(
                moveDirection,
                targetDirection,
                _maxDeltaAngle * Mathf.Deg2Rad * Time.deltaTime,
                Mathf.Infinity).normalized;
        }

        private void FixedUpdate()
        {
            rb.rotation = Quaternion.LookRotation(moveDirection);
            rb.velocity = moveDirection * speed;
        }
        
        public void Respawn(float newSpeed)
        {
            speed = newSpeed;
            moveDirection = transform.forward;
        }
    }
}