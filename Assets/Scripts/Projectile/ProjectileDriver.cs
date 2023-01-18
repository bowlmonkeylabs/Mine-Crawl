using System;
using BML.ScriptableObjectCore.Scripts.SceneReferences;
using BML.Scripts.Player;
using UnityEngine;

namespace BML.Scripts
{
    public class ProjectileDriver : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float speed = 10f;
        [SerializeField] private string deflectLayer = "EnemyProjectileDeflected";
        [SerializeField] private TransformSceneReference mainCameraRef;

        private Vector3 moveDirection;

        private void Start()
        {
            moveDirection = transform.forward;
        }

        private void FixedUpdate()
        {
            rb.velocity = moveDirection * speed;
        }

        public void ChangeDirection(HitInfo hitInfo)
        {
            ChangeDirection(hitInfo.HitDirection);
        }

        public void ChangeDirection(Vector3 dir)
        {
            gameObject.layer = LayerMask.NameToLayer(deflectLayer);
            rb.position = mainCameraRef.Value.position;
            moveDirection = dir.normalized;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
    }
}