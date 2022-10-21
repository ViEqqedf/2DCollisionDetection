using System;
using System.Collections.Generic;
using UnityEngine;

namespace Collision.Shape {
    public enum AABBProjectionType {
        HorizontalStart,
        HorizontalEnd,
        VerticalStart,
        VerticalEnd,
    }

    public class ProjectionPoint {
        public CollisionObject collisionObject;
        public AABBProjectionType projectionType;
        public float value;
    }

    public class AABB {
        private Vector3 lowerBound;
        private Vector3 upperBound;
        // [ViE]: 'int' is for GC optimization
        private Dictionary<int, ProjectionPoint> projectionPoints;

        public AABB(CollisionObject collisionObject, Vector3 lowerBound, Vector3 upperBound) {
            this.lowerBound = new Vector3();
            this.upperBound = new Vector3();
            this.projectionPoints = new Dictionary<int, ProjectionPoint>();
            this.projectionPoints.Add((int) AABBProjectionType.HorizontalStart,
                new ProjectionPoint() {
                    projectionType = AABBProjectionType.HorizontalStart,
                    collisionObject = collisionObject,
                });
            this.projectionPoints.Add((int) AABBProjectionType.HorizontalEnd,
                new ProjectionPoint() {
                    projectionType = AABBProjectionType.HorizontalEnd,
                    collisionObject = collisionObject,
                });
            this.projectionPoints.Add((int) AABBProjectionType.VerticalStart,
                new ProjectionPoint() {
                    projectionType = AABBProjectionType.VerticalStart,
                    collisionObject = collisionObject,
                });
            this.projectionPoints.Add((int) AABBProjectionType.VerticalEnd,
                new ProjectionPoint() {
                    projectionType = AABBProjectionType.VerticalEnd,
                    collisionObject = collisionObject,
                });

            Apply(lowerBound, upperBound);
            UpdateAllProjectionPoint();
        }

        /// <summary>
        /// NOTE: Only using in AABB Tree build
        /// </summary>
        public AABB(Vector3 lowerBound, Vector3 upperBound) {
            this.lowerBound = new Vector3();
            this.upperBound = new Vector3();

            Apply(lowerBound, upperBound);
            UpdateAllProjectionPoint();
        }

        public void Apply(Vector3 lower, Vector3 upper) {
            this.lowerBound.Set(lower.x, lower.y, lower.z);
            this.upperBound.Set(upper.x, upper.y, upper.z);
        }

        public ProjectionPoint GetProjectionPoint(AABBProjectionType projectionType) {
            return UpdateProjectionPoint(projectionType);
        }

        public ProjectionPoint UpdateProjectionPoint(AABBProjectionType projectionType) {
            ProjectionPoint point = projectionPoints[(int) projectionType];

            switch (projectionType) {
                case AABBProjectionType.HorizontalStart:
                    point.value = lowerBound.x + point.collisionObject.position.x;
                    break;
                case AABBProjectionType.HorizontalEnd:
                    point.value = upperBound.x + point.collisionObject.position.x;
                    break;
                case AABBProjectionType.VerticalStart:
                    point.value = lowerBound.z + point.collisionObject.position.z;
                    break;
                case AABBProjectionType.VerticalEnd:
                    point.value = upperBound.z + point.collisionObject.position.z;
                    break;
                default:
                    throw new Exception("获得AABB投影点时传入了不存在的投影类型");
            }

            return point;
        }

        public void UpdateAllProjectionPoint() {
            UpdateProjectionPoint(AABBProjectionType.HorizontalStart);
            UpdateProjectionPoint(AABBProjectionType.HorizontalEnd);
            UpdateProjectionPoint(AABBProjectionType.VerticalStart);
            UpdateProjectionPoint(AABBProjectionType.VerticalEnd);
        }
    }
}