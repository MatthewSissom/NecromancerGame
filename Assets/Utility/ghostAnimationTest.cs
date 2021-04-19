using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ghostAnimationTest : MonoBehaviour
{
    public bool minor;
    public bool major;
    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        minor = false;
        major = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(minor)
        {
            minor = false;
            animator.SetTrigger(0);
        }
        if (major)
        {
            major = false;
            animator.SetTrigger(1);
        }
    }
}
