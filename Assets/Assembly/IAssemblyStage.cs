using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssemblyStage
{
    IEnumerator Execute(GameObject skeleton, IAssemblyStage previous);
}

public interface IikTransformProvider
{
    SkeletonTransforms Transforms { get; }
}

public interface IikTargetProvider
{
    SkeletonTransforms Targets { get; }
}
