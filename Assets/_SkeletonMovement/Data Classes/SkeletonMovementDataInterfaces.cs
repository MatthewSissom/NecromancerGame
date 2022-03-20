using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDelayedTracerData
{
    Transform Transform { get; }
    // delays should not be negitive
    float Delay { get; }
}
