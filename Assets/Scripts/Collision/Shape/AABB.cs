using UnityEngine;

namespace Collision.Shape {
    public class AABB {
        private Vector3 lowerBound;
        private Vector3 upperBound;

        public AABB(Vector3 lowerBound, Vector3 upperBound) {
            ApplyShapeCurAttr(lowerBound, upperBound);
        }

        public void ApplyShapeCurAttr(Vector3 lower, Vector3 upper) {
            this.lowerBound.Set(lower.x, lower.y, lower.z);
            this.upperBound.Set(upper.x, upper.y, upper.z);
        }
    }
}