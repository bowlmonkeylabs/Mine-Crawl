﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XNode;

namespace BML.Scripts.Player.Items { 
    public class ItemTreeGraphNode : Node {
        [Input(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection From;
        [Output(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphConnection To;

        public bool Obtained = false;
        public PlayerItem Item;

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