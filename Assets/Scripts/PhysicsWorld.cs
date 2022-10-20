using System.Collections;
using System.Collections.Generic;
using Collision;
using UnityEngine;

public interface IPhysicsWorld {
    public bool AddCollisionObject(CollisionObject collisionObject);
    public bool RemoveCollisionObject(CollisionObject collisionObject);
}

public class PhysicsWorld : MonoBehaviour, IPhysicsWorld {
    public List<CollisionObject> collisionList;

    void Start() {
        collisionList = new List<CollisionObject>();
    }

    void Update()
    {
        Tick(0.033f);
    }

    private void Tick(float timeSpan) {
        CollisionDetection(timeSpan);
    }

    #region Collision

    private void CollisionDetection(float timeSpan) {
        BroadPhase();
        NarrowPhase();
    }

    private void BroadPhase() {
        #region Sweep And Prune

        for (int i = 0, count = collisionList.Count; i < count; i++) {

        }

        #endregion
    }

    private void NarrowPhase() {

    }

    #endregion

    #region Interface

    public bool AddCollisionObject(CollisionObject collisionObject) {
        collisionObject.InitCollisionObject();

        collisionList.Add(collisionObject);
        return true;
    }

    public bool RemoveCollisionObject(CollisionObject collisionObject) {
        collisionList.Remove(collisionObject);
        return true;
    }

    #endregion
}