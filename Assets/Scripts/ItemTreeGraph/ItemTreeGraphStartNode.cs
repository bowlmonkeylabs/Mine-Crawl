using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace BML.Scripts.ItemTreeGraph { 
    public class ItemTreeGraphStartNode : Node {

        [Output(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection Start;

        public PlayerItem FirstItemInTree => ((ItemTreeGraphNode)this.GetValue(this.GetOutputPort("Start"))).Item;
        
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

            var itemTreeGraphTo = (ItemTreeGraphNode)to.node;
            itemTreeGraphTo.PropagateUpdateToConnected(this);
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port)
        {
            return port.Connection?.node;
        }
    }
}