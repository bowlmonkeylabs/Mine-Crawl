using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using UnityEngine;

namespace BML.Scripts
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private GameEvent _onBuyHealth;
        [SerializeField] private IntVariable _playerHealth;
        [SerializeField] private IntVariable _oreCount;
        [SerializeField] private int healthIncrease = 1;
        [SerializeField] private int oreCost = 10;

        private void Awake()
        {
            _onBuyHealth.Subscribe(BuyHealth);
        }

        private void OnDestroy()
        {
            _onBuyHealth.Unsubscribe(BuyHealth);
        }

        private void BuyHealth()
        {
            if (_oreCount.Value < oreCost) return;
            
            _playerHealth.Value += healthIncrease;
            _oreCount.Value -= oreCost;
        }
    }
}