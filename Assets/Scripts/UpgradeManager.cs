using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts
{
    public class UpgradeManager : MonoBehaviour
    {
        [SerializeField] private IntVariable _oreCount;

        [TitleGroup("Health")]
        [SerializeField] private GameEvent _onBuyHealth;
        [SerializeField] private IntVariable _playerHealth;
        [SerializeField] private int healthIncrease = 1;
        [SerializeField] private int healthOreCost = 5;
        
        [TitleGroup("Torch")]
        [SerializeField] private GameEvent _onBuyTorch;
        [SerializeField] private IntVariable _playerTorchCount;
        [SerializeField] private int torchIncrease = 3;
        [SerializeField] private int torchOreCost = 3;

        private void Awake()
        {
            _onBuyHealth.Subscribe(BuyHealth);
            _onBuyTorch.Subscribe(BuyMarkers);
        }

        private void OnDestroy()
        {
            _onBuyHealth.Unsubscribe(BuyHealth);
            _onBuyTorch.Unsubscribe(BuyMarkers);
        }

        private void BuyHealth()
        {
            if (_oreCount.Value < healthOreCost) return;
            
            _playerHealth.Value += healthIncrease;
            _oreCount.Value -= healthOreCost;
        }

        private void BuyMarkers()
        {
            if (_oreCount.Value < torchOreCost) return;

            _playerTorchCount.Value += torchIncrease;
            _oreCount.Value -= torchOreCost;
        }
    }
}