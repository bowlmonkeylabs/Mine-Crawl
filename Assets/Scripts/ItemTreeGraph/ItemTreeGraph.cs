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
    [ExecuteAlways]
    public class ItemTreeGraph : NodeGraph, IResettableScriptableObject
    {
        #region Inspector
        
        [SerializeField] private PlayerInventory _playerInventory;
        [SerializeField] private IntVariable _maxSlottedTrees;

        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            UpdateTreeFromInventory();
            
            _playerInventory.OnPassiveStackableItemAdded += UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemRemoved += UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemChanged += UpdateTreeFromInventory;
            
            _playerInventory.OnPassiveStackableItemTreeAdded += UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemTreeRemoved += UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemTreeChanged += UpdateTreeFromInventory;

            _playerInventory.OnReset += UpdateTreeFromInventory;
        }

        private void OnDisable()
        {
            _playerInventory.OnPassiveStackableItemAdded -= UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemRemoved -= UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemChanged -= UpdateTreeFromInventory;
            
            _playerInventory.OnPassiveStackableItemTreeAdded -= UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemTreeRemoved -= UpdateTreeFromInventory;
            _playerInventory.OnPassiveStackableItemTreeChanged -= UpdateTreeFromInventory;

            _playerInventory.OnReset -= UpdateTreeFromInventory;
        }

        #endregion
        
        private static int MAX_ITEM_TREE_RECURSION_DEPTH = 99;

        public void TraverseItemTree(ItemTreeGraphStartNode startNode, Action<ItemTreeGraphNode> nodeAction) {
            var itemNodes = startNode.GetOutputPort("Start").GetConnections().Select(nodePort => nodePort.node);
            int depthCount = 0;
            while (itemNodes.Count() > 0 && depthCount <= MAX_ITEM_TREE_RECURSION_DEPTH)
            {
                depthCount++;

                itemNodes.ForEach(node => nodeAction.Invoke(node as ItemTreeGraphNode));
                
                itemNodes = itemNodes.SelectMany(node => node.GetOutputPort("To").GetConnections().Select(nodePort => nodePort.node));
            }

            if (depthCount >= MAX_ITEM_TREE_RECURSION_DEPTH)
            {
                Debug.LogError("Exceeded max recursion depth when traversing item upgrade tree. Increase recursion limit or adjust item tree?");
            }
        }

        public List<PlayerItem> GetUnobtainedItemPool(bool updateStatusInGraphWhileTraversing = true) 
        {
            var unobtainedItemPool = new List<PlayerItem>();

            var startNodes = this.nodes.OfType<ItemTreeGraphStartNode>();
            // var slottedStartNodes = startNodes.Where(node => (node as ItemTreeGraphStartNode).Slotted);
            var slottedStartNodes = _playerInventory.PassiveStackableItemTrees;
            
            if(slottedStartNodes.Count >= _maxSlottedTrees.Value) {
                startNodes = slottedStartNodes;
            }

            foreach (var startNode in startNodes)
            {
                var nodesToCheck = new HashSet<ItemTreeGraphNode>(
                    startNode.Outputs
                        .First()
                        .GetConnections().Where(nodePort => nodePort.IsInput).Select(nodePort => nodePort.node)
                        .OfType<ItemTreeGraphNode>()
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

        public void MarkItemAsObtained(PlayerItem item) 
        {
            var nodeWithItem = nodes.OfType<ItemTreeGraphNode>().FirstOrDefault(node => node.Item == item);
            UpdateTreeFromInventory(nodeWithItem);
        }

        public void UpdateTreeFromInventory()
        {
            var startNodes = nodes.OfType<ItemTreeGraphStartNode>();
            foreach (var itemTreeGraphStartNode in startNodes)
            {
                bool isSlotted = _playerInventory.PassiveStackableItemTrees.Contains(itemTreeGraphStartNode);
                itemTreeGraphStartNode.Slotted = isSlotted;

                itemTreeGraphStartNode.NumberOfObtainedItemsInTree = 0;
            }
            
            var itemNodes = nodes.OfType<ItemTreeGraphNode>();
            foreach (var itemTreeGraphNode in itemNodes)
            {
                bool isObtained = _playerInventory.PassiveStackableItems.Contains(itemTreeGraphNode.Item);
                itemTreeGraphNode.Obtained = isObtained;

                itemTreeGraphNode.TreeStartNode.NumberOfObtainedItemsInTree += isObtained ? 1 : 0;
            }
        }
        public void UpdateTreeFromInventory(ItemTreeGraphNode itemNode)
        {
            bool isObtained = _playerInventory.PassiveStackableItems.Contains(itemNode.Item);
            itemNode.Obtained = isObtained;
            
            var treeStartNode = itemNode.TreeStartNode ?? GetTreeStartNodeForItem(itemNode.Item);
            UpdateTreeFromInventory(treeStartNode);
        }
        public void UpdateTreeFromInventory(PlayerItem item)
        {
            var itemNode = nodes.OfType<ItemTreeGraphNode>().FirstOrDefault(node => node.Item == item);
            UpdateTreeFromInventory(itemNode);
        }
        public void UpdateTreeFromInventory(ItemTreeGraphStartNode treeStartNode)
        {
            treeStartNode.Slotted = _playerInventory.PassiveStackableItemTrees.Contains(treeStartNode);
            treeStartNode.NumberOfObtainedItemsInTree = _playerInventory.PassiveStackableItems.Count(item => item.PassiveStackableTreeStartNode == treeStartNode);
        }

        public ItemTreeGraphStartNode GetTreeStartNodeForItem(PlayerItem item) {
            var nodeWithItem = nodes.FirstOrDefault(node => {
                if(node is ItemTreeGraphNode) {
                    return (node as ItemTreeGraphNode).Item == item;
                }

                return false;
            });

            var startNode = nodeWithItem.GetInputPort("From").GetConnections().FirstOrDefault().node;
            int depthCount = 0;
            while(!(startNode is ItemTreeGraphStartNode) && depthCount <= MAX_ITEM_TREE_RECURSION_DEPTH) {
                startNode = startNode.GetInputPort("From").GetConnections().FirstOrDefault().node;
                depthCount++;
            }

            return startNode as ItemTreeGraphStartNode;
        }

        public List<ItemTreeGraphStartNode> GetSlottedTreeStartNodes() {
            return nodes.Where(node => node is ItemTreeGraphStartNode && (node as ItemTreeGraphStartNode).Slotted)
                .Select(node => node as ItemTreeGraphStartNode)
                .ToList();
        }
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;

        public void ResetScriptableObject() {
            nodes.ForEach(node => {
                if(node is ItemTreeGraphNode) {
                    (node as ItemTreeGraphNode).Obtained = false;
                }
                if(node is ItemTreeGraphStartNode) {
                    (node as ItemTreeGraphStartNode).Slotted = false;
                }
            });
            
            OnReset?.Invoke();
        }
    }
}