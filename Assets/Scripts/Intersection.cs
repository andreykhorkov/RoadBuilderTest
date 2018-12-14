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
        public List<Vector3> IntersectionPoints;
        public Node Node { get; set; }

        public Intersection(Node node)
        {
            Node = node;

            IntersectionPoints = new List<Vector3>();

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

            foreach (var boundPointsKeyValueA in node.NodeDataDict)
            {
                foreach (var boundPointsKeyValueB in node.NodeDataDict)
                {
                    if (boundPointsKeyValueA.Value == boundPointsKeyValueB.Value)
                    {
                        continue;
                    }

                    SetIntersectionPoint2(boundPointsKeyValueA.Value.Segment.LeftBound, boundPointsKeyValueB.Value.Segment.RightBound);
                    SetIntersectionPoint2(boundPointsKeyValueA.Value.Segment.LeftBound, boundPointsKeyValueB.Value.Segment.LeftBound);
                    SetIntersectionPoint2(boundPointsKeyValueA.Value.Segment.RightBound, boundPointsKeyValueB.Value.Segment.RightBound);
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
                IntersectionPoints.Add(leftIntersectionBoundPos);
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
                IntersectionPoints.Add(rightIntersectionBoundPos);
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

        private void SetIntersectionPoint2(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var point = LineLineIntersection(boundA, boundB);

            if (boundA.BoundIntersectionPoints.Count == 0 && boundB.BoundIntersectionPoints.Count == 0 && !CheckPoint(point, boundA) && !CheckPoint(point, boundB))
            {
                IntersectionPoints.Add(point);

                //foreach (var boundPoints in Node.NodeDataDict.Values)
                //{
                //    if (!Mathf.Approximately(Vector3.SqrMagnitude(boundPoints.LeftBoundPoint - point), 0))
                //    {
                //        boundPoints.LeftBoundPoint = point; //changing bound points according to intersection points
                //    }

                //    if (!Mathf.Approximately(Vector3.SqrMagnitude(boundPoints.RightBoundPoint - point), 0))
                //    {
                //        boundPoints.RightBoundPoint = point;
                //    }
                //}
            }
        }

        public List<Vector3> GizmosPoints { get; set; } = new  List<Vector3>();

        public void Temp()
        {
            foreach (var boundPointsKeyValueA in Node.NodeDataDict)
            {
                foreach (var boundPointsKeyValueB in Node.NodeDataDict)
                {
                    if (boundPointsKeyValueA.Value == boundPointsKeyValueB.Value)
                    {
                        continue;
                    }

                    FindIntersectionPoints(boundPointsKeyValueA.Value.Segment.LeftBound, boundPointsKeyValueB.Value.Segment.RightBound);
                    FindIntersectionPoints(boundPointsKeyValueA.Value.Segment.LeftBound, boundPointsKeyValueB.Value.Segment.LeftBound);
                    FindIntersectionPoints(boundPointsKeyValueA.Value.Segment.RightBound, boundPointsKeyValueB.Value.Segment.RightBound);
                }
            }
        }

        private void FindIntersectionPoints(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var point = LineLineIntersection(boundA, boundB);

            //if (CheckPoint(point, boundA) && CheckPoint(point, boundB))
            {
                GizmosPoints.Add(point);
            }
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
