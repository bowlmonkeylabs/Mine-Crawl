﻿using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    [CreateAssetMenu(fileName = "caveMinimapParameters", menuName = "BML/Cave Gen/Minimap Parameters", order = 0)]
    public class CaveMinimapParameters : ScriptableObject
    {
        #region Inspector

        [SerializeField] public GameObject PrefabCaveNode;
        [SerializeField] public GameObject PrefabCaveNodeConnection;

        [SerializeField] public Color CulledColor;
        [SerializeField] public Color VisibleColor;
        [SerializeField] public Color VisitedColor;
        [SerializeField] public Color OccupiedColor;

        [SerializeField] public float LineConnectionEndOffset = 0f;
        [SerializeField] public float RelativeScale_Unassigned = 0.5f;
        [SerializeField] public float RelativeScale_Small = 0.5f;
        [SerializeField] public float RelativeScale_Medium = 0.75f;
        [SerializeField] public float RelativeScale_Large = 1f;
        
        [SerializeField] public Color Color_StartRoom = Color.green;
        [SerializeField] public Color Color_EndRoom = Color.red;
        [SerializeField] public Color Color_MerchantRoom = Color.magenta;

        [SerializeField] public SafeFloatValueReference ZoomLevel;

        [SerializeField] public bool RestrictMapRadius = true;
        [SerializeField] public float MapPlayerRadius = 30f;

        #endregion

        #region Unity lifecycle

        #endregion

        #region Public interface

        public float GetNodeTypeRelativeScale(CaveNodeType nodeType)
        {
            switch (nodeType)
            {
                default:
                case CaveNodeType.Unassigned:
                    return RelativeScale_Unassigned;
                    break;
                case CaveNodeType.Small:
                    return RelativeScale_Small;
                    break;
                case CaveNodeType.Medium:
                    return RelativeScale_Medium;
                    break;
                case CaveNodeType.Large:
                    return RelativeScale_Large;
                    break;
            }
        }

        #endregion
    }
}