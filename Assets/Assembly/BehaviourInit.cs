using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class BehaviourInit : IAssemblyStage
{
    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        string assertError = "Assembly pipline is set up incorrectly: " + previous.GetType() +
            " comes before " + GetType() + " but does not implement " + typeof(IBehaviourDataProvider).ToString();
        Assert.IsTrue(previous is IBehaviourDataProvider, assertError);

        IBehaviourDataProvider dataProvider = previous as IBehaviourDataProvider;

        Transform root = skeleton.transform;
        while (root.parent != null)
            root = root.parent;
        SkeletonBehaviour behaviour = root.GetComponent<SkeletonBehaviour>();
        behaviour.BehaviorInit(
            dataProvider.LayoutData,
            new SkeletonPathTunables(.1f, behaviour.Speed) //TEMP
            );

        PlayPenState.Instance.SetSkeleton(skeleton);
        yield break;
    }
}
