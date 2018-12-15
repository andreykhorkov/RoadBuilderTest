using System;
using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class RoadSegment
    {
        public class Bound
        {
            public Vector3 BorderPointA { get; }
            public Vector3 BorderPointB { get; }
            public List<Vector3> BoundIntersectionPoints { get; } = new List<Vector3>();

            public Bound(Vector3 borderPointA, Vector3 borderPointB)
            {
                BorderPointA = borderPointA;
                BorderPointB = borderPointB;
            }
        }

        public Node PointA { get; }
        public Node PointB { get; }
        public Bound LeftBound { get; }
        public Bound RightBound { get; }

        public RoadSegment(Node pointA, Node pointB)
        {
            PointA = pointA;
            PointB = pointB;
            var tuple = new Tuple<Node, Node>(pointA, pointB);
            LeftBound = new Bound(pointA.NodeDataDict[tuple].LeftBoundPoint, pointB.NodeDataDict[tuple].LeftBoundPoint);
            RightBound = new Bound(pointA.NodeDataDict[tuple].RightBoundPoint, pointB.NodeDataDict[tuple].RightBoundPoint);
        }
    }

    public class Intersection
    {
        public Node Node { get; }
        public List<Vector3> IntersectionBoundPoints { get; set; } = new List<Vector3>();
        public List<RoadSegment.Bound> LeftBounds { get; set; } = new List<RoadSegment.Bound>();
        public List<RoadSegment.Bound> RightBounds { get; set; } = new List<RoadSegment.Bound>();

        public Intersection(Node node)
        {
            Node = node;

            foreach (var boundPointsKeyValueA in node.NodeDataDict)
            {
                foreach (var boundPointsKeyValueB in node.NodeDataDict)
                {
                    if (boundPointsKeyValueA.Value == boundPointsKeyValueB.Value)
                    {
                        continue;
                    }

                    SetIntersectionPoint(boundPointsKeyValueA.Value.Segment.LeftBound, boundPointsKeyValueB.Value.Segment.RightBound);
                    SetIntersectionPoint(boundPointsKeyValueA.Value.Segment.LeftBound, boundPointsKeyValueB.Value.Segment.LeftBound);
                    SetIntersectionPoint(boundPointsKeyValueA.Value.Segment.RightBound, boundPointsKeyValueB.Value.Segment.RightBound);
                }
            }

            foreach (var boundPointsValue in node.NodeDataDict.Values)
            {
                FilterSegmentIntersectionPoints(boundPointsValue.Segment);
            }
        }

        private void FilterSegmentIntersectionPoints(RoadSegment segment)
        {
            var outerNode = segment.PointA == Node ? segment.PointB : segment.PointA;
            var minSqrDist = float.MaxValue;
            var tuple = new Tuple<Node, Node>(segment.PointA, segment.PointB);

            if (segment.LeftBound.BoundIntersectionPoints.Count > 0)
            {
                var leftIntersectionBoundPos = Vector3.zero;

                foreach (var point in segment.LeftBound.BoundIntersectionPoints)
                {
                    var nodeData = outerNode.NodeDataDict[tuple];

                    var sqrDist = Vector3.SqrMagnitude(nodeData.LeftBoundPoint - point);

                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        leftIntersectionBoundPos = point;
                    }
                }

                Node.NodeDataDict[tuple].LeftBoundPoint = leftIntersectionBoundPos;
            }

            if (segment.RightBound.BoundIntersectionPoints.Count > 0)
            {
                minSqrDist = float.MaxValue;
                var rightIntersectionBoundPos = Vector3.zero;

                foreach (var point in segment.RightBound.BoundIntersectionPoints)
                {
                    var nodeData = outerNode.NodeDataDict[tuple];

                    var sqrDist = Vector3.SqrMagnitude(nodeData.RightBoundPoint - point);

                    if (sqrDist < minSqrDist)
                    {
                        minSqrDist = sqrDist;
                        rightIntersectionBoundPos = point;
                    }
                }

                Node.NodeDataDict[tuple].RightBoundPoint = rightIntersectionBoundPos;
            }
        }

        private static void SetIntersectionPoint(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var point = LineLineIntersection(boundA, boundB);

            if (CheckPoint(point, boundA) && CheckPoint(point, boundB))
            {
                boundA.BoundIntersectionPoints.Add(point);
                boundB.BoundIntersectionPoints.Add(point);
            }
        }

        public void FindIntersectionBoundPoints()
        {
            var outerNodes = new List<Node>();

            foreach (var nodeData in Node.NodeDataDict)
            {
                var outerNode = nodeData.Key.Item1 == Node ? nodeData.Key.Item2 : nodeData.Key.Item1;
                outerNodes.Add(outerNode);
            }

            outerNodes.Sort(new ClockwiseNodeComparer(Node));
            LeftBounds.Clear();
            RightBounds.Clear();

            foreach (var outerNode in outerNodes)
            {
                var tuple = new Tuple<Node, Node>(outerNode, Node);

                NodeData nodeData;

                if (!Node.NodeDataDict.TryGetValue(tuple, out nodeData))
                {
                    tuple = new Tuple<Node, Node>(Node, outerNode);

                    if (!Node.NodeDataDict.TryGetValue(tuple, out nodeData))
                    {
                        Debug.Log($"there is no nodaData for given nodes: {Node}: {outerNode}");
                        return;
                    }
                }

                IntersectionBoundPoints.Add(Node.NodeDataDict[tuple].LeftBoundPoint);
                IntersectionBoundPoints.Add(Node.NodeDataDict[tuple].RightBoundPoint);

                var seg = nodeData.Segment;
                LeftBounds.Add(CheckBoundDirection(outerNode, seg.LeftBound, isLeftBound: true) ? seg.LeftBound : seg.RightBound); 
                RightBounds.Add(CheckBoundDirection(outerNode, seg.RightBound, isLeftBound: false) ? seg.RightBound : seg.LeftBound); 
            }

            if (LeftBounds.Count != RightBounds.Count)
            {
                Debug.LogError($"amount of left bound and right bound for the intersection {Node.Id} does not match");
                return;
            }

            for (int i = 0; i < LeftBounds.Count - 1; i++)
            {
                IntersectionBoundPoints.Add(LineLineIntersection(LeftBounds[i], RightBounds[i + 1]));
            }

            IntersectionBoundPoints.Add(LineLineIntersection(RightBounds[0], LeftBounds[RightBounds.Count - 1]));
        }

        private bool CheckBoundDirection(Node outerNode, RoadSegment.Bound bound, bool isLeftBound)
        {
            var distA = Vector3.SqrMagnitude(outerNode.Position - bound.BorderPointA);
            var distB = Vector3.SqrMagnitude(outerNode.Position - bound.BorderPointB);
            var outerNodeSideBound = distA < distB ? bound.BorderPointA : bound.BorderPointB;

            var dir = outerNodeSideBound - outerNode.Position;
            var dotProd = Vector3.Dot(Vector3.up, Vector3.Cross(dir, Node.Position - outerNode.Position));
            var isAligned = dotProd * (isLeftBound ? 1 : -1) > 0;

            return isAligned;
        }

        public static bool CheckPoint(Vector3 pos, RoadSegment.Bound bound)
        {
            var dirSegment = bound.BorderPointB - bound.BorderPointA;
            var dirToPos = pos - bound.BorderPointA;
            var segmentMagn = dirSegment.magnitude;
            var segmentPosMagn = dirToPos.magnitude;

            var isPosOnSegment = segmentPosMagn / segmentMagn < 1 && Vector3.Dot(dirSegment, dirToPos) > 0;

            return isPosOnSegment;
        }

        public static Vector3 LineLineIntersection(Vector3 A, Vector3 B, Vector3 C, Vector3 D)
        {
            // Line AB represented as a1x + b1z = c1 
            var a1 = B.z - A.z;
            var b1 = A.x - B.x;
            var c1 = a1 * A.x + b1 * A.z;

            // Line CD represented as a2x + b2z = c2 
            var a2 = D.z - C.z;
            var b2 = C.x - D.x;
            var c2 = a2 * C.x + b2 * C.z;

            var determinant = a1 * b2 - a2 * b1;

            if (Mathf.Approximately(determinant, 0))
            {
                // The lines are parallel. 
                Debug.Log("parallel");
                return new Vector3(float.MaxValue, float.MaxValue);
            }

            var x = (b2 * c1 - b1 * c2) / determinant;
            var z = (a1 * c2 - a2 * c1) / determinant;
            return new Vector3(x, 0, z);
        }

        public static Vector3 LineLineIntersection(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var A = boundA.BorderPointA;
            var B = boundA.BorderPointB;
            var C = boundB.BorderPointA;
            var D = boundB.BorderPointB;

            return LineLineIntersection(A, B, C, D);
        }
    }
}
