using System;
using BML.Scripts.Utils;
using UnityEngine;

namespace BML.Scripts
{
    public class DamageOnTrigger : MonoBehaviour
    {
        [SerializeField] private int _damage = 1;
        [SerializeField] private LayerMask _damageMask;
        [SerializeField] private float _time = 2;

        private float _lastTime = float.PositiveInfinity;
        private bool _canDamage = false;

        private void OnTriggerStay(Collider other)
        {
            if(!_canDamage) {
                return;
            }

            GameObject otherObj = other.gameObject;
            if (!otherObj.IsInLayerMask(_damageMask)) return;

            Damageable damageable = otherObj.GetComponent<Damageable>();
            if (damageable != null) {
                damageable.TakeDamage(_damage);
                _canDamage = false;
            }
        }

        private void Update() {
            if(float.IsPositiveInfinity(_lastTime) || (Time.time - _lastTime) >= _time) {
                _lastTime = Time.time;
                _canDamage = true;
            }
        }
    }
}