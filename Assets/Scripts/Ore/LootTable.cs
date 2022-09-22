using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BML.Scripts
{
    [CreateAssetMenu(fileName = "LootTable", menuName = "BML/Loot Table", order = 0)]
    public class LootTable : ScriptableObject
    {
        [Serializable]
        private class TableEntry
        {
            public float DropChance;
            public UnityEvent OnDrop;
        }

        [SerializeField] private List<TableEntry> _entries;

        public void Evaluate(float value)
        {
            float acc = 0;
            foreach (var tableEntry in _entries)
            {
                // TODO this doesn't work if the DropChances don't sum to 1
                acc += tableEntry.DropChance;
                if (value <= acc)
                {
                    tableEntry.OnDrop?.Invoke();
                    break;
                }
            }
        }
    }
}