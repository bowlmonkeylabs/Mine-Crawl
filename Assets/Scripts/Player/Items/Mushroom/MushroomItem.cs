using System;
using BML.ScriptableObjectCore.Scripts.Events;
using BML.ScriptableObjectCore.Scripts.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.Player.Items.Mushroom
{
    [Serializable]
    internal class MushroomItemVisual
    {
        public Sprite Sprite;
        public GameObject ObjectPrefab;
    }
    
    [InlineEditor()]
    [CreateAssetMenu(fileName = "MushroomItem", menuName = "BML/Player/MushroomItem", order = 0)]
    public class MushroomItem : PlayerItem, IResettableScriptableObject
    {
        [SerializeField] private DynamicGameEvent _onItemDiscovered;
        
        [NonSerialized, ShowInInspector, ReadOnly] private MushroomItemVisual _mushroomItemVisual;
        [NonSerialized, ShowInInspector] private bool _isKnown;

        public override string Name => _isKnown ? _name : "???";

        public bool IsKnown
        {
            get => _isKnown;
            set => _isKnown = value;
        }
        
        public override void OnAfterApplyEffect()
        {
            base.OnAfterApplyEffect();
            
            if (!_isKnown)
            {
                _isKnown = true;
                _onItemDiscovered.Raise(this);
            }
        }

        internal MushroomItemVisual MushroomItemVisual
        {
            get => _mushroomItemVisual;
            set
            {
                _mushroomItemVisual = value;
                _objectPrefab = _mushroomItemVisual.ObjectPrefab;
                _icon = _mushroomItemVisual.Sprite;
            }
        }
        
        #region IResettableScriptableObject
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;

        public new void ResetScriptableObject()
        {
            _isKnown = false;
            
            base.ResetScriptableObject();
        }
        
        #endregion
        
    }
}