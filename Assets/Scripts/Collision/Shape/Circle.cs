using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics.Collision.Shape {
    public class Circle : CollisionShape {
        private int resolution = 2;
        public readonly float radius;

        public Circle(float radius) : base(ShapeType.Circle) {
            this.radius = radius;

            localVertices = new List<Vector3>();
            vertices = new List<Vector3>();
            for (int i = -resolution; i < resolution; i++) {
                localVertices.Add(CreateCirclePoint(i, resolution));
                vertices.Add(Vector3.zero);
            }
            for (int i = resolution; i > -resolution; i--) {
                localVertices.Add(CreateCirclePoint(resolution, i));
                vertices.Add(Vector3.zero);
            }
            for (int i = resolution; i > -resolution; i--) {
                localVertices.Add(CreateCirclePoint(i, -resolution));
                vertices.Add(Vector3.zero);
            }
            for (int i = -resolution; i < resolution; i++) {
                localVertices.Add(CreateCirclePoint(-resolution, i));
                vertices.Add(Vector3.zero);
            }
        }

        private Vector3 CreateCirclePoint(int i, int j) {
            float squareX = i * 1.0f / resolution;
            float squareZ = j * 1.0f / resolution;

            return new Vector3(
                radius * squareX * Mathf.Sqrt(1 - squareZ * squareZ * 0.5f), 0,
                radius * squareZ * Mathf.Sqrt(1 - squareX * squareX * 0.5f));
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            float realR = vertices[resolution].magnitude;

            lowerBound = new Vector3(-realR, 0, -realR);
            upperBound = new Vector3(realR, 0, realR);
        }
    }
}