using System;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Variables.ReferenceTypeVariables;
using BML.Utils;
using BML.Utils.Random;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace BML.ScriptableObjectCore.Scripts.Variables
{
    public enum LootTableKey
    {
        Nothing = 0,
        CommonOre = 1,
        CommonOre2 = 2,
        RareOre = 3,
        RareOre2 = 4,
        
        Health = 10,
        Health2 = 11,
        Health3 = 12,
        
        Bomb = 15,
        Bomb2 = 16,
        
        Consumable = 20,
        Consumable2 = 21,
        Consumable3 = 22,
        Consumable4 = 23,
        Consumable5 = 24,
        Consumable6 = 25,
        Consumable7 = 26,
        Consumable8 = 27,
        Consumable9 = 28,
        Consumable10 = 29,
    }
    
    [Serializable]
    public struct LootTableEntry<TKey, TValue> : ICloneable
    {
        [OnValueChanged("ClearEntryIfKeyIsNothing")]
        [HideLabel]
        public LootTableKey Key;

        private void ClearEntryIfKeyIsNothing()
        {
            if (this.Key == LootTableKey.Nothing)
            {
                Drops = null;
            }
        }

        [InlineProperty, AssetsOnly]
        [ListDrawerSettings(ShowFoldout = true, DefaultExpandedState = true)]
        [InfoBox("Empty", visibleIfMemberName: "@Drops.Length == 0 && Key != LootTableKey.Nothing", infoMessageType: InfoMessageType.Warning)]
        [InfoBox("Not empty?", visibleIfMemberName: "@Drops.Length > 0 && Key == LootTableKey.Nothing", infoMessageType: InfoMessageType.Warning)]
        [HideIf("@Key == LootTableKey.Nothing")]
        public TValue[] Drops;

        public object Clone()
        {
            var clone = (LootTableEntry<TKey, TValue>)this.MemberwiseClone();
            clone.Drops = (TValue[])this.Drops?.Clone();
            return clone;
        }
    }

    [Serializable]
    public class LootTable<TKey, TValue> : ICloneable
    {
        [ShowInInspector, ShowIf("@_duplicateKeys.Length > 0")]
        [InfoBox("", InfoMessageType.Error, visibleIfMemberName:"@_duplicateKeys.Length > 0")]
        private LootTableKey[] _duplicateKeys => Entries.Options.Select(pair => pair.Value.Key).GroupBy(key => key)
            .Where(group => group.Count() >= 2).Select(group => group.Key).ToArray();
        
        [SerializeField, InlineProperty, HideLabel]
        public WeightedReferenceOptions<LootTableEntry<TKey, TValue>> Entries;

        public object Clone()
        {
            var clone = (LootTable<TKey, TValue>)this.MemberwiseClone();
            clone.Entries = (WeightedReferenceOptions<LootTableEntry<TKey, TValue>>)Entries.Clone();
            return clone;
        }

        public LootTableEntry<TKey, TValue> Evaluate(float randomRoll)
        {
            return Entries.RandomWithWeights(randomRoll);
        }

        public bool HasKey(LootTableKey key)
        {
            return Entries.Options.Any(pair => pair.Value.Key == key);
        }

        public void ModifyProbability(LootTableKey key, float addAmount)
        {
            var lootTableEntry = Entries.Options.FirstOrDefault(entry => entry.Value.Key == key);
            if (lootTableEntry == null)
            {
                // Debug.LogError($"No loot table entry found for {key}.");
                // return;
                throw new Exception($"No loot table entry found for {key}.");
            }

            var prevWeight = lootTableEntry.Weight;
            lootTableEntry.Weight = Mathf.Clamp01(lootTableEntry.Weight + addAmount);
            bool wasLocked = lootTableEntry.Lock;
            lootTableEntry.Lock = true;
            bool ableToNormalize = Entries.Normalize();
            if (!ableToNormalize)
            {
                Debug.LogError("Unable to normalize weights after adjustment; the modification was not applied.");
                lootTableEntry.Weight = prevWeight;
            }
            lootTableEntry.Lock = wasLocked;
        }
    }
    
    [Required]
    [CreateAssetMenu(fileName = "LootTableVariable", menuName = "BML/Variables/LootTableVariable", order = 0)]
    public class LootTableVariable : ReferenceTypeVariable<LootTable<LootTableKey, GameObject>>
    {
        public string GetName() => name;
        public string GetDescription() => Description;
    }
    
    [Serializable]
    [InlineProperty]
    public class LootTableReference : Reference<LootTable<LootTableKey, GameObject>, LootTableVariable> { }
}