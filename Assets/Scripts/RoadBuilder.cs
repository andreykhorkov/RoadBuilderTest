using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace experimental
{
    public class ClockwiseComparer : IComparer<Vector3>
    {
        public Vector3 origin { get; set; }

        public ClockwiseComparer(Vector3 origin)
        {
            this.origin = origin;
        }

        public int Compare(Vector3 first, Vector3 second)
        {
            return IsClockwise(first, second, origin);
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
        public Dictionary<Node, Intersection> Intersections = new Dictionary<Node, Intersection>();

        void Start()
        {
            BuildRoads();
            foreach (var road in roads)
            {
                
            }

            CreateRoadMesh(roads[0], roads[0].name);
        }

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

                        if (prevIndex >= 0)
                        {
                            roadSegmentsList.Add(new RoadSegment(road.Nodes[prevIndex], road.Nodes[nodeIndex]));
                        }

                        if (nextIndex <= road.Nodes.Length - 1)
                        {
                            roadSegmentsList.Add(new RoadSegment(road.Nodes[nodeIndex], road.Nodes[nextIndex]));
                        }
                    }
                    
                    Intersections.Add(node, new Intersection(roadSegmentsList, node));
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

        private void FindStandalonePoints()
        {
            foreach (var intersection in Intersections.Values)
            {
                intersection.IntersectionPoints.Sort(new ClockwiseComparer(intersection.Node.Position));
            }
        }

        void OnDrawGizmos()
        {
            BuildRoads();

            foreach (var intersection in Intersections.Values)
            {
                Gizmos.color = Color.yellow; 
                Gizmos.DrawSphere(intersection.Node.Position, 0.2f);

                foreach (var point in intersection.IntersectionPoints)
                {
                    Gizmos.DrawSphere(point, 0.5f);
                }

                Gizmos.color = Color.cyan;

                foreach (var boundPoints in intersection.Node.BoundPoints.Values)
                {
                    Gizmos.DrawSphere(boundPoints.LeftBoundPoint, 0.5f);
                    Gizmos.DrawSphere(boundPoints.RightBoundPoint, 0.5f);
                }
            }

            DrawIntersectionBounds();
        }

        private void DrawIntersectionBounds()
        {
            Gizmos.color = Color.red;

            foreach (var intersection in Intersections.Values)
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
                var pointA = nodes[i].Position;
                var node = nodes[i];
                Node prevNode = null;
                Node nextNode = null;

                if (i > 0)
                {
                    prevNode = nodes[i - 1];
                }

                if (i < nodes.Length - 1)
                {
                    nextNode = nodes[i + 1];
                    var pointB = nextNode.Position;
                    var direction = pointB - pointA;
                    perp = Vector3.Cross(direction, Vector3.up).normalized;
                }

                var avg = (prevPerp + perp).normalized;
                var cos = Vector3.Dot(avg, perp);
                var leftBoundPosition = pointA + avg * road.Width * 0.5f / cos;
                var rightBoundPosition = pointA - avg * road.Width * 0.5f / cos;

                //Gizmos.color = Color.green;
                //Gizmos.DrawLine(leftBoundPosition, rightBoundPosition);

                if (prevNode != null)
                {
                    node.BoundPoints[new Tuple<Node, Node>(prevNode, node)] = new NodeBoundPoints(leftBoundPosition, rightBoundPosition);
                }

                if (nextNode != null)
                {
                    node.BoundPoints[new Tuple<Node, Node>(node, nextNode)] = new NodeBoundPoints(leftBoundPosition, rightBoundPosition);
                }

                prevPerp = perp;
            }


            //for (int i = 0; i < nodes.Length; i++)
            //{
            //    Gizmos.color = Color.red;

            //    foreach (var point in nodes[i].BoundPoints)
            //    {
            //        Gizmos.DrawSphere(point.Value.LeftBoundPoint, 0.3f);
            //        Gizmos.DrawSphere(point.Value.RightBoundPoint, 0.3f);
            //    }
            //}
        }

        private void CreateRoadMesh(Road road, string name)
        {
            if (road.Nodes.Length < 2)
            {
                Debug.Log($"{road.name} has less than 2 nodes");
                return;
            }

            var go = new GameObject(name);
            go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();

            var vertices = new Vector3[road.Nodes.Length * 2];
            var triangles = new int[3 * 2 * (road.Nodes.Length - 1)];
            var uvs = new Vector2[vertices.Length];
            var vertexIndex = 0;
            var triIndex = 0;

            for (int nodeIndex = 0; nodeIndex < road.Nodes.Length; nodeIndex++)
            {
                var node = road.Nodes[nodeIndex];

                if (Intersections.ContainsKey(node))
                {
                    Debug.Log(node);
                }


                //var node = road.Nodes[nodeIndex];
                //var prevIndex = nodeIndex - 1;
                //var nextIndex = nodeIndex + 1;
                //Tuple<Node, Node> tupleAB = null;
                //Tuple<Node, Node> tupleBC = null;

                //if (prevIndex >= 0)
                //{
                //    var prevNode = road.Nodes[prevIndex];
                //    tupleAB = new Tuple<Node, Node>(prevNode, node);
                //}

                //if (nextIndex <= road.Nodes.Length - 1)
                //{
                //    var nextNode = road.Nodes[nextIndex];
                //    tupleBC = new Tuple<Node, Node>(node, nextNode);
                //}

                //if (tupleAB != null)
                //{
                //    Test(node, tupleAB, vertices, ref vertexIndex, nodeIndex, road, uvs, triangles, ref triIndex);
                //}

                //if (tupleBC != null)
                //{
                //    Test(node, tupleBC, vertices, ref vertexIndex, nodeIndex, road, uvs, triangles, ref triIndex);
                //}
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            mf.mesh = mesh;
        }

        private static bool Test(Node node, Tuple<Node, Node> tuple, Vector3[] vertices, ref int vertexIndex, int nodeIndex, Road road, Vector2[] uvs, int[] triangles, ref int triIndex)
        {
            NodeBoundPoints boundPoints;

            if (!node.BoundPoints.TryGetValue(tuple, out boundPoints))
            {
                Debug.Log($"can't find bound points for segment {tuple.Item1}:{tuple.Item2}");
                return false;
            }

            vertices[vertexIndex] = boundPoints.LeftBoundPoint;
            vertices[vertexIndex + 1] = boundPoints.RightBoundPoint;

            var completionPercent = nodeIndex / (float)(road.Nodes.Length - 1);
            var v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertexIndex] = new Vector2(0, v);
            uvs[vertexIndex + 1] = new Vector2(1, v);

            if (nodeIndex < road.Nodes.Length - 1)
            {
                triangles[triIndex] = vertexIndex;
                triangles[triIndex + 1] = vertexIndex + 2;
                triangles[triIndex + 2] = vertexIndex + 1;
                triangles[triIndex + 3] = vertexIndex + 2;
                triangles[triIndex + 4] = vertexIndex + 3;
                triangles[triIndex + 5] = vertexIndex + 1;
            }

            triIndex += 6;
            vertexIndex += 2;

            return true;
        }
    }
}
