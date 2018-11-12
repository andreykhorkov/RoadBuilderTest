using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadBuilder : MonoBehaviour
{
    public static Mesh CreateRoadMesh(Vector3[] anchors, float width = 2)
    {
        
        var vertices = new Vector3[anchors.Length * 2];
        var triangles = new int[3 * 2 * (anchors.Length - 1)];
        var uvs = new Vector2[vertices.Length];
        var vertexIndex = 0;
        var triIndex = 0;
        

        for (int anchorIndex = 0; anchorIndex < anchors.Length; anchorIndex++)
        { 
            var direction = Vector3.zero;

            if (anchorIndex < anchors.Length - 1)
            {
                direction += anchors[anchorIndex + 1] - anchors[anchorIndex];
            }

            if (anchorIndex > 0)
            {
                direction += anchors[anchorIndex] - anchors[anchorIndex - 1];
            }

            direction.Normalize();

            var left = new Vector3(-direction.z, 0, direction.x);

            vertices[vertexIndex] = anchors[anchorIndex] + left * 0.5f * width;
            vertices[vertexIndex + 1] = anchors[anchorIndex] - left * 0.5f * width;

            var completionPercent = anchorIndex / (float) (anchors.Length - 1);
            var v = 1 - Mathf.Abs(2 * completionPercent - 1);
            uvs[vertexIndex] = new Vector2(0, v);
            uvs[vertexIndex + 1] = new Vector2(1, v);

            if (anchorIndex < anchors.Length - 1)
            {
                triangles[triIndex] = vertexIndex;
                triangles[triIndex + 1] = vertexIndex + 2;
                triangles[triIndex + 2] = vertexIndex + 1;
                triangles[triIndex + 3] = vertexIndex + 1;
                triangles[triIndex + 4] = vertexIndex + 2;
                triangles[triIndex + 5] = vertexIndex + 3;
            }

            vertexIndex += 2;
            triIndex += 6;
        }

        var mesh = new Mesh
        {
            vertices = vertices,
            triangles = triangles,
            uv = uvs
        };
        
        return mesh;
    }
}
