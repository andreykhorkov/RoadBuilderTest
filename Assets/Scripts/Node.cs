using System;
using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class NodeData
    {
        public Vector3 LeftBoundPoint { get; set; }
        public Vector3 RightBoundPoint { get; set; }
        public Vector3 IntersectionPerpendicularPoint { get; private set; }
        public Vector3 IntersectionPerpendicular { get; set; }
        public RoadSegment Segment { get; set; }

        public NodeData(Vector3 leftBoundPoint, Vector3 rightBoundPoint)
        {
            LeftBoundPoint = leftBoundPoint;
            RightBoundPoint = rightBoundPoint;
        }

        public void SetIntersectionPerpendicularPoint(Node outerPoint)
        {
            var leftDist = Vector3.SqrMagnitude(outerPoint.Position - LeftBoundPoint);
            var rightDist = Vector3.SqrMagnitude(outerPoint.Position - RightBoundPoint);

            IntersectionPerpendicularPoint = leftDist < rightDist ? LeftBoundPoint : RightBoundPoint;
        }
    }

    public class Node : MonoBehaviour
    {
        [SerializeField] private int id;

        public Dictionary<Tuple<Node, Node>, NodeData> NodeDataDict = new Dictionary<Tuple<Node, Node>, NodeData>();
        public Vector3 Position => transform.position;
        public int Id => id;
    }
}
