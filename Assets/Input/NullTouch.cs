using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//work around for touches that shouldn't do anything but still have a finger on the screen
public class NullTouch : TouchProxy
{
    override protected void Update() {}
    protected override void OnTriggerEnter(Collider other) {}
}
