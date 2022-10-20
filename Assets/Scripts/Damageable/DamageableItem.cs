using UnityEngine.Events;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using BML.Scripts.Attributes;

namespace BML.Scripts {
    [Serializable]
    public class DamageableItem
    {
        private enum DamageModifierType {
            Multiplier,
            Integer,
            Override
        }
        
        [EnumFlags] [SerializeField] private DamageType _damageType;
        [SerializeField] private DamageModifierType _damageModifierType;
        [ShowIf("_damageModifierType", DamageModifierType.Multiplier)] [SerializeField] private int _damageMultiplier = 1;
        [ShowIf("_damageModifierType", DamageModifierType.Integer)] [SerializeField] private int _damageResistance = 0;
        [ShowIf("_damageModifierType", DamageModifierType.Override)] [SerializeField] private int _damageOverride = 0;
        [SerializeField] private UnityEvent<HitInfo> _onDamage;
        [SerializeField] private UnityEvent<HitInfo> _onFailDamage;
        [SerializeField] private UnityEvent<HitInfo> _onDeath;

        public DamageType DamageType { get {return _damageType;}}
        public UnityEvent<HitInfo> OnDamage { get {return _onDamage;}}
        public UnityEvent<HitInfo> OnFailDamage { get {return _onFailDamage;}}
        public UnityEvent<HitInfo> OnDeath { get {return _onDeath;}}

        public int ApplyDamage(int damage) {
            if(_damageModifierType == DamageModifierType.Override) {
                return _damageOverride;
            }

            if(_damageModifierType == DamageModifierType.Integer) {
                return Math.Max(0, damage - _damageResistance);
            }

            return damage * _damageMultiplier;
        }
    }
}
