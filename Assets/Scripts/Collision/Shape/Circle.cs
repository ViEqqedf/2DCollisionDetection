using Unity.Mathematics;
using UnityEngine;
namespace CustomPhysics.Collision.Shape {
    public class Circle : CollisionShape {
        public readonly float radius;
        private float3 lastLowerBound;
        private float3 lastUpperBound;

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
            float squareX = i * 1.0f / 2;
            float squareZ = j * 1.0f / 2;

            return new float3(
                radius * squareX * Mathf.Sqrt(1 - squareZ * squareZ * 0.5f), 0,
                radius * squareZ * Mathf.Sqrt(1 - squareX * squareX * 0.5f));
        }

        protected override void GetBound(
            bool isPositionDirty, bool isRotationDirty, bool isScaleDirty,
            out float3 lowerBound, out float3 upperBound) {
            if (!isPositionDirty && !isScaleDirty) {
                lowerBound = lastLowerBound;
                upperBound = lastUpperBound;

                return;
            }

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0, count = vertices.Length; i < count; i++) {
                float3 curVertex = vertices[i];
                if (curVertex.x < minX) {
                    minX = curVertex.x;
                }
                if (curVertex.z < minZ) {
                    minZ = curVertex.z;
                }
                if (curVertex.x > maxX) {
                    maxX = curVertex.x;
                }
                if (curVertex.z > maxZ) {
                    maxZ = curVertex.z;
                }
            }

            lowerBound = new float3(minX, 0, minZ);
            upperBound = new float3(maxX, 0, maxZ);
            lastLowerBound = lowerBound;
            lastUpperBound = upperBound;
        }
    }
}