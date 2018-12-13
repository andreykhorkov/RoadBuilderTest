using System;
using UnityEditor;
using UnityEngine;

namespace experimental
{
    [ExecuteInEditMode, CanEditMultipleObjects]
    public class Road : MonoBehaviour
    {
        [SerializeField] private float width;
        [SerializeField] private Node[] nodes;
        [SerializeField] private int id;

        public int Id => id;
        public float Width => width;
        public Node[] Nodes => nodes;

        void OnDrawGizmos()
        {
            return;

            Gizmos.color = Color.cyan;

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                NodeData prevNodeData;
                NodeData currentNodeData;
                NodeData nextNodeData;

                if (i > 0)
                {
                    var prevNode = nodes[i - 1];
                    var tuple = new Tuple<Node, Node>(prevNode, node);

                    if (prevNode.NodeDataDict.TryGetValue(tuple, out prevNodeData) && node.NodeDataDict.TryGetValue(tuple, out currentNodeData))
                    {
                        Gizmos.DrawLine(prevNodeData.LeftBoundPoint, currentNodeData.LeftBoundPoint);
                        Gizmos.DrawLine(prevNodeData.RightBoundPoint, currentNodeData.RightBoundPoint);
                    }
                }

                if (i < nodes.Length - 1)
                {
                    var nextNode = nodes[i + 1];

                    var tuple = new Tuple<Node, Node>(nextNode, node);

                    if (node.NodeDataDict.TryGetValue(tuple, out nextNodeData) && nextNode.NodeDataDict.TryGetValue(tuple, out currentNodeData))
                    {
                        Gizmos.DrawLine(currentNodeData.LeftBoundPoint, nextNodeData.LeftBoundPoint);
                        Gizmos.DrawLine(currentNodeData.RightBoundPoint, nextNodeData.RightBoundPoint);
                    }
                }
            }
        }
    }
}
