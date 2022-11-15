using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics.Collision.Shape {
    public class Circle : CollisionShape {
        private int resolution = 2;
        public readonly float radius;

        public Circle(float radius) : base(ShapeType.Circle) {
            this.radius = radius;

            localVertices = new float3[16] {
                CreateCirclePoint(-2, 2),
                CreateCirclePoint(-1, 2),
                CreateCirclePoint(0, 2),
                CreateCirclePoint(1, 2),
                CreateCirclePoint(2, 2),
                CreateCirclePoint(2, 1),
                CreateCirclePoint(2, 0),
                CreateCirclePoint(2, -1),
                CreateCirclePoint(2, -2),
                CreateCirclePoint(1, -2),
                CreateCirclePoint(0, -2),
                CreateCirclePoint(-1, -2),
                CreateCirclePoint(-2, -2),
                CreateCirclePoint(-2, -1),
                CreateCirclePoint(-2, 0),
                CreateCirclePoint(-2, 1),
            };

            vertices = new float3[16] {
                float3.zero, float3.zero, float3.zero, float3.zero, float3.zero, float3.zero,
                float3.zero, float3.zero, float3.zero, float3.zero, float3.zero, float3.zero,
                float3.zero, float3.zero, float3.zero, float3.zero,
            };
        }

        private float3 CreateCirclePoint(int i, int j) {
            float squareX = i * 1.0f / resolution;
            float squareZ = j * 1.0f / resolution;

            return new float3(
                radius * squareX * Mathf.Sqrt(1 - squareZ * squareZ * 0.5f), 0,
                radius * squareZ * Mathf.Sqrt(1 - squareX * squareX * 0.5f));
        }

        protected override void GetBound(out float3 lowerBound, out float3 upperBound) {
            float realR = math.distance(vertices[resolution], float3.zero);

            lowerBound = new float3(-realR, 0, -realR);
            upperBound = new float3(realR, 0, realR);
        }
    }
}