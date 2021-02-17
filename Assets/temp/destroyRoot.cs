using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyRoot : MonoBehaviour
{
    [SerializeField]
    string activateOnState;
    [SerializeField]
    int skips;

    public void RemoveChildren()
    {
        if (skips > 0)
        {
            skips--;
            return;
        }

        while (transform.childCount != 0)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(0).parent = null;
        }

        //Destroy(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod(activateOnState, "Begin", RemoveChildren);
    }
}
