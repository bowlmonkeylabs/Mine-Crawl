using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

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
        private int[] rollbackSeedDropdownValues => seedValuesHist.ToArray();
        private Queue<int> seedValuesHist = new System.Collections.Generic.Queue<int>();
        private void LogSeedHist()
        {
            seedValuesHist.Enqueue(_seed);
            while (seedValuesHist.Count > 10)
            {
                seedValuesHist.Dequeue();
            }
        }
        private void RollbackSeed()
        {
            Seed = _rollbackSeed;
        }

        [TitleGroup("Poisson")]
        [LabelText("Sample Radius")]
        [Range(2f, 50f)]
        public float PoissonSampleRadius = 1f;
        
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