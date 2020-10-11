using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This touch searches for a bone and then becomes another touch based
//on which tool the player currently has selected
public class SearchingTouch : TouchProxy
{
    private float height;

    private BoxCollider myVolume;
    //rad starts smaller and grows larger over a fraction of a second to avoid picking up bones on
    //the outside of the rad instead of bones closer to the center
    private float radMult;

    // Start is called before the first frame update
    void Start()
    {
        myVolume = gameObject.GetComponent<BoxCollider>();
        height = transform.position.y;
        radMult = .1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (radMult < 1)
            radMult += Time.deltaTime * 5;
        transform.up = Camera.main.transform.position - transform.position;
        myVolume.size = new Vector3(radius * 2 * radMult, myVolume.size.y, radius * 2 * radMult);
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        Bone b = other.GetComponentInParent<Bone>();
        if (b)
        {
            FloatingTouch newTouch = InputManager.Instance.ReplaceWith<FloatingTouch>(this);
            b.PickedUp();
            newTouch?.SetBone(b);
        }
    }
}
