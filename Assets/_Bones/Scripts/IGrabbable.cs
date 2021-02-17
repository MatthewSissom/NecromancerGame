using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGrabbable
{
    Transform transform { get; }
    void PickedUp();
    void Dropped();
}
