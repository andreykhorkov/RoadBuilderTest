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

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < nodes.Length - 1; i++)
            {
                var node = nodes[i];
                var nextNode = nodes[i + 1];

                Vector3 currentLeftBound;
                Vector3 currentRightBound;
                Vector3 nextLeftBound;
                Vector3 nextRightBound;

                if (node.LeftBound.TryGetValue(this, out currentLeftBound) && nextNode.LeftBound.TryGetValue(this, out nextLeftBound))
                {
                    Gizmos.DrawLine(currentLeftBound, nextLeftBound);
                }

                if (node.RightBound.TryGetValue(this, out currentRightBound) && nextNode.RightBound.TryGetValue(this, out nextRightBound))
                {
                    Gizmos.DrawLine(currentRightBound, nextRightBound);
                }
            }
        }
    }
}
