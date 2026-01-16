using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using XNode;

namespace BML.Scripts.ItemTreeGraph 
{ 
    public class ItemTreeGraphNode : Node, IResettableScriptableObject
    {
        [Input(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection From;
        [Output(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection To;

        [ShowInInspector, ReadOnly, Required]
        public ItemTreeGraphStartNode TreeStartNode;

        public bool Obtained => ObtainedCount > 0;
        public bool ObtainedMax => ObtainedLimit > 0 && ObtainedCount >= ObtainedLimit;

        [ShowInInspector, NonSerialized, ReadOnly]
        // this status is cached here at runtime for UI purposes, but this should not be referenced or relied upon for other purpoes; PlayerInventory is the source of truth.
        public int ObtainedCount = 0;
        
        public int ObtainedLimit = 1; // <=0 means no limit
        
        public PlayerItem Item;

        // Use this for initialization
        protected override void Init()
        {
            base.Init();
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);

            if (to.node is ItemTreeGraphNode && from.node is ItemTreeGraphNode)
            {
                var itemTreeGraphTo = (ItemTreeGraphNode)to.node;
                var itemTreeGraphFrom = (ItemTreeGraphNode)from.node;
                itemTreeGraphTo.PropagateUpdateToConnected(itemTreeGraphFrom.TreeStartNode);
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
#endif
            }
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);

            if (port.IsInput)
            {
                this.PropagateUpdateToConnected(null);
#if UNITY_EDITOR
                AssetDatabase.SaveAssets();
#endif
            }
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            switch (port.fieldName)
            {
                case "To":
                    if (To == null) To = new ItemGraphConnection();
                    To.Item = this.Item;
                    return To;
                default:
                    return null;
            }
        }

        public void PropagateUpdateToConnected(ItemTreeGraphStartNode treeStartNode)
        {
            TreeStartNode = treeStartNode;
            if (Item != null)
            {
                Item.PassiveStackableTreeStartNode = treeStartNode;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Item);
#endif
            }
            
            var nodesConnectedToOutputs = this.Outputs.Where(nodePort => nodePort.IsConnected)
                .SelectMany(nodePort => nodePort.GetConnections()).Select(connection => connection.node);
            foreach (var node in nodesConnectedToOutputs)
            {
                if (node != this && node is ItemTreeGraphNode)
                {
                    var itemTreeNode = (ItemTreeGraphNode)node;
                    itemTreeNode.PropagateUpdateToConnected(treeStartNode);
                }
            }
        }

        #region IResettableScriptableObject

        public void ResetScriptableObject()
        {
            ObtainedCount = 0;
        }

        // disable warning about unused event
        #pragma warning disable 67
        public event IResettableScriptableObject.OnResetScriptableObject OnReset;
        #pragma warning restore 67
        
        #endregion
        
    }
}