using System;
using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class NodeBoundPoints
    {
        public Vector3 LeftBoundPoint { get; set; }
        public Vector3 RightBoundPoint { get; set; }

        public NodeBoundPoints(Vector3 leftBoundPoint, Vector3 rightBoundPoint)
        {
            LeftBoundPoint = leftBoundPoint;
            RightBoundPoint = rightBoundPoint;
        }
    }

    public class Node : MonoBehaviour
    {
        [SerializeField] private int id;

        public Dictionary<Tuple<Node, Node>, NodeBoundPoints> BoundPoints = new Dictionary<Tuple<Node, Node>, NodeBoundPoints>();
        public Vector3 Position => transform.position;
        public int Id => id;
    }
}
