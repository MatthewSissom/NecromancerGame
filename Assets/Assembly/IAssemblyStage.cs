using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssemblyStage
{
    IEnumerator Execute(GameObject skeleton, IAssemblyStage previous);
}

public interface IikTransformProvider
{
    LabledSkeletonData<Transform> Transforms { get; }
}

public interface IikTargetProvider
{
    LabledSkeletonData<Transform> Targets { get; }
}

public interface IBehaviourDataProvider
{
    SkeletonLayoutData LayoutData { get; }
}
