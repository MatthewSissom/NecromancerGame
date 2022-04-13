using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertIntoArmature : IAssemblyStage
{
    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        GameObject armature = TableManager.Instance.EmptyArmature;
        EmptyArmatureData armatureData = armature.GetComponent<EmptyArmatureData>();


        yield break;
    }
}
