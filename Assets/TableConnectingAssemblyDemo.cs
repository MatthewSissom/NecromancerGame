using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableConnectingAssemblyDemo : MonoBehaviour
{
    [SerializeField]
    Transform testBonesRoot;

    // Start is called before the first frame update
    void Start()
    {
        // enable gameobjects if proper state is set
        if (DebugModes.StateMode == DebugModes.EStateDebugMode.AssemblyAndPlayPen)
        {
            for (int i = 0; i < testBonesRoot.childCount; i++)
            {
                testBonesRoot.GetChild(i).gameObject.SetActive(true);
            }
        }
    }
}
