using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour
{
    [SerializeField] private RoadBuilder roadBuilder;

    private Vector3[] vectors;

    void Start()
    {
        vectors = new Vector3[200];

        for (int i = 0; i < vectors.Length; i++)
        {
            vectors[i] = Vector3.forward * i + Vector3.right * 0.1f * i;
        }

        var go = new GameObject();
        var meshFilter = go.AddComponent<MeshFilter>();
        go.AddComponent<MeshRenderer>();

        var mesh = RoadBuilder.CreateRoadMesh(vectors, 5);
        meshFilter.mesh = mesh;

    }

    void OnDrawGizmos()
    {
        if (ReferenceEquals(vectors, null))
        {
            return;
        }

        Gizmos.color = Color.red;

        for (int i = 0; i < vectors.Length; i++)
        {
            Gizmos.DrawSphere(vectors[i], 0.1f);
        }
    }
}
