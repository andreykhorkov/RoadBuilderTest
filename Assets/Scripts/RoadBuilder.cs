﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace experimental
{
    public class ClockwiseComparer : IComparer<Vector3>
    {
        public Vector3 Origin { get; set; }

        public ClockwiseComparer(Vector3 origin)
        {
            Origin = origin;
        }

        public int Compare(Vector3 first, Vector3 second)
        {
            return IsClockwise(first, second, Origin);
        }

        public static int IsClockwise(Vector3 first, Vector3 second, Vector3 origin)
        {
            if (first == second)
                return 0;

            var firstOffset = first - origin;
            var secondOffset = second - origin;

            var angle1 = Mathf.Atan2(firstOffset.x, firstOffset.z);
            var angle2 = Mathf.Atan2(secondOffset.x, secondOffset.z);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            return firstOffset.sqrMagnitude < secondOffset.sqrMagnitude ? -1 : 1;
        }
    }

    public class ClockwiseNodeComparer : IComparer<Node>
    {
        public Node Origin { get; set; }

        public ClockwiseNodeComparer(Node origin)
        {
            Origin = origin;
        }

        public int Compare(Node first, Node second)
        {
            return IsClockwise(first, second, Origin);
        }

        public static int IsClockwise(Node first, Node second, Node origin)
        {
            if (first == second)
                return 0;

            var firstOffset = first.Position - origin.Position;
            var secondOffset = second.Position - origin.Position;

            var angle1 = Mathf.Atan2(firstOffset.x, firstOffset.z);
            var angle2 = Mathf.Atan2(secondOffset.x, secondOffset.z);

            if (angle1 < angle2)
                return -1;

            if (angle1 > angle2)
                return 1;

            return firstOffset.sqrMagnitude < secondOffset.sqrMagnitude ? -1 : 1;
        }
    }

    public class RoadBuilder : MonoBehaviour
    {
        [SerializeField] private Road[] roads;
        [SerializeField] private float intersectionPadding; 

        public Dictionary<Node, List<KeyValuePair<int, Road>>> CheckedNodes = new Dictionary<Node, List<KeyValuePair<int, Road>>>();
        public Dictionary<Node, Intersection> Intersections = new Dictionary<Node, Intersection>();

        void Start()
        {
            BuildRoads();
            CreateRoadMeshes();

            foreach (var intersection in Intersections.Values)
            {
                CreateIntersectionMesh(intersection);
            }
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
                    for (int i = 0; i < roadList.Count; i++)
                    {
                        var indexRoadPair = roadList[i];
                        var road = indexRoadPair.Value;
                        var nodeIndex = indexRoadPair.Key;
                        var prevIndex = nodeIndex - 1;
                        var nextIndex = nodeIndex + 1;

                        if (prevIndex >= 0)
                        {
                            var tuple = new Tuple<Node, Node>(road.Nodes[prevIndex], node);
                            node.NodeDataDict[tuple].SetRoadSegment();
                        }

                        if (nextIndex <= road.Nodes.Length - 1)
                        {
                            var tuple = new Tuple<Node, Node>(node, road.Nodes[nextIndex]);
                            node.NodeDataDict[tuple].SetRoadSegment();
                        }
                    }
                    
                    Intersections.Add(node, new Intersection(node, intersectionPadding));
                }
            }
        }

        private void BuildRoads()
        {
            SetRoadBounds();
            FindIntersections();
            SetPerpendiculars();
            FindIntersectionsBoundPoints();
        }

        private void CreateRoadMeshes()
        {
            foreach (var road in roads)
            {
                CreateRoadMesh(road, road.name);
            }
        }

        private void SetPerpendiculars()
        {
            foreach (var intersection in Intersections)
            {
                foreach (var nodeData in intersection.Value.Node.NodeDataDict)
                {
                    var outerPoint = intersection.Value.Node == nodeData.Key.Item1
                        ? nodeData.Key.Item2
                        : nodeData.Key.Item1;

                    nodeData.Value.SetIntersectionPerpendicularPoint(outerPoint, intersection.Value.IntersectionBoundPoints);
                }
            }
        }

        private void FindIntersectionsBoundPoints()
        {
            foreach (var intersection in Intersections)
            {
                intersection.Value.FindIntersectionBoundPoints();
            }
        }

        private void SetRoadBounds()
        {
            for (int i = 0; i < roads.Length; i++)
            {
                var road = roads[i];
                SetBounds(road);
            }
        }

        void OnDrawGizmos()
        {
            BuildRoads();
            DrawRoadSegments();
            DrawIntersectionBoundPoints();
            DrawIntersectionBounds();
        }

        private void DrawIntersectionBoundPoints()
        {
            Gizmos.color = Color.black;

            foreach (var intersection in Intersections)
            {
                foreach (var point in intersection.Value.IntersectionBoundPoints)
                {
                    Gizmos.DrawSphere(point, 0.1f);
                }
            }
        }

        private void DrawRoadSegments()
        {
            foreach (var road in roads)
            {
                Node prevNode = null;

                foreach (var roadNode in road.Nodes)
                {
                    foreach (var nodeData in roadNode.NodeDataDict)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawSphere(nodeData.Value.Node.Position, 0.3f);

                        Gizmos.color = Color.blue;
                        Gizmos.DrawSphere(nodeData.Value.LeftBoundPoint, 0.3f);

                        Gizmos.color = Color.red;
                        Gizmos.DrawSphere(nodeData.Value.RightBoundPoint, 0.3f);

                        Gizmos.color = Color.cyan;
                        Gizmos.DrawLine(nodeData.Key.Item1.Position, nodeData.Key.Item2.Position);

                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(nodeData.Value.LeftBoundPoint, nodeData.Value.RightBoundPoint);
                    }

                    if (prevNode != null)
                    {
                        var tuple = new Tuple<Node, Node>(prevNode, roadNode);
                        var prevRoadData = prevNode.NodeDataDict[tuple];
                        var currentNodeData = roadNode.NodeDataDict[tuple];

                        Gizmos.color = Color.blue;
                        Gizmos.DrawLine(prevRoadData.LeftBoundPoint, currentNodeData.LeftBoundPoint);
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(prevRoadData.RightBoundPoint, currentNodeData.RightBoundPoint);
                    }

                    prevNode = roadNode;
                }
            }
        }

        private void DrawIntersectionBounds()
        {
            Gizmos.color = Color.yellow;

            foreach (var intersection in Intersections.Values)
            {
                for (int i = 0; i < intersection.IntersectionBoundPoints.Count - 1; i++)
                {
                    var point = intersection.IntersectionBoundPoints[i];
                    var nextPoint = intersection.IntersectionBoundPoints[i + 1];

                    Gizmos.DrawLine(point, nextPoint);
                }

                if(intersection.IntersectionBoundPoints.Count > 0)   
                    Gizmos.DrawLine(intersection.IntersectionBoundPoints[0], intersection.IntersectionBoundPoints[intersection.IntersectionBoundPoints.Count - 1]);
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
                Tuple<Node, Node> tuple;

                if (prevNode != null)
                {
                    tuple = new Tuple<Node, Node>(prevNode, node);
                    node.NodeDataDict[tuple] = new NodeData(leftBoundPosition, rightBoundPosition, node, tuple);
                }

                if (nextNode != null)
                {
                    tuple = new Tuple<Node, Node>(node, nextNode);
                    node.NodeDataDict[tuple] = new NodeData(leftBoundPosition, rightBoundPosition, node, tuple);
                }

                prevPerp = perp;
            }
        }

        private List<List<Node>> GetSubRoads(Road road)
        {
            var subRoads = new List<List<Node>>();
            var subRoad = new List<Node>();

            for (int i = 0; i < road.Nodes.Length; i++)
            {
                var node = road.Nodes[i];
                subRoad.Add(node);

                if (Intersections.ContainsKey(node))
                {
                    subRoads.Add(subRoad);
                    subRoad = new List<Node> { node };
                }
            }

            subRoads.Add(subRoad);

            return subRoads;
        }

        private void CreateRoadMesh(Road road, string subRoadName)
        {
            if (road.Nodes.Length < 2)
            {
                Debug.Log($"{road.name} has less than 2 nodes");
                return;
            }

            var subRoads = GetSubRoads(road);

            for (int i = 0; i < subRoads.Count; i++)
            {
                if (subRoads[i].Count < 2)
                {
                    continue;
                }

                CreateSubRoadMesh(subRoads[i], $"{subRoadName}_{i}");
            } 
        }

        private static void CreateSubRoadMesh(IReadOnlyList<Node> subRoad, string name)
        {
            var go = new GameObject(name);
            go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();

            var vertices = new Vector3[subRoad.Count * 2];
            var triangles = new int[3 * 2 * (subRoad.Count - 1)];
            var uvs = new Vector2[vertices.Length];
            var vertexIndex = 0;
            var triIndex = 0;
            var node = subRoad[0];
            var nextNode = subRoad[1];
            var tuple = new Tuple<Node, Node>(node, nextNode);

            for (int nodeIndex = 0; nodeIndex < subRoad.Count; nodeIndex++)
            {
                node = subRoad[nodeIndex];

                if (nodeIndex < subRoad.Count - 1)
                {
                    nextNode = subRoad[nodeIndex + 1];
                    tuple = new Tuple<Node, Node>(node, nextNode);
                }

                NodeData nodeData;

                if (!node.NodeDataDict.TryGetValue(tuple, out nodeData))
                {
                    Debug.Log($"can't find bound points for segment {tuple.Item1}:{tuple.Item2}");
                    return;
                }

                vertices[vertexIndex] = nodeData.LeftBoundPoint;
                vertices[vertexIndex + 1] = nodeData.RightBoundPoint;

                var completionPercent = nodeIndex / (float)(subRoad.Count - 1);
                var v = 1 - Mathf.Abs(2 * completionPercent - 1);
                uvs[vertexIndex] = new Vector2(0, v);
                uvs[vertexIndex + 1] = new Vector2(1, v);

                if (nodeIndex < subRoad.Count - 1)
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
            }

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            mf.mesh = mesh;
        }

        public struct Projections
        {
            public Vector3 XProj { get; }
            public Vector3 YProj { get; }

            public Projections(Vector3 xProj, Vector3 yProj)
            {
                XProj = xProj;
                YProj = yProj;
            }
        }

        public void CreateIntersectionMesh(Intersection intersection)
        {
            var vertices = new Vector3[intersection.IntersectionBoundPoints.Count + 1];
            var triangles = new int[3 * intersection.IntersectionBoundPoints.Count];
            var vertexIndex = 1;
            var triIndex = 0;

            vertices[0] = intersection.Node.Position;

            for (int i = 0; i < intersection.IntersectionBoundPoints.Count; i++)
            {
                var point = intersection.IntersectionBoundPoints[i];
                vertices[vertexIndex] = point;

                if (i < intersection.IntersectionBoundPoints.Count - 1)
                {
                    triangles[triIndex] = vertexIndex;
                    triangles[triIndex + 1] = vertexIndex + 1;
                    triangles[triIndex + 2] = 0;

                    triIndex += 3;
                    vertexIndex += 1;
                }
            }

            triangles[triIndex] = intersection.IntersectionBoundPoints.Count;
            triangles[triIndex + 1] = 1;
            triangles[triIndex + 2] = 0;

            var uvs = GetUVs(intersection);

            var go = new GameObject($"Intersection: {intersection.Node.Id}");
            var mr = go.AddComponent<MeshRenderer>();
            var mf = go.AddComponent<MeshFilter>();

            var mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs
            };

            mf.mesh = mesh;

            var mat = Resources.Load<Material>("Lane");
            mr.material = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
        }

        private static Vector2[] GetUVs(Intersection intersection)
        {
            var maxX = float.MinValue;
            var minX = float.MaxValue;

            var maxZ = float.MinValue;
            var minZ = float.MaxValue;

            for (int i = 0; i < intersection.IntersectionBoundPoints.Count; i++)
            {
                var point = intersection.IntersectionBoundPoints[i] - intersection.Node.Position;

                if (point.x > maxX)
                {
                    maxX = point.x;
                }
                else if (point.x < minX)
                {
                    minX = point.x;
                }

                if (point.z > maxZ)
                {
                    maxZ = point.z;
                }
                else if (point.z < maxZ)
                {
                    minZ = point.z;
                }
            }

            var lowerLeft = new Vector3(minX, 0, minZ);
            var topRight = new Vector3(maxX, 0, maxZ);
            var width = Mathf.Abs(maxX) + Mathf.Abs(minX);
            var height = Mathf.Abs(maxZ) + Mathf.Abs(minZ);
            var uvs = new Vector2[intersection.IntersectionBoundPoints.Count + 1];

            for (int i = 0; i < intersection.IntersectionBoundPoints.Count; i++)
            {
                var pointRelativeToLowerLeft = intersection.IntersectionBoundPoints[i] - intersection.Node.Position - lowerLeft;
                uvs[i] = new Vector2(pointRelativeToLowerLeft.x / width, pointRelativeToLowerLeft.z / height);
            }

            //uvs[uvs.Length - 1] = new Vector2(intersection.Node.Position.x / width, intersection.Node.Position.z / height);
            //uvs[0] = new Vector2(lowerLeft.x / width, lowerLeft.z / height);

            return uvs;
        }
    }
}
