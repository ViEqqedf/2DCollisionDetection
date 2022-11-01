using System;
using Physics.Collision;
using Physics.Collision.Shape;
using UnityEngine;

namespace Physics {
    public class CollisionObjectProxy : MonoBehaviour {
        public CollisionObject target;
        public ShapeType shape;
        public bool isInControl = false;
        public int nP = 1;
        public int velocity = 7;

        private void Update() {
            transform.position = target.position;
            transform.rotation = Quaternion.Euler(target.rotation);
            transform.localScale = target.scale;
            shape = target.shape.shapeType;

            if (isInControl) {
                if (nP == 1) {
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
                    if (Input.GetKey(KeyCode.G)) {
                        target.Rotate(new Vector3(0, -1, 0));
                    }
                    if (Input.GetKey(KeyCode.H)) {
                        target.Rotate(new Vector3(0, 1, 0));
                    }
                    if (Input.GetKeyDown(KeyCode.B)) {
                        target.Scale(Vector3.one);
                    }
                    if (Input.GetKeyDown(KeyCode.N)) {
                        target.Scale(-Vector3.one);
                    }

                    if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.D) ||
                        Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.S)) {
                        target.SetVelocity(Vector3.zero);
                    }
                } else if (nP == 2) {
                    if (Input.GetKeyDown(KeyCode.LeftArrow)) {
                        target.AddVelocity(velocity * Vector3.left);
                    }
                    if (Input.GetKeyDown(KeyCode.RightArrow)) {
                        target.AddVelocity(velocity * Vector3.right);
                    }
                    if (Input.GetKeyDown(KeyCode.UpArrow)) {
                        target.AddVelocity(velocity * Vector3.forward);
                    }
                    if (Input.GetKeyDown(KeyCode.DownArrow)) {
                        target.AddVelocity(velocity * Vector3.back);
                    }

                    if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.DownArrow) ||
                        Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.RightArrow)) {
                        target.SetVelocity(Vector3.zero);
                    }
                }
            }
        }
    }
}