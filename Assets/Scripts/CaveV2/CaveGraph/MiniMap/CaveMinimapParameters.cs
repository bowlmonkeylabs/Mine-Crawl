using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph.Minimap
{
    [CreateAssetMenu(fileName = "caveMinimapParameters", menuName = "BML/Cave Gen/Minimap Parameters", order = 0)]
    public class CaveMinimapParameters : ScriptableObject
    {
        #region Inspector

        [SerializeField] public GameObject PrefabCaveNode;
        [SerializeField] public GameObject PrefabCaveNodeConnection;

        [SerializeField] public Color DefaultColor;
        [SerializeField] public Color VisitedColor;

        #endregion

        #region Unity lifecycle

        #endregion
    }
}