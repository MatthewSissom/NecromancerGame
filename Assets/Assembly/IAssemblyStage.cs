using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAssemblyStage
{
    IEnumerator Execute(GameObject skeleton, IAssemblyStage previous);
}

public interface IAssemblySkeletonChanger
{
    GameObject NewSkeleton { get; }
}

public interface IikTransformProvider
{
    LabledSkeletonData<Transform> Transforms { get; }
    LabledSkeletonData<Transform> ChainEnds { get; }
}

public interface IikTargetProvider
{
    LabledSkeletonData<Transform> Targets { get; }
}

public interface IBehaviourDataProvider
{
    SkeletonLayoutData LayoutData { get; }
}
