using System.Collections.Generic;
using UnityEngine;

namespace experimental
{
    public class Node : MonoBehaviour
    {
        [SerializeField] private int id;

        public Dictionary<Road, Vector3> LeftBound { get; set; } = new Dictionary<Road, Vector3>();
        public Dictionary<Road, Vector3> RightBound { get; set; } = new Dictionary<Road, Vector3>();
        public Vector3 Position => transform.position;
        public int Id => id;
    }
}
