using UnityEngine;

namespace CustomPhysics.Collision {
    public enum AccelerationType {
        ForAWhile,
        ReachTargetVelocity,
    }

    public abstract class Acceleration {
        public AccelerationType type;
        public Vector3 acceleration;
        public Vector3 curVelocity;
        public bool isEnded = false;

        public Acceleration(AccelerationType type, Vector3 acceleration,
            Vector3 initVelocity = new Vector3()) {
            this.type = type;
            this.acceleration = acceleration;
            this.curVelocity = initVelocity;
        }

        public abstract void Tick(float timeSpan);
    }

    public class AccelerationForAWhile : Acceleration {
        public float remainingTime = 0;

        public AccelerationForAWhile(float duration, Vector3 acceleration, float initVelocity) :
            base(AccelerationType.ForAWhile, acceleration) {
            this.remainingTime = duration;
            this.curVelocity = initVelocity * acceleration.normalized;
        }

        public override void Tick(float timeSpan) {
            if (timeSpan <= 0) {
                isEnded = true;
                return;
            }

            curVelocity += acceleration * timeSpan;
            remainingTime -= timeSpan;
        }
    }

    public class AccelerationForTargetVelocity : Acceleration {
        public Vector3 targetVelocity;
        public Vector3 lastVelocity;

        public AccelerationForTargetVelocity(Vector3 acceleration,
            float targetVelocityValue, float initVelocityValue) :
            base(AccelerationType.ReachTargetVelocity, acceleration) {
            this.targetVelocity = targetVelocityValue * acceleration.normalized;
            this.curVelocity = initVelocityValue * acceleration.normalized;
            this.lastVelocity = curVelocity;
        }

        public override void Tick(float timeSpan) {
            float targetX = targetVelocity.x;
            float targetZ = targetVelocity.z;
            if (lastVelocity.x > targetX != curVelocity.x > targetX ||
                lastVelocity.z > targetZ != curVelocity.z > targetZ) {
                isEnded = true;
                return;
            }

            lastVelocity = curVelocity;
            curVelocity += acceleration * timeSpan;
        }
    }
}