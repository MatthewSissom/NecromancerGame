using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDebugPause : IAssemblyStage
{
    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous)
    {
        Debug.Break();
        yield break;
    }

    // TEMP
    bool IAssemblyStage.ExecutedSuccessfully() { return true; }
}
