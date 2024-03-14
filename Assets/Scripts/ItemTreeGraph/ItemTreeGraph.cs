using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.ScriptableObjectCore.Scripts.Variables;
using BML.Scripts.ItemTreeGraph;
using BML.Scripts.Utils;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;
using XNode;

namespace BML.Scripts.Player.Items
{
    //To represent connections in the item graph
    [Serializable]
    public class ItemGraphConnection
    {
        public PlayerItem Item;
    }

    [CreateAssetMenu(fileName = "ItemTreeGraph", menuName = "BML/Graphs/ItemTreeGraph", order = 0)]
    [ExecuteAlways]
    public class ItemTreeGraph : NodeGraph, IResettableScriptableObject
    {
        #region Inspector
        
        [SerializeField] private PlayerInventory _playerInventory;

        #endregion
        
        #region Unity lifecycle

        private void OnEnable()
        {
            UpdateTreeFromInventory();
            
            _playerInventory.PassiveStackableItems.OnItemAdded += UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItems.OnItemRemoved += UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector += UpdateTreeFromInventory;
            
            _playerInventory.PassiveStackableItemTrees.OnItemAdded += UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItemTrees.OnItemRemoved += UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector += UpdateTreeFromInventory;

            _playerInventory.OnReset += UpdateTreeFromInventory;
        }

        private void OnDisable()
        {
            _playerInventory.PassiveStackableItems.OnItemAdded -= UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItems.OnItemRemoved -= UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItems.OnAnyItemChangedInInspector -= UpdateTreeFromInventory;
            
            _playerInventory.PassiveStackableItemTrees.OnItemAdded -= UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItemTrees.OnItemRemoved -= UpdateTreeFromInventory;
            _playerInventory.PassiveStackableItemTrees.OnAnyItemChangedInInspector -= UpdateTreeFromInventory;

            _playerInventory.OnReset -= UpdateTreeFromInventory;
        }

        #endregion
        
        private static int MAX_ITEM_TREE_RECURSION_DEPTH = 99;

        public void TraverseItemTree(ItemTreeGraphStartNode startNode, Action<ItemTreeGraphNode> nodeAction) 
        {
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

        public List<PlayerItem> GetUnobtainedItemPool(int playerLevel, bool updateStatusInGraphWhileTraversing = true) 
        {
            var unobtainedItemPool = new List<PlayerItem>();

            List<ItemTreeGraphStartNode> startNodes;

            int numOpenTreeSlots = _playerInventory.PassiveStackableItemTrees.Slots.Where(slot => slot.Item == null && slot.Filter == SlotTypeFilter.None).Count(); // Tree slots with a filter applied are reserved for "ability choice" trees, so we need to exclude them here.

            var choiceForThisLevel = nodes.OfType<ItemTreeGraphChoiceNode>()
                .Where(choice => choice.Evaluated == false)
                .FirstOrDefault(choice => choice.LevelRequirement <= playerLevel);
            int slotsAvailableForChoice = _playerInventory.PassiveStackableItemTrees.Slots.Count(slot => slot.Filter != SlotTypeFilter.None);

            if (choiceForThisLevel && slotsAvailableForChoice > 0)
            {
                startNodes = choiceForThisLevel
                    .GetOutputPort("Choices")
                    .GetConnections().Select(nodePort => (nodePort.node as ItemTreeGraphStartNode))
                    .ToList();
            }
            else if (numOpenTreeSlots == 0)
            {
                startNodes = _playerInventory.PassiveStackableItemTrees.Items;
            }
            else
            {
                var evaluatedChoices = this.nodes.OfType<ItemTreeGraphChoiceNode>()
                    .Where(node => node.Evaluated)
                    .Select(node => node.GetValue(node.GetOutputPort("Choices")) as ItemTreeGraphStartNode)
                    .Where(startNode => startNode != null)
                    .ToList();

                var otherStartNodes = this.nodes.OfType<ItemTreeGraphStartNode>()
                    .Where(node => !node.GetInputPort("Choices").IsConnected)
                    .ToList();

                startNodes = evaluatedChoices.Concat(otherStartNodes).ToList();
            }

            var nextAvailableTreeNodes = startNodes.SelectMany(startNode => GetAvailableItemsForStartNode(startNode, updateStatusInGraphWhileTraversing)); 
            unobtainedItemPool.AddRange(nextAvailableTreeNodes);
            
            return unobtainedItemPool;
        }
        
        private List<PlayerItem> GetAvailableItemsForStartNode(ItemTreeGraphStartNode startNode, bool updateStatusInGraphWhileTraversing = true)
        {
            var emptyList = new List<ItemTreeGraphNode>();
            Func<ItemTreeGraphNode, IEnumerable<ItemTreeGraphNode>> getChildren = node =>
            {
                bool playerHas = _playerInventory.PassiveStackableItems.Items.Contains(node.Item);
                if (updateStatusInGraphWhileTraversing)
                {
                    node.Obtained = playerHas;
                }
                if (playerHas)
                {
                    var children = node
                        .GetOutputPort("To")
                        .GetConnections()
                        .Select(port => port.node as ItemTreeGraphNode);
                    return children;
                }
                return emptyList;
            };
            return startNode
                .GetOutputPort("Start")
                .GetConnections()
                .SelectMany(port => BfsUtils.BreadthFirstSearch<ItemTreeGraphNode>(port.node as ItemTreeGraphNode, getChildren).leafNodes)
                .Where(node => !node.Obtained)
                .Select(node => node.Item)
                .ToList();
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
                if (itemTreeGraphNode.TreeStartNode == null)
                    continue;
                
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
            
            // if has choices, mark as evaluated and mark what choice was made
            var choicesPort = treeStartNode.GetInputPort("Choices");
            if (choicesPort.IsConnected)
            {
                foreach (var port in choicesPort.GetConnections())
                {
                    var startNode = (port.node as ItemTreeGraphChoiceNode);
                    startNode.MarkEvaluated(treeStartNode);
                }
            } 
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
        
        [Button]
        public void UpdateAllTreeStartNodes() 
        {
            var startNodes = nodes.OfType<ItemTreeGraphStartNode>();
            foreach (var itemTreeGraphStartNode in startNodes)
            {
                itemTreeGraphStartNode.PropagateUpdateToConnected();
            }
        }
        
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;

        public void ResetScriptableObject() 
        {
            Debug.Log("Resetting ItemTreeGraph");
            var resettableNodes = this.nodes.OfType<IResettableScriptableObject>();
            foreach (var node in resettableNodes)
            {
                node.ResetScriptableObject();
            }
            OnReset?.Invoke();
        }
    }
}