using System;
using System.Collections.Generic;
using UnityEngine;

namespace Physics.Collision.Shape {
    public enum AABBProjectionType {
        HorizontalStart,
        HorizontalEnd,
        VerticalStart,
        VerticalEnd,
    }

    public class ProjectionPoint {
        public CollisionObject collisionObject;
        public AABBProjectionType projectionType;
        public float value {
            get {
                // TODO: 添加一个脏标记
                UpdateProjectionPoint();
                return _value;
            }
        }
        private float _value;

        public ProjectionPoint(CollisionObject collisionObject, AABBProjectionType projectionType) {
            this.collisionObject = collisionObject;
            this.projectionType = projectionType;
        }

        private void UpdateProjectionPoint() {
            AABB aabb = collisionObject.shape.aabb;

            switch (projectionType) {
                case AABBProjectionType.HorizontalStart:
                    _value = aabb.lowerBound.x + collisionObject.position.x;
                    break;
                case AABBProjectionType.HorizontalEnd:
                    _value = aabb.upperBound.x + collisionObject.position.x;
                    break;
                case AABBProjectionType.VerticalStart:
                    _value = aabb.lowerBound.z + collisionObject.position.z;
                    break;
                case AABBProjectionType.VerticalEnd:
                    _value = aabb.upperBound.z + collisionObject.position.z;
                    break;
                default:
                    throw new Exception("获得AABB投影点时传入了不存在的投影类型");
            }
        }
    }

    public class AABB {
        public Vector3 lowerBound { get; private set; }
        public Vector3 upperBound { get; private set; }
        // [ViE]: 'int' is for GC optimization

        public AABB(Vector3 lowerBound, Vector3 upperBound) {
            this.lowerBound = new Vector3();
            this.upperBound = new Vector3();
            Apply(lowerBound, upperBound);
        }

        public void Apply(Vector3 lower, Vector3 upper) {
            this.lowerBound = new Vector3(lower.x, lower.y, lower.z);
            this.upperBound = new Vector3(upper.x, upper.y, upper.z);
        }
    }
}