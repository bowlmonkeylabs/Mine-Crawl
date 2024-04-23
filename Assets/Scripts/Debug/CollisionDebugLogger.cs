using System;
using UnityEngine;

namespace BML.Scripts
{
    public class CollisionDebugLogger : MonoBehaviour
    {
        private void OnCollisionEnter(Collision other)
        {
            Debug.Log(other.gameObject.name);
        }
    }
}