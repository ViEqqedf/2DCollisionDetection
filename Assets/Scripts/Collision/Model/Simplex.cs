using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Model {
    public class Simplex {
        public List<Vector3> points = new List<Vector3>();
        public List<Vector3> fromA = new List<Vector3>();
        public List<Vector3> fromB = new List<Vector3>();

        public void Clear() {
            points.Clear();
            fromA.Clear();
            fromB.Clear();
        }

        public int PointCount() {
            return points.Count;
        }

        public Vector3 GetVertex(int index) {
            return points[index];
        }

        public SupportPoint GetSupportPoint(int i) {
            return new SupportPoint
            {
                point = points[i],
                fromA = fromA[i],
                fromB = fromB[i],
            };
        }

        public void Add(SupportPoint point) {
            this.points.Add(point.point);
            fromA.Add(point.fromA);
            fromB.Add(point.fromB);
        }

        public void Remove(int index) {
            points.RemoveAt(index);
            fromA.RemoveAt(index);
            fromB.RemoveAt(index);
        }

        public Vector3 GetLastPoint() {
            return points[^1];
        }

        public bool Contains(Vector3 point) {
            return PhysicsTool.Contains(this.points, point);
        }
    }
}