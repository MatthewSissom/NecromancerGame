using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
    Transform transform { get; }
    Rigidbody Rb { get; }
    void PickedUp();
    void Dropped();
}
