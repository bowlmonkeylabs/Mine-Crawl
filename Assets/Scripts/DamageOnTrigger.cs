using System;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts
{
    public class DamageOnTrigger : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _damageMask;
        private void OnTriggerEnter(Collider other)
        {
            GameObject otherObj = other.gameObject;
            if (!otherObj.IsInLayerMask(_damageMask)) return;

            Damageable damageable = otherObj.GetComponent<Damageable>();
            if (damageable != null)
                damageable.TakeDamage(_damage);
        }
    }
}