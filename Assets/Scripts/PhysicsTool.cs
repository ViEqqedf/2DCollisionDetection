using System.Collections.Generic;
using Physics.Collision;
using UnityEngine;

namespace Physics {
    public static class PhysicsTool {
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