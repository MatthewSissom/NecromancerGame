using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReassignParents : IAssemblyStage
{
    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous)
    {
        Debug.Log("reassigning parents...");
        foreach (BoneGroup bone in TableManager.Instance.boneObjects)
        {
            if (!bone.isRoot)
            {
                bone.gameObject.transform.parent = bone.Parent.gameObject.transform;
            }
        }
        yield break;
    }

    // TEMP
    bool IAssemblyStage.ExecutedSuccessfully() { return true; }
}
