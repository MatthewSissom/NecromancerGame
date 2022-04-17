#define USING_IK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USING_IK
using UnityEngine.Assertions;
using RootMotion.FinalIK;
#endif

public class IkInit : IAssemblyStage
{
#if USING_IK
    private class ChainInitData 
    {
        public FABRIK Component { get; private set; }
        public List<Transform> NewChain { get; private set; }
        public ChainInitData(FABRIK component, List<Transform> newChain)
        {
            Component = component;
            NewChain = newChain;
        }
        public static ChainInitData GetData(FABRIK component, List<Transform> newChain)
        {
            return new ChainInitData(component, newChain);
        }
    }
#endif

    // Todo add cat behavior to empty armature
    // PlayPenState.Instance.SetSkeleton(EmptyArmature);

    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous) 
    {
#if USING_IK
        string assertError = "Assembly pipline is set up incorrectly: " + previous.GetType() + 
            " comes before " + GetType() + " but does not implement " + typeof(IikTransformProvider).ToString();
        Assert.IsTrue(previous is IikTransformProvider, assertError);

        RebuildIKChains((previous as IikTransformProvider).Transforms);
#endif

        yield break;
    }

#if USING_IK
    LabledSkeletonData<FABRIK> GetIkComponents(LabledSkeletonData<Transform> chainStarts)
    {
        return chainStarts.Convert((Transform t) => t.GetComponent<FABRIK>());
    }

    LabledSkeletonData<Transform> GetTargets(LabledSkeletonData<FABRIK> chains) 
    {
        Transform GetTransform(FABRIK chain)
        {
            if (chain == null)
                return null;
            return chain.solver?.target;
        }
        return chains.Convert(GetTransform);
    }

    List<Transform> GetTransformList(Transform start, Transform endTarget)
    {
        List<Transform> transforms = new List<Transform>();
        Transform last = null;

        void AddTransform(Transform add)
        {
            transforms.Add(add);
            last = add;
        }
        AddTransform(start);

        while (last.childCount == 0 && (last.position - endTarget.position).magnitude < 0.0001f)
        {
            AddTransform(last.GetChild(0));
        }

        return transforms;
    }

    Transform GetRoot(Transform findParentOf)
    {
        if (findParentOf.parent == null)
            return findParentOf;
        return GetRoot(findParentOf.parent);
    }

    LabledSkeletonData<ChainInitData> PackageNewChainData(LabledSkeletonData<FABRIK> ikComponents, LabledSkeletonData<List<Transform>> chain)
    {
        return ikComponents.Combine(chain, ChainInitData.GetData);
    }

    void RebuildIKChains(SkeletonTransforms transforms)
    {
        // Get basic data
        LabledSkeletonData<FABRIK> ikComponents = GetIkComponents(transforms);
        LabledSkeletonData<Transform> targets = GetTargets(ikComponents);
        LabledSkeletonData<List<Transform>> transformLists = transforms.Combine(targets, GetTransformList);
        FABRIKRoot mFABRIKRoot = GetRoot(transforms.Shoulder).GetComponentInChildren<FABRIKRoot>();

        // Check to make sure no IK chains were missed
        List<FABRIK> ikChains = DebugSearchForChains(GetRoot(transforms.Shoulder));
        foreach (var chain in ikChains)
            Assert.IsTrue(ikComponents.Contains(chain), "Ik chain missed by ik init!");

        // rebuild transform chains
        List<FABRIK> invaildChains = new List<FABRIK>();
        LabledSkeletonData<ChainInitData> newChainData = PackageNewChainData(ikComponents, transformLists);
        foreach(var data in newChainData.ToList())
        {
            IKSolverFABRIK ikSolver = data.Component.solver;
            ikSolver.SetChain(data.NewChain.ToArray(), ikSolver.GetRoot());
            string message = string.Empty;

            bool isValid = !ikSolver.IsValid(ref message);
            if (!isValid)
            {
                Debug.LogError("Failed to init ik chain: " + message);
                invaildChains.Add(data.Component);
                continue;
            }

            data.Component.enabled = isValid;
        }

        // prepare data to be passed to next step, remove disabled transforms
        LabledSkeletonData<Transform> enabledTransforms = newChainData.Convert(
            (ChainInitData data) => invaildChains.Contains(data.Component) ? null : data.NewChain[0]
        );
    }

    List<FABRIK> DebugSearchForChains(Transform skeletonRoot)
    {
        List<FABRIK> fabrikChains = new List<FABRIK>();

        // Recursively search for ikChains
        void SearchForChains(Transform armatureNode)
        {
            FABRIK chain = armatureNode.GetComponent<FABRIK>();
            if (chain)
                fabrikChains.Add(chain);

            for (int i = 0, count = armatureNode.childCount; i < count; i++)
            {
                SearchForChains(armatureNode.GetChild(i));
            }
        }

        SearchForChains(skeletonRoot);
        return fabrikChains;
    }

#endif
}
