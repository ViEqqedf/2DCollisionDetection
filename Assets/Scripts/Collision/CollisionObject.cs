using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using CustomPhysics.Collision.Model;
using CustomPhysics.Collision.Shape;
using Unity.Mathematics;

namespace CustomPhysics.Collision {
    [Flags]
    public enum CollisionFlags {
        Default = 1 << 0,
        StaticObject = 1 << 1,
        NoContactResponse = 1 << 2,
    }

    public struct CollisionShot {
        public CollisionObject target;
        public float3 penetrateVec;
        public int count;
    }

    public interface ICollisionObject {
        public void InitCollisionObject();
        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType);
        public void SetActive(bool value);
        public CollisionFlags GetFlag();
        public bool HasFlag(CollisionFlags flag);
        public void AddFlag(CollisionFlags flag);
        public void SetFlag(CollisionFlags flag);
        public void RemoveFlag(CollisionFlags flag);
        public float3 GetCurPosition();
        public float GetCurRotation();
        public void SetCurPos(float3 value);
        public void Translate(float3 diff);
        public void TranslateTo(float3 value);
        public void Rotate(float3 diff);
        public void RotateTo(float3 value);
        public void Scale(float diff);
        public void ScaleTo(float value);
        public float3 GetActiveVelocity();
        public void RecordInputMoveVelocity(float3 value);
        public bool AddInputMoveVelocity(float3 diff, float timeSpan);
        public void SetInputMoveVelocity(float3 value);
        public void AddExternalVelocity(float3 diff);
        public void AddAcceleration(Acceleration accelerationInfo);
        public void RemoveAcceleration(Acceleration accelerationInfo);
        public ReadOnlyCollection<int> GetAllShotsFromCo();
        public CollisionShot GetShotByTargetCollisionId(int coId);

        #region CollisionHandle

        public void AddEnterHandle(Action<CollisionObject> action);
        public void RemoveEnterHandle(Action<CollisionObject> action);
        public void AddStayHandle(Action<CollisionObject> action);
        public void RemoveStayHandle(Action<CollisionObject> action);
        public void AddExitHandle(Action<CollisionObject> action);
        public void RemoveExitHandle(Action<CollisionObject> action);

        #endregion
    }

    public class CollisionObject : ICollisionObject{
        private static int publicId = 1;
        public static bool dirtyControlFlag = true;
        public int id { get; private set; }
        public int indexInWorld { get; private set; }
        public CollisionShape shape { get; private set; }
        public bool isActive { get; private set; }
        public CollisionFlags flags { get; private set; }
        public Object contextObject;
        public float3 position { get; private set; }
        public float3 nextPosition { get; private set; }
        public float3 rotation { get; private set; }
        public float scale { get; private set; }
        public int level { get; private set; }

        public List<Acceleration> accelerations;
        public float3 velocity;
        public float3 lastInputMoveVelocity;
        public float3 inputMoveVelocity;
        public float3 lastResolveVelocity;
        public float3 resolveVelocity;

        public Dictionary<int, CollisionShot> collisionShotsDic;
        public ReadOnlyCollection<int> outerReadOnlyCollisionShotList;
        public List<int> collisionShotList;
        public Action<CollisionObject> enterAction;
        public Action<CollisionObject> stayAction;
        public Action<CollisionObject> exitAction;

        public float3[] verticesRefer = null;
        public int verticesCount = 0;

        // TODO: 添加一个脏标记
        public bool isDirty;

        public CollisionObject(CollisionShape shape, Object contextObject,
            float3 startPos, float startRotation = 0, int level = 0) {
            this.id = publicId++;
            this.isActive = true;
            this.shape = shape;
            this.flags = CollisionFlags.Default;
            this.position = startPos;
            this.nextPosition = startPos;
            this.rotation = new float3(0, startRotation, 0);
            this.contextObject = contextObject;
            this.scale = 1;
            this.level = level;
            this.accelerations = new List<Acceleration>();
            this.collisionShotsDic = new Dictionary<int, CollisionShot>();
            this.collisionShotList = new List<int>();
            this.outerReadOnlyCollisionShotList = new ReadOnlyCollection<int>(collisionShotList);
            SetDirty(true);
        }

        public static bool IsSameCollisionObject(CollisionObject obj1, CollisionObject obj2) {
            return obj1.id == obj2.id;
        }

        public void SetIndexInWorld(int index) {
            this.indexInWorld = index;
        }

        public void TryToCreateCollisionShot(CollisionObject target, float3 penetrateVec) {
            int targetId = target?.id ?? -1;
            int oriCount = -2;
            if (collisionShotsDic.TryGetValue(targetId, out CollisionShot shotInDic)) {
                oriCount = shotInDic.count;
            }

            collisionShotsDic[targetId] = new CollisionShot() {
                target = target,
                penetrateVec = penetrateVec,
                count = oriCount == -2 ? 2 : 1,
            };

            if (oriCount == -2) {
                collisionShotList.Add(targetId);
                enterAction?.Invoke(target);
            }
        }

        public bool HasForwardVelocity(float timeSpan) {
            float3 activeVelocity = GetActiveVelocity() * timeSpan;
            return math.dot(activeVelocity, activeVelocity + lastResolveVelocity) > PhysicsWorld.epsilon;
        }

        public void SetNextPosition(float3 nextPos) {
            this.nextPosition = nextPos;
        }

        private void SetDirty(bool value) {
            this.isDirty = value;
        }

        #region Interface

        public void InitCollisionObject() {
            shape.UpdateShape();

            int count = shape.localVertices.Length;
            float3 origin = (shape.aabb.upperBound + shape.aabb.lowerBound) / 2;
            for (int i = 0; i < count; i++) {
                shape.localVertices[i] -= origin;
            }

            shape.UpdateShape();
            shape.ApplyWorldVertices(position, rotation, scale);
            verticesRefer = shape.vertices;
            verticesCount = verticesRefer.Length;
            SetDirty(false);
        }

        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType) {
            return new ProjectionPoint(this, projectionType);
        }

        public void SetActive(bool value) {
            this.isActive = value;
        }

        public CollisionFlags GetFlag() {
            return flags;
        }

        public bool HasFlag(CollisionFlags flag) {
            return flags.HasFlag(flag);
        }

        public void AddFlag(CollisionFlags flag) {
            this.flags |= flag;
        }

        public void SetFlag(CollisionFlags flag) {
            this.flags = flag;
        }

        public void RemoveFlag(CollisionFlags flag) {
            this.flags &= ~flag;
        }

        public float3 GetCurPosition() {
            return position;
        }

        public float GetCurRotation() {
            return rotation.y;
        }

        public void SetCurPos(float3 value) {
            position = nextPosition = value;
            SetDirty(true);
        }

        public void Translate(float3 diff) {
            nextPosition += diff;
            SetDirty(true);
        }

        public void TranslateTo(float3 value) {
            nextPosition = value;
            SetDirty(true);
        }

        public void Rotate(float3 diff) {
            ApplyRotation(rotation + diff);
            SetDirty(true);
        }

        public void RotateTo(float3 value) {
            ApplyRotation(value);
            SetDirty(true);
        }

        public void Scale(float diff) {
            ApplyScale(scale + diff);
            SetDirty(true);
        }

        public void ScaleTo(float value) {
            ApplyScale(value);
            SetDirty(true);
        }

        public float3 GetActiveVelocity() {
            float3 result = velocity + inputMoveVelocity;
            for (int i = 0, count = accelerations.Count; i < count; i++) {
                result += accelerations[i].curVelocity;
            }

            return result;
        }

        public void AddExternalVelocity(float3 diff) {
            this.velocity += diff;
            SetDirty(true);
        }

        public void AddAcceleration(Acceleration accelerationInfo) {
            accelerations.Add(accelerationInfo);
            SetDirty(true);
        }

        public void RemoveAcceleration(Acceleration accelerationInfo) {
            accelerations.Remove(accelerationInfo);
            SetDirty(true);
        }

        public void RecordInputMoveVelocity(float3 value) {
            if (math.distancesq(value, float3.zero) == 0) {
                lastInputMoveVelocity = value;
            }
        }

        public bool AddInputMoveVelocity(float3 diff, float timeSpan) {
            this.inputMoveVelocity += diff;
            SetDirty(true);

            return HasForwardVelocity(timeSpan);
        }

        public void SetInputMoveVelocity(float3 value) {
            this.inputMoveVelocity = value;
            RecordInputMoveVelocity(value);
            SetDirty(true);
        }

        public ReadOnlyCollection<int> GetAllShotsFromCo() {
            return outerReadOnlyCollisionShotList;
        }

        public CollisionShot GetShotByTargetCollisionId(int coId) {
            collisionShotsDic.TryGetValue(coId, out CollisionShot shot);
            return shot;
        }

        public void AddEnterHandle(Action<CollisionObject> action) {
            enterAction += action;
        }

        public void RemoveEnterHandle(Action<CollisionObject> action) {
            enterAction -= action;
        }

        public void AddStayHandle(Action<CollisionObject> action) {
            stayAction += action;
        }

        public void RemoveStayHandle(Action<CollisionObject> action) {
            stayAction -= action;
        }

        public void AddExitHandle(Action<CollisionObject> action) {
            exitAction += action;
        }

        public void RemoveExitHandle(Action<CollisionObject> action) {
            exitAction -= action;
        }
        #endregion

        #region CollisionHandle

        public void ApplyPosition() {
            position = nextPosition;
            if (isDirty) {
                shape.ApplyWorldVertices(position, rotation, scale);
            }
        }

        public void ApplyRotation(float3 newRotation) {
            this.rotation = newRotation;
        }

        public void ApplyScale(float newScale) {
            this.scale = newScale;
        }
        public void AddResolveVelocity(float3 diff) {
            this.resolveVelocity += diff;
        }

        public void SetResolveVelocity(float3 velocity) {
            this.resolveVelocity = velocity;
        }

        public void CleanActiveVelocity() {
            this.lastInputMoveVelocity = this.inputMoveVelocity;

            this.lastResolveVelocity = this.resolveVelocity;
            this.inputMoveVelocity = this.resolveVelocity = float3.zero;
        }

        public float3 GetFarthestPointInDir(float3 dir) {
            float maxDis = float.MinValue;
            float3 farthestPoint = float3.zero;
            for (int i = 0; i < verticesCount; ++i) {
                float3 curPoint = verticesRefer[i];
                float dis = math.dot(curPoint, dir);
                if (dis > maxDis) {
                    maxDis = dis;
                    farthestPoint = curPoint;
                }
            }
            return farthestPoint;
        }

        #endregion
    }
}