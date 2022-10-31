using UnityEngine;

namespace Physics.Collision.Model {
    public class Edge {
        public SupportPoint a;
        public SupportPoint b;
        public Vector3 normal;
        public float distance;
        public int index;
    }
}