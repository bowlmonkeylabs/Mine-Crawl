using BML.ScriptableObjectCore.Scripts.Variables.SafeValueReferences;
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

        [SerializeField] public SafeFloatValueReference ZoomLevel;

        [SerializeField] public bool RestrictMapRadius = true;
        [SerializeField] public float MapPlayerRadius = 30f;

        #endregion

        #region Unity lifecycle

        #endregion
    }
}