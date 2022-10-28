using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Shape {
    public class CustomShape : CollisionShape {
        public CustomShape(Vector3[] localVertices) : base(ShapeType.Custom, localVertices) {
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0, count = localVertices.Count; i < count; i++) {
                if (localVertices[i].x < minX) {
                    minX = localVertices[i].x;
                }
                if (localVertices[i].y < minZ) {
                    minZ = localVertices[i].y;
                }
                if (localVertices[i].x > maxX) {
                    maxX = localVertices[i].x;
                }
                if (localVertices[i].y > maxZ) {
                    maxZ = localVertices[i].y;
                }
            }

            lowerBound = new Vector3(minX, 0, minZ);
            upperBound = new Vector3(maxX, 0, maxZ);
        }
    }
}