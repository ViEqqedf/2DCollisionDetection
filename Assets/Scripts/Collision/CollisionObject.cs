using System;
using Physics.Collision.Shape;
using UnityEngine;
using Object = System.Object;

namespace Physics.Collision {
    [Flags]
    public enum CollisionFlags {
        Default = 0,
        StaticObject = 1,
        NoContactResponse = 2,
    }

    public class CollisionObject {
        private static int publicId = 1;

        public int id;
        public CollisionShape shape;
        public CollisionFlags flags = CollisionFlags.Default;
        public Object contextObject;
        public Vector3 position;
        public Vector3 rotation;
        public Vector3 scale;

        public CollisionObject(CollisionShape shape, Object contextObject) {
            this.id = publicId++;
            this.shape = shape;
            this.contextObject = contextObject;
        }

        public static bool IsSameCollisionObject(CollisionObject obj1, CollisionObject obj2) {
            return obj1.id == obj2.id;
        }

        public void InitCollisionObject() {
            shape.InitShape(this);
        }
    }
}