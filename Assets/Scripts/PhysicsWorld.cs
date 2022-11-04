using System;
using System.Collections;
using System.Collections.Generic;
using Physics.Collision;
using Physics.Collision.Model;
using Physics.Collision.Shape;
using Physics.Test;
using Physics.Tool;
using UnityEngine;
using UnityEngine.Profiling;
using CollisionFlags = Physics.Collision.CollisionFlags;
using Random = UnityEngine.Random;

namespace Physics {
    public interface IPhysicsWorld {
        public bool AddCollisionObject(CollisionObject collisionObject);
        public bool RemoveCollisionObject(CollisionObject collisionObject);
    }

    public class PhysicsWorld : MonoBehaviour, IPhysicsWorld {
        public int tickFrame = 0;
        public int borderRadius = 15;
        public float epsilon = 0.0001f;
        public int maxIterCount = 10;
        public List<CollisionObject> collisionList;
        private List<CollisionPair> collisionPairs;

        private List<ProjectionPoint> broadScanList;
        private Dictionary<int, ProjectionPoint> broadStartPoints;
        // private List<ProjectionPoint> horAABBProjList;
        private List<ProjectionPoint> verAABBProjList;
        private List<Vector3> simplexList = new List<Vector3>();



        void Start() {
            Application.targetFrameRate = 60;
            collisionList = new List<CollisionObject>();
            collisionPairs = new List<CollisionPair>();

            broadScanList = new List<ProjectionPoint>();
            broadStartPoints = new Dictionary<int, ProjectionPoint>();
            // horAABBProjList = new List<ProjectionPoint>();
            verAABBProjList = new List<ProjectionPoint>();

            // Test0();
            // Test1();
            Test2();
            // Test3();
            // Test4();
        }

        #region Test

        private void Test0() {
            int range = 50;
            for (int i = 0; i < range; i++) {
                // CreateATestRect(new Vector3(Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f)));
                CreateATestRect(Vector3.zero);
            }

            // CreateATestRect(new Vector3(0f, 0, 0f));
            // CreateATestRect(new Vector3(-0.1f, 0, -0.1f), 1);
            // CreateATestRect(new Vector3(0f, 0, 1f), -1);
            // CreateATestRect(new Vector3(0f, 0, -1f));
            // CreateATestRect(new Vector3(0.2f, 0, 0.3f));
            // CreateATestRect(new Vector3(0.3f, 0, -0.2f));
            // CreateATestRect(new Vector3(-0.2f, 0, 0.3f));
            // CreateATestRect(new Vector3(-0.3f, 0, -0.2f));
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
            int range = 30;
            for (int i = 0; i < range; i++) {
                // Vector3 spawnPos = new Vector3(
                    // Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                // CreateATestCircle(1, spawnPos);
                CreateATestCircle(1, Vector3.zero);
            }

            // CreateATestCircle(1, Vector3.zero);
            // CreateATestCircle(1, Vector3.right);
        }

        private void Test3() {
            int range = 1;
            for (int i = 0; i < range; i++) {
                Vector3 spawnPos = new Vector3(
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));

                CreateATestRect(Vector3.zero);
                CreateATestCircle(1, Vector3.zero);
                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(2, 0, -2), new Vector3(-2, 0, -3)},
                    Vector3.zero, 1);
                spawnPos = new Vector3(
                    Random.Range(-0.1f, 0.1f), 0, Random.Range(-0.1f, 0.1f));
                CreateACustomShape(new Vector3[] {
                    new Vector3(-2, 0, 0), new Vector3(0, 0, 1), new Vector3(1, 0, 1),
                    new Vector3(2, 0, 0), new Vector3(2, 0, -2), new Vector3(-2, 0, -3),
                    new Vector3(-2, 0, 0)}, spawnPos, 1);
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
            CollisionObject co = new CollisionObject(shape, null, pos, 0, level);
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

        public void CreateATestRect(Vector3 pos, int level = 0) {
            CollisionShape shape = new Physics.Collision.Shape.Rect(1, 1);
            CollisionObject co = new CollisionObject(shape, null, pos, 0, level);
            AddCollisionObject(co);
            GameObject go = CreateMesh(co);
        }

        public void CreateATestCircle(float radius, Vector3 pos) {
            CollisionShape shape = new Physics.Collision.Shape.Circle(radius);
            CollisionObject co = new CollisionObject(shape, null, pos);
            AddCollisionObject(co);
            GameObject go = CreateMesh(co);
        }

        #endregion

        void Update()
        {
            Tick(Time.deltaTime);
        }

        private void Tick(float timeSpan) {
            tickFrame++;

            Profiler.BeginSample("[ViE] CollisionDetection");
            CollisionDetection(timeSpan);
            Profiler.EndSample();
            Profiler.BeginSample("[ViE] ApplyAcceleration");
            ApplyAcceleration(timeSpan);
            Profiler.EndSample();
            Profiler.BeginSample("[ViE] Resolve");
            Resolve(timeSpan);
            Profiler.EndSample();
            Profiler.BeginSample("[ViE] ApplyVelocity");
            ApplyVelocity(timeSpan);
            Profiler.EndSample();
        }

        #region Collision

        private void CollisionDetection(float timeSpan) {
            Profiler.BeginSample("[ViE] BroadPhase");
            BroadPhase();
            Profiler.EndSample();
            Profiler.BeginSample("[ViE] NarrowPhase");
            NarrowPhase();
            Profiler.EndSample();
        }

        private void BroadPhase() {
            PhysicsCachePool.RecycleCollisionPair(collisionPairs);

            SweepAndPrune();
            // DynamicBVH();
        }

        private void SweepAndPrune() {
            int projCount = verAABBProjList.Count;
            if (projCount <= 0) {
                return;
            }

            // 升序排序
            if (projCount >= 2) {
                for (int i = 1; i < projCount; i++) {
                    var proj = verAABBProjList[i];
                    for (int j = i - 1; j >= 0; j--) {
                        if (verAABBProjList[j].value > proj.value) {
                            verAABBProjList[j + 1] = verAABBProjList[j];
                            verAABBProjList[j] = proj;
                        } else {
                            break;
                        }
                    }
                }
            }

            broadScanList.Clear();
            // (collisionId, ProjectionPoint)
            broadStartPoints.Clear();

            // 在竖直方向上检查
            broadScanList.Add(verAABBProjList[0]);
            broadStartPoints.Add(verAABBProjList[0].collisionObject.id, verAABBProjList[0]);
            for (int i = 1, count = verAABBProjList.Count; i < count; i++) {
                CollisionObject horProjObj = verAABBProjList[i].collisionObject;

                if (verAABBProjList[i].projectionType == AABBProjectionType.VerticalEnd) {
                    ProjectionPoint startPoint = broadStartPoints[horProjObj.id];
                    broadStartPoints.Remove(horProjObj.id);
                    broadScanList.Remove(startPoint);
                } else if (verAABBProjList[i].projectionType == AABBProjectionType.VerticalStart) {
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

                    broadScanList.Add(verAABBProjList[i]);
                    broadStartPoints.Add(horProjObj.id, verAABBProjList[i]);
                }
            }

            // 在水平方向上检查已确定的对
            // for (int i = collisionPairs.Count - 1; i >= 0; i--) {
            //     CollisionPair pair = collisionPairs[i];
            //     CollisionObject first = pair.first;
            //     CollisionObject second = pair.second;
            //     float fstStart = first.GetProjectionPoint(AABBProjectionType.HorizontalStart).value;
            //     float fstEnd = first.GetProjectionPoint(AABBProjectionType.HorizontalEnd).value;
            //     float sndStart = second.GetProjectionPoint(AABBProjectionType.HorizontalStart).value;
            //     float sndEnd = second.GetProjectionPoint(AABBProjectionType.HorizontalEnd).value;
            //     if (fstStart <= sndStart && sndStart <= fstEnd ||
            //         sndStart <= fstStart && fstStart <= sndEnd) {
            //        // 重叠
            //         continue;
            //     } else {
            //         collisionPairs.Remove(pair);
            //     }
            // }

            // for (int i = 0, count = collisionPairs.Count; i < count; i++) {
                // CollisionPair pair = collisionPairs[i];
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

                simplexList.Clear();

                bool isCollided = false;
                bool isAllCircle = fst.shape.shapeType == ShapeType.Circle &&
                        snd.shape.shapeType == ShapeType.Circle;

                if (isAllCircle) {
                    // 圆碰撞对的计算简单且常用，单独处理以加速
                    float fstRadius = ((Circle) fst.shape).radius * fst.scale;
                    float sndRadius = ((Circle) snd.shape).radius * snd.scale;
                    float radiusDis = fstRadius + sndRadius;
                    isCollided = Vector3.Distance(fst.position, snd.position) - radiusDis <= 0;
                    if (isCollided && !fst.flags.HasFlag(CollisionFlags.NoContactResponse) &&
                        !snd.flags.HasFlag(CollisionFlags.NoContactResponse)) {
                        Vector3 oriVec = snd.position - fst.position;
                        float oriDis = oriVec.magnitude;
                        if (oriDis < epsilon) {
                            float separation = Mathf.Max(fstRadius, sndRadius);
                            oriVec = separation * new Vector3(
                                i * tickFrame % 7, 0, i * tickFrame % 17).normalized;
                        } else {
                            oriVec = oriVec.normalized;
                        }
                        pair.penetrateVec = (radiusDis - oriDis) * oriVec;
                    }
                } else {
                    isCollided = GJK(fst, snd, simplexList);
                    if (isCollided && !fst.flags.HasFlag(CollisionFlags.NoContactResponse) &&
                        !snd.flags.HasFlag(CollisionFlags.NoContactResponse)) {
                        pair.penetrateVec = EPA(fst, snd, simplexList);
                    }
                }

                if(!isCollided){
                    PhysicsCachePool.RecycleCollisionPair(pair);
                    collisionPairs.Remove(pair);
                } else {
                    // Debug.Log($"{tickFrame} {fst.id}与{snd.id}窄检测碰撞，穿透向量为{pair.penetrateVec}，长度为{pair.penetrateVec.magnitude}");
                }
            }
        }

        private bool GJK(CollisionObject fst, CollisionObject snd, List<Vector3> simplex) {
            bool isCollision = false;
            Vector3 supDir = Vector3.zero;

            supDir = FindFirstDirection(fst, snd);
            simplex.Add(Support(supDir, fst, snd));
            simplex.Add(Support(-supDir, fst, snd));

            Vector3 fstVertex = simplex[0];
            Vector3 sndVertex = simplex[1];

            supDir = -PhysicsTool.GetClosestPointToOrigin(fstVertex, sndVertex);
            for (int i = 0; i < maxIterCount; ++i) {
                if (supDir.sqrMagnitude < float.Epsilon) {
                    isCollision = true;
                    break;
                }

                Vector3 p = Support(supDir, fst, snd);
                if (Vector3.SqrMagnitude(p - fstVertex) < epsilon ||
                    Vector3.SqrMagnitude(p - sndVertex) < epsilon) {
                    isCollision = false;
                    break;
                }

                simplex.Add(p);

                if (PhysicsTool.IsPointInTriangle(simplex, Vector3.zero)) {
                    isCollision = true;
                    break;
                }

                supDir = FindNextDirection(simplex);
            }

            return isCollision;
        }

        private Vector3 EPA(CollisionObject fst, CollisionObject snd, List<Vector3> simplex) {
            if (simplex.Count > 2) {
                FindNextDirection(simplex);
            }

            SimplexEdge simplexEdge = PhysicsCachePool.GetSimplexEdgeFromPool();
            simplexEdge.InitEdges(simplex);
            Edge curEpaEdge = null;
            float dis = 0;

            for (int i = 0; i < maxIterCount; ++i) {
                Edge e = simplexEdge.FindClosestEdge();
                curEpaEdge = e;

                Vector3 point = Support(e.normal, fst, snd);
                float distance = Vector3.Dot(point, e.normal);
                if (distance - e.distance < epsilon) {
                    dis = distance;
                    break;
                }

                simplexEdge.InsertEdgePoint(e, point);
            }

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

        private Vector3 FindNextDirection(List<Vector3> simplex) {
            int pointCount = simplex.Count;

            if (pointCount == 2) {
                Vector3 crossPoint = PhysicsTool.GetClosestPointToOrigin(
                    simplex[0], simplex[1]);
                return -crossPoint;
            } else if (pointCount == 3) {
                Vector3 crossOnCA = PhysicsTool.GetClosestPointToOrigin(
                    simplex[2], simplex[0]);
                Vector3 crossOnCB = PhysicsTool.GetClosestPointToOrigin(
                    simplex[2], simplex[1]);

                if (crossOnCA.sqrMagnitude < crossOnCB.sqrMagnitude) {
                    simplex.RemoveAt(1);
                    return -crossOnCA;
                } else {
                    simplex.RemoveAt(0);
                    return -crossOnCB;
                }
            } else {
                Debug.Log("[ViE] 单纯形有错误的边数");
                return Vector3.zero;
            }
        }

        private Vector3 Support(Vector3 dir, CollisionObject fst, CollisionObject snd) {
            Vector3 a = fst.GetFarthestPointInDir(dir);
            Vector3 b = snd.GetFarthestPointInDir(-dir);

            Vector3 supPoint = a - b;

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

                float depth = pair.penetrateVec.magnitude;
                float coefficient = 0.4f;
                float tolerance = 0.01f;
                float rate = coefficient * Mathf.Max(0, depth - tolerance);
                Vector3 penetrateDir = pair.penetrateVec.normalized;
                Vector3 resolveVec = rate * penetrateDir;

                if (pair.first.level > pair.second.level) {
                    pair.second.AddResolveVelocity(resolveVec);
                    float externalRate = Vector3.Dot(pair.second.velocity, -penetrateDir) * timeSpan;
                    float resolveRate = Vector3.Dot(pair.second.resolveVelocity, penetrateDir);
                    if (externalRate > resolveRate) {
                        pair.second.AddResolveVelocity((externalRate - resolveRate) * penetrateDir);
                    }
                } else if(pair.first.level < pair.second.level) {
                    pair.first.AddResolveVelocity(-resolveVec);
                    float externalRate = Vector3.Dot(pair.first.velocity, penetrateDir) * timeSpan;
                    float resolveRate = Vector3.Dot(pair.first.resolveVelocity, -penetrateDir);
                    if (externalRate > resolveRate) {
                        pair.first.AddResolveVelocity(-(externalRate - resolveRate) * penetrateDir);
                    }
                } else {
                    float fstExternalRate = Vector3.Dot(pair.first.velocity, penetrateDir) * timeSpan;
                    float fstResolveRate = Vector3.Dot(pair.first.resolveVelocity, -penetrateDir);
                    float sndExternalRate = Vector3.Dot(pair.second.velocity, -penetrateDir) * timeSpan;
                    float sndResolveRate = Vector3.Dot(pair.second.resolveVelocity, penetrateDir);

                    bool fstMoving = fstExternalRate > epsilon;
                    bool sndMoving = sndExternalRate > epsilon;
                    if (fstMoving != sndMoving) {
                        if (fstMoving) {
                            pair.first.AddResolveVelocity(-resolveVec);
                            pair.first.AddResolveVelocity(-(fstExternalRate - fstResolveRate) * penetrateDir);
                        }
                        if (sndMoving) {
                            pair.second.AddResolveVelocity(resolveVec);
                            pair.second.AddResolveVelocity((sndExternalRate - sndResolveRate) * penetrateDir);
                        }
                    } else {
                        pair.first.AddResolveVelocity(-resolveVec);
                        pair.second.AddResolveVelocity(resolveVec);

                        if (Vector3.Dot(pair.first.velocity, penetrateDir) > 0 &&
                            Vector3.Dot(pair.second.velocity, -penetrateDir) > 0) {
                            pair.first.AddResolveVelocity(-(fstExternalRate - fstResolveRate) * penetrateDir);
                            pair.second.AddResolveVelocity((sndExternalRate - sndResolveRate) * penetrateDir);
                        }
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

                // 处理空气墙
                if (!co.flags.HasFlag(CollisionFlags.StaticObject)) {
                    float curDis = Vector3.Distance(co.position, Vector3.zero);
                    float nextDis = Vector3.Distance(co.nextPosition, Vector3.zero);
                    bool curCoInRange = curDis <= borderRadius;
                    bool nextCoInRange = nextDis < borderRadius;
                    if (curCoInRange && !nextCoInRange) {
                        co.nextPosition = (borderRadius - 0.01f) * co.nextPosition.normalized;
                    }
                }

                co.ApplyPosition();

                co.CleanResolveVelocity();
            }
        }

        #region Interface

        public bool AddCollisionObject(CollisionObject collisionObject) {
            collisionObject.InitCollisionObject();
            collisionList.Add(collisionObject);

            // horAABBProjList.Add(
                // collisionObject.GetProjectionPoint(AABBProjectionType.HorizontalStart));
            // horAABBProjList.Add(
                // collisionObject.GetProjectionPoint(AABBProjectionType.HorizontalEnd));
            verAABBProjList.Add(
                collisionObject.GetProjectionPoint(AABBProjectionType.VerticalStart));
            verAABBProjList.Add(
                collisionObject.GetProjectionPoint(AABBProjectionType.VerticalEnd));
            return true;
        }

        public bool RemoveCollisionObject(CollisionObject collisionObject) {
            collisionList.Remove(collisionObject);
            return true;
        }

        #endregion
    }
}