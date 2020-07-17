using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class rayPixel: MonoBehaviour
{
    public bool isActive { get; private set; }

    protected Color activeColor;
    protected Color inactiveColor;  

    SpriteRenderer mSprite;

    protected virtual void Start()
    {
        mSprite = gameObject.GetComponent<SpriteRenderer>();
    }

    public virtual int Refresh()
    {
        isActive = Physics.Raycast(new Ray(transform.position, Vector3.up));
        mSprite.color = isActive ? activeColor : inactiveColor;
        return isActive ? 0 : -1;
    }
}
