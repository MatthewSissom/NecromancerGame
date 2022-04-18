using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InsertIntoArmature : IAssemblyStage, IikTransformProvider
{
    public LabledSkeletonData<Transform> Transforms { get; private set; }

    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        GameObject armature = TableManager.Instance.EmptyArmature;
        EmptyArmatureData armatureData = armature.GetComponent<EmptyArmatureData>();

        ResidualBoneData head = null;
        ResidualBoneData neck = null;
        ResidualBoneData shoulder = null;
        ResidualBoneData hip = null;
        ResidualBoneData tail = null;
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
        if(head)
            armature.transform.Find("Targets/HeadTarget").position = head.AddEmptyCapObject().transform.position;
        if (tail)
            armature.transform.Find("Targets/TailTarget").position = tail.AddEmptyCapObject().transform.position;
        if (flf)
            armature.transform.Find("Targets/FLPawTarget").position = flf.AddEmptyCapObject().transform.position;
        if (frf)
            armature.transform.Find("Targets/FRPawTarget").position = frf.AddEmptyCapObject().transform.position;
        if (blf)
            armature.transform.Find("Targets/BLPawTarget").position = blf.AddEmptyCapObject().transform.position;
        if (brf)
            armature.transform.Find("Targets/BRPawTarget").position = brf.AddEmptyCapObject().transform.position;

        //put intermediate gameobjects between each bone, at connection points and at the end of limbs, head, tail
        if(head)
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
        if (hip)
        {
            Transform pelvisArmatureSpot = armature.transform.Find("Root/Spine/Pelvis");
            pelvisArmatureSpot.parent = hip.parentBone.transform;
            hip.transform.parent = pelvisArmatureSpot;
            armature.transform.Find("Targets/PelvisTarget").position =
                hip.vertexPositions[ResidualBoneData.Opposite(hip.myParentConnectLocation)];
            foreach (Transform child in hip.transform)
            {
                child.transform.parent = pelvisArmatureSpot.Find("LowerTail");
            }
        }

        //construct skeletonTransforms
        //need to use trinary because of unity's weird custom null
        Transforms = new SkeletonTransforms(
            head    == null ? null : head    .transform,
            shoulder== null ? null : shoulder.transform,
            hip     == null ? null : hip     .transform,
            tail    == null ? null : tail    .transform,
            fll     == null ? null : fll     .transform,
            frl     == null ? null : frl     .transform,
            bll     == null ? null : bll     .transform,
            brl     == null ? null : brl     .transform
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
