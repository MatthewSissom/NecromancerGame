using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stopwatch : MonoBehaviour
{
    public GameObject hand;

    public void SetHandPercentage(float percent)
    {
        hand.transform.rotation = Quaternion.Euler(-90, 0, percent * 360);
    }
}
