using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.ScriptableObjectCore.Scripts.Managers;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace BML.Scripts.ItemTreeGraph { 
    public class ItemTreeGraphStartNode : Node, IResettableScriptableObject, IHasSlotType<SlotTypeFilter>
    {
        [Input(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphChoiceConnection Choices;
        [Output(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection Start;

        public SlotTypeFilter SlotTypeFilter => _slotTypeFilter; 
        [SerializeField]
        private SlotTypeFilter _slotTypeFilter = SlotTypeFilter.None;

        public PlayerItem FirstItemInTree => ((ItemTreeGraphNode)this.GetOutputPort("Start").GetConnections().FirstOrDefault()?.node)?.Item;

        [ShowInInspector, NonSerialized, ReadOnly]
        // this status is cached here at runtime for UI purposes, but this should not be referenced or relied upon for other purpoes; PlayerInventory is the source of truth.
        public bool Slotted = false;

        [ShowInInspector, NonSerialized, ReadOnly]
        public int NumberOfObtainedItemsInTree = 0;

        // Use this for initialization
        protected override void Init() 
        {
            base.Init();
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);

            if (from.node == this)
            {
                var itemTreeGraphTo = (ItemTreeGraphNode)to.node;
                itemTreeGraphTo.PropagateUpdateToConnected(this);
            }
        }

        public void PropagateUpdateToConnected()
        {
            foreach (var nodePort in this.GetOutputPort("Start").GetConnections())
            {
                (nodePort.node as ItemTreeGraphNode)?.PropagateUpdateToConnected(this);
            }
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            switch (port.fieldName)
            {
                case "Start":
                    if (Start == null) Start = new ItemGraphConnection();
                    Start.Item = null;
                    return Start;
                default:
                    return null;
            }
        }

        #region IResettableScriptableObject

        public void ResetScriptableObject()
        {
            Slotted = false;
        }

        public event IResettableScriptableObject.OnResetScriptableObject OnReset;
        
        #endregion
    }
}