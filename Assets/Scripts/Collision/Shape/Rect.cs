using UnityEngine;

namespace Collision.Shape {
    public class Rect : CollisionShape {
        public Rect(ShapeType shapeType) : base(shapeType) {
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            lowerBound = Vector3.zero;
            upperBound = Vector3.zero;
        }
    }
}