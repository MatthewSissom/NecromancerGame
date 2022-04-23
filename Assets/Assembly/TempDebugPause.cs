using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempDebugPause : IAssemblyStage
{
    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous)
    {
#if UNITY_EDITOR
        Debug.Break();
#endif
        yield break;
    }
}
