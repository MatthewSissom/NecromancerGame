//#define USING_IK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USING_IK
using UnityEngine.Assertions;
using RootMotion.FinalIK;
#endif

public class IkInit : IAssemblyStage, IikTransformProvider, IikTargetProvider
{
    public LabledSkeletonData<Transform> Transforms { get; private set; }
    public LabledSkeletonData<Transform> Targets { get; private set; }

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
        FABRIK GetFabrikComponent(Transform t)
        {
            if (t == null)
                return null;
            FABRIK chain =  t.GetComponent<FABRIK>();
            return chain;
        }
        return chainStarts.Convert(GetFabrikComponent);
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
        if (start == null || endTarget == null)
        {
            Assert.IsTrue(start == null, "IkInit Error: Valid chain start has no target.");
            return null;
        }

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

    void RebuildIKChains(LabledSkeletonData<Transform> transforms)
    {
        // Assume that all chains are invalid to start
        List<FABRIK> invaildChains = SearchHigharchyForChains(GetRoot(transforms.Shoulder));

        // Get basic data
        LabledSkeletonData<FABRIK> ikComponents = GetIkComponents(transforms);
        LabledSkeletonData<Transform> targets = GetTargets(ikComponents);
        LabledSkeletonData<List<Transform>> transformLists = transforms.Combine(targets, GetTransformList);
        FABRIKRoot mFABRIKRoot = GetRoot(transforms.Shoulder).GetComponentInChildren<FABRIKRoot>();


        // Rebuild transform chains
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
                continue;
            }

            data.Component.enabled = isValid;
            if(data.Component.enabled)
                invaildChains.Remove(data.Component);
        }

        // prepare data to be passed to next step, remove disabled transforms
        newChainData = newChainData.Convert(
            (ChainInitData data) => invaildChains.Contains(data.Component) ? null : data
        );
        Transforms = newChainData.Convert(
            (ChainInitData data) => data?.NewChain[0]
        );
        Targets = newChainData.Convert(
            (ChainInitData data) => (data == null ? null : data)?.Component?.solver?.target
        );
    }

    List<FABRIK> SearchHigharchyForChains(Transform skeletonRoot)
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
