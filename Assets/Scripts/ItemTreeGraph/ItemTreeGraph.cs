using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.Variables;
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
        [SerializeField] private IntVariable _maxSlottedTrees;

        private static int MAX_ITEM_TREE_RECURSION_DEPTH = 99; 

        public void TraverseItemTree(ItemTreeGraphStartNode startNode, Action<ItemTreeGraphNode> nodeAction) {
            var itemNodes = startNode.GetOutputPort("Start").GetConnections().Select(nodePort => nodePort.node);
            int depthCount = 0;
            while (itemNodes.Count() > 0 && depthCount <= MAX_ITEM_TREE_RECURSION_DEPTH)
            {
                depthCount++;

                itemNodes.ForEach(node => nodeAction.Invoke(node as ItemTreeGraphNode));
                
                itemNodes = itemNodes.SelectMany(node => node.GetOutputPort("From").GetConnections().Select(nodePort => nodePort.node));
            }

            if (depthCount >= MAX_ITEM_TREE_RECURSION_DEPTH)
            {
                Debug.LogError("Exceeded max recursion depth when traversing item upgrade tree. Increase recursion limit or adjust item tree?");
            }
        }

        public List<PlayerItem> GetUnobtainedItemPool() 
        {
            var unobtainedItemPool = new List<PlayerItem>();

            var startNodes = this.nodes.Where(node => node is ItemTreeGraphStartNode);
            var slottedStartNodes = startNodes.Where(node => (node as ItemTreeGraphStartNode).Slotted);
            
            if(slottedStartNodes.Count() >= _maxSlottedTrees.Value) {
                startNodes = slottedStartNodes;
            }

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
                    var nodesWithStatus = nodesToCheck.Select(node =>
                        (node: node, playerHas: _playerInventory.PassiveStackableItems.Contains(node.Item)));
                    
                    var unobtainedNodes = nodesWithStatus.Where(node => !node.playerHas).ToList();
                    unobtainedItemPool.AddRange(unobtainedNodes.Select(node => node.node.Item));
                    foreach (var node in unobtainedNodes)
                    {
                        nodesToCheck.Remove(node.node);
                    }

                    var obtainedNodes = nodesWithStatus.Where(node => node.playerHas).ToList();
                    var childrenOfObtainedNodes = obtainedNodes.SelectMany(node =>
                            node.node.Outputs.First()
                                .GetConnections()
                                .Where(port => port.IsInput)
                                .Select(port => port.node as ItemTreeGraphNode))
                                .Where(node => node != null);
                    foreach (var node in obtainedNodes)
                    {
                        nodesToCheck.Remove(node.node);
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
            GetTreeStartNodeForItem(item).Slotted = true;
        }

        public ItemTreeGraphStartNode GetTreeStartNodeForItem(PlayerItem item) {
            var nodeWithItem = nodes.FirstOrDefault(node => {
                if(node is ItemTreeGraphNode) {
                    return (node as ItemTreeGraphNode).Item == item;
                }

                return false;
            });

            var startNode = nodeWithItem.Inputs.FirstOrDefault()?.GetConnections().Where(port => port.IsOutput).FirstOrDefault()?.node;
            int depthCount = 0;
            while(!(startNode is ItemTreeGraphStartNode) && depthCount <= MAX_ITEM_TREE_RECURSION_DEPTH) {
                startNode = nodeWithItem.Inputs.FirstOrDefault()?.GetConnections().Where(port => port.IsOutput).FirstOrDefault()?.node;
                depthCount++;
            }

            return startNode as ItemTreeGraphStartNode;
        }

        public List<ItemTreeGraphStartNode> GetSlottedTreeStartNodes() {
            return nodes.Where(node => node is ItemTreeGraphStartNode && (node as ItemTreeGraphStartNode).Slotted)
                .Select(node => node as ItemTreeGraphStartNode)
                .ToList();
        }

        public int GetObtainedCount(ItemTreeGraphStartNode startNode) {
            int obtainedCount = 0;
            TraverseItemTree(startNode, node => obtainedCount += node.Obtained ? 1 : 0);
            return obtainedCount;
        }

        public void ResetScriptableObject() {
            nodes.ForEach(node => {
                if(node is ItemTreeGraphNode) {
                    (node as ItemTreeGraphNode).Obtained = false;
                }
                if(node is ItemTreeGraphStartNode) {
                    (node as ItemTreeGraphStartNode).Slotted = false;
                }
            });
        }
    }
}