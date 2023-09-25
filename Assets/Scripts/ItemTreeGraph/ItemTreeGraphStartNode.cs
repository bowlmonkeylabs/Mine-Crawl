using System.Collections;
using System.Collections.Generic;
using BML.Scripts.Player.Items;
using UnityEngine;
using XNode;

namespace BML.Scripts.ItemTreeGraph { 
    public class ItemTreeGraphStartNode : Node {

        [Output(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection Start;

        public bool Slotted = false;

        // Use this for initialization
        protected override void Init() {
            base.Init();
        }

        // Return the correct value of an output port when requested
        public override object GetValue(NodePort port) {
            return null; // Replace this
        }
    }
}