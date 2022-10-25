using System.Collections.Generic;
using Physics.Collision;
using UnityEngine;

namespace Physics {
    public static class PhysicsTool {
        /// <summary>
        /// 检查点是否在三角形内
        /// </summary>
        /// <param name="fst">三角形点1</param>
        /// <param name="snd">三角形点2</param>
        /// <param name="trd">三角形点3</param>
        /// <param name="point">要检查的点</param>
        /// <returns></returns>
        public static bool IsPointInTriangle(Vector3 fst, Vector3 snd, Vector3 trd, Vector3 point) {
            Vector3 v0 = trd - fst;
            Vector3 v1 = snd - fst;
            Vector3 v2 = point - fst;

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

        /// <summary>
        /// 检查点是否在一个多边形中
        /// </summary>
        /// <param name="vertices">点集，顺序要求能够从头到尾连接闭环</param>
        /// <param name="point">要检查的点</param>
        /// <returns></returns>
        public static bool IsPointInPolygon(List<Vector3> vertices, Vector3 point) {
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
                    float edgeSlope = edgeFrom.x - edgeTo.x / edgeFrom.y - edgeTo.y;
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

        public static bool IsPointOnSegment(Vector3 lineP1, Vector3 lineP2, Vector3 point) {
            // collinear && in range
            return Vector3.Cross(lineP1 - point, lineP2 - point) == Vector3.zero &&
                   Vector3.Dot(lineP1 - point, lineP1 - point) <= 0;
        }

        public static Vector3 GetPerpendicularToOrigin(Vector3 a, Vector3 b)
        {
            Vector3 ab = b - a;
            Vector3 ao = Vector3.zero - a;

            float projection = Vector3.Dot(ab, ao) / ab.sqrMagnitude;
            return a + ab * projection;
        }

        public static Vector3 GetFarthestPointInDir(CollisionObject collisionObject, Vector3 dir) {
            List<Vector3> vertices = collisionObject.shape.vertices;
            float maxDis = float.MinValue;
            Vector3 extremePoint = Vector3.zero;
            for (int i = 0, count = vertices.Count; i < count; i++) {
                float dis = Vector3.Dot(vertices[i], dir);
                if (dis > maxDis) {
                    maxDis = dis;
                    extremePoint = vertices[i];
                }
            }

            return extremePoint;
        }
    }
}