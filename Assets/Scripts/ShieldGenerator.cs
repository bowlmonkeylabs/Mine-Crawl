using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Events;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace BML.Scripts
{
    public class ShieldGenerator : MonoBehaviour
    {
        [SerializeField] private Health _health;
        [SerializeField] private GameEvent _onDestroyLastShieldPylon;
        [SerializeField] private MMF_Player _enableShieldFeedbacks;
        [SerializeField] private MMF_Player _disableShieldFeedbacks;
        [SerializeField] private List<Health> _pylonHealthList = new List<Health>();

        #region Unity Lifecylce

        private void Start()
        {
            UpdateShieldStatus();
        }

        private void OnEnable()
        {
            foreach (var pylonHealth in _pylonHealthList)
            {
                pylonHealth.OnDeath += UpdateShieldStatus;
                pylonHealth.OnRevive += UpdateShieldStatus;
            }
        }
        
        private void OnDisable()
        {
            foreach (var pylonHealth in _pylonHealthList)
            {
                pylonHealth.OnDeath -= UpdateShieldStatus;
                pylonHealth.OnRevive -= UpdateShieldStatus;
            }
        }

        #endregion

        private void UpdateShieldStatus()
        {
            int pylonsAlive = _pylonHealthList.Count(p => !p.IsDead);
            if (pylonsAlive > 0)
            {
                _health.SetInvincible(true);
                _enableShieldFeedbacks.PlayFeedbacks();
            }
            else
            {
                _health.SetInvincible(false);
                _disableShieldFeedbacks.PlayFeedbacks();
                _onDestroyLastShieldPylon.Raise();
            }
        }
    }
}