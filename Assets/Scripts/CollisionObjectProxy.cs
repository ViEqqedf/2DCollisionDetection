using System;
using Physics.Collision;
using UnityEngine;

namespace Physics {
    public class CollisionObjectProxy : MonoBehaviour {
        public CollisionObject target;
        public bool IsInControl = false;
        public int velocity = 7;

        private void Update() {
            transform.position = target.position;

            if (IsInControl) {
                if (Input.GetKeyDown(KeyCode.A)) {
                    target.AddVelocity(velocity * Vector3.left);
                }
                if (Input.GetKeyDown(KeyCode.D)) {
                    target.AddVelocity(velocity * Vector3.right);
                }
                if (Input.GetKeyDown(KeyCode.W)) {
                    target.AddVelocity(velocity * Vector3.forward);
                }
                if (Input.GetKeyDown(KeyCode.S)) {
                    target.AddVelocity(velocity * Vector3.back);
                }

                if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
                    Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) {
                    target.SetVelocity(Vector3.zero);
                }
            }
        }
    }
}