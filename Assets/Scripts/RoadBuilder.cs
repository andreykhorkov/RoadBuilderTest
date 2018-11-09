using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder : MonoBehaviour
{
    public static Mesh CreateRoadMesh(Vector3[] anchors, float width = 2)
    {
        var mesh = new Mesh();
        var vertices = new Vector3[anchors.Length * 2];
        var triangles = new int[2 * (vertices.Length - 1)];


        return mesh;
    }
}
