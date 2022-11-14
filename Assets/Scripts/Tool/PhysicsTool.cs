using System.Collections.Generic;
using CustomPhysics.Collision;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

namespace CustomPhysics.Tool {
    public static class PhysicsTool {
        #region Old

        /// <summary>
        /// 检查点是否在一个多边形中
        /// </summary>
        /// <param name="vertices">点集，顺序要求能够从头到尾连接闭环</param>
        /// <param name="point">要检查的点</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<Vector3> vertices, Vector3 point) {
            if (vertices.Count <= 2) {
                return false;
            }

            // PNPoly, 射线法
            bool flag = false;
            for (int i = 0, count = vertices.Count, j = count - 1; i < count; j = i++) {
                Vector3 edgeFrom = vertices[i];
                Vector3 edgeTo = vertices[j];
                // 被测点是否在边上
                if (IsPointOnSegment(edgeFrom, edgeTo, point)) {
                    return true;
                }

                // 等价于min(edgeFrom.y, edgeTo.y) < point.y <= max(edgeFrom.y, edgeTo.y)
                // 排除了不会相交的边，同时排除了edgeFrom.y == edgeTo.y的情况
                bool verticalInRange = edgeFrom.y > point.y != edgeTo.y > point.y;
                if (verticalInRange) {
                    float edgeSlope = (edgeTo.x - edgeFrom.x) / (edgeTo.y - edgeFrom.y);
                    float xOfPointOnEdge = edgeFrom.x + edgeSlope * (point.y - edgeFrom.y);
                    bool isPointLeftToEdge = point.x - xOfPointOnEdge < 0;
                    // 被测点是否在测试边的左侧（假想中的射线向右发射）
                    if(isPointLeftToEdge) {
                        flag = !flag;
                    }
                }
            }

            return flag;
        }

        /// <summary>
        /// 点是否在线段上
        /// </summary>
        /// <param name="lineStart">线段起点</param>
        /// <param name="lineEnd">线段终点</param>
        /// <param name="point">检查点</param>
        /// <returns></returns>
        public static bool IsPointOnSegment(Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
            // collinear && in range
            return Vector3.Cross(lineStart - point, lineEnd - point) == Vector3.zero &&
                   Vector3.Dot(lineStart - point, lineEnd - point) <= 0;
        }

        #endregion

        /// <summary>
        /// 检查点是否在三角形内
        /// </summary>
        /// <param name="points">点集</param>
        /// <param name="point">要检查的点</param>
        /// <returns></returns>
        public static bool IsPointInTriangle(List<Vector3> points, Vector3 point) {
            Vector3 v0 = points[2] - points[0];
            Vector3 v1 = points[1] - points[0];
            Vector3 v2 = point - points[0];

            float dot00 = Vector3.Dot(v0, v0);
            float dot01 = Vector3.Dot(v0, v1);
            float dot02 = Vector3.Dot(v0, v2);
            float dot11 = Vector3.Dot(v1, v1);
            float dot12 = Vector3.Dot(v1, v2);
            float denominator = 1 / (dot00 * dot11 - dot01 * dot01);

            // condition: u >= 0 && v >= 0 && u + v <= 1
            float u = (dot11 * dot02 - dot01 * dot12) * denominator;
            if (u < 0 || u > 1) {
                return false;
            }
            float v = (dot00 * dot12 - dot01 * dot02) * denominator;
            if (v < 0 || v > 1) {
                return false;
            }
            return u + v <= 1;
        }

        public static Vector3 GetPerpendicularToOrigin(Vector3 a, Vector3 b) {
            Vector3 ab = b - a;
            Vector3 ao = -a;

            float sqrLength = ab.sqrMagnitude;
            if (sqrLength < float.Epsilon) {
                return Vector3.zero;
            }

            float projection = Vector3.Dot(ab, ao) / sqrLength;

            // return a + ab * projection;
            return new Vector3(a.x + projection * ab.x, a.y + projection * ab.y,
                a.z + projection * ab.z);
        }

        [BurstCompile]
        public static float3 GetClosestPointToOrigin(float3 a, float3 b) {
            float3 ab = b - a;
            float3 ao = -a;

            float sqrLength = math.distancesq(ab.x, ab.z);

            // ab点重合了
            if(sqrLength < float.Epsilon) {
                return a;
            }

            float projection = math.dot(ab, ao) / sqrLength;
            if (projection < 0) {
                return a;
            }
            else if (projection > 1.0f) {
                return b;
            }
            else {
                return new float3(a.x + projection * ab.x, a.y + projection * ab.y,
                    a.z + projection * ab.z);
            }
        }
    }
}