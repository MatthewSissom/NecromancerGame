using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class GhostAnimationSetter : MonoBehaviour
{
    Animator animator;

    public float percentage;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponentInChildren<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        doOnUpdate();
    }

    private void doOnUpdate()
    {
            animator.Play("New State", 0, (percentage/100)%1);
            animator.speed = 0;
        
        animator.Update(0);
    }
}
