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

    public interface ICollisionObject {
        public void InitCollisionObject();
        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType);
        public void Translate(Vector3 diff);
        public void AddVelocity(Vector3 diff);
    }

    public class CollisionObject : ICollisionObject{
        private static int publicId = 1;
        public int id;
        public CollisionShape shape;
        public CollisionFlags flags = CollisionFlags.Default;
        public Object contextObject;
        public Vector3 position;
        public Vector3 nextPosition;
        public Vector3 rotation;
        public Vector3 scale;
        public int level = 0;

        public Vector3 velocity;
        public Vector3 resolveVelocity;

        // TODO: 添加一个脏标记

        public CollisionObject(CollisionShape shape, Object contextObject, Vector3 startPos) {
            this.id = publicId++;
            this.shape = shape;
            this.position = startPos;
            this.nextPosition = startPos;
            this.contextObject = contextObject;
        }

        public CollisionObject(CollisionShape shape, Object contextObject,
            Vector3 startPos, int level) {
            this.id = publicId++;
            this.shape = shape;
            this.position = startPos;
            this.nextPosition = startPos;
            this.contextObject = contextObject;
            this.level = level;
        }

        public static bool IsSameCollisionObject(CollisionObject obj1, CollisionObject obj2) {
            return obj1.id == obj2.id;
        }

        public void ApplyPosition() {
            position = nextPosition;
            shape.ApplyWorldVertices(position);
        }

        public void InitCollisionObject() {
            shape.InitShape();
            shape.ApplyWorldVertices(position);
        }

        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType) {
            return new ProjectionPoint(this, projectionType);
        }

        public void Translate(Vector3 diff) {
            nextPosition += diff;
        }

        public void AddVelocity(Vector3 diff) {
            this.velocity += diff;
        }

        public void SetVelocity(Vector3 finalVelocity) {
            this.velocity = finalVelocity;
        }

        public void AddResolveVelocity(Vector3 diff) {
            this.resolveVelocity += diff;
        }

        public void CleanResolveVelocity() {
            this.resolveVelocity = Vector3.zero;
        }
    }
}