using System;
using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class NodeData
    {
        public Node Node { get; }
        public Vector3 LeftBoundPoint { get; set; }
        public Vector3 RightBoundPoint { get; set; }
        public Vector3 IntersectionPerpendicularPoint { get; private set; }
        public Vector3 IntersectionPerpendicular { get; private set; }
        public RoadSegment Segment { get; set; }
        public Vector3 PerpendicularIntersectionPoint { get; set; }

        public NodeData(Vector3 leftBoundPoint, Vector3 rightBoundPoint, Node node)
        {
            LeftBoundPoint = leftBoundPoint;
            RightBoundPoint = rightBoundPoint;
            Node = node;
        }

        public void SetIntersectionPerpendicularPoint(Node outerPoint, List<Vector3> intersectionPoints)
        {
            var leftDist = Vector3.SqrMagnitude(outerPoint.Position - LeftBoundPoint);
            var rightDist = Vector3.SqrMagnitude(outerPoint.Position - RightBoundPoint);

            IntersectionPerpendicularPoint = leftDist < rightDist ? LeftBoundPoint : RightBoundPoint;
            IntersectionPerpendicular = Vector3.Cross(Segment.PointA.Position - Segment.PointB.Position, Vector3.down);
            var boundToIntersect = leftDist < rightDist ? Segment.RightBound : Segment.LeftBound;

            PerpendicularIntersectionPoint = Intersection.LineLineIntersection(IntersectionPerpendicularPoint,
                    IntersectionPerpendicularPoint + IntersectionPerpendicular, boundToIntersect.BorderPointA, boundToIntersect.BorderPointB);

            intersectionPoints.Add(PerpendicularIntersectionPoint);

            if (leftDist < rightDist)
            {
                RightBoundPoint = PerpendicularIntersectionPoint;
            }
            else
            {
                LeftBoundPoint = PerpendicularIntersectionPoint;
            }
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
