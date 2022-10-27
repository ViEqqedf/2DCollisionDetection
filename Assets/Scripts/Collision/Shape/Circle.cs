using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Shape {
    public class Circle : CollisionShape {
        private float radius;

        public Circle(float radius) : base(ShapeType.Circle) {
            this.radius = radius;

            localVertices = new List<Vector3>();
            vertices = new List<Vector3>();
            for (int i = -2; i < 0; i++) {
                for (int j = 0; j < 2; j++) {
                    localVertices.Add(CreateCirclePoint(i, j));
                    vertices.Add(Vector3.zero);
                }
            }
            for (int i = 0; i < 2; i++) {
                for (int j = 2; j > 0; j--) {
                    localVertices.Add(CreateCirclePoint(i, j));
                    vertices.Add(Vector3.zero);
                }
            }
            for (int i = 2; i > 0; i--) {
                for (int j = 0; j > -2; j--) {
                    localVertices.Add(CreateCirclePoint(i, j));
                    vertices.Add(Vector3.zero);
                }
            }
            for (int i = 0; i > -2; i--) {
                for (int j = -2; j < 0; j++) {
                    localVertices.Add(CreateCirclePoint(i, j));
                    vertices.Add(Vector3.zero);
                }
            }
        }

        private Vector3 CreateCirclePoint(int i, int j) {
            Vector3 squarePoint = new Vector3((i - 2) * 0.25f, 0, (j - 2) * 0.25f);
            float squareX = squarePoint.x * radius;
            float squareZ = squarePoint.z * radius;

            return new Vector3(
                squareX * Mathf.Sqrt(1 - squareZ * squareZ * 0.5f), 0,
                squareZ * Mathf.Sqrt(1 - squareX * squareX * 0.5f));
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            lowerBound = new Vector3(-radius, 0, -radius);
            upperBound = new Vector3(radius, 0, radius);
        }
    }
}