using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BML.Scripts.Player.Items;
using Sirenix.OdinInspector;
using UnityEngine;
using XNode;

namespace BML.Scripts.ItemTreeGraph 
{
    //To represent connections in the item graph
    [Serializable]
    public class ItemGraphChoiceConnection
    {
        
    }
    
    public class ItemTreeGraphChoiceNode : Node
    {
        [Output(connectionType = ConnectionType.Multiple, typeConstraint = TypeConstraint.Strict)] public ItemGraphChoiceConnection Choices;

        public int LevelRequirement = 0;
        
        // this status is cached here at runtime for UI purposes, but this should not be referenced or relied upon for other purpoes; PlayerInventory is the source of truth.
        [ShowInInspector, NonSerialized, ReadOnly]
        public bool Evaluated = false;
        [ShowInInspector, NonSerialized, ReadOnly]
        public int? EvaluatedChoiceIndex = null;

        // Use this for initialization
        protected override void Init() 
        {
            base.Init();
        }

        public override void OnCreateConnection(NodePort from, NodePort to)
        {
            base.OnCreateConnection(from, to);
        }

        public override void OnRemoveConnection(NodePort port)
        {
            base.OnRemoveConnection(port);
        }

        public override object GetValue(NodePort port)
        {
            if (port.fieldName == "Choices" && Evaluated)
            {
                return port
                    .GetConnections()
                    .Skip(Mathf.Max(0, EvaluatedChoiceIndex.Value - 1))
                    .First();
            }

            return null;
        }
        
        public void MarkEvaluated(ItemTreeGraphStartNode startNode)
        {
            var choices = this.GetOutputPort("Choices").GetConnections().Select(port => port.node as ItemTreeGraphStartNode).ToList();
            var choiceIndex = choices.IndexOf(startNode);
            MarkEvaluated(choiceIndex);
        }
        
        private void MarkEvaluated(int choiceIndex)
        {
            Evaluated = true;
            EvaluatedChoiceIndex = choiceIndex;
        }
    }
}