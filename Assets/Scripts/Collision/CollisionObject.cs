using System;
using Collision.Shape;
using UnityEngine;
using Object = System.Object;

namespace Collision {
    [Flags]
    public enum CollisionFlags {
        Default = 0,
        StaticObject = 1,
        NoContactResponse = 2,
    }

    public class ProjectionPoint {
        public CollisionObject collisionObject;
        public bool isStartPoint;
        public float value;
    }

    public class CollisionObject {
        public int id;
        public CollisionShape shape;
        public CollisionFlags flags = CollisionFlags.Default;
        public Object battleObject;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public CollisionObject(CollisionShape shape, Object battleObject) {
            this.shape = shape;
            this.battleObject = battleObject;
        }

        public static bool IsSameCollisionObject(CollisionObject obj1, CollisionObject obj2) {
            return obj1.id == obj2.id;
        }

        public void InitCollisionObject() {
            GetAABB(out Vector3 lowerBound, out Vector3 upperBound);
            shape.InitShape(lowerBound, upperBound);
        }

        public virtual void GetAABB(out Vector3 lowerBound, out Vector3 upperBound) {
            lowerBound = Vector3.zero;
            upperBound = Vector3.zero;
        }
    }
}