using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Model {
    public class SimplexEdge {
        public List<Edge> edges = new List<Edge>();

        public void Clear() {
            PhysicsCachePool.RecycleEdge(edges);
        }

        public void InitEdges(Simplex simplex) {
            if (simplex.PointCount() != 2) {
                throw new Exception("边的数量错误");
            }

            edges.Add(CreateInitEdge(simplex.GetSupportPoint(0), simplex.GetSupportPoint(1)));
            edges.Add(CreateInitEdge(simplex.GetSupportPoint(1), simplex.GetSupportPoint(0)));

            UpdateEdgeIndex();
        }

        public Edge FindClosestEdge() {
            float minDistance = float.MaxValue;
            Edge ret = null;
            for (int i = 0, count = edges.Count; i < count; i++) {
                Edge e = edges[i];
                if (e.distance < minDistance) {
                    ret = e;
                    minDistance = e.distance;
                }
            }
            return ret;
        }

        public void InsertEdgePoint(Edge e, SupportPoint point) {
            Edge e1 = CreateEdge(e.a, point);
            edges[e.index] = e1;

            Edge e2 = CreateEdge(point, e.b);
            edges.Insert(e.index + 1, e2);

            UpdateEdgeIndex();
        }

        public void UpdateEdgeIndex() {
            for (int i = 0, count = edges.Count; i < count; ++i) {
                edges[i].index = i;
            }
        }

        public Edge CreateEdge(SupportPoint a, SupportPoint b) {
            Edge e = PhysicsCachePool.GetEdgeFromPool();
            e.a = a;
            e.b = b;

            e.normal = PhysicsTool.GetPerpendicularToOrigin(a.point, b.point);
            float lengthSq = e.normal.sqrMagnitude;
            // 单位化边
            if (lengthSq > float.Epsilon) {
                e.distance = Mathf.Sqrt(lengthSq);
                e.normal *= 1.0f / e.distance;
            }
            else {
                // 向量垂直定则
                Vector3 v = a.point - b.point;
                v.Normalize();
                e.normal = new Vector3(v.z, 0, -v.x);
            }
            return e;
        }

        private Edge CreateInitEdge(SupportPoint a, SupportPoint b) {
            Edge e = PhysicsCachePool.GetEdgeFromPool();
            e.a = a;
            e.b = b;
            e.distance = 0;

            Vector3 perp = PhysicsTool.GetPerpendicularToOrigin(a.point, b.point);
            e.distance = perp.magnitude;

            // 向量垂直定则
            Vector3 v = a.point - b.point;
            v.Normalize();
            e.normal = new Vector3(v.z, 0, -v.x);

            return e;
        }
    }
}