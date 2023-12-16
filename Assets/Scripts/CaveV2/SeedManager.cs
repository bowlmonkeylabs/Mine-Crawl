using System;
using Sirenix.OdinInspector;
using UnityEngine;
using BML.ScriptableObjectCore.Scripts.Variables;
using System.Collections.Generic;
using MoreMountains.Tools;
using System.Linq;

namespace BML.Scripts.CaveV2
{
    public class SeedManager : MMPersistentSingleton<SeedManager>
    {
        [SerializeField, InlineEditor] private SeedHistory _seedHistory;
        [SerializeField] private BoolReference _retrySameSeed;

        [ShowInInspector, ReadOnly] private bool seedInitialized = false;
        [ShowInInspector, ReadOnly] private Dictionary<string, int> steppedSeeds = new Dictionary<string, int>();

        private int baseSteppedSeed;

        public int Seed
        {
            get {
                InitializeSeed();
                return _seedHistory.Seed;
            }
        }

        protected override void Awake() 
        {
            base.Awake();
            
            InitializeSeed();
        }

        public bool LockSeed
        {
            get => _seedHistory.LockSeed;
        }

        public void UpdateRandomSeed(bool logSeedHist = true)
        {
            _seedHistory.UpdateRandomSeed(logSeedHist);
        }

        private void TryInitSteppedSeed(string key) {
            if(baseSteppedSeed != Seed) {
                steppedSeeds.Clear();
                baseSteppedSeed = Seed;
            }

            if(!steppedSeeds.ContainsKey(key)) {
                int step = key.Select((c, i) => ((int) c) * i).Sum();
                steppedSeeds.Add(key, baseSteppedSeed + step);
            }
        }

        public int GetSteppedSeed(string key) {
            TryInitSteppedSeed(key);
            return steppedSeeds[key];
        }

        public void UpdateSteppedSeed(string key, object val = null) {
            TryInitSteppedSeed(key);
            if(val != null) {
                steppedSeeds[key] = (int) val;
                return;
            }
            steppedSeeds[key] += 1;
        }

        private void InitializeSeed() {
            if(!seedInitialized) {
                if(!_retrySameSeed.Value) {
                    _seedHistory.UpdateRandomSeed();
                }
                
                seedInitialized = true;
            }
        }
    }
}
