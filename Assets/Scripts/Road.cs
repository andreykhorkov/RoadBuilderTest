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
            Gizmos.color = Color.cyan;

            for (int i = 0; i < nodes.Length; i++)
            {
                var node = nodes[i];
                NodeBoundPoints prevNodeBoundPoints;
                NodeBoundPoints currentNodeBoundPoints;
                NodeBoundPoints nextNodeBoundPoints;

                if (i > 0)
                {
                    var prevNode = nodes[i - 1];
                    var tuple = new Tuple<Node, Node>(prevNode, node);

                    if (prevNode.BoundPoints.TryGetValue(tuple, out prevNodeBoundPoints) && node.BoundPoints.TryGetValue(tuple, out currentNodeBoundPoints))
                    {
                        Gizmos.DrawLine(prevNodeBoundPoints.LeftBoundPoint, currentNodeBoundPoints.LeftBoundPoint);
                        Gizmos.DrawLine(prevNodeBoundPoints.RightBoundPoint, currentNodeBoundPoints.RightBoundPoint);
                    }
                }

                if (i < nodes.Length - 1)
                {
                    var nextNode = nodes[i + 1];

                    var tuple = new Tuple<Node, Node>(nextNode, node);

                    if (node.BoundPoints.TryGetValue(tuple, out nextNodeBoundPoints) && nextNode.BoundPoints.TryGetValue(tuple, out currentNodeBoundPoints))
                    {
                        Gizmos.DrawLine(currentNodeBoundPoints.LeftBoundPoint, nextNodeBoundPoints.LeftBoundPoint);
                        Gizmos.DrawLine(currentNodeBoundPoints.RightBoundPoint, nextNodeBoundPoints.RightBoundPoint);
                    }
                }
            }
        }
    }
}
