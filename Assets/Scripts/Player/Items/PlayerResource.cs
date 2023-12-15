using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace BML.Scripts.Player.Items
{
    public delegate void OnAmountChanged();

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerResource", menuName = "BML/Player/PlayerResource", order = 0)]
    public class PlayerResource : ScriptableObject
    {
        #region Inspector
        
        [SerializeField, FoldoutGroup("Descriptors")] private string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea (7, 10)] private string _description;
        [SerializeField, FoldoutGroup("Descriptors")] private string _iconText;
        [SerializeField, FoldoutGroup("Descriptors"), PreviewField(100, ObjectFieldAlignment.Left)] private Sprite _icon;
        
        [FormerlySerializedAs("_playerCount")] [SerializeField, FoldoutGroup("Player")] IntVariable _playerAmount;
        [SerializeField, FoldoutGroup("Player")] private SafeIntValueReference _playerAmountLimit;

        #endregion

        #region Public interface
        
        public string IconText => _iconText;

        public int PlayerAmount 
        {
            get => _playerAmount.Value;
            set
            {
                var newAmount = value;
                if (_playerAmountLimit.Value >= 0)
                {
                    newAmount = Mathf.Min(_playerAmountLimit.Value, newAmount);
                }
                newAmount = Mathf.Max(0, newAmount);
                _playerAmount.Value = newAmount;
            }
        }

        public bool UseAmountLimit => (_playerAmountLimit.Value >= 0);
        public int? PlayerAmountLimit => (UseAmountLimit ? (int?)_playerAmountLimit.Value : null);

        public bool IsAtAmountLimit => (UseAmountLimit && (_playerAmount.Value >= _playerAmountLimit.Value));

        public event OnAmountChanged OnAmountChanged;

        public bool CanAddResource(int addAmount)
        {
            var requestedHypotheticalAmount = (PlayerAmount + addAmount);
            var clampedHypotheticalAmount = Mathf.Max(0, Mathf.Min((PlayerAmountLimit ?? requestedHypotheticalAmount), requestedHypotheticalAmount));
            var hypotheticalDelta = (clampedHypotheticalAmount - PlayerAmount);
            return (hypotheticalDelta != 0);
        }

        #endregion

        #region Unity lifecycle

        private void OnEnable()
        {
            _playerAmount.Subscribe(InvokeOnAmountChanged);
            _playerAmountLimit.Subscribe(OnAmountLimitChanged);
        }

        private void OnDisable()
        {
            _playerAmount.Unsubscribe(InvokeOnAmountChanged);
            _playerAmountLimit.Unsubscribe(OnAmountLimitChanged);
        }
        
        private void InvokeOnAmountChanged()
        {
            OnAmountChanged?.Invoke();
        }

        #endregion

        private void OnAmountLimitChanged()
        {
            PlayerAmount = PlayerAmount; // update amount to force re-check of the limits
        }
        
    }
}
