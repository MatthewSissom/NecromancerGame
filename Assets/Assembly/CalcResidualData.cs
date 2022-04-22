using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalcResidualData : IAssemblyStage
{
    protected virtual ResidualBoneData GetRootAndInitRBD(GameObject skeleton)
    {
        //creating and populating residual data with initial position/parent-child relations
        foreach (GrabbableGroup bone in TableManager.Instance.boneObjects)
        {
            bone.GetComponent<ResidualBoneData>().PopulateDataFrom(bone);
            bone.GetComponent<ResidualBoneData>().PopulateChildrenFrom(bone);
        }

        ResidualBoneData rootBoneData = null;
        foreach (GrabbableGroup bone in TableManager.Instance.boneObjects)
        {
            TableManager.Instance.residualBoneData.Add(bone.residualBoneData);
            if (bone.isRoot)
            {
                rootBoneData = bone.residualBoneData;
            }
        }
        return rootBoneData;
    }

    IEnumerator IAssemblyStage.Execute(GameObject skeleton, IAssemblyStage previous)
    {
        //assemble list of residual bone datas and identify root bone
        ResidualBoneData rootBoneData = GetRootAndInitRBD(skeleton);

        //construct spine, identify head and tail
        List<Transform> orderedSpine = new List<Transform>();

        //step from root to head
        ResidualBoneData boneDataStepper = rootBoneData.GetChild(BoneVertexType.FrontPrimary);

        ResidualBoneData headBoneData = null;
        if (boneDataStepper == null)
        {
            boneDataStepper = rootBoneData;
            headBoneData = boneDataStepper;
            boneDataStepper.MarkHead();
        } else
        {
            while (boneDataStepper.getAcrossVertex() != null)
            {
                boneDataStepper.MarkShoulderSideSpine();
                orderedSpine.Add(boneDataStepper.transform);
                boneDataStepper = boneDataStepper.GetChild(boneDataStepper.getAcrossVertex().Value);
            }
            headBoneData = boneDataStepper;
            boneDataStepper.MarkHead();
            orderedSpine.Reverse();
            orderedSpine.Add(rootBoneData.transform);
        }

        //step from root to tail
        boneDataStepper = rootBoneData.GetChild(BoneVertexType.BackPrimary);
        while (boneDataStepper.getAcrossVertex() != null)
        {
            boneDataStepper.MarkHipSideSpine();
            orderedSpine.Add(boneDataStepper.transform);
            boneDataStepper = boneDataStepper.GetChild(boneDataStepper.getAcrossVertex().Value);
        }
        ResidualBoneData tailBoneData = boneDataStepper;
        boneDataStepper.MarkTail();

        //find hip and shoulder, looking for bones with most children (limbs)

        //substep 1: categorize bones as three-childers or two-childers
        boneDataStepper = headBoneData;
        ResidualBoneData boneDataStepper2 = tailBoneData;
        bool skipHeadSide = headBoneData == rootBoneData;
        bool skipTailSide = tailBoneData == rootBoneData;
        bool doTailStepFlag = false;
        List<ResidualBoneData> threeChilders = new List<ResidualBoneData>();
        List<ResidualBoneData> twoChilders = new List<ResidualBoneData>();
        List<ResidualBoneData> backups = new List<ResidualBoneData>();
        backups.Add(headBoneData.parentBone == null ? headBoneData : headBoneData.parentBone);
        backups.Add(tailBoneData.parentBone == null ? tailBoneData : tailBoneData.parentBone);
        while(threeChilders.Count < 2 && (boneDataStepper2.parentBone || boneDataStepper.parentBone))
        {
            if(doTailStepFlag && !skipTailSide)
            {
                if(!boneDataStepper2.parentBone)
                {
                    doTailStepFlag = !doTailStepFlag;
                    continue;
                }
                boneDataStepper2 = boneDataStepper2.parentBone;
                if(boneDataStepper2.numChildren >= 3)
                {
                    threeChilders.Add(boneDataStepper2);
                }
                if(boneDataStepper2.numChildren == 2)
                {
                    twoChilders.Add(boneDataStepper2);
                }
            } else if(!skipHeadSide)
            {
                if (!boneDataStepper.parentBone)
                {
                    doTailStepFlag = !doTailStepFlag;
                    continue;
                }
                boneDataStepper = boneDataStepper.parentBone;
                if (boneDataStepper.numChildren >= 3)
                {
                    threeChilders.Add(boneDataStepper);
                }
                if (boneDataStepper.numChildren == 2)
                {
                    twoChilders.Add(boneDataStepper);
                }
            }
            doTailStepFlag = !doTailStepFlag;
        }

        //substep 2: pick which of the selected bones are the shoulder/hip
        //prefer bones with more children, then prefer bones closer to head/tail
        ResidualBoneData shoulderBoneData = null;
        ResidualBoneData hipBoneData = null;
        foreach(ResidualBoneData bd in threeChilders)
        {
            if(shoulderBoneData == bd || hipBoneData == bd)
            {
                continue;
            }
            if(shoulderBoneData == null && bd.isShoulderSideSpine)
            {
                shoulderBoneData = bd;
            } else if(hipBoneData == null && bd.isHipSideSpine)
            {
                hipBoneData = bd;
            }
        }
        foreach (ResidualBoneData bd in threeChilders)
        {
            if (shoulderBoneData == bd || hipBoneData == bd)
            {
                continue;
            }
            if (shoulderBoneData == null)
            {
                shoulderBoneData = bd;
            }
            else if (hipBoneData == null)
            {
                hipBoneData = bd;
            }
        }
        foreach (ResidualBoneData bd in twoChilders)
        {
            if (shoulderBoneData == bd || hipBoneData == bd)
            {
                continue;
            }
            if (shoulderBoneData == null && bd.isShoulderSideSpine)
            {
                shoulderBoneData = bd;
            } else if (hipBoneData == null && bd.isHipSideSpine)
            {
                hipBoneData = bd;
            }
        }
        foreach (ResidualBoneData bd in twoChilders)
        {
            if (shoulderBoneData == bd || hipBoneData == bd)
            {
                continue;
            }
            if (shoulderBoneData == null)
            {
                shoulderBoneData = bd;
            }
            else if (hipBoneData == null)
            {
                hipBoneData = bd;
            }
        }
        foreach (ResidualBoneData bd in backups)
        {
            if (shoulderBoneData == bd || hipBoneData == bd)
            {
                continue;
            }
            if (shoulderBoneData == null && bd.isShoulderSideSpine)
            {
                shoulderBoneData = bd;
            } else if (hipBoneData == null && bd.isHipSideSpine)
            {
                hipBoneData = bd;
            }
        }
        foreach (ResidualBoneData bd in backups)
        {
            if (shoulderBoneData == bd || hipBoneData == bd)
            {
                continue;
            }
            if (shoulderBoneData == null)
            {
                shoulderBoneData = bd;
            }
            else if (hipBoneData == null)
            {
                hipBoneData = bd;
            }
        }

        //@Jimmie null ref here x2
        shoulderBoneData.MarkShoulder();
        hipBoneData.MarkHip();

        ResidualBoneData neck = shoulderBoneData.GetChild(BoneVertexType.FrontPrimary);
        if(!neck)
        {
            neck = shoulderBoneData.parentBone;
        }
        neck.MarkNeck();
        ResidualBoneData tailStart = hipBoneData.GetChild(BoneVertexType.BackPrimary);
        if(!tailStart)
        {
            tailStart = hipBoneData.parentBone;
        }
        tailStart.MarkTailStart();
        //find and mark starts of limbs (just left/right of shoulder/hip)
        BoneVertexType shoulderLeftDir = ResidualBoneData.Left(shoulderBoneData.myParentConnectLocation);
        BoneVertexType shoulderRightDir = ResidualBoneData.Right(shoulderBoneData.myParentConnectLocation);

        ResidualBoneData FLLStart = null;
        ResidualBoneData FRLStart = null;
        if(shoulderBoneData.GetChild(shoulderLeftDir))
        {
            FLLStart = shoulderBoneData.GetChild(shoulderLeftDir);
            shoulderBoneData.GetChild(shoulderLeftDir).MarkFLLStart();
        }
        if (shoulderBoneData.GetChild(shoulderRightDir))
        {
            FRLStart = shoulderBoneData.GetChild(shoulderRightDir);
            shoulderBoneData.GetChild(shoulderRightDir).MarkFRLStart();
        }

        BoneVertexType hipLeftDir = ResidualBoneData.Left(hipBoneData.myParentConnectLocation);
        BoneVertexType hipRightDir = ResidualBoneData.Right(hipBoneData.myParentConnectLocation);

        ResidualBoneData BLLStart = null;
        ResidualBoneData BRLStart = null;
        if (hipBoneData.GetChild(hipLeftDir))
        {
            BRLStart = hipBoneData.GetChild(hipLeftDir);
            hipBoneData.GetChild(hipLeftDir).MarkBRLStart();
        }
        if (hipBoneData.GetChild(hipRightDir))
        {
            BLLStart = hipBoneData.GetChild(hipRightDir);
            hipBoneData.GetChild(hipRightDir).MarkBLLStart();
        }


        //find and mark ends of legs (i.e. feet)
        //also gets bone-bone lengths of limbs
        //(not 100% sure if we needed that but i had it written somewhere in my notes)
        ResidualBoneData FLFoot = null;
        float FLLength = 0;
        ResidualBoneData FRFoot = null;
        float FRLength = 0;
        ResidualBoneData BLFoot = null;
        float BLLength = 0;
        ResidualBoneData BRFoot = null;
        float BRLength = 0;

        boneDataStepper = FLLStart;
        if(boneDataStepper)
        {
            while (boneDataStepper.getAcrossVertex() != null)
            {
                boneDataStepper = boneDataStepper.GetChild(boneDataStepper.getAcrossVertex().Value);
            }
            FLFoot = boneDataStepper;
            FLLength = FLFoot.distanceToParentedBone(null, (ResidualBoneData d) => d.isFLLStart);
            FLLStart.myLegLength = FLLength;

            boneDataStepper.MarkFLLFoot();
        }

        boneDataStepper = FRLStart;
        if (boneDataStepper)
        {
            while (boneDataStepper.getAcrossVertex() != null)
            {
                boneDataStepper = boneDataStepper.GetChild(boneDataStepper.getAcrossVertex().Value);
            }
            FRFoot = boneDataStepper;
            FRLength = FRFoot.distanceToParentedBone(null, (ResidualBoneData d) => d.isFRLStart);
            FRLStart.myLegLength = FRLength;
            boneDataStepper.MarkFRLFoot();
        }

        boneDataStepper = BLLStart;
        if (boneDataStepper)
        {
            while (boneDataStepper.getAcrossVertex() != null)
            {
                boneDataStepper = boneDataStepper.GetChild(boneDataStepper.getAcrossVertex().Value);
            }
            BLFoot = boneDataStepper;
            BLLength = BLFoot.distanceToParentedBone(null, (ResidualBoneData d) => d.isBLLStart);
            BLLStart.myLegLength = BLLength;
            boneDataStepper.MarkBLLFoot();
        }

        boneDataStepper = BRLStart;
        if (boneDataStepper)
        {
            while (boneDataStepper.getAcrossVertex() != null)
            {
                boneDataStepper = boneDataStepper.GetChild(boneDataStepper.getAcrossVertex().Value);
            }
            BRFoot = boneDataStepper;
            BRLength = BRFoot.distanceToParentedBone(null, (ResidualBoneData d) => d.isBRLStart);
            BRLStart.myLegLength = BRLength;
            boneDataStepper.MarkBRLFoot();
        }

        //TODO: head and tail should be empty game objects created on end of head/tail
        //TODO: limb start transforms should be at the offset position
        //TODO: create empty objects at ends of limbs (like pad in testcat)
        
        yield break;
    }
}
