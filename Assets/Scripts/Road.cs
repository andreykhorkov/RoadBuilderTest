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

            //for (int i = 0; i < nodes.Length; i++)
            //{
            //    var node = nodes[i];
            //    NodeBoundPoints prevNodeBoundPoints;

            //    if (i > 0)
            //    {
            //        var prevNode = nodes[i - 1];
            //        var tuple = new Tuple<Node, Node>(prevNode, node);

            //        if (node.BoundPoints.TryGetValue(tuple, out prevNodeBoundPoints))
            //        {
            //            prevNodeBoundPoints.LeftBoundPoint
            //        }
            //    }

            //    var nextNode = nodes[i + 1];

            //    Vector3 currentLeftBound;
            //    Vector3 currentRightBound;
            //    Vector3 nextLeftBound;
            //    Vector3 nextRightBound;

            //    if (node.BoundPoints.TryGetValue(this, out currentLeftBound) && nextNode.LeftBoundPoints.TryGetValue(this, out nextLeftBound))
            //    {
            //        Gizmos.DrawLine(currentLeftBound, nextLeftBound);
            //    }

            //    if (node.RightBoundPoints.TryGetValue(this, out currentRightBound) && nextNode.RightBoundPoints.TryGetValue(this, out nextRightBound))
            //    {
            //        Gizmos.DrawLine(currentRightBound, nextRightBound);
            //    }
            //}
        }
    }
}
