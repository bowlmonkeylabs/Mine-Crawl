using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace BML.Scripts.CaveV2
{
    [CreateAssetMenu(fileName = "CaveGenParams", menuName = "BML/CaveGenParams", order = 0)]
    public class CaveGenParameters : ScriptableObject
    {
        #region Inspector

        public int Seed
        {
            get => _seed;
            set
            {
                LogSeedHist();
                _seed = value;
            }
        }
        
        [HorizontalGroup("Seed")]
        [DisableIf("$LockSeed")]
        [OnValueChanged("LogSeedHist")]
        // [TitleGroup("Seed")]
        [SerializeField] private int _seed;
        
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

        [TitleGroup("Poisson")]
        [LabelText("Sample Radius")]
        [Range(2f, 50f)]
        public float PoissonSampleRadius = 1f;
        
        [TitleGroup("Poisson")]
        [LabelText("Bounds")]
        public Bounds PoissonBounds = new Bounds(Vector3.zero, Vector3.one * 5);
    
        [TitleGroup("Poisson")]
        // [HorizontalGroup("PoissonBoundsPadding", LabelWidth = 0.3f)]
        [LabelText("Bounds Padding")]
        [MinValue("Vector3.zero")]
        [InlineButton("AutoPoissonBoundsPadding", "Auto")]
        public Vector3 PoissonBoundsPadding = Vector3.one;
        
        [TitleGroup("Poisson")]
        // [HorizontalGroup("PoissonBoundsPadding", LabelWidth = 0.05f)]
        [LabelText("Bounds Padding")]
        [EnumToggleButtons]
        public PaddingType PoissonBoundsPaddingInnerOuter = PaddingType.Inner;
        public enum PaddingType
        {
            Inner, Outer
        }
        
        [TitleGroup("Graph processing")]
        [Range(0f, 1f)]
        public float MaxEdgeLengthFactor = 0.25f;
        
        [TitleGroup("Graph processing")]
        [Range(1, 90)]
        public int MaxEdgeSteepnessAngle = 30;
        
        #endregion

        #region Buttons

        private void AutoPoissonBoundsPadding()
        {
            PoissonBoundsPadding = Vector3.one * PoissonSampleRadius / 2;
        }

        #endregion
        
        #region Unity lifecycle
        
        public delegate void OnValidateFunction();
        public event OnValidateFunction OnValidateEvent;

        private void OnValidate()
        {
            OnValidateEvent?.Invoke();
        }

        #endregion

        #region Utils

        public void UpdateRandomSeed()
        {
            if (LockSeed) return;

            Seed = Random.Range(Int32.MinValue, Int32.MaxValue);
        }

        public Bounds GetBoundsWithPadding(Bounds bounds, PaddingType paddingType)
        {
            Vector3 poissonBoundsWithPadding = bounds.size;
            if (this.PoissonBoundsPaddingInnerOuter == PaddingType.Inner &&
                paddingType == PaddingType.Inner)
            {
                poissonBoundsWithPadding = bounds.size - this.PoissonBoundsPadding * 2;
            }
            else if (this.PoissonBoundsPaddingInnerOuter == PaddingType.Outer &&
                     paddingType == PaddingType.Outer)
            {
                poissonBoundsWithPadding = bounds.size + this.PoissonBoundsPadding * 2;
            }
            return new Bounds(bounds.center, poissonBoundsWithPadding);
        }

        #endregion
    }
}