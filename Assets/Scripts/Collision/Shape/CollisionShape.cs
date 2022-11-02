using System;
using System.Collections.Generic;
using Physics.Collision.Model;
using UnityEngine;

namespace Physics.Collision.Shape {
    public enum ShapeType {
        Undefined,
        Rect,
        Circle,
        Custom,
    }

    public abstract class CollisionShape {
        public readonly ShapeType shapeType;
        public List<Vector3> localVertices { get; protected set; }
        public List<Vector3> vertices { get; protected set; }
        public AABB aabb { get; protected set; }

        public CollisionShape(ShapeType shapeType) {
            this.shapeType = shapeType;
            this.localVertices = new List<Vector3>();
            this.vertices = new List<Vector3>();
            this.aabb = new AABB(Vector3.zero, Vector3.zero);
        }

        public CollisionShape(ShapeType shapeType, Vector3[] localVertices) {
            if (localVertices == null || localVertices.Length < 3) {
                throw new Exception("顶点数量<3，不足以构建凸多边形");
            }
            this.shapeType = shapeType;
            this.localVertices = new List<Vector3>(localVertices);
            this.vertices = new List<Vector3>(localVertices);
            this.aabb = new AABB(Vector3.zero, Vector3.zero);
        }

        public virtual void UpdateShape() {
            GetBound(out Vector3 lowerBound, out Vector3 upperBound);
            this.aabb.Apply(lowerBound, upperBound);
        }

        public void ApplyWorldVertices(Vector3 origin, Vector3 rotate, float scale) {
            for (int i = 0, count = this.vertices.Count; i < count; i++) {
                Vector3 localPoint = scale * localVertices[i];
                Vector3 rotateVec = Quaternion.Euler(rotate) * localPoint;
                vertices[i] = new Vector3(rotateVec.x + origin.x,
                    rotateVec.y + origin.y, rotateVec.z + origin.z);
            }

            UpdateShape();
        }

        protected abstract void GetBound(out Vector3 lowerBound, out Vector3 upperBound);
    }
}