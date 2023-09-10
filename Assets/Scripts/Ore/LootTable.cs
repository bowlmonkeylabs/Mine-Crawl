using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "BML/Loot Table", order = 0)]
    public class LootTable : ScriptableObject
    {
        [Serializable]
        public class LootTableEntry
        {
            public float DropChance;
            public UnityEvent OnDrop;
            public List<GameObject> DropPrefabs;
        }

        [SerializeField] private List<LootTableEntry> _entries;

        public List<LootTableEntry> LootTableEntries { get => _entries; }

        private List<List<LootTableEntry>> _lootTableHistory = new List<List<LootTableEntry>>();

        public List<GameObject> Evaluate(float value)
        {
            float acc = 0;
            foreach (var tableEntry in _entries)
            {
                // TODO this doesn't work if the DropChances don't sum to 1
                acc += tableEntry.DropChance;
                if (value <= acc)
                {
                    tableEntry.OnDrop?.Invoke();
                    return tableEntry.DropPrefabs;
                }
            }
            return null;
        }

        public void OverrideLootTable(LootTable overidingLootTable) {
            _lootTableHistory.Add(new List<LootTableEntry>(LootTableEntries));
            LootTableEntries.Clear();
            LootTableEntries.AddRange(overidingLootTable.LootTableEntries);
        }
    }
}