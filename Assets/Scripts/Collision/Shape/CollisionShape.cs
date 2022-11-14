﻿using System;
using System.Collections.Generic;
using CustomPhysics.Collision.Model;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics.Collision.Shape {
    public enum ShapeType {
        Undefined,
        Rect,
        Circle,
        Custom,
    }

    public struct TestShape {
        public ShapeType shapeType;
        public List<Vector3> localVertices;
        public List<Vector3> vertices;
        public AABB aabb;
    }

    public abstract class CollisionShape {
        public ShapeType shapeType { get; protected set; }
        public List<float3> localVertices { get; protected set; }
        public List<float3> vertices { get; protected set; }
        public AABB aabb { get; protected set; }

        public CollisionShape(ShapeType shapeType) {
            this.shapeType = shapeType;
            this.localVertices = new List<float3>();
            this.vertices = new List<float3>();
            this.aabb = new AABB(Vector3.zero, Vector3.zero);
        }

        public CollisionShape(ShapeType shapeType, float3[] localVertices) {
            if (localVertices == null || localVertices.Length < 3) {
                throw new Exception("顶点数量<3，不足以构建凸多边形");
            }
            this.shapeType = shapeType;
            this.localVertices = new List<float3>(localVertices);
            this.vertices = new List<float3>(localVertices);
            this.aabb = new AABB(Vector3.zero, Vector3.zero);
        }

        public void UpdateShape() {
            GetBound(out Vector3 lowerBound, out Vector3 upperBound);
            this.aabb = new AABB(lowerBound, upperBound);
        }

        public void ApplyWorldVertices(Vector3 origin, Vector3 rotate, float scale) {
            for (int i = 0, count = this.vertices.Count; i < count; i++) {
                Vector3 localPoint = scale * localVertices[i];
                Vector3 rotateVec = Quaternion.Euler(rotate) * localPoint;
                vertices[i] = new float3(rotateVec.x + origin.x,
                    rotateVec.y + origin.y, rotateVec.z + origin.z);
            }

            UpdateShape();
        }

        protected abstract void GetBound(out Vector3 lowerBound, out Vector3 upperBound);
    }
}