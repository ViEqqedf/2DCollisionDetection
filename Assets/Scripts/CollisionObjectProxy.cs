using System;
using Physics.Collision;
using Unity.VisualScripting;
using UnityEngine;

namespace Physics {
    public class CollisionObjectProxy : MonoBehaviour {
        public CollisionObject target;
        public bool IsInControl = false;

        private void Update() {
            transform.position = target.position;

            if (IsInControl) {
                if (Input.GetKey(KeyCode.A)) {
                    target.Translate(0.02f * Vector3.left);
                } else if (Input.GetKey(KeyCode.D)) {
                    target.Translate(0.02f * Vector3.right);
                } else if (Input.GetKey(KeyCode.W)) {
                    target.Translate(0.02f * Vector3.forward);
                } else if (Input.GetKey(KeyCode.S)) {
                    target.Translate(0.02f * Vector3.back);
                }
            }
        }
    }
}