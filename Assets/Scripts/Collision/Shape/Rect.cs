using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Shape {
    public class Rect : CollisionShape {
        private float horizontalWidth = 0;
        private float verticalWidth = 0;

        public Rect(float horizontalWidth, float verticalWidth) : base(ShapeType.Rect) {
            this.horizontalWidth = horizontalWidth;
            this.verticalWidth = verticalWidth;

            localVertices = new List<Vector3>() {
                new Vector3(-horizontalWidth / 2, 0, -verticalWidth / 2),
                new Vector3(-horizontalWidth / 2, 0, verticalWidth / 2),
                new Vector3(horizontalWidth / 2, 0, -verticalWidth / 2),
                new Vector3(horizontalWidth / 2, 0, verticalWidth / 2),
            };
            vertices = new List<Vector3>(
                new Vector3[] {Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero,});
        }

        protected override void GetBound(out Vector3 lowerBound, out Vector3 upperBound) {
            lowerBound = new Vector3(-horizontalWidth / 2 , 0, -verticalWidth / 2);
            upperBound = new Vector3(horizontalWidth / 2 , 0, verticalWidth / 2);
        }
    }
}