using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Shape {
    public enum AABBProjectionType {
        HorizontalStart,
        HorizontalEnd,
        VerticalStart,
        VerticalEnd,
    }

    public class AABB {
        public Vector3 lowerBound { get; private set; }
        public Vector3 upperBound { get; private set; }

        public AABB(Vector3 lowerBound, Vector3 upperBound) {
            this.lowerBound = new Vector3();
            this.upperBound = new Vector3();
            Apply(lowerBound, upperBound);
        }

        public void Apply(Vector3 lower, Vector3 upper) {
            this.lowerBound = new Vector3(lower.x, lower.y, lower.z);
            this.upperBound = new Vector3(upper.x, upper.y, upper.z);
        }
    }
}