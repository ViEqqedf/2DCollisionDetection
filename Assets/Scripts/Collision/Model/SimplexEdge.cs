using System;
using System.Collections.Generic;
using CustomPhysics.Tool;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics.Collision.Model {
    public class SimplexEdge {
        private Edge[] edges = new Edge[PhysicsWorld.maxIterCount + 1];
        private int edgeCount = 0;

        public void Clear() {
            PhysicsCachePool.RecycleEdge(edges);
            edgeCount = 0;
        }

        public void InitEdges(List<float3> simplex) {
            if (simplex.Count != 2) {
                throw new Exception("边的数量错误");
            }

            edges[0] = CreateInitEdge(simplex[0], simplex[1]);
            edgeCount++;
            edges[1] = CreateInitEdge(simplex[1], simplex[0]);
            edgeCount++;

            UpdateEdgeIndex();
        }

        public Edge FindClosestEdge() {
            float minDis = float.MaxValue;
            Edge result = null;
            for (int i = 0; i < edgeCount; i++) {
                Edge e = edges[i];
                if (e.distance < minDis) {
                    result = e;
                    minDis = e.distance;
                }
            }
            return result;
        }

        public void InsertEdgePoint(Edge e, float3 point) {
            PhysicsWorld.createEdgeCalc(e.a, point, out float distance1, out float3 normal1);
            Edge e1 = PhysicsCachePool.GetEdgeFromPool();
            e1.a = e.a;
            e1.b = point;
            e1.distance = distance1;
            e1.normal = normal1;

            Edge oldEdge = edges[e.index];
            edges[e.index] = e1;
            // PhysicsCachePool.RecycleEdge(oldEdge);

            PhysicsWorld.createEdgeCalc(point, e.b, out float distance2, out float3 normal2);
            // 使用oldEdge的目的是减少缓存池操作
            Edge e2 = oldEdge;
            e2.a = point;
            e2.b = e.b;
            e2.distance = distance2;
            e2.normal = normal2;
            edgeCount++;
            for (int i = edgeCount; i >= e.index + 1; i--) {
                edges[i] = edges[i - 1];
            }
            edges[e.index + 1] = e2;

            UpdateEdgeIndex();
        }

        public void UpdateEdgeIndex() {
            for (int i = 0; i < edgeCount; ++i) {
                edges[i].index = i;
            }
        }

        private Edge CreateInitEdge(float3 a, float3 b) {
            Edge e = PhysicsCachePool.GetEdgeFromPool();
            e.a = a;
            e.b = b;
            e.distance = 0;

            PhysicsWorld.perpCalc(a, b, out float3 result);
            // float3 perp = PhysicsTool.GetPerpendicularToOrigin(a, b);
            e.distance = math.distance(result, float3.zero);

            // 向量垂直定则
            float3 v = a - b;
            v = math.normalizesafe(v);
            e.normal = new float3(v.z, 0, -v.x);

            return e;
        }
    }
}