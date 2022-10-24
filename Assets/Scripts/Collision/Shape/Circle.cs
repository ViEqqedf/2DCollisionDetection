using UnityEngine;

namespace Physics.Collision.Shape {
    public class Circle : CollisionShape {
        public Circle(ShapeType shapeType) : base(shapeType) {
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            lowerBound = Vector3.zero;
            upperBound = Vector3.zero;
        }
    }
}