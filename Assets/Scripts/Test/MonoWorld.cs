using System.Collections;
using System.Collections.Generic;
using CustomPhysics;
using UnityEngine;

public class MonoWorld : MonoBehaviour {
    public PhysicsWorld world;

    // Start is called before the first frame update
    void Start() {
        world = new PhysicsWorld();
        world.Init();
    }

    void Update()
    {
        world.Tick(Time.deltaTime);
    }
}