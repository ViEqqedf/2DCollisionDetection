using System;
using System.Collections;
using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Shape;
using UnityEngine;

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
            DynamicBVH();
        }

        private void SweepAndPrune() {
            // Update Projection
            for (int i = 0, count = collisionList.Count; i < count; i++) {
                collisionList[i].shape.aabb.UpdateAllProjectionPoint();
            }

            // Ascending Order Sort
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

            // Scan on horizontal
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

            // Scan on vertical, only check determined horizontal pair
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
            // GJK
            for (int i = broadphasePair.Count - 1; i >= 0; i--) {
                CollisionPair pair = broadphasePair[i];
                CollisionObject fst = pair.first;
                CollisionObject snd = pair.second;

                if (GJK(fst, snd, out List<Vector3> simplex)) {

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

                // The last vertex added to the simplex is collinear with the search direction,
                // no new simplex can be found, end
                if (Vector3.Dot(supPoint, supDir) < 0) {
                    return false;
                }

                simplex.Add(supPoint);
                if (PhysicsTool.IsPointInTriangle(simplex[0], simplex[1], simplex[2], Vector3.zero)) {
                    return true;
                }

                supDir = FindNextSupDir(simplex);
            }
        }

        private Vector3 FindNextSupDir(List<Vector3> simplex) {
            if (simplex.Count == 2) {
                Vector3 crossPoint = PhysicsTool.GetPerpendicularToOrigin(simplex[0], simplex[1]);
                // Take the vector that's near the origin
                return Vector3.zero - crossPoint;
            } else if (simplex.Count == 3) {
                Vector3 cross20 = PhysicsTool.GetPerpendicularToOrigin(simplex[2], simplex[0]);
                Vector3 cross21 = PhysicsTool.GetPerpendicularToOrigin(simplex[2], simplex[1]);

                // Take the vector that's near the origin, remove the far one
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