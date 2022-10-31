using System;
using System.Collections;
using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Model;
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
        public float epsilon = 0.0001f;
        public int maxIterCount = 10;
        public List<CollisionObject> collisionList;

        private List<ProjectionPoint> broadScanList;
        private Dictionary<int, ProjectionPoint> broadStartPoints;
        private List<ProjectionPoint> horAABBProjList;
        // private List<ProjectionPoint> verAABBProjList;

        private List<CollisionPair> collisionPairs;

        void Start() {
            Application.targetFrameRate = 60;
            collisionList = new List<CollisionObject>();
            collisionPairs = new List<CollisionPair>();

            broadScanList = new List<ProjectionPoint>();
            broadStartPoints = new Dictionary<int, ProjectionPoint>();
            horAABBProjList = new List<ProjectionPoint>();
            // verAABBProjList = new List<ProjectionPoint>();

            Test0();
            // Test1();
            // Test2();
            // Test3();
            // Test4();
        }

        #region Test

        private void Test0() {
            int range = 50;
            for (int i = 0; i < range; i++) {
                CreateATestRect(new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f)));
            }

            // CreateATestRect(Vector3.zero);
            // CreateATestRect(new Vector3(0.5f, 0, 0.5f));
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
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

                CreateATestRect(Vector3.zero);
                // CreateACustomShape(new Vector3[] {
                    // new Vector3(-2, 0, 0), new Vector3(0, 0, 1),
                    // new Vector3(2, 0, 0), new Vector3(0, 0, -1)}, spawnPos, 0);

                spawnPos = new Vector3(
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(2, 0, -2), new Vector3(-2, 0, -3),
                    new Vector3(-2, 0, 0)}, spawnPos, 0);
                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(2, 0, -2), new Vector3(-2, 0, -3),
                    new Vector3(-2, 0, 0)}, spawnPos, 0);
            }
        }

        private void Test4() {
            CreateACustomShape(new Vector3[] {Vector3.zero, new Vector3(-1.54f, 0, 4.75f),
                new Vector3(2.5f, 0, 7.69f), new Vector3(6.54f, 0, 4.75f), new Vector3(5, 0, 0),},
                Vector3.zero, 0);

            CreateACustomShape(new Vector3[] {Vector3.zero, new Vector3(-1.54f, 0, 4.75f),
                new Vector3(2.5f, 0, 7.69f), new Vector3(6.54f, 0, 4.75f), new Vector3(5, 0, 0),},
                Vector3.zero, 0);
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
            CollisionShape shape = new Physics.Collision.Shape.Circle(2);
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
            ApplyAcceleration(timeSpan);
            // Resolve(timeSpan);
            ApplyVelocity(timeSpan);
        }

        #region Collision

        private void CollisionDetection(float timeSpan) {
            BroadPhase();
            NarrowPhase();
        }

        private void BroadPhase() {
            PhysicsCachePool.RecycleCollisionPair(collisionPairs);

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
                    var proj = horAABBProjList[i];
                    for (int j = i - 1; j >= 0; j--) {
                        if (horAABBProjList[j].value > proj.value) {
                            horAABBProjList[j + 1] = horAABBProjList[j];
                            horAABBProjList[j] = proj;
                        } else {
                            break;
                        }
                    }
                }
            }

            broadScanList.Clear();
            // (collisionId, ProjectionPoint)
            broadStartPoints.Clear();

            // 在水平方向上检查
            broadScanList.Add(horAABBProjList[0]);
            broadStartPoints.Add(horAABBProjList[0].collisionObject.id, horAABBProjList[0]);
            for (int i = 1, count = horAABBProjList.Count; i < count; i++) {
                CollisionObject horProjObj = horAABBProjList[i].collisionObject;

                if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalEnd) {
                    ProjectionPoint startPoint = broadStartPoints[horProjObj.id];
                    broadStartPoints.Remove(horProjObj.id);
                    broadScanList.Remove(startPoint);
                } else if (horAABBProjList[i].projectionType == AABBProjectionType.HorizontalStart) {
                    for (int j = 0, scanCount = broadScanList.Count; j < scanCount; j++) {
                        CollisionObject scanObj = broadScanList[j].collisionObject;

                        CollisionPair pair = PhysicsCachePool.GetCollisionPairFromPool();
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

                        collisionPairs.Add(pair);
                    }

                    broadScanList.Add(horAABBProjList[i]);
                    broadStartPoints.Add(horProjObj.id, horAABBProjList[i]);
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

            // for (int i = 0, count = broadphasePair.Count; i < count; i++) {
                // CollisionPair pair = broadphasePair[i];
                // Debug.Log($"{tickFrame} {pair.first.id}与{pair.second.id}粗检测碰撞");
            // }
        }

        private void DynamicBVH() {
        }

        private void NarrowPhase() {
            for (int i = collisionPairs.Count - 1; i >= 0; i--) {
                CollisionPair pair = collisionPairs[i];
                CollisionObject fst = pair.first;
                CollisionObject snd = pair.second;

                if (GJK(fst, snd, out Simplex simplex)) {
                    Profiler.BeginSample("[ViE] EPA");
                    pair.penetrateVec = EPA(fst, snd, simplex);
                    Profiler.EndSample();
                    // Debug.Log($"{tickFrame} {fst.id}与{snd.id}窄检测碰撞，穿透向量为{pair.penetrateVec}，长度为{pair.penetrateVec.magnitude}");
                } else {
                    collisionPairs.Remove(pair);
                }
            }
        }

        private bool GJK(CollisionObject fst, CollisionObject snd, out Simplex simplex) {
            Profiler.BeginSample("[ViE] GJK");
            simplex = PhysicsCachePool.GetSimplexFromPool();
            bool isCollision = false;
            Vector3 supDir = Vector3.zero;

            supDir = FindFirstDirection(fst, snd);
            simplex.Add(Support(supDir, fst, snd));
            simplex.Add(Support(-supDir, fst, snd));

            Vector3 fstVertex = simplex.GetVertex(0);
            Vector3 sndVertex = simplex.GetVertex(1);

            supDir = -PhysicsTool.GetClosestPointToOrigin(fstVertex, sndVertex);
            for (int i = 0; i < maxIterCount; ++i) {
                if (supDir.sqrMagnitude < float.Epsilon) {
                    isCollision = true;
                    break;
                }

                SupportPoint p = Support(supDir, fst, snd);
                if (Vector3.SqrMagnitude(p.point - fstVertex) < epsilon ||
                    Vector3.SqrMagnitude(p.point - sndVertex) < epsilon) {
                    isCollision = false;
                    break;
                }

                simplex.Add(p);

                if (simplex.Contains(Vector3.zero)) {
                    isCollision = true;
                    break;
                }

                supDir = FindNextDirection(simplex);
            }

            Profiler.EndSample();
            return isCollision;
        }

        private Vector3 EPA(CollisionObject fst, CollisionObject snd, Simplex simplex) {
            if (simplex.PointCount() > 2) {
                FindNextDirection(simplex);
            }

            SimplexEdge simplexEdge = PhysicsCachePool.GetSimplexEdgeFromPool();
            simplexEdge.InitEdges(simplex);
            Edge curEpaEdge = null;
            float dis = 0;

            for (int i = 0; i < maxIterCount; ++i) {
                Edge e = simplexEdge.FindClosestEdge();
                curEpaEdge = e;

                SupportPoint point = Support(e.normal, fst, snd);
                float distance = Vector3.Dot(point.point, e.normal);
                if (distance - e.distance < epsilon) {
                    dis = distance;
                    break;
                }

                simplexEdge.InsertEdgePoint(e, point);
            }

            PhysicsCachePool.RecycleSimplex(simplex);
            PhysicsCachePool.RecycleSimplexEdge(simplexEdge);

            return dis * curEpaEdge.normal;
        }

        public Vector3 FindFirstDirection(CollisionObject a, CollisionObject b) {
            Vector3 dir = a.shape.vertices[0] - b.shape.vertices[0];
            if (dir.sqrMagnitude < epsilon) {
                dir = a.shape.vertices[1] - b.shape.vertices[0];
            }
            return dir;
        }

        private Vector3 FindNextDirection(Simplex simplex) {
            int pointCount = simplex.PointCount();

            if (pointCount == 2) {
                Vector3 crossPoint = PhysicsTool.GetClosestPointToOrigin(
                    simplex.GetVertex(0), simplex.GetVertex(1));
                return -crossPoint;
            } else if (pointCount == 3) {
                Vector3 crossOnCA = PhysicsTool.GetClosestPointToOrigin(
                    simplex.GetVertex(2), simplex.GetVertex(0));
                Vector3 crossOnCB = PhysicsTool.GetClosestPointToOrigin(
                    simplex.GetVertex(2), simplex.GetVertex(1));

                if (crossOnCA.sqrMagnitude < crossOnCB.sqrMagnitude) {
                    simplex.Remove(1);
                    return -crossOnCA;
                } else {
                    simplex.Remove(0);
                    return -crossOnCB;
                }
            } else {
                Debug.Log("[ViE] 单纯形有错误的边数");
                return Vector3.zero;
            }
        }

        private SupportPoint Support(Vector3 dir, CollisionObject fst, CollisionObject snd) {
            Vector3 a = fst.GetFarthestPointInDir(dir);
            Vector3 b = snd.GetFarthestPointInDir(-dir);

            SupportPoint supPoint = PhysicsCachePool.GetSupPointFromPool();
            supPoint.point = a - b;
            supPoint.fromA = a;
            supPoint.fromB = b;

            return supPoint;
        }

        #endregion

        private void ApplyAcceleration(float timeSpan) {
            for (int i = 0, count = collisionList.Count; i < count; i++) {
                collisionList[i].velocity += collisionList[i].acceleration * timeSpan;
            }
        }

        private void Resolve(float timeSpan) {
            for (int i = 0, count = collisionPairs.Count; i < count; i++) {
                CollisionPair pair = collisionPairs[i];
                if (pair.first.level == pair.second.level) {
                    pair.first.AddResolveVelocity(-pair.penetrateVec / 2);
                    pair.second.AddResolveVelocity(pair.penetrateVec / 2);
                } else {
                    if (pair.first.level > pair.second.level) {
                        pair.second.AddResolveVelocity(pair.penetrateVec);
                    } else {
                        pair.first.AddResolveVelocity(-pair.penetrateVec);
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