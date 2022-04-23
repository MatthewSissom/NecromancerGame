using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertIntoArmature : IAssemblyStage, IikTransformProvider, IAssemblySkeletonChanger
{
    public LabledSkeletonData<Transform> Transforms { get; private set; }
    public LabledSkeletonData<Transform> ChainEnds { get; private set; }
    public GameObject NewSkeleton { get; private set; }

    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        GameObject armature = TableManager.Instance.EmptyArmature;
        armature.transform.parent = null;
        EmptyArmatureData armatureData = armature.GetComponent<EmptyArmatureData>();
        NewSkeleton = armature;

        ResidualBoneData head = null;
        ResidualBoneData neck = null;
        ResidualBoneData shoulder = null;
        ResidualBoneData hip = null;
        ResidualBoneData tail = null;
        ResidualBoneData tailStart = null;
        ResidualBoneData fll = null;
        ResidualBoneData frl = null;
        ResidualBoneData bll = null;
        ResidualBoneData brl = null;
        ResidualBoneData flf = null;
        ResidualBoneData frf = null;
        ResidualBoneData blf = null;
        ResidualBoneData brf = null;
        foreach (ResidualBoneData bone in TableManager.Instance.residualBoneData)
        {
            if (bone.isHead)
            {
                head = bone;
            }
            if (bone.isNeck)
            {
                neck = bone;
            }
            if (bone.isShoulder)
            {
                shoulder = bone;
            }
            if (bone.isHip)
            {
                hip = bone;
            }
            if (bone.isTail)
            {
                tail = bone;
            }
            if (bone.isTailStart)
            {
                tailStart = bone;
            }
            if (bone.isFLLStart)
            {
                fll = bone;
            }
            if (bone.isFRLStart)
            {
                frl = bone;
            }
            if (bone.isBLLStart)
            {
                bll = bone;
            }
            if (bone.isBRLStart)
            {
                brl = bone;
            }
            if (bone.isFLLFoot)
            {
                flf = bone;
            }
            if (bone.isFRLFoot)
            {
                frf = bone;
            }
            if (bone.isBLLFoot)
            {
                blf = bone;
            }
            if (bone.isBRLFoot)
            {
                brf = bone;
            }
        }

        //here's where i'm stuck - the CalcResidualData stage is an IikTransformProvider and has working transforms in its SkeletonTransforms,
        //but it's a couple steps in the past so i can't just look at previous here. I could either make all the intermediate steps also IikTransformProviders
        //and just pass the SkeletonTransforms through, or, rebuild a new SkeletonTransforms by finding all the transforms like above, but both feel like the wrong approach

        //////

        //targets go (positionally only) at the empty game objects at the end of limbs/head/tail
        GameObject headCap    = null;
        GameObject neckCap    = null;
        GameObject shoulderCap    = null;
        GameObject hipCap    = null;
        GameObject tailCap    = null;
        GameObject tailStartCap    = null;
        GameObject fllCap    = null;
        GameObject frlCap    = null;
        GameObject bllCap    = null;
        GameObject brlCap    = null;
        if (head)
        {
            GameObject gameObject = head.AddEmptyCapObject();
            headCap = gameObject;
            armature.transform.Find("Targets/HeadTarget").position = gameObject.transform.position;
        }
        if (tail)
        {
            GameObject gameObject = tail.AddEmptyCapObject();
            tailCap = gameObject;
            armature.transform.Find("Targets/TailTarget").position = gameObject.transform.position;
        }
        if (flf)
        {
            GameObject gameObject = flf.AddEmptyCapObject();
            fllCap = gameObject;
            armature.transform.Find("Targets/FLPawTarget").position = gameObject.transform.position;
        }
        if (frf)
        {
            GameObject gameObject = frf.AddEmptyCapObject();
            frlCap = gameObject;
            armature.transform.Find("Targets/FRPawTarget").position = gameObject.transform.position;
        }
        if (blf)
        {
            GameObject gameObject = blf.AddEmptyCapObject();
            bllCap = gameObject;
            armature.transform.Find("Targets/BLPawTarget").position = gameObject.transform.position;
        }
        if (brf)
        {
            GameObject gameObject = brf.AddEmptyCapObject();
            brlCap = gameObject;
            armature.transform.Find("Targets/BRPawTarget").position = gameObject.transform.position;
        }

        ChainEnds = new SkeletonTransforms(
            headCap == null ? null :     headCap.transform,
        shoulderCap == null ? null : shoulderCap.transform,
             hipCap == null ? null :      hipCap.transform,
            tailCap == null ? null :     tailCap.transform,
             fllCap == null ? null :      fllCap.transform,
             frlCap == null ? null :      frlCap.transform,
             bllCap == null ? null :      bllCap.transform,
             brlCap == null ? null :      brlCap.transform
        );

        //put intermediate gameobjects between each bone, at connection points and at the end of limbs, head, tail
        if (head)
            MakeEmptyConnectors(head);
        if(tail)
            MakeEmptyConnectors(tail);
        if(flf)
            MakeEmptyConnectors(flf);
        if (frf)
            MakeEmptyConnectors(frf);
        if (blf)
            MakeEmptyConnectors(blf);
        if (brf)
            MakeEmptyConnectors(brf);

        //position offsets for legs
        // take first bone on each leg, find its connection point to the parent, use that position
        if (fll)
            armature.transform.Find("Root/FLeftOffset").position = fll.AddEmptyObjectBetweenSelfAndParent().transform.position;
        if (frl) 
            armature.transform.Find("Root/FRightOffset").position = frl.AddEmptyObjectBetweenSelfAndParent().transform.position;
        if (bll)
            armature.transform.Find("Root/Spine/Pelvis/BLeftOffset").position = bll.AddEmptyObjectBetweenSelfAndParent().transform.position;
        if (brl)
            armature.transform.Find("Root/Spine/Pelvis/BRightOffset").position = brl.AddEmptyObjectBetweenSelfAndParent().transform.position;

        //add legs as children of their offsets
        if (fll)
            fll.transform.parent = armature.transform.Find("Root/FLeftOffset");
        if (frl)
            frl.transform.parent = armature.transform.Find("Root/FRightOffset");
        if (bll)
            bll.transform.parent = armature.transform.Find("Root/Spine/Pelvis/BLeftOffset");
        if (brl)
            brl.transform.parent = armature.transform.Find("Root/Spine/Pelvis/BRightOffset");

        //add neck, going forward from root (shoulder) as child of neck
        if(neck)
            neck.transform.parent = armature.transform.Find("Root/Neck");
        //add spine, going back from root (shoulder) and including root as child of spine
        //add pelvis as child of pelvis bone add "pelvis" empty gameobject to end of last spine bone
        if(shoulder)
            shoulder.transform.parent = armature.transform.Find("Root/Spine");
        if(tailStart)
            tailStart.transform.parent = armature.transform.Find("Root/Spine/Pelvis/LowerTail");
        if (hip)
        {
            Transform tailArmatureSpot = armature.transform.Find("Root/Spine/Pelvis/LowerTail");
            Transform pelvisArmatureSpot = armature.transform.Find("Root/Spine/Pelvis");
            pelvisArmatureSpot.parent = hip.parentBone.transform;
            hip.transform.parent = pelvisArmatureSpot;
            armature.transform.Find("Targets/PelvisTarget").position =
                hip.vertexPositions[ResidualBoneData.Opposite(hip.myParentConnectLocation)];

            foreach (Transform child in hip.transform)
            {
                child.transform.parent = tailArmatureSpot;
            }
        }

        //construct skeletonTransforms
        //need to use trinary because of unity's weird custom null
        Transforms = new SkeletonTransforms(
            head    == null ? null : neck     .transform.parent, //if head and neck aren't working,
            shoulder== null ? null : shoulder .transform.parent, //try stepping up/down the skeleton
            hip     == null ? null : hip      .transform.parent, //with more or fewer .parent calls,
            tail    == null ? null : tailStart.transform.parent, //i think this is right though
            fll     == null ? null : fll      .transform.parent,
            frl     == null ? null : frl      .transform.parent,
            bll     == null ? null : bll      .transform.parent,
            brl     == null ? null : brl      .transform.parent
        );

        yield break;
    }

    private void MakeEmptyConnectors(ResidualBoneData tipBone)
    {
        ResidualBoneData stepper = tipBone;
        while (!stepper.isRoot && !stepper.isLegStart)
        {
            stepper.AddEmptyObjectBetweenSelfAndParent();
            stepper = stepper.parentBone;
        }
    }
}
