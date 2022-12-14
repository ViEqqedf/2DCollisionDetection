using System;
using System.Collections.Generic;
using CustomPhysics.Collision.Model;
using CustomPhysics.Collision.Shape;
using UnityEngine;
using Object = System.Object;

namespace CustomPhysics.Collision {
    [Flags]
    public enum CollisionFlags {
        Default = 1 << 0,
        StaticObject = 1 << 1,
        NoContactResponse = 1 << 2,
    }

    public interface ICollisionObject {
        public void InitCollisionObject();
        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType);
        public bool HasFlag(CollisionFlags flag);
        public void AddFlag(CollisionFlags flag);
        public void RemoveFlag(CollisionFlags flag);
        public Vector3 GetCurPosition();
        public float GetCurRotation();
        public void Translate(Vector3 diff);
        public void TranslateTo(Vector3 value);
        public void Rotate(Vector3 diff);
        public void RotateTo(Vector3 value);
        public void Scale(float diff);
        public void ScaleTo(float value);
        public Vector3 GetActiveVelocity();
        public void SetInputMoveVelocity(Vector3 value);
        public void AddExternalVelocity(Vector3 diff);
        public void SetExternalVelocity(Vector3 value);
        public void AddAcceleration(Acceleration accelerationInfo);
        public void RemoveAcceleration(Acceleration accelerationInfo);
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

        public List<Acceleration> accelerations;
        public Vector3 velocity;
        public Vector3 inputMoveVelocity;
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
            accelerations = new List<Acceleration>();
        }

        public static bool IsSameCollisionObject(CollisionObject obj1, CollisionObject obj2) {
            return obj1.id == obj2.id;
        }

        #region Interface

        public void InitCollisionObject() {
            shape.UpdateShape();

            int count = shape.localVertices.Count;
            Vector3 origin = (shape.aabb.upperBound + shape.aabb.lowerBound) / 2;
            for (int i = 0; i < count; i++) {
                shape.localVertices[i] -= origin;
            }

            shape.UpdateShape();
            shape.ApplyWorldVertices(position, rotation, scale);
        }

        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType) {
            return new ProjectionPoint(this, projectionType);
        }

        public bool HasFlag(CollisionFlags flag) {
            return flags.HasFlag(flag);
        }

        public void AddFlag(CollisionFlags flag) {
            this.flags |= flag;
        }

        public void RemoveFlag(CollisionFlags flag) {
            this.flags &= ~flag;
        }

        public Vector3 GetCurPosition() {
            return position;
        }

        public float GetCurRotation() {
            return rotation.y;
        }

        public void Translate(Vector3 diff) {
            nextPosition += diff;
        }

        public void TranslateTo(Vector3 value) {
            nextPosition = value;
        }

        public void Rotate(Vector3 diff) {
            ApplyRotation(rotation + diff);
        }

        public void RotateTo(Vector3 value) {
            ApplyRotation(value);
        }

        public void Scale(float diff) {
            ApplyScale(scale + diff);
        }

        public void ScaleTo(float value) {
            ApplyScale(value);
        }

        public Vector3 GetActiveVelocity() {
            Vector3 result = velocity + inputMoveVelocity;
            for (int i = 0, count = accelerations.Count; i < count; i++) {
                result += accelerations[i].curVelocity;
            }

            return result;
        }

        public void AddExternalVelocity(Vector3 diff) {
            this.velocity += diff;
        }

        public void AddAcceleration(Acceleration accelerationInfo) {
            accelerations.Add(accelerationInfo);
        }

        public void RemoveAcceleration(Acceleration accelerationInfo) {
            accelerations.Remove(accelerationInfo);
        }

        public void SetInputMoveVelocity(Vector3 value) {
            this.inputMoveVelocity = value;
        }

        public void SetExternalVelocity(Vector3 finalVelocity) {
            this.velocity = finalVelocity;
        }

        #endregion

        #region CollisionHandle

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
        public void AddResolveVelocity(Vector3 diff) {
            this.resolveVelocity += diff;
        }

        public void CleanVelocity() {
            this.resolveVelocity = this.inputMoveVelocity = Vector3.zero;
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

        #endregion
    }
}