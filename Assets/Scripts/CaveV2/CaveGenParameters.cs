using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

namespace BML.Scripts.CaveV2
{
    [CreateAssetMenu(fileName = "CaveGenParams", menuName = "BML/Cave Gen/CaveGenParams", order = 0)]
    public class CaveGenParameters : ScriptableObject
    {
        #region Inspector

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

        [Serializable]
        public struct SteepnessRange
        {
            [MinMaxSlider(0, 90, true)]
            public Vector2 Angle;
            [MinMaxSlider(0, 90, true)]
            public Vector2 EdgeRadius;
        }
        
        [TitleGroup("Graph processing")]
        [Range(0f, 10f)]
        public float MaxEdgeLengthFactor = 1.5f;
        
        [TitleGroup("Graph processing"), PropertySpace(10f, 10f)]
        public List<SteepnessRange> SteepnessRanges = new List<SteepnessRange>();

        [TitleGroup("Graph processing")]
        [UnityEngine.Tooltip("If true, cave node scaling is calculated based on the closeness of adjacent nodes.")]
        public bool CalculateRoomSize = false;
        
        [TitleGroup("Graph processing")]
        [ShowIf("$CalculateRoomSize")] [Indent]
        [UnityEngine.Tooltip("If true, cave node scaling is calculated based adjacency before nodes are filtered to the resulting level path.")]
        public bool CalculateRoomSizeBasedOnRawAdjacency = false;
        
        [TitleGroup("Graph processing")]
        [MinMaxSlider(0.1f, 10f, true)]
        [ShowIf("$CalculateRoomSize")] [Indent]
        public Vector2 RoomScaling = Vector2.one;

        [TitleGroup("Graph processing")]
        public bool OnlyShortestPathBetweenStartAndEnd = true;
        
        [TitleGroup("Graph processing")]
        public bool UseOffshootsFromMainPath = true;
        
        [TitleGroup("Graph processing")]
        [ShowIf("$UseOffshootsFromMainPath")] [Indent]
        public int NumOffshoots = 2;

        [TitleGroup("Graph processing")] [MinMaxSlider(1, 50, true)]
        [ShowIf("$UseOffshootsFromMainPath")] [Indent]
        public Vector2Int MinMaxOffshootLength = new Vector2Int(1, 3);

        [TitleGroup("Graph processing")]
        public bool MinimumSpanningTree = true;
        
        [TitleGroup("Graph processing")]
        [ShowIf("$MinimumSpanningTree")] [Indent]
        public int MinimumSpanningNodes = 3;
        
        [TitleGroup("Graph processing")]
        [ShowIf("$MinimumSpanningTree")] [Indent]
        public bool MinimumSpanningTree_Debug1 = false;
        
        [TitleGroup("Graph processing")]
        [ShowIf("$MinimumSpanningTree")] [Indent]
        public bool MinimumSpanningTree_Debug2 = false;
        
        [TitleGroup("Graph processing")]
        [ShowIf("$MinimumSpanningTree")] [Indent]
        public bool MinimumSpanningTree_Debug3 = false;
        
        [TitleGroup("Graph processing")]
        public bool RemoveOrphanNodes = true;

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