using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    public GameObject hand;
    public float angle = 0;

    private void Awake()
    {
        angle = 0;
    }

    public void SetHandPercentage(float percent)
    {
        var newAngle = percent * 360;
        hand.transform.RotateAround(hand.transform.position, Vector3.up, newAngle - angle);
        angle = newAngle;
    }
}
