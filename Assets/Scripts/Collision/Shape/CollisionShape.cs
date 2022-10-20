using UnityEngine;

namespace Collision.Shape {
    public enum ShapeType {
        Undefined,
        Rect,
        Circle,
        Custom,
    }

    public abstract class CollisionShape {
        public ShapeType shapeType { get; private set; }
        public AABB aabb { get; protected set; }

        public CollisionShape(ShapeType shapeType) {
            this.shapeType = shapeType;
        }

        public void InitShape(Vector3 lowerBound, Vector3 upperBound) {
            this.aabb = new AABB(lowerBound, upperBound);
        }
    }
}