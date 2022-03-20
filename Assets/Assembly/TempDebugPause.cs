using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDebugPause : IAssemblyStage
{
    IEnumerator IAssemblyStage.Execute(GameObject skeleton)
    {
        Debug.Break();
        yield break;
    }

    // TEMP
    bool IAssemblyStage.ExecutedSuccessfully() { return true; }
}
