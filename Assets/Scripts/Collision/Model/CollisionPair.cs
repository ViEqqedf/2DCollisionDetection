using Unity.Burst;
using UnityEngine;

namespace CustomPhysics.Collision.Model {
    [BurstCompile(CompileSynchronously = true)]
    public struct CollisionPair {
        public CollisionObject first;
        public CollisionObject second;
        public Vector3 penetrateVec;
    }
}