using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.ItemTreeGraph;
using UnityEngine;
using XNode;

namespace BML.Scripts.Player.Items
{
    //To represent connections in the item graph
    [Serializable]
    public class ItemGraphConnection
    {

    }

    [CreateAssetMenu(fileName = "ItemTreeGraph", menuName = "BML/Graphs/ItemTreeGraph", order = 0)]
    public class ItemTreeGraph : NodeGraph, IResettableScriptableObject {
        public List<PlayerItem> GetUnobtainedItemPool() {
            var unobtainedItemPool = new List<PlayerItem>();

            var startNodes = this.nodes.Where(node => node is ItemTreeGraphStartNode);
            foreach(var startNode in startNodes) {
                var itemNodes = startNode.Outputs
                    .Select(port => port.GetConnections().FirstOrDefault(c => c.IsInput)?.node)
                    .Where(node => node != null)
                    .Select(node => node as ItemTreeGraphNode)
                    .ToList();
                    
                while(itemNodes.Count() > 0) {
                    var haveAnyNodesBeenObtained = itemNodes.Any(itemNode => itemNode.Obtained);
                    if(!haveAnyNodesBeenObtained) {
                        unobtainedItemPool.Add(itemNodes[UnityEngine.Random.Range(0, itemNodes.Count())].Item);
                        break;
                    }

                    itemNodes = itemNodes
                        .SelectMany(node => node.Outputs.Select(port => port.GetConnections().FirstOrDefault(c => c.IsInput)?.node))
                        .Where(node => node != null)
                        .Select(node => node as ItemTreeGraphNode)
                        .ToList();
                }
            }
            return unobtainedItemPool;
        }

        public void MarkItemAsObtained(PlayerItem item) {
            var nodeWithItem = nodes.FirstOrDefault(node => {
                if(node is ItemTreeGraphNode) {
                    return (node as ItemTreeGraphNode).Item == item;
                }

                return false;
            });
            (nodeWithItem as ItemTreeGraphNode).Obtained = true;
        }

        public void ResetScriptableObject() {
            nodes.ForEach(node => {
                if(node is ItemTreeGraphNode) {
                    (node as ItemTreeGraphNode).Obtained = false;
                }
            });
        }
    }
}