using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemoveExcessBones : IAssemblyStage
{
    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous)
    {
        Debug.Log("removing excess bones...");
        /*foreach (BoneGroup bone in TableManager.Instance.boneObjects)
        {
            if(!bone.isAttached)
            {
                TableManager.Instance.boneObjects.Remove(bone);
                Object.Destroy(bone.gameObject);
            }
        }*/
        for(int i = 0; i < TableManager.Instance.boneObjects.Count; i++)
        {
            BoneGroup bone = TableManager.Instance.boneObjects[i];
            if (!bone.isAttached)
            {
                TableManager.Instance.boneObjects.RemoveAt(i);
                i--;
                Object.Destroy(bone.gameObject);
            }
        }
        yield break;
    }
}
