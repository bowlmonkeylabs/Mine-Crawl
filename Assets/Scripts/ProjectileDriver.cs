using System;
using UnityEngine;

namespace BML.Scripts
{
    public class ProjectileDriver : MonoBehaviour
    {
        [SerializeField] private Rigidbody rb;
        [SerializeField] private float velocity = 10f;

        private void FixedUpdate()
        {
            rb.velocity = transform.forward * velocity;
        }
    }
}