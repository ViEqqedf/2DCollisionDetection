using System;
using System.Collections;
using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Shape;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Physics {
    public interface IPhysicsWorld {
        public bool AddCollisionObject(CollisionObject collisionObject);
        public bool RemoveCollisionObject(CollisionObject collisionObject);
    }

    public class PhysicsWorld : MonoBehaviour, IPhysicsWorld {
        public List<CollisionObject> collisionList;
        private List<ProjectionPoint> horAABBProjList;
        private List<ProjectionPoint> verAABBProjList;
        private List<CollisionPair> broadphasePair;
        private int collisionId;

        void Start() {
            collisionList = new List<CollisionObject>();
            horAABBProjList = new List<ProjectionPoint>();
            verAABBProjList = new List<ProjectionPoint>();
            broadphasePair = new List<CollisionPair>();
            collisionId = 0;
        }

        void Update()
        {
            Tick(0.033f);
        }

        private void Tick(float timeSpan) {
            CollisionDetection(timeSpan);
            ApplyForces(timeSpan);
            Resolve(timeSpan);
            ApplyVelocity(timeSpan);
        }

        #region Collision

        private void CollisionDetection(float timeSpan) {
            BroadPhase();
            NarrowPhase();
        }

        private void BroadPhase() {
            broadphasePair.Clear();

            SweepAndPrune();
            // DynamicBVH();
        }

        private void SweepAndPrune() {
            // 更新投影
            for (int i = 0, count = collisionList.Count; i < count; i++) {
                collisionList[i].shape.aabb.UpdateAllProjectionPoint();
            }

            // 升序排序
            if (horAABBProjList.Count >= 2) {
                for (int i = 1, count = horAABBProjList.Count; i < count; i++) {
                    var key = horAABBProjList[i];
                    int j = i - 1;
                    while (j >= 0 && key.value < horAABBProjList[j].value) {
                        horAABBProjList[j + 1] = horAABBProjList[j];
                        j--;
                    }
                    horAABBProjList[j + 1] = key;
                }
            }

            List<ProjectionPoint> scanList = new List<ProjectionPoint>();
            // (collisionId, ProjectionPoint)
            Dictionary<int, ProjectionPoint> startPoints = new Dictionary<int, ProjectionPoint>();

            // 在水平方向上检查
            scanList.Add(horAABBProjList[0]);
            startPoints.Add(horAABBProjList[0].collisionObject.id, horAABBProjList[0]);
            for (int i = 1, count = horAABBProjList.Count; i < count; i++) {
                if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalEnd) {
                    ProjectionPoint startPoint = startPoints[horAABBProjList[i].collisionObject.id];
                    startPoints.Remove(horAABBProjList[i].collisionObject.id);
                    scanList.Remove(startPoint);
                } else if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalStart) {
                    for (int j = 0, scanCount = scanList.Count; j < scanCount; j++) {
                        broadphasePair.Add(new CollisionPair() {
                        first = scanList[j].collisionObject,
                        second = horAABBProjList[i].collisionObject,
                        });
                    }
                    scanList.Add(horAABBProjList[i]);
                }
            }

            // 在竖直方向上检查已确定的对
            for (int i = broadphasePair.Count - 1; i >= 0; i--) {
                CollisionPair pair = broadphasePair[i];
                AABB first = pair.first.shape.aabb;
                AABB second = pair.second.shape.aabb;
                float fstStart = first.GetProjectionPoint(AABBProjectionType.VerticalStart).value;
                float fstEnd = first.GetProjectionPoint(AABBProjectionType.VerticalEnd).value;
                float sndStart = second.GetProjectionPoint(AABBProjectionType.VerticalStart).value;
                float sndEnd = second.GetProjectionPoint(AABBProjectionType.VerticalEnd).value;
                if (fstStart <= sndStart && fstEnd >= sndEnd ||
                    fstStart >= sndEnd && fstEnd <= sndEnd) {
                    // 在垂直方向上重叠
                    continue;
                } else {
                    broadphasePair.Remove(pair);
                }
            }
        }

        private void DynamicBVH() {
            if (horAABBProjList.Count >= 2)
            {
                for (int i = 1, count = horAABBProjList.Count; i < count; i++)
                {
                    var key = horAABBProjList[i];
                    int j = i - 1;
                    while (j >= 0 && key.value < horAABBProjList[j].value)
                    {
                        horAABBProjList[j + 1] = horAABBProjList[j];
                        j--;
                    }
                    horAABBProjList[j + 1] = key;
                }
            }

            for (int i = broadphasePair.Count - 1; i >= 0; i--) {
                CollisionPair pair = broadphasePair[i];
                AABB first = pair.first.shape.aabb;
                AABB second = pair.second.shape.aabb;
                float fstStart = first.GetProjectionPoint(AABBProjectionType.VerticalStart).value;
                float fstEnd = first.GetProjectionPoint(AABBProjectionType.VerticalEnd).value;
                float sndStart = second.GetProjectionPoint(AABBProjectionType.VerticalStart).value;
                float sndEnd = second.GetProjectionPoint(AABBProjectionType.VerticalEnd).value;
                if (fstStart <= sndStart && fstEnd >= sndEnd ||
                    fstStart >= sndEnd && fstEnd <= sndEnd) {
                    // overlap on vertical
                    continue;
                } else {
                    broadphasePair.Remove(pair);
                }
            }

            for (int i = 0, count = collisionList.Count; i < count; i++) {
                collisionList[i].shape.aabb.UpdateAllProjectionPoint();
            }


            List<ProjectionPoint> scanList = new List<ProjectionPoint>();
            // (collisionId, ProjectionPoint)
            Dictionary<int, ProjectionPoint> startPoints = new Dictionary<int, ProjectionPoint>();

            scanList.Add(horAABBProjList[0]);
            startPoints.Add(horAABBProjList[0].collisionObject.id, horAABBProjList[0]);
            for (int i = 1, count = horAABBProjList.Count; i < count; i++) {
                if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalEnd) {
                    ProjectionPoint startPoint = startPoints[horAABBProjList[i].collisionObject.id];
                    startPoints.Remove(horAABBProjList[i].collisionObject.id);
                    scanList.Remove(startPoint);
                } else if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalStart) {
                    for (int j = 0, scanCount = scanList.Count; j < scanCount; j++) {
                        broadphasePair.Add(new CollisionPair() {
                            first = scanList[j].collisionObject,
                            second = horAABBProjList[i].collisionObject,
                        });
                    }
                    scanList.Add(horAABBProjList[i]);
                }
            }
        }

        private void NarrowPhase() {
            for (int i = broadphasePair.Count - 1; i >= 0; i--) {
                CollisionPair pair = broadphasePair[i];
                CollisionObject fst = pair.first;
                CollisionObject snd = pair.second;

                if (GJK(fst, snd, out List<Vector3> simplex)) {
                    EPA(fst, snd, simplex);
                } else {
                    broadphasePair.Remove(pair);
                }
            }
        }

        private bool GJK(CollisionObject fst, CollisionObject snd, out List<Vector3> simplex) {
            simplex = new List<Vector3>();
            Vector3 supDir = (snd.position - fst.position).normalized;
            simplex.Add(Support(supDir, fst, snd));
            supDir = -supDir;
            while (true) {
                Vector3 supPoint = Support(supDir, fst, snd);

                // 找不到新的单纯形，结束
                if (Vector3.Dot(supPoint, supDir) < 0) {
                    return false;
                }

                simplex.Add(supPoint);
                if (simplex.Count == 3 && PhysicsTool.IsPointInTriangle(
                        simplex[0], simplex[1], simplex[2], Vector3.zero)) {
                    return true;
                }

                supDir = FindNextSupDir(simplex);
            }
        }

        private Vector3 EPA(CollisionObject fst, CollisionObject snd, List<Vector3> simplex) {
            // 1. 建立多边形

            // 2. 开始迭代
            while (true) {
                int vertexIndex = 0;
                Vector3 edgeNormal = Vector3.zero;
                float minDis = float.MaxValue;
                // 3. 找到一条离原点最近的边
                for (int i = 0, count = simplex.Count; i < count; i++) {
                    Vector3 lineTo = i != count - 1 ? simplex[i + 1] : simplex[0];
                    Vector3 verticalLine = PhysicsTool.GetPerpendicularToOrigin(simplex[i], lineTo);
                    float magnitude = verticalLine.magnitude;
                    if (magnitude < minDis) {
                        vertexIndex = i != count - 1 ? i + 1 : 0;
                        edgeNormal = -verticalLine;
                        minDis = magnitude;
                    }
                }
                // 4. 使用该边的原理原点的垂线作为新的迭代方向，寻找一个新的单纯形点
                Vector3 supPoint = Support(edgeNormal, fst, snd);
                if (PhysicsTool.IsPointInPolygon(simplex, supPoint)) {
                    // 5. 如果该点已经包含于单纯形中，返回该向量作为穿透向量
                    return edgeNormal;
                } else {
                    // 6. 否则使用该点扩展单纯形
                    simplex.Insert(vertexIndex, supPoint);
                }
            }
        }

        private Vector3 FindNextSupDir(List<Vector3> simplex) {
            if (simplex.Count == 2) {
                Vector3 crossPoint = PhysicsTool.GetPerpendicularToOrigin(simplex[0], simplex[1]);
                // 取靠近原点的方向
                return Vector3.zero - crossPoint;
            } else if (simplex.Count == 3) {
                Vector3 cross20 = PhysicsTool.GetPerpendicularToOrigin(simplex[2], simplex[0]);
                Vector3 cross21 = PhysicsTool.GetPerpendicularToOrigin(simplex[2], simplex[1]);

                // 取靠近原点的方向，然后取其朝向原点的向量作为下一迭代方向
                if (cross20.sqrMagnitude < cross21.sqrMagnitude) {
                    // TODO:[ViE] memory sort consume
                    simplex.RemoveAt(1);
                    return Vector3.zero - cross20;
                } else {
                    // TODO:[ViE] memory sort consume
                    simplex.RemoveAt(0);
                    return Vector3.zero - cross21;
                }
            } else {
                // Meaningless
                throw new Exception("单纯形的点数量错误");
            }
        }

        private Vector3 Support(Vector3 iterDir, CollisionObject collA, CollisionObject collB) {
            iterDir = iterDir.normalized;
            Vector3 fst = PhysicsTool.GetFarthestPointInDir(collA, iterDir);
            Vector3 snd = PhysicsTool.GetFarthestPointInDir(collB, -iterDir);

            return fst - snd;
        }

        #endregion

        private void ApplyForces(float timeSpan) {

        }

        private void Resolve(float timeSpan) {

        }

        private void ApplyVelocity(float timeSpan) {

        }

        #region Interface

        public bool AddCollisionObject(CollisionObject collisionObject) {
            collisionObject.InitCollisionObject();
            collisionList.Add(collisionObject);

            horAABBProjList.Add(collisionObject.shape.aabb.GetProjectionPoint(
                AABBProjectionType.HorizontalStart));
            horAABBProjList.Add(collisionObject.shape.aabb.GetProjectionPoint(
                AABBProjectionType.HorizontalEnd));
            verAABBProjList.Add(collisionObject.shape.aabb.GetProjectionPoint(
                AABBProjectionType.VerticalStart));
            verAABBProjList.Add(collisionObject.shape.aabb.GetProjectionPoint(
                AABBProjectionType.VerticalEnd));
            return true;
        }

        public bool RemoveCollisionObject(CollisionObject collisionObject) {
            collisionList.Remove(collisionObject);
            return true;
        }

        #endregion
    }
}