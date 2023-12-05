using System;
using Pathfinding;
using UnityEngine;

namespace BML.Scripts.Pathfinding
{
    public class BMLAIPath : AIPath
    {
        public bool IsPathPossible { get; protected set; }

        protected override void Update()
        {
            base.Update();
            UpdateIsPathPossible();
        }

        private void UpdateIsPathPossible()
        {
            GraphNode node1;
            GraphNode node2;

            try
            {
                node1 = AstarPath.active.GetNearest(transform.position, NNConstraint.Default).node;
                node2 = AstarPath.active.GetNearest(destination, NNConstraint.Default).node;
                
                IsPathPossible =  PathUtilities.IsPathPossible(node1, node2);
            }
            catch (NullReferenceException e)
            {
                IsPathPossible = false;
            }
        }
    }
}