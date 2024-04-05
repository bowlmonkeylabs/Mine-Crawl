using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.Utilities;

namespace BML.Scripts.Utils
{
    public static class BfsUtils
    {
        public static (List<T> visitedNodes, List<T> leafNodes) BreadthFirstSearch<T>(T startNode, Func<T, IEnumerable<T>> getChildren)
        {
            var visited = new HashSet<T>();
            var leafNodes = new List<T>();
            var queue = new Queue<T>();
            queue.Enqueue(startNode);

            while (queue.Count > 0)
            {
                var node = queue.Dequeue();
                if (!visited.Contains(node))
                {
                    visited.Add(node);

                    var children = getChildren(node).ToList();
                    if (children.Count == 0)
                    {
                        leafNodes.Add(node);
                    }
                    else
                    {
                        foreach (var child in children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }

            return (visited.ToList(), leafNodes);
        }
    }
}