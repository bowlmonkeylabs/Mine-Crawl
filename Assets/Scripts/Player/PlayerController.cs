﻿using System;
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
        [SerializeField] private Transform _mainCamera;
        [SerializeField] private float _interactDistance = 5f;
        [SerializeField] private LayerMask _interactMask;

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

        private void OnPrimary()
        {
            TryUsePickaxe();
        }

        private void TryUsePickaxe()
        {
            RaycastHit hit;
            if (Physics.Raycast(_mainCamera.position, _mainCamera.forward, out hit, _interactDistance, _interactMask, QueryTriggerInteraction.Collide))
            {
                InteractionReceiver interactionReceiver = hit.collider.GetComponent<InteractionReceiver>();
                if (interactionReceiver == null) return;

                interactionReceiver.ReceiveInteraction();
            }
        }
    }
}