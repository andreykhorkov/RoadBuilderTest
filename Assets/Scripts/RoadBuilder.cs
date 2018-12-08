using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class ClockwiseComparer : IComparer<Vector3>
    {
        private Vector3 m_Origin;
        public Vector3 origin { get { return m_Origin; } set { m_Origin = value; } }

        public ClockwiseComparer(Vector3 origin)
        {
            m_Origin = origin;
        }

        public int Compare(Vector3 first, Vector3 second)
        {
            return IsClockwise(first, second, m_Origin);
        }

        public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
        {
            if (first == second)
                return 0;

            Vector3 firstOffset = first - origin;
            Vector3 secondOffset = second - origin;

            float angle1 = Mathf.Atan2(firstOffset.x, firstOffset.z);
            float angle2 = Mathf.Atan2(secondOffset.x, secondOffset.z);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            // Check to see which point is closest
            return (firstOffset.sqrMagnitude < secondOffset.sqrMagnitude) ? -1 : 1;
        }
    }

    public class RoadBuilder : MonoBehaviour
    {
        [SerializeField] private Road[] roads;

        public Dictionary<Node, List<KeyValuePair<int, Road>>> CheckedNodes = new Dictionary<Node, List<KeyValuePair<int, Road>>>();
        public List<Intersection> Intersections = new List<Intersection>();

        private void FindIntersections()
        {
            CheckedNodes.Clear();
            Intersections.Clear();

            for (int i = 0; i < roads.Length; i++)
            {
                var road = roads[i];

                for (int j = 0; j < road.Nodes.Length; j++)
                {
                    var node = road.Nodes[j];
                    List<KeyValuePair<int, Road>> roadsList;

                    if (!CheckedNodes.TryGetValue(node, out roadsList))
                    {
                        CheckedNodes.Add(node, new List<KeyValuePair<int, Road>>{new KeyValuePair<int, Road>(j, road)});
                        continue;
                    }

                    roadsList.Add(new KeyValuePair<int, Road>(j, road));
                }
            }

            foreach (var pair in CheckedNodes)
            {
                var roadList = pair.Value;
                var node = pair.Key;

                if (roadList.Count <= 1)
                {
                    continue;
                }

                if (roadList.Count > 1)
                {
                    var roadSegmentsList = new List<RoadSegment>();

                    for (int i = 0; i < roadList.Count; i++)
                    {
                        var indexRoadPair = roadList[i];
                        var road = indexRoadPair.Value;
                        var nodeIndex = indexRoadPair.Key;
                        var prevIndex = nodeIndex - 1;
                        var nextIndex = nodeIndex + 1;
                        Vector3 currentNodeLeftPos;
                        Vector3 currentNodeRightPos;

                        if (!road.Nodes[nodeIndex].LeftBound.TryGetValue(road, out currentNodeLeftPos)
                            || !road.Nodes[nodeIndex].RightBound.TryGetValue(road, out currentNodeRightPos))
                        {
                            continue;
                        }

                        if (prevIndex >= 0)
                        {
                            var prevNodeLeftPos = road.Nodes[prevIndex].LeftBound[road];
                            var prevNodeRightPos = road.Nodes[prevIndex].RightBound[road];

                            roadSegmentsList.Add(new RoadSegment(road.Nodes[prevIndex], road.Nodes[nodeIndex], road));
                        }

                        if (nextIndex <= road.Nodes.Length - 1)
                        {
                            Vector3 nextNodeLeftPos;
                            Vector3 nextNodeRightPos;

                            if (!road.Nodes[nextIndex].LeftBound.TryGetValue(road, out nextNodeLeftPos)
                                || !road.Nodes[nextIndex].RightBound.TryGetValue(road, out nextNodeRightPos))
                            {
                                continue;
                            }

                            roadSegmentsList.Add(new RoadSegment(road.Nodes[nextIndex], road.Nodes[nodeIndex], road));
                        }
                    }
                    
                    Intersections.Add(new Intersection(roadSegmentsList, node));
                }
            }
        }

        private void BuildRoads()
        {
            SetRoadBounds();
            FindIntersections();
            FindStandalonePoints();
        }

        private void SetRoadBounds()
        {
            for (int i = 0; i < roads.Length; i++)
            {
                var road = roads[i];
                SetBounds(road);
            }
        }

        private static void FindStlnPoint(IEnumerable<RoadSegment> segments, Intersection intersection)
        {
            foreach (var seg in segments)
            {
                if (seg.LeftBound.BoundIntersectionPoints.Count > 0)
                {
                    continue;
                }

                var point = (seg.LeftBound.BorderPointA - intersection.Node.Position).sqrMagnitude <
                            (seg.LeftBound.BorderPointB - intersection.Node.Position).sqrMagnitude
                    ? seg.PointA
                    : seg.PointB;

                intersection.IntersectionPoints.Add(point.Position);
            }
        }

        private void FindStandalonePoints()
        {
            foreach (var intersection in Intersections)
            {
                FindStlnPoint(intersection.RoadSegments, intersection);

                intersection.IntersectionPoints.Sort(new ClockwiseComparer(intersection.Node.Position));
            }
        }

        void OnDrawGizmos()
        {
            BuildRoads();

            foreach (var intersection in Intersections)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawSphere(intersection.Node.Position, 0.5f);
                Gizmos.color = Color.yellow;

                foreach (var point in intersection.IntersectionPoints)
                {
                    Gizmos.DrawSphere(point, 0.5f);
                }

                foreach (var pos in intersection.Node.LeftBound.Values)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(pos, 0.5f);
                }

                foreach (var pos in intersection.Node.RightBound.Values)
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(pos, 0.5f);
                }
            }

            DrawIntersectionBounds();
        }

        private void DrawIntersectionBounds()
        {
            Gizmos.color = Color.red;

            foreach (var intersection in Intersections)
            {
                for (int i = 0; i < intersection.IntersectionPoints.Count - 1; i++)
                {
                    var point = intersection.IntersectionPoints[i];
                    var nextPoint = intersection.IntersectionPoints[i + 1];

                    Gizmos.DrawLine(point, nextPoint);
                }

                if(intersection.IntersectionPoints.Count - 1 > 0)   
                    Gizmos.DrawLine(intersection.IntersectionPoints[0], intersection.IntersectionPoints[intersection.IntersectionPoints.Count - 1]);
            }
        }

        private static void SetBounds(Road road)
        {
            var nodes = road.Nodes;
            var perp = Vector3.zero;
            var prevPerp = perp;

            for (int i = 0; i < nodes.Length; i++)
            {
                Gizmos.color = Color.blue;

                var pointA = nodes[i].Position;
                var node = nodes[i];

                if (i < nodes.Length - 1)
                {

                    var pointB = nodes[i + 1].Position;
                    var direction = pointB - pointA;
                    perp = Vector3.Cross(direction, Vector3.up).normalized;

                    Gizmos.color = Color.magenta;
                    Gizmos.DrawLine(pointA, pointB);
                }

                var avg = (prevPerp + perp).normalized;
                var cos = Vector3.Dot(avg, perp);
                node.LeftBound[road] = pointA + avg * road.Width * 0.5f / cos;
                node.RightBound[road] = pointA - avg * road.Width * 0.5f / cos;
                Gizmos.DrawSphere(node.LeftBound[road], 0.25f);
                Gizmos.DrawSphere(node.RightBound[road], 0.25f);
                
                prevPerp = perp;

                var prevLeftBound = node.LeftBound[road];
                var prevRightBound = node.RightBound[road];

                Gizmos.color = Color.cyan;
                Gizmos.DrawSphere(prevLeftBound, 0.3f);
                Gizmos.color = Color.magenta;
                Gizmos.DrawSphere(prevRightBound, 0.3f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(node.LeftBound[road], node.RightBound[road]);
            }
        }
    }
}
