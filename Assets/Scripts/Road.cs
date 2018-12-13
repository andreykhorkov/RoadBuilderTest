using System;
using UnityEditor;
using UnityEngine;

namespace experimental
{
    [ExecuteInEditMode, CanEditMultipleObjects]
    public class Road : MonoBehaviour
    {
        [SerializeField] private float width;
        [SerializeField] private Node[] nodes;
        [SerializeField] private int id;

        public int Id => id;
        public float Width => width;
        public Node[] Nodes => nodes;
    }
}
