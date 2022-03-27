using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveGrabbableInfo : IAssemblyStage
{
    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous)
    {
        Debug.Log("removing grabbable info...");
        foreach (BoneGroup bone in TableManager.Instance.boneObjects)
        {
            bone.CleanUpIndicators();
        }
        yield break;
    }

    // TEMP
    bool IAssemblyStage.ExecutedSuccessfully() { return true; }
}
