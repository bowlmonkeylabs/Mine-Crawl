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
    [Serializable]
    public enum LootTableKey
    {
        Nothing = 0,
        Health = 1 << 0,
        Health2 = 1 << 1,
        CommonOre = 1 << 2,
        CommonOre2 = 1 << 3,
        RareOre = 1 << 4,
    }
    
    [Serializable]
    public struct LootTableEntry : ICloneable
    {
        [OnValueChanged("ClearEntryIfKeyIsNothing")]
        [HideLabel]
        public LootTableKey Key;

        private void ClearEntryIfKeyIsNothing()
        {
            if (this.Key == LootTableKey.Nothing)
            {
                DropPrefabs = null;
            }
        }
        
        [InlineProperty, AssetsOnly]
        [ListDrawerSettings(Expanded = true)]
        [InfoBox("Empty", visibleIfMemberName: "@DropPrefabs.Length == 0 && Key != LootTableKey.Nothing", infoMessageType: InfoMessageType.Warning)]
        [InfoBox("Not empty?", visibleIfMemberName: "@DropPrefabs.Length > 0 && Key == LootTableKey.Nothing", infoMessageType: InfoMessageType.Warning)]
        [HideIf("@Key == LootTableKey.Nothing")]
        public GameObject[] DropPrefabs;

        public object Clone()
        {
            var clone = (LootTableEntry)this.MemberwiseClone();
            clone.DropPrefabs = (GameObject[])this.DropPrefabs?.Clone();
            return clone;
        }
    }

    [Serializable]
    public class LootTable : ICloneable
    {
        [ShowInInspector, ShowIf("@_duplicateKeys.Length > 0")]
        [InfoBox("", InfoMessageType.Error, visibleIfMemberName:"@_duplicateKeys.Length > 0")]
        private LootTableKey[] _duplicateKeys => Entries.Options.Select(pair => pair.Value.Key).GroupBy(key => key)
            .Where(group => group.Count() >= 2).Select(group => group.Key).ToArray();
        
        [SerializeField, InlineProperty, HideLabel]
        public WeightedReferenceOptions<LootTableEntry> Entries;

        public object Clone()
        {
            var clone = (LootTable)this.MemberwiseClone();
            clone.Entries = (WeightedReferenceOptions<LootTableEntry>)Entries.Clone();
            return clone;
        }

        public LootTableEntry Evaluate(float randomRoll)
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
#warning throw or log?
                // Debug.LogError($"No loot table entry found for {key}.");
                throw new Exception($"No loot table entry found for {key}.");
                return;
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
    public class LootTableVariable : ReferenceTypeVariable<LootTable>
    {
        public string GetName() => name;
        public string GetDescription() => Description;
    }
    
    [Serializable]
    [InlineProperty]
    public class LootTableReference : Reference<LootTable, LootTableVariable> { }
}