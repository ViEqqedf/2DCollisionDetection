using UnityEngine;

namespace Physics.Collision {
    public class CollisionPair {
        public CollisionObject first;
        public CollisionObject second;
        public Vector3 penetrationVec;
    }
}