using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Shape {
    public enum ShapeType {
        Undefined,
        Rect,
        Circle,
        Custom,
    }

    public abstract class CollisionShape {
        public ShapeType shapeType { get; private set; }
        public List<Vector3> vertices { get; private set; }
        public AABB aabb { get; protected set; }

        public CollisionShape(ShapeType shapeType) {
            this.shapeType = shapeType;
        }

        public virtual void InitShape(CollisionObject collisionObject) {
            GetBound(out Vector3 lowerBound, out Vector3 upperBound);
            this.aabb = new AABB(collisionObject, lowerBound, upperBound);
            this.vertices = new List<Vector3>();
        }

        protected abstract void GetBound(out Vector3 lowerBound, out Vector3 upperBound);
    }
}