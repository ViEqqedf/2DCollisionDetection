using System.Collections;
using System.Collections.Generic;
using Collision;
using Collision.Shape;
using UnityEngine;

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
        for (int i = 0, count = broadphasePair.Count; i < count; i++) {
            CollisionPair pair = broadphasePair[i];
            CollisionObject fst = pair.first;
            CollisionObject snd = pair.second;

            List<Vector3> supPoints = new List<Vector3>();
            Vector3 iterDir = (snd.position - fst.position).normalized;
            // Vector3 supPoint = Support(iterDir);
        }
    }

    private Vector3 Support(Vector3 iterDir, Vector3 dirStartPoint, CollisionObject collisionObject) {
        iterDir = iterDir.normalized;
        Vector3 extremeVec = Vector3.zero;
        float exVecDisToOrigin = 0;
        for (int i = 0, count = collisionObject.shape.vertices.Count; i < count; i++) {
            Vector3 curVertex = collisionObject.shape.vertices[i];
            float curProjLength = Vector3.Dot((curVertex - dirStartPoint), iterDir.normalized);
            Vector3 curProjPoint = dirStartPoint + curProjLength * iterDir;
            float curDisToOrigin = Vector3.Distance(curProjPoint, Vector3.zero);
            if (curDisToOrigin > exVecDisToOrigin) {
                extremeVec = curProjPoint;
                exVecDisToOrigin = curDisToOrigin;
            }
        }

        return extremeVec;
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