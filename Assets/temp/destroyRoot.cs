using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyRoot : MonoBehaviour
{
    [SerializeField]
    int skips = default;

    public void RemoveChildren()
    {
        if (!this)
            return;
        if (skips > 0)
        {
            skips--;
            return;
        }
        gameObject.SetActive(true);
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod(typeof(GhostManager), "End", RemoveChildren);
        gameObject.SetActive(false);
    }
}
