using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCatState : MonoBehaviour
{
    public void SetVars()
    {
        transform.position = new Vector3(-4, -.5f, -4);
        transform.rotation = Quaternion.Euler(0, 0, 0);
        transform.parent = null;
        GetComponent<TestWalk>().enabled = true;
    }
    public void Start()
    {
        GameManager.Instance.AddEventMethod("CatWalkStart", "Begin", SetVars);
    }
}
