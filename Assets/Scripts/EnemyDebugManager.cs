using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts
{
    public class EnemyDebugManager : MonoBehaviour
    {
        [SerializeField] private Transform _enemyContainer;
        [SerializeField] private GameEvent _onAddEnemyHealth;
        [SerializeField] private int _healthAmountToAdd = 10;

        private void OnEnable()
        {
            _onAddEnemyHealth.Subscribe(AddEnemmyHealth);
        }

        private void OnDisable()
        {
            _onAddEnemyHealth.Unsubscribe(AddEnemmyHealth);
        }

        private void AddEnemmyHealth()
        {
            Health[] enemyHealths = _enemyContainer.GetComponentsInChildren<Health>();
            
            foreach (var health in enemyHealths)
            {
                health.Heal(_healthAmountToAdd);
            }
        }
    }
}