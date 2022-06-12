using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FlyingWormConsole3.FullSerializer;
using PlasticPipe.PlasticProtocol.Messages;

namespace BML.Scripts.Cave.DirectedGraph
{
    public class DirectedGraph<TNodeKey, TNodeData, TConnectionData>
    {
        private Dictionary<TNodeKey, Node> _nodes;
        private HashSet<NodeConnection> _nodeConnections;

        public Node? Start { get; private set; } = null;

        public DirectedGraph()
        {
            _nodes = new Dictionary<TNodeKey, Node>();
            _nodeConnections = new HashSet<NodeConnection>();
        }

        #region Public methods

        public IEnumerable<Node> GetAllNodesUnordered()
        {
            return _nodes.Values;
        }

        public bool AddNode(Node node)
        {
            if (!_nodes.ContainsKey(node.Key))
            {
                _nodes.Add(node.Key, node);
                return true;
            }
            return false;
        }

        public bool AddConnection(Node fromNode, Node toNode, TConnectionData connectionData)
        {
            var connection = new NodeConnection(fromNode, toNode, connectionData);
            if (!_nodeConnections.Contains(connection))
            {
                _nodeConnections.Add(connection);
                return true;
            }
            return false;
        }

        public bool SetStartNode(Node node)
        {
            if (Start != null)
            {
                return false;
            }

            if (!_nodes.ContainsKey(node.Key))
            {
                var tryAdd = AddNode(node);
                if (tryAdd)
                {
                    Start = node;
                    return true;
                }
            }
            else
            {
                Start = node;
                return true;
            }

            return false;
        }

        #endregion

        #region Class definitions
        
        public class Node
        {
            public TNodeKey Key { get; private set; }
            public List<NodeConnection> Connections { get; private set; }
            public TNodeData Data { get; private set; }
            
            public Node(TNodeKey key, List<NodeConnection> connections, TNodeData data)
            {
                Key = key;
                Connections = connections;
                Data = data;
            }
            public Node(TNodeKey key, TNodeData data) : this(key, new List<NodeConnection>(), data)
            {
                
            }

            public bool AddConnection(Node toNode, TConnectionData connectionData)
            {
                var connection = new NodeConnection(this, toNode, connectionData);
                Connections.Add(connection);
                return true;
            }
        }
        
        public class NodeConnection
        {
            public Node FromNode { get; private set; }
            public Node ToNode { get; private set; }
            public TConnectionData Data { get; private set; }

            public NodeConnection(Node fromNode, Node toNode, TConnectionData data)
            {
                FromNode = fromNode;
                ToNode = toNode;
                Data = data;
            }
        }
        
        #endregion
    }
}