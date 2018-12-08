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
            public bool HasIntersectionPoint { get; set; }
            public List<Vector3> BoundIntersectionPoints { get; } = new List<Vector3>();

            public Bound(Vector3 borderPointA, Vector3 borderPointB)
            {
                BorderPointA = borderPointA;
                BorderPointB = borderPointB;
            }
        }

        public Node PointA { get; }
        public Node PointB { get; }
        public Road Road { get; }
        public Bound LeftBound { get; }
        public Bound RightBound { get; }

        public RoadSegment(Node pointA, Node pointB, Road road)
        {
            PointA = pointA;
            PointB = pointB;
            Road = road;
            LeftBound = new Bound(pointA.LeftBound[road], pointB.LeftBound[road]);
            RightBound = new Bound(pointA.RightBound[road], pointB.RightBound[road]);
        }
    }

    public class Intersection
    {
        public List<RoadSegment> RoadSegments;
        public List<Vector3> IntersectionPoints;
        public Node Node { get; set; }

        public Intersection(List<RoadSegment> roadSegments, Node node)
        {
            RoadSegments = roadSegments;
            Node = node;

            IntersectionPoints = new List<Vector3>();

            for (int i = 0; i < roadSegments.Count; i++)
            {
                for (int j = 0; j < roadSegments.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var roadSegmentI = roadSegments[i];
                    var roadSegmentJ = roadSegments[j];

                    SetIntersectionPoint(roadSegmentI.LeftBound, roadSegmentJ.RightBound); 
                    SetIntersectionPoint(roadSegmentI.LeftBound, roadSegmentJ.LeftBound); 
                    SetIntersectionPoint(roadSegmentI.RightBound, roadSegmentJ.RightBound);
                }
            }

            for (int i = 0; i < roadSegments.Count; i++)
            {
                for (int j = 0; j < roadSegments.Count; j++)
                {
                    if (i == j)
                    {
                        continue;
                    }

                    var roadSegmentI = roadSegments[i];
                    var roadSegmentJ = roadSegments[j];

                    SetIntersectionPoint2(roadSegmentI.LeftBound, roadSegmentJ.RightBound);
                    SetIntersectionPoint2(roadSegmentI.LeftBound, roadSegmentJ.LeftBound);
                    SetIntersectionPoint2(roadSegmentI.RightBound, roadSegmentJ.RightBound);
                }
            }

            FilterSegmentsIntersectionPoints(roadSegments);
        }

        private void FilterSegmentsIntersectionPoints(List<RoadSegment> segments)
        {
            //foreach (var segment in segments)
            //{
            //    if (Mathf.Approximately(Vector3.SqrMagnitude(segment.PointA.Position - Node.Position), 0))
            //    {

            //    }


            //    foreach (var point in segment.SegmentIntersectionPoints)
            //    {
            //        IntersectionPoints.Add(point);
            //    }

            //    continue;




            //    if (segment.SegmentIntersectionPoints.Count == 0)
            //    {
            //        continue;
            //    }

            //    var furthestSqrMag = float.MinValue;
            //    var furthestPoint = Vector3.zero;

            //    foreach (var point in segment.SegmentIntersectionPoints)
            //    {
            //        var sqrDist = Vector3.SqrMagnitude(Node.Position - point);

            //        if (sqrDist > furthestSqrMag)
            //        {
            //            furthestSqrMag = sqrDist;
            //            furthestPoint = point;
            //        }
            //    }

            //    if (isLeft)
            //    {
            //        Node.LeftBound[segment.Road] = furthestPoint;
            //    }
            //    else
            //    {
            //        Node.RightBound[segment.Road] = furthestPoint;
            //    }

            //    IntersectionPoints.Add(furthestPoint);
            //}
        }

        private void SetIntersectionPoint(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var point = LineLineIntersection(boundA, boundB);

            if (CheckPoint(point, boundA) && CheckPoint(point, boundB))
            {
                boundA.BoundIntersectionPoints.Add(point);
                boundB.BoundIntersectionPoints.Add(point);
                boundA.HasIntersectionPoint = true;
                boundB.HasIntersectionPoint = true;
            }
        }

        private void SetIntersectionPoint2(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var point = LineLineIntersection(boundA, boundB);

            if (!boundA.HasIntersectionPoint && !boundB.HasIntersectionPoint && !CheckPoint(point, boundA) && !CheckPoint(point, boundB))
            {
                IntersectionPoints.Add(point);
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

        private static Vector3 LineLineIntersection(RoadSegment.Bound boundA, RoadSegment.Bound boundB)
        {
            var A = boundA.BorderPointA;
            var B = boundA.BorderPointB;
            var C = boundB.BorderPointA;
            var D = boundB.BorderPointB;

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
