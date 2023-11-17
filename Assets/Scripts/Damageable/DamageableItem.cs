using UnityEngine.Events;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections.Generic;
using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.Attributes;

namespace BML.Scripts {
    [Serializable]
    public class DamageableItem
    {
        private enum DamageModifierType {
            Multiplier,
            Integer,
            Override,
            None
        }

        [TextArea, SerializeField, HideLabel, FoldoutGroup("Preview", expanded: true)]
        private string _damageTypesPreview;
        
        [SerializeField, FoldoutGroup("Params"), ValidateInput("DisplayValues"), EnumFlags]
        private DamageType _damageType;
        
        [SerializeField, FoldoutGroup("Params")]
        private DamageModifierType _damageModifierType = DamageModifierType.None;

        [SerializeField, FoldoutGroup("Params"), ShowIf("_damageModifierType",
             DamageModifierType.Multiplier)]
        private SafeIntValueReference _damageMultiplier = new SafeIntValueReference(1);
        
        [SerializeField, FoldoutGroup("Params"), ShowIf("_damageModifierType", 
             DamageModifierType.Integer)]
        private SafeIntValueReference _damageResistance;
        
        [SerializeField, FoldoutGroup("Params"), ShowIf("_damageModifierType", 
             DamageModifierType.Override)]
        private SafeIntValueReference _damageOverride;
        
        [SerializeField, FoldoutGroup("Params")] private UnityEvent<HitInfo> _onDamage;
        [SerializeField, FoldoutGroup("Params")] private UnityEvent<HitInfo> _onFailDamage;
        [SerializeField, FoldoutGroup("Params")] private UnityEvent<HitInfo> _onDeath;

        public DamageType DamageType { get {return _damageType;}}
        public UnityEvent<HitInfo> OnDamage { get {return _onDamage;}}
        public UnityEvent<HitInfo> OnFailDamage { get {return _onFailDamage;}}
        public UnityEvent<HitInfo> OnDeath { get {return _onDeath;}}

        private int ApplyDamage(int damage)
        {
            switch (_damageModifierType)
            {
                case DamageModifierType.Override:
                    return _damageOverride.Value;
                case DamageModifierType.Integer:
                    return Math.Max(0, damage - _damageResistance.Value);
                case DamageModifierType.Multiplier:
                    return damage * _damageMultiplier.Value;
                case DamageModifierType.None:
                default:
                    return damage;
            }
        }
        
        public HitInfo ApplyDamage(HitInfo hitInfo)
        {
            int computedDamage = ApplyDamage(hitInfo.Damage);
            hitInfo.Damage = computedDamage;
            return hitInfo;
        }

        private bool DisplayValues(DamageType selectedDamageTypes) {
            _damageTypesPreview = selectedDamageTypes.ToString("F");
            return true;
        }
    }
}
