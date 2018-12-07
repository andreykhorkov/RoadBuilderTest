using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class RoadSegment
    {
        public Vector3 PointA { get; }
        public Vector3 PointB { get; }
        public bool HasIntersectionPoint { get; set; }
        public List<Vector3> SegmentIntersectionPoints { get; } = new List<Vector3>();
        public Road Road { get; }

        public RoadSegment(Vector3 pointA, Vector3 pointB, Road road)
        {
            PointA = pointA;
            PointB = pointB;
            Road = road;
        }
    }

    public class Intersection
    {
        public List<RoadSegment> LeftSegments;
        public List<RoadSegment> RightSegments;
        public List<Vector3> IntersectionPoints;
        public Node Node { get; set; }

        public Intersection(List<RoadSegment> leftSegments, List<RoadSegment> rightSegments, Node node)
        {
            LeftSegments = leftSegments;
            RightSegments = rightSegments;
            Node = node;

            IntersectionPoints = new List<Vector3>();

            for (int i = 0; i < leftSegments.Count; i++)
            {
                for (int j = 0; j < leftSegments.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    SetIntersectionPoint(leftSegments[i], rightSegments[j]); 
                    SetIntersectionPoint(leftSegments[i], leftSegments[j]); 
                    SetIntersectionPoint(rightSegments[i], rightSegments[j]);
                }
            }

            for (int i = 0; i < leftSegments.Count; i++)
            {
                for (int j = 0; j < leftSegments.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    SetIntersectionPoint2(leftSegments[i], rightSegments[j]);
                    SetIntersectionPoint2(leftSegments[i], leftSegments[j]);
                    SetIntersectionPoint2(rightSegments[i], rightSegments[j]);
                }
            }

            FilterSegmentsIntersectionPoints(leftSegments, true);
            FilterSegmentsIntersectionPoints(rightSegments, false);
        }

        private void FilterSegmentsIntersectionPoints(List<RoadSegment> segments, bool isLeft)
        {
            foreach (var segment in segments)
            {
                if (segment.SegmentIntersectionPoints.Count == 0)
                {
                    continue;
                }

                var furthestSqrMag = float.MinValue;
                var furthestPoint = Vector3.zero;

                foreach (var point in segment.SegmentIntersectionPoints)
                {
                    var sqrDist = Vector3.SqrMagnitude(Node.Position - point);

                    if (sqrDist > furthestSqrMag)
                    {
                        furthestSqrMag = sqrDist;
                        furthestPoint = point;
                    }
                }

                if (isLeft)
                {
                    Node.LeftBound[segment.Road] = furthestPoint;
                }
                else
                {
                    Node.RightBound[segment.Road] = furthestPoint;
                }

                IntersectionPoints.Add(furthestPoint);
            }
        }

        private void SetIntersectionPoint(RoadSegment segmentA, RoadSegment segmentB)
        {
            var point = LineLineIntersection(segmentA, segmentB);

            if (CheckPoint(point, segmentA) && CheckPoint(point, segmentB))
            {
                segmentA.SegmentIntersectionPoints.Add(point);
                segmentB.SegmentIntersectionPoints.Add(point);
                segmentA.HasIntersectionPoint = true;
                segmentB.HasIntersectionPoint = true;
            }
        }

        private void SetIntersectionPoint2(RoadSegment segmentA, RoadSegment segmentB)
        {
            var point = LineLineIntersection(segmentA, segmentB);

            if (!segmentA.HasIntersectionPoint && !segmentB.HasIntersectionPoint && !CheckPoint(point, segmentA) && !CheckPoint(point, segmentB))
            {
                IntersectionPoints.Add(point);
            }
        }

        public static bool CheckPoint(Vector3 pos, RoadSegment segment)
        {
            var dirSegment = segment.PointB - segment.PointA;
            var dirToPos = pos - segment.PointA;
            var segmentMagn = dirSegment.magnitude;
            var segmentPosMagn = dirToPos.magnitude;

            var isPosOnSegment = segmentPosMagn / segmentMagn < 1 && Vector3.Dot(dirSegment, dirToPos) > 0;

            return isPosOnSegment;
        }

        private static Vector3 LineLineIntersection(RoadSegment segmentA, RoadSegment segmentB)
        {
            var A = segmentA.PointA;
            var B = segmentA.PointB;
            var C = segmentB.PointA;
            var D = segmentB.PointB;

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
    }
}
