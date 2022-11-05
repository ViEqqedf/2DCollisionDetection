using System.Collections.Generic;
using UnityEngine;

namespace CustomPhysics.Collision.Shape {
    public class Rect : CollisionShape {
        private float horizontalWidth = 0;
        private float verticalWidth = 0;

        public Rect(float horizontalWidth, float verticalWidth) : base(ShapeType.Rect) {
            this.horizontalWidth = horizontalWidth;
            this.verticalWidth = verticalWidth;

            localVertices = new List<Vector3>() {
                new Vector3(-horizontalWidth / 2, 0, -verticalWidth / 2),
                new Vector3(-horizontalWidth / 2, 0, verticalWidth / 2),
                new Vector3(horizontalWidth / 2, 0, verticalWidth / 2),
                new Vector3(horizontalWidth / 2, 0, -verticalWidth / 2),
            };
            vertices = new List<Vector3>(
                new Vector3[] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,});
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0, count = vertices.Count; i < count; i++) {
                if (vertices[i].x < minX) {
                    minX = vertices[i].x;
                }
                if (vertices[i].z < minZ) {
                    minZ = vertices[i].z;
                }
                if (vertices[i].x > maxX) {
                    maxX = vertices[i].x;
                }
                if (vertices[i].z > maxZ) {
                    maxZ = vertices[i].z;
                }
            }

            lowerBound = new Vector3(minX, 0, minZ);
            upperBound = new Vector3(maxX, 0, maxZ);
        }
    }
}