using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private IntReference _health;
        [SerializeField] private GameEvent _onTakeDamage;
        [SerializeField] private GameEvent _onDeath;

        public void TakeDamage(int damage)
        {
            _health.Value -= 1;
            if (_health.Value <= 0)
                OnDeath();
        }

        private void OnDeath()
        {
            // Do something
        }
    }
}