using BML.Scripts.CaveV2.CaveGraph.NodeData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.CaveGraph
{
    public class CaveNodeConnectionDataDebugComponent : MonoBehaviour
    {
        [ReadOnly] public CaveNodeConnectionData CaveNodeConnectionData;
        [ReadOnly] public CaveGenComponentV2 CaveGenerator; 
    }
}