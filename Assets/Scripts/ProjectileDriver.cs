using System;
using BML.Scripts.Player;
using UnityEngine;

namespace BML.Scripts
{
    public class ProjectileDriver : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float speed = 10f;

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
            moveDirection = dir.normalized;
        }

        public void SetSpeed(float newSpeed)
        {
            speed = newSpeed;
        }
    }
}