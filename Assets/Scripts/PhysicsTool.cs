using System.Collections.Generic;
using Physics.Collision;
using UnityEngine;

namespace Physics {
    public static class PhysicsTool {
        #region Old

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

        /// <summary>
        /// 获得线段的一个正交向量，不保证几何相交
        /// </summary>
        /// <param name="lineStart">线段起点</param>
        /// <param name="lineEnd">线段终点</param>
        /// <returns></returns>
        public static Vector3 GetPerpendicularVector(Vector3 lineStart, Vector3 lineEnd) {
            float zDiff = lineStart.z - lineEnd.z;
            return zDiff < 0.00001f ? Vector3.forward :
                new Vector3(1, 0, -1 * (lineStart.x - lineEnd.x) / zDiff);
        }

        /// <summary>
        /// 获得碰撞体在某方向上值最大的点
        /// </summary>
        /// <param name="collisionObject">碰撞体</param>
        /// <param name="dir">方向</param>
        /// <returns></returns>
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

        #endregion

        public static bool Contains(List<Vector3> points, Vector3 point) {
            int n = points.Count;
            if (n < 3) {
                return false;
            }

            // 先计算出内部的方向
            int innerSide = WhichSide(points[0], points[1], points[2]);

            // 通过判断点是否均在三条边的内侧，来判定单形体是否包含点
            for (int i = 0; i < n; ++i) {
                int iNext = (i + 1) % n;
                int side = WhichSide(points[i], points[iNext], point);

                if (side == 0) {
                    // 在边界上
                    return true;
                }

                if (side != innerSide) {
                    // 在外部
                    return false;
                }
            }

            return true;
        }

        public static Vector3 GetPerpendicularToOrigin(Vector3 a, Vector3 b) {
            Vector3 ab = b - a;
            Vector3 ao = -a;

            float sqrLength = ab.sqrMagnitude;
            if (sqrLength < float.Epsilon) {
                return Vector3.zero;
            }

            float projection = Vector3.Dot(ab, ao) / sqrLength;
            return a + ab * projection;
        }

        public static int WhichSide(Vector3 a, Vector3 b, Vector3 c) {
            Vector3 ab = b - a;
            Vector3 ac = c - a;
            float cross = ab.x * ac.z - ab.z * ac.x;
            return cross > 0 ? 1 : (cross < 0 ? -1 : 0);
        }

        public static Vector3 GetClosestPointToOrigin(Vector3 a, Vector3 b) {
            Vector3 ab = b - a;
            Vector3 ao = -a;

            float sqrLength = ab.sqrMagnitude;

            // ab点重合了
            if(sqrLength < float.Epsilon) {
                return a;
            }

            float projection = Vector3.Dot(ab, ao) / sqrLength;
            if (projection < 0) {
                return a;
            }
            else if (projection > 1.0f) {
                return b;
            }
            else {
                return a + ab * projection;
            }
        }
    }
}