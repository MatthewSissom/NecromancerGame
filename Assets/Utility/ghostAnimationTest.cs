using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ghostAnimationTest : MonoBehaviour
{
    public bool minor;
    public bool major;
    public bool randomShock;
    public float shockMagnitude;

    public GameObject follow;
    Vector3 followPos;

    GhostBehavior ghost;
    GhostPhysics physics;

    // Start is called before the first frame update
    void Start()
    {
        minor = false;
        major = false;
        ghost = GetComponent<GhostBehavior>();
        physics = ghost.body;
    }

    // Update is called once per frame
    void Update()
    {
        if(minor)
        {
            ghost.MinorShock();
            minor = false;
        }
        if (major)
        {
            ghost.MajorShock();
            major = false;
        }
        if(randomShock)
        {
            Vector3 random = Random.insideUnitSphere;
            physics.rb.AddForce(random * shockMagnitude);
            randomShock = false;
        }
        if(follow && follow.transform.position != followPos)
        {
            followPos = follow.transform.position;
            physics.MoveToPosition(followPos, .2f, true);
        }
    }
}
