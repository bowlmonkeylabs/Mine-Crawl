using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.ItemTreeGraph;
using Sirenix.Utilities;
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
    public class ItemTreeGraph : NodeGraph, IResettableScriptableObject
    {
        [SerializeField] private PlayerInventory _playerInventory;

        private static int MAX_ITEM_TREE_RECURSION_DEPTH = 99; 
        public List<PlayerItem> GetUnobtainedItemPool() 
        {
            var unobtainedItemPool = new List<PlayerItem>();

            var startNodes = this.nodes.Where(node => node is ItemTreeGraphStartNode);
            foreach (var startNode in startNodes)
            {
                var nodesToCheck = new HashSet<ItemTreeGraphNode>(
                    startNode.Outputs
                        .First()
                        .GetConnections().Where(nodePort => nodePort.IsInput).Select(nodePort => nodePort.node)
                        .Select(node => node as ItemTreeGraphNode)
                        .Where(node => node != null)
                );

                int depthCount = 0;
                while (nodesToCheck.Count > 0 && depthCount <= MAX_ITEM_TREE_RECURSION_DEPTH)
                {
                    depthCount++;
                    var nodesWithObtainedStatus = nodesToCheck.Select(node =>
                        (node: node, obtained: _playerInventory.PassiveStackableItems.Contains(node.Item)));
                    
                    var unobtainedNodes = nodesWithObtainedStatus.Where(node => !node.obtained).ToList();
                    unobtainedItemPool.AddRange(unobtainedNodes.Select(node => node.node.Item));
                    foreach (var node in unobtainedNodes)
                    {
                        nodesToCheck.Remove(node.node);
                    }

                    var obtainedNodes = nodesToCheck.Where(node => node.Obtained).ToList();
                    var childrenOfObtainedNodes = obtainedNodes.SelectMany(node =>
                            node.Outputs.First()
                                .GetConnections()
                                .Where(port => port.IsInput)
                                .Select(port => port.node as ItemTreeGraphNode))
                                .Where(node => node != null);
                    foreach (var node in obtainedNodes)
                    {
                        nodesToCheck.Remove(node);
                    }
                    nodesToCheck.AddRange(childrenOfObtainedNodes);
                }

                if (depthCount >= MAX_ITEM_TREE_RECURSION_DEPTH)
                {
                    Debug.LogError("Exceeded max recursion depth when traversing item upgrade tree. Increase recursion limit or adjust item tree?");
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