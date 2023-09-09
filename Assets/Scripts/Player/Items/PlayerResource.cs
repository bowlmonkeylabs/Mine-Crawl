using System;
using BML.ScriptableObjectCore.Scripts.Variables;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace BML.Scripts.Player.Items
{
    public delegate void OnAmountChanged();

    [InlineEditor()]
    [CreateAssetMenu(fileName = "PlayerResource", menuName = "BML/Player/PlayerResource", order = 0)]
    public class PlayerResource : ScriptableObject
    {
        [SerializeField, FoldoutGroup("Descriptors")] private string _name;
        [SerializeField, FoldoutGroup("Descriptors"), TextArea (7, 10)] private string _description;
        [SerializeField, FoldoutGroup("Descriptors")] private Sprite _icon;
        [SerializeField, FoldoutGroup("Descriptors")] private string _iconText;

        [SerializeField, FoldoutGroup("Player")] IntVariable _playerCount;

        public event OnAmountChanged OnAmountChanged;

        void OnEnable() {
            _playerCount.Subscribe(InvokeOnAmountChanged);
        }

        void OnDisable() {
            _playerCount.Unsubscribe(InvokeOnAmountChanged);
        }

        private void InvokeOnAmountChanged() {
            OnAmountChanged?.Invoke();
        }

        public int PlayerCount {
            get => _playerCount.Value;
            set => _playerCount.Value = Mathf.Min(0, value);
        }
        public string IconText { get => _iconText; }
    }
}
