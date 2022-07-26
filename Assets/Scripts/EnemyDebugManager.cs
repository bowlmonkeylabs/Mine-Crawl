﻿using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts
{
    public class EnemyDebugManager : MonoBehaviour
    {
        [SerializeField] private Transform _levelObjectContainer;
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
            EnemyController[] enemyControllers = _levelObjectContainer.GetComponentsInChildren<EnemyController>();
            
            foreach (var enemyController in enemyControllers)
            {
                Damageable enemyDamagable = enemyController.GetComponent<Damageable>();
                enemyDamagable.Health += _healthAmountToAdd;
            }
        }
    }
}