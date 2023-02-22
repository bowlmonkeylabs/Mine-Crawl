using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections.Generic;
using System;

namespace BML.Scripts.CaveV2 {
    [CreateAssetMenu(fileName = "SeedHistory", menuName = "BML/SeedHistory", order = 0)]
    public class SeedHistory : ScriptableObject
    {
        public int Seed
        {
            get => _seed;
        }

        [HorizontalGroup("Seed")]
        [DisableIf("$LockSeed")]
        [OnValueChanged("LogSeedHist")]
        // [TitleGroup("Seed")]
        [SerializeField] private int _seed = 0;
        
        [HorizontalGroup("Seed", Width = 0.4f)]
        [LabelText("Lock")]
        public bool LockSeed;

        [ValueDropdown("rollbackSeedDropdownValues")]
        [OnValueChanged("RollbackSeed")]
        [SerializeField] private int _rollbackSeed;
        private int[] rollbackSeedDropdownValues => _seedValuesHist.ToArray();
        [SerializeField, HideInInspector]
        private List<int> _seedValuesHist = new List<int>(_SeedValuesHistCapacity);
        private static readonly int _SeedValuesHistCapacity = 10;
        private void LogSeedHist()
        {
            if (_seedValuesHist.Count >= _SeedValuesHistCapacity)
            {
                var removeCount = Math.Max(0, _seedValuesHist.Count - _SeedValuesHistCapacity) + 1;
                _seedValuesHist.RemoveRange(0, removeCount);
            }
            _seedValuesHist.Add(_seed);
        }
        private void RollbackSeed()
        {
            // Purposefully avoid the public setter, so that rolling back to a seed does not log it to hist again.
            _seed = _rollbackSeed;
        }

        [PropertyOrder(-1)]
        [ButtonGroup("Seed")]
        [Button("Random Seed")]
        public bool UpdateRandomSeed(bool logSeedHist = true)
        {
            if (LockSeed) return false;
                
            if (logSeedHist) LogSeedHist();
            
            _seed = Random.Range(Int32.MinValue, Int32.MaxValue);

            return true;
        }
    }
}
