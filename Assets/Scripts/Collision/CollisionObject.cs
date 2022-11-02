using System;
using Physics.Collision.Model;
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
        public void Rotate(Vector3 diff);
        public void RotateTo(Vector3 value);
        public void Scale(float diff);
        public void ScaleTo(float value);
        public void AddVelocity(Vector3 diff);
        public void AddAcceleration(Vector3 diff);
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
        public float scale = 1;
        public int level = 0;

        public Vector3 acceleration;
        public Vector3 velocity;
        public Vector3 resolveVelocity;

        // TODO: 添加一个脏标记

        public CollisionObject(CollisionShape shape, Object contextObject,
            Vector3 startPos, float startRotation = 0, int level = 0) {
            this.id = publicId++;
            this.shape = shape;
            this.position = startPos;
            this.nextPosition = startPos;
            this.rotation = new Vector3(0, startRotation, 0);
            this.contextObject = contextObject;
            this.level = level;
        }

        public static bool IsSameCollisionObject(CollisionObject obj1, CollisionObject obj2) {
            return obj1.id == obj2.id;
        }

        public void ApplyPosition() {
            position = nextPosition;
            shape.ApplyWorldVertices(position, rotation, scale);
        }

        public void ApplyRotation(Vector3 newRotation) {
            this.rotation = newRotation;
        }

        public void ApplyScale(float newScale) {
            this.scale = newScale;
        }

        public void InitCollisionObject() {
            shape.UpdateShape();

            // float totalX = 0;
            // float totalZ = 0;
            int count = shape.localVertices.Count;
            // for (int i = 0; i < count; i++) {
            //     totalX += shape.localVertices[i].x;
            //     totalZ += shape.localVertices[i].z;
            // }
            // Vector3 center = new Vector3(totalX / count, 0, totalZ / count);

            Vector3 origin = (shape.aabb.upperBound + shape.aabb.lowerBound) / 2;
            for (int i = 0; i < count; i++) {
                // shape.localVertices[i] += center;
                shape.localVertices[i] -= origin;
            }

            shape.UpdateShape();
            shape.ApplyWorldVertices(position, rotation, scale);
        }

        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType) {
            return new ProjectionPoint(this, projectionType);
        }

        public void Translate(Vector3 diff) {
            nextPosition += diff;
        }

        public void Rotate(Vector3 diff) {
            ApplyRotation(rotation + diff);
        }

        public void RotateTo(Vector3 value) {
            ApplyRotation(rotation);
        }

        public void Scale(float diff) {
            ApplyScale(scale + diff);
        }

        public void ScaleTo(float value) {
            ApplyScale(value);
        }

        public void AddVelocity(Vector3 diff) {
            this.velocity += diff;
        }

        public void AddAcceleration(Vector3 diff) {
            this.acceleration += diff;
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

        public Vector3 GetFarthestPointInDir(Vector3 dir) {
            float maxDis = float.MinValue;
            Vector3 farthestPoint = Vector3.zero;
            for (int i = 0, count = shape.vertices.Count; i < count; ++i) {
                Vector3 curPoint = shape.vertices[i];
                float dis = Vector3.Dot(curPoint, dir);
                if (dis > maxDis) {
                    maxDis = dis;
                    farthestPoint = curPoint;
                }
            }
            return farthestPoint;
        }
    }
}