using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Model {
    public class Simplex {
        public List<Vector3> points = new List<Vector3>();

        public void Clear() {
            points.Clear();
        }

        public int PointCount() {
            return points.Count;
        }

        public Vector3 GetVertex(int index) {
            return points[index];
        }

        public Vector3 GetSupportPoint(int i) {
            Vector3 point = points[i];

            return point;
        }

        public void Add(Vector3 point) {
            this.points.Add(point);
        }

        public void Remove(int index) {
            points.RemoveAt(index);
        }

        public Vector3 GetLastPoint() {
            return points[^1];
        }

        public bool Contains(Vector3 point) {
            return PhysicsTool.Contains(this.points, point);
        }
    }
}