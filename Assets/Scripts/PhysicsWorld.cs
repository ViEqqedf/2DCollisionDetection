using System;
using System.Collections;
using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Shape;
using UnityEngine;
using UnityEngine.Profiling;
using Random = UnityEngine.Random;

namespace Physics {
    public interface IPhysicsWorld {
        public bool AddCollisionObject(CollisionObject collisionObject);
        public bool RemoveCollisionObject(CollisionObject collisionObject);
    }

    public class PhysicsWorld : MonoBehaviour, IPhysicsWorld {
        public static int tickFrame = 0;
        public List<CollisionObject> collisionList;
        private List<ProjectionPoint> horAABBProjList;
        // private List<ProjectionPoint> verAABBProjList;
        private List<CollisionPair> broadphasePair;

        void Start() {
            Application.targetFrameRate = 60;
            collisionList = new List<CollisionObject>();
            horAABBProjList = new List<ProjectionPoint>();
            // verAABBProjList = new List<ProjectionPoint>();
            broadphasePair = new List<CollisionPair>();

            // Test0();
            // Test1();
            // Test2();
            Test3();
        }

        #region Test

        private void Test0() {
            // int range = 50;
            // for (int i = 0; i < range; i++) {
            //     CreateATestRect(new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f)));
            // }

            CreateATestRect(Vector3.zero);
            CreateATestRect(new Vector3(0.5f, 0, 0.5f));
        }

        private void Test1() {
            int range = 1;
            for (int i = 0; i < range; i++) {
                Vector3 spawnPos = new Vector3(
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

                CreateATestRect(new Vector3(
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f)));
                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(0, 0, -1)}, spawnPos, i);
            }
        }

        private void Test2() {
            int range = 2;
            for (int i = 0; i < range; i++) {
                Vector3 spawnPos = new Vector3(
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

                CreateATestCircle(spawnPos);
            }
        }

        private void Test3() {
            int range = 1;
            for (int i = 0; i < range; i++) {
                Vector3 spawnPos = new Vector3(
                    Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(0, 0, -1)}, spawnPos, 0);

                spawnPos = new Vector3(
                    Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));
                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(2, 0, -2), new Vector3(-2, 0, -3),
                    new Vector3(-2, 0, 0)}, spawnPos, 1);
            }
        }

        public void CreateACustomShape(Vector3[] vertices, Vector3 pos, int level) {
            CollisionShape shape = new Physics.Collision.Shape.CustomShape(vertices);
            CollisionObject co = new CollisionObject(shape, null, pos, level);
            AddCollisionObject(co);
            GameObject go = CreateMesh(co);
        }

        public GameObject CreateMesh(CollisionObject co) {
            GameObject go = new GameObject(co.shape.shapeType.ToString());
            Mesh m;
            go.AddComponent<MeshFilter>().mesh = m = new Mesh();
            m.name = Guid.NewGuid().ToString();
            m.vertices = co.shape.localVertices.ToArray();

            int vertexCount = co.shape.vertices.Count;
            int triCount = vertexCount - 2;
            int[] triangles = new int[3 * triCount];
            for (int i = 0, triIndex = 0; i < triCount; triIndex = 3 * ++i) {
                triangles[triIndex] = 0;
                triangles[triIndex + 1] = (i + 1) % vertexCount;
                triangles[triIndex + 2] = (i + 2) % vertexCount;
            }
            m.triangles = triangles;
            go.AddComponent<MeshRenderer>();
            go.AddComponent<CollisionObjectProxy>().target = co;

            return go;
        }

        public void CreateATestRect(Vector3 pos) {
            CollisionShape shape = new Physics.Collision.Shape.Rect(1, 1);
            CollisionObject co = new CollisionObject(shape, null, pos);
            AddCollisionObject(co);
            GameObject go = CreateMesh(co);
        }

        public void CreateATestCircle(Vector3 pos) {
            CollisionShape shape = new Physics.Collision.Shape.Circle(1);
            CollisionObject co = new CollisionObject(shape, null, pos);
            AddCollisionObject(co);
            GameObject go = CreateMesh(co);
        }

        #endregion

        void Update()
        {
            Profiler.BeginSample("[ViE]");
            Tick(Time.deltaTime);
            Profiler.EndSample();
        }

        private void Tick(float timeSpan) {
            tickFrame++;

            CollisionDetection(timeSpan);
            ApplyForces(timeSpan);
            // Resolve(timeSpan);
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
            int projCount = horAABBProjList.Count;
            if (projCount <= 0) {
                return;
            }

            // 升序排序
            if (projCount >= 2) {
                for (int i = 1; i < projCount; i++) {
                    var temp = horAABBProjList[i];
                    for (int j = i - 1; j >= 0; j--) {
                        if (horAABBProjList[j].value > temp.value) {
                            horAABBProjList[j + 1] = horAABBProjList[j];
                            horAABBProjList[j] = temp;
                        } else {
                            break;
                        }
                    }
                }
            }

            List<ProjectionPoint> scanList = new List<ProjectionPoint>();
            // (collisionId, ProjectionPoint)
            Dictionary<int, ProjectionPoint> startPoints = new Dictionary<int, ProjectionPoint>();

            // 在水平方向上检查
            scanList.Add(horAABBProjList[0]);
            startPoints.Add(horAABBProjList[0].collisionObject.id, horAABBProjList[0]);
            for (int i = 1, count = horAABBProjList.Count; i < count; i++) {
                CollisionObject horProjObj = horAABBProjList[i].collisionObject;

                if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalEnd) {
                    ProjectionPoint startPoint = startPoints[horProjObj.id];
                    startPoints.Remove(horProjObj.id);
                    scanList.Remove(startPoint);
                } else if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalStart) {
                    for (int j = 0, scanCount = scanList.Count; j < scanCount; j++) {
                        CollisionObject scanObj = scanList[j].collisionObject;

                        CollisionPair pair = new CollisionPair();
                        int jLevel = scanObj.level;
                        int iLevel = horProjObj.level;

                        // 让碰撞对内碰撞体的顺序规律一致
                        if (jLevel == iLevel) {
                            int iIndex = collisionList.IndexOf(horProjObj);
                            int jIndex = collisionList.IndexOf(scanObj);
                            pair.first = iIndex < jIndex ? horProjObj : scanObj;
                            pair.second = iIndex > jIndex ? horProjObj : scanObj;
                        } else {
                            pair.first = jLevel > iLevel ? horProjObj : scanObj;
                            pair.second = jLevel < iLevel ? horProjObj : scanObj;
                        }

                        broadphasePair.Add(pair);
                    }

                    scanList.Add(horAABBProjList[i]);
                    startPoints.Add(horProjObj.id, horAABBProjList[i]);
                }
            }

            // 在竖直方向上检查已确定的对
            // for (int i = broadphasePair.Count - 1; i >= 0; i--) {
                // CollisionPair pair = broadphasePair[i];
                // CollisionObject first = pair.first;
                // CollisionObject second = pair.second;
                // float fstStart = first.GetProjectionPoint(AABBProjectionType.VerticalStart).value;
                // float fstEnd = first.GetProjectionPoint(AABBProjectionType.VerticalEnd).value;
                // float sndStart = second.GetProjectionPoint(AABBProjectionType.VerticalStart).value;
                // float sndEnd = second.GetProjectionPoint(AABBProjectionType.VerticalEnd).value;
                // if (fstStart <= sndStart && sndStart <= fstEnd ||
                    // sndStart <= fstStart && fstStart <= sndEnd) {
                    //// 在垂直方向上重叠
                    // continue;
                // } else {
                    // broadphasePair.Remove(pair);
                // }
            // }

            for (int i = 0, count = broadphasePair.Count; i < count; i++) {
                CollisionPair pair = broadphasePair[i];
                Debug.Log($"{tickFrame} {pair.first.id}与{pair.second.id}粗检测碰撞");
            }
        }

        private void DynamicBVH() {
        }

        private void NarrowPhase() {
            for (int i = broadphasePair.Count - 1; i >= 0; i--) {
                CollisionPair pair = broadphasePair[i];
                CollisionObject fst = pair.first;
                CollisionObject snd = pair.second;

                if (GJK(fst, snd, out List<Vector3> simplex)) {
                    pair.penetrateVec = EPA(fst, snd, simplex);
                    Debug.Log($"{tickFrame} {fst.id}与{snd.id}窄检测碰撞，穿透向量为{pair.penetrateVec}，长度为{pair.penetrateVec.magnitude}");
                } else {
                    broadphasePair.Remove(pair);
                }
            }
        }

        private bool GJK(CollisionObject fst, CollisionObject snd, out List<Vector3> simplex) {
            int iterCount = 0;

            simplex = new List<Vector3>();
            Vector3 supDir = (snd.position - fst.position).normalized;
            simplex.Add(Support(supDir, fst, snd));
            supDir = -supDir;
            while (true) {
                iterCount++;
                Vector3 supPoint = Support(supDir, fst, snd);

                // 找不到新的单纯形，结束
                if (Vector3.Dot(supPoint, supDir) < 0) {
                    return false;
                }

                simplex.Add(supPoint);
                int vertexCount = simplex.Count;
                if (vertexCount == 3 && PhysicsTool.IsPointInTriangle(
                        simplex[0], simplex[1], simplex[2], Vector3.zero)) {
                    return true;
                }

                if (iterCount > 10) {
                    Debug.LogWarning("[ViE] GJK超时！");
                    return false;
                }

                supDir = FindNextSupDir(simplex);
            }
        }

        private Vector3 EPA(CollisionObject fst, CollisionObject snd, List<Vector3> simplex) {
            if (simplex.Count > 2) {
                // 此处的目的是去掉可能的经过原点的边，让下方穿透向量的计算不会出现异常的0最短距离
                FindNextSupDir(simplex);
            }

            int iterCount = 0;
            while (true) {
                if (++iterCount > 10) {
                    Debug.LogWarning("[ViE] EPA超时");
                    return Vector3.zero;
                }

                Vector3 edgeNormal = Vector3.zero;
                float minDis = float.MaxValue;
                int vertexIndex = 0;
                Vector3 minLineFrom = Vector3.zero;
                Vector3 minLineTo = Vector3.zero;
                // 找到一条离原点最近的边
                for (int i = 0, count = simplex.Count, j = count - 1; i < count; j = i++) {
                    Vector3 lineTo = simplex[j];
                    Vector3 verticalLine = PhysicsTool.GetPerpendicularToOrigin(simplex[i], lineTo);
                    float magnitude = verticalLine.magnitude;
                    if (magnitude < minDis) {
                        edgeNormal = -verticalLine;
                        minDis = magnitude;
                        vertexIndex = j;
                        minLineFrom = simplex[i];
                        minLineTo = lineTo;
                    }
                }
                // 使用该边的远离原点的垂线作为新的迭代方向，寻找一个新的单纯形点
                Vector3 supPoint = Support(edgeNormal, fst, snd);
                if (PhysicsTool.IsPointInPolygon(simplex, supPoint)) {
                    // 如果该点已经包含于单纯形中，返回该向量作为穿透向量
                    Debug.Log($"{tickFrame}  此次EPA最短垂线为{edgeNormal}，距离{minDis}");
                    return edgeNormal;
                } else {
                    // 否则使用该点扩展单纯形
                    simplex.Insert(vertexIndex, supPoint);
                }
            }
        }

        private Vector3 FindNextSupDir(List<Vector3> simplex) {
            if (simplex.Count == 2) {
                Vector3 crossPoint = PhysicsTool.GetPerpendicularToOrigin(simplex[0], simplex[1]);

                if (crossPoint.magnitude < 0.00001f) {
                    // 当单纯形的边经过原点时，上方的计算将失效，因此手动给出一个正交向量
                    crossPoint = PhysicsTool.GetPerpendicularVector(simplex[0], simplex[1]);
                }
                // 取靠近原点的方向
                return -crossPoint;
            } else if (simplex.Count == 3) {
                Vector3 cross20 = PhysicsTool.GetPerpendicularToOrigin(simplex[2], simplex[0]);
                Vector3 cross21 = PhysicsTool.GetPerpendicularToOrigin(simplex[2], simplex[1]);

                // 取靠近原点的方向，然后取其朝向原点的向量作为下一迭代方向
                if (cross20.sqrMagnitude < cross21.sqrMagnitude) {
                    // TODO:[ViE] memory sort consume
                    simplex.RemoveAt(1);
                    return -cross20;
                } else {
                    // TODO:[ViE] memory sort consume
                    simplex.RemoveAt(0);
                    return -cross21;
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
            for (int i = 0, count = broadphasePair.Count; i < count; i++) {
                CollisionPair pair = broadphasePair[i];
                if (pair.first.level == pair.second.level) {
                    pair.first.AddResolveVelocity(pair.penetrateVec / 2);
                    pair.second.AddResolveVelocity(-pair.penetrateVec / 2);
                } else {
                    if (pair.first.level > pair.second.level) {
                        pair.second.AddResolveVelocity(pair.penetrateVec);
                    } else {
                        pair.first.AddResolveVelocity(pair.penetrateVec);
                    }
                }
            }
        }

        private void ApplyVelocity(float timeSpan) {
            // 最后一步
            for (int i = 0, count = collisionList.Count; i < count; i++) {
                CollisionObject co = collisionList[i];

                Vector3 resultantVelocity = co.velocity * timeSpan + co.resolveVelocity;
                co.Translate(resultantVelocity);

                co.ApplyPosition();

                co.CleanResolveVelocity();
            }
        }

        #region Interface

        public bool AddCollisionObject(CollisionObject collisionObject) {
            collisionObject.InitCollisionObject();
            collisionList.Add(collisionObject);

            horAABBProjList.Add(
                collisionObject.GetProjectionPoint(AABBProjectionType.HorizontalStart));
            horAABBProjList.Add(
                collisionObject.GetProjectionPoint(AABBProjectionType.HorizontalEnd));
            // verAABBProjList.Add(
                // collisionObject.GetProjectionPoint(AABBProjectionType.VerticalStart));
            // verAABBProjList.Add(
                // collisionObject.GetProjectionPoint(AABBProjectionType.VerticalEnd));
            return true;
        }

        public bool RemoveCollisionObject(CollisionObject collisionObject) {
            collisionList.Remove(collisionObject);
            return true;
        }

        #endregion
    }
}