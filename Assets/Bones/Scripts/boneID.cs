using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boneID : ScriptableObject
{
    // Start is called before the first frame update
    private int id = 1;

    public void OnEnable()
    {
        id = 1;
    }

    public int GetID()
    {
        return id++;
    }

}
