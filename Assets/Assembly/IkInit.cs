//#define USING_IK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USING_IK
using RootMotion.FinalIK;
#endif

public class IkInit : IAssemblyStage
{
    // Todo add cat behavior to empty armature
    // PlayPenState.Instance.SetSkeleton(EmptyArmature);

    IEnumerator IAssemblyStage.Execute(GameObject skeleton) 
    {
#if USING_IK
        RebuildIKChains(skeleton, out List<TransformChain> validTransformChains);
        AddMovementComponents(skeleton.transform, validTransformChains);
#endif

        yield break;
    }
    // TEMP
    bool IAssemblyStage.ExecutedSuccessfully() { return true; }

#if USING_IK
    List<FABRIK> FindIkChains(GameObject skeletonRoot)
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

        SearchForChains(skeletonRoot.transform);
        return fabrikChains;
    }

    List<TransformChain> GetTransformChains(List<FABRIK> ikChains)
    {
        List<TransformChain> transformChains = new List<TransformChain>();
        foreach (var fChain in ikChains)
        {
            var bones = fChain.solver.bones;
            Transform[] transforms = new Transform[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                transforms[i] = bones[i].transform;
            }
            if (transforms[0].gameObject.name.ToLower() == "spine")
                transformChains.Add(new SpineTransformChain(transforms,
                    null,
                    fChain.solver.target.gameObject,
                    IsOffset(transforms[0].gameObject)
                    ));
            else
                transformChains.Add(
                    new TransformChain(transforms,
                        fChain.solver.target.gameObject,
                        IsOffset(transforms[0].gameObject)
                    ));
        }
        return transformChains;
    }

    void RebuildIKChains(GameObject skeleton, out List<TransformChain> validTransformChains)
    {
        List<FABRIK> ikChains = FindIkChains(skeleton);
        List<TransformChain> currentTransformChains = GetTransformChains(ikChains);
        FABRIKRoot mFABRIKRoot = skeleton.GetComponentInChildren<FABRIKRoot>();


        // create and clean up transform chains
        List<TransformChain> emptyChains = new List<TransformChain>();
        for(int i = 0; i < ikChains.Count; i++)
        {
            var fChain = ikChains[i];
            var tChain = currentTransformChains[i];

            tChain.DestroyAdditionalEmpties();

            if (tChain.Count() > 1)
            {
                fChain.solver.SetChain(tChain.GetList().ToArray(), fChain.solver.GetRoot());
                string message = string.Empty;
                if (!fChain.solver.IsValid(ref message))
                    emptyChains.Add(tChain);
            }
            else
            {
                emptyChains.Add(tChain);
            }
        }

        mFABRIKRoot.enabled = true;
        validTransformChains = new List<TransformChain>();
        for (int i = 0; i < ikChains.Count; i++)
        {
            bool isValid = !emptyChains.Contains(currentTransformChains[i]);
            ikChains[i].enabled = isValid;
            if (isValid)
                validTransformChains.Add(currentTransformChains[i]);
        }
    }

    void AddMovementComponents(Transform skeletonTransform, List<TransformChain> transformChains)
    {
        //limb data
        List<LimbEnd> limbEnds = new List<LimbEnd>();
        List<LimbEnd.LimbLocationTag> locations = new List<LimbEnd.LimbLocationTag>();
        List<LimbEnd.LimbTag> types = new List<LimbEnd.LimbTag>();

        //movement data
        List<Transform> spineAlignedTargets = new List<Transform>();
        //always add the root as a key target
        spineAlignedTargets.Add(skeletonTransform);

        int frontIndex = -1; //holds the index of a front leg in the array
        int backIndex = -1;  //holds the index of a back leg in the array

        Transform chainTargets = skeletonTransform.GetChild(1);
        foreach (var tChain in transformChains)
        {
            var transforms = tChain.GetList();
            var start = transforms[0].gameObject;

            GameObject target = tChain.Target;
            if (!target)
                continue;
            var endGO = transforms[transforms.Count - 1].gameObject;
            target.transform.position = endGO.transform.position;

            //check if the chain is spine aligned
            if (!IsOffset(start))
            {
                var tKey = tChain.Target.name.Substring(0, 2);
                switch (tKey)
                {
                    case "Ta":
                        spineAlignedTargets.Add(tChain.Target.transform);
                        continue;
                    case "Pe":
                        spineAlignedTargets.Add(tChain.Target.transform);
                        continue;
                    case "He":
                        spineAlignedTargets.Add(tChain.Target.transform);
                        continue;
                }
                continue;
            }

            void UpdateTags(ref int index)
            {
                if (index == -1) //first front leg added, add it as a single limb
                {
                    index = types.Count;
                    if(tChain.Count() > 2)
                        types.Add(LimbEnd.LimbTag.Single);
                    else
                        types.Add(LimbEnd.LimbTag.StumpSingle);

                }
                else
                {
                    //update old tags
                    if (types[index] == LimbEnd.LimbTag.Single)
                        types[index] = LimbEnd.LimbTag.Pair; 
                    if (types[index] == LimbEnd.LimbTag.StumpSingle)
                        types[index] = LimbEnd.LimbTag.Stump;

                    //add new tag
                    if (tChain.Count() > 2)
                        types.Add(LimbEnd.LimbTag.Pair);
                    else
                        types.Add(LimbEnd.LimbTag.Stump);
                }
            }

            //use the first two letters to find the matching target
            string key = start.gameObject.name.Substring(0, 2);
            switch (key)
            {
                //update tags for limb ends
                case "FR":
                    locations.Add(LimbEnd.LimbLocationTag.FrontRight);
                    UpdateTags(ref frontIndex);
                    break;
                case "FL":
                    locations.Add(LimbEnd.LimbLocationTag.FrontLeft);
                    UpdateTags(ref frontIndex);
                    break;
                case "BR":
                    locations.Add(LimbEnd.LimbLocationTag.BackRight);
                    UpdateTags(ref backIndex);
                    break;
                case "BL":
                    locations.Add(LimbEnd.LimbLocationTag.BackLeft);
                    UpdateTags(ref backIndex);
                    break;
                default:
                    Debug.LogError("Limb key: " + key + " for object " + start.gameObject.name + " not recognized");
                    break;
            }
            var newLimbMarker = endGO.AddComponent<LimbEnd>();
            newLimbMarker.LimbInit(
                tChain.WorldLength(),
                target,
                start
                );
            limbEnds.Add(newLimbMarker);
        }

        Vector3 forward = skeletonTransform.forward;
        int IsCloserToTail(Transform t1, Transform t2)
        {
            return System.Math.Sign(
                Vector3.Dot(forward, t1.position) - Vector3.Dot(forward, t2.position)
                );
        }
        spineAlignedTargets.Sort(IsCloserToTail);
        int shoulderIndex = spineAlignedTargets.IndexOf(skeletonTransform);

        //tags can only be set once all limbs have been processed, because other limbs can change types
        for (int i = 0; i < limbEnds.Count; i++)
        {
            //find the nearest target on the spine
            float minDistance = float.MaxValue;
            int minIndex = -1;
            for(int j = 0; j < spineAlignedTargets.Count; j++)
            {
                var target = spineAlignedTargets[j];
                var distFromTarget = (target.position - limbEnds[i].LimbStart.transform.position).magnitude;
                if (distFromTarget < minDistance)
                {
                    minIndex = j;
                    minDistance = distFromTarget;
                }
            }
            limbEnds[i].SetTags(types[i],locations[i],minIndex);
        }


        float[] distances = new float[spineAlignedTargets.Count];
        for(int i = 0; i < distances.Length; i++)
        {
            distances[i] = (spineAlignedTargets[i].transform.position - skeletonTransform.transform.position).magnitude;
        }

        var behavior = skeletonTransform.gameObject.AddComponent<CatBehavior>();
        behavior.BehaviorInit(
            limbEnds,
            spineAlignedTargets.ToArray(),
            distances,
            shoulderIndex
            );
    }

    bool IsOffset(GameObject limbTransform)
    {
        return limbTransform.name.EndsWith("Offset");
    }

#endif
}
