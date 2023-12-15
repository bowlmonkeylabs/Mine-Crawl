using System.Collections;
using System.Collections.Generic;
using BML.Scripts.CaveV2.CaveGraph.NodeData;
using Sirenix.OdinInspector;
using UnityEngine;

namespace BML.Scripts.CaveV2.SpawnObjects
{
    public class SpawnedObjectCaveNodeData : MonoBehaviour
    {
        [ShowInInspector] public ICaveNodeData CaveNode { get; set; }
    }
}
