using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssemblyStage
{
    IEnumerator Execute(GameObject skeleton);
    bool ExecutedSuccessfully();
}
