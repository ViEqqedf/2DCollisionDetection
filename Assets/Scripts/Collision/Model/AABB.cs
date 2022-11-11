using System;
using System.Collections.Generic;
using Unity.Burst;
using UnityEngine;

namespace CustomPhysics.Collision.Model {
    public enum AABBProjectionType {
        HorizontalStart,
        HorizontalEnd,
        VerticalStart,
        VerticalEnd,
    }

    [BurstCompile(CompileSynchronously = true)]
    public struct AABB {
        public Vector3 lowerBound;
        public Vector3 upperBound;

        public AABB(Vector3 lowerBound, Vector3 upperBound) {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
        }
    }
}