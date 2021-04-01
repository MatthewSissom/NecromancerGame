//#define USING_IK

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if USING_IK
using RootMotion.FinalIK;
#endif


public class BoneAssembler : State
{
    [SerializeField]
    //root gameObject for connection colliders, the higharchy of this gameObject and the
    //empty armature must be identical except for objects with the TableConnectionArea component
    Transform connectionColliders = default;
    Transform emptyArmature = default;

    //list of all connection areas
    List<TableConnectionArea> connectionAreas;
    //holds all areas that belong to a limb starting from sections closest to the body
    List<List<TableConnectionArea>> limbs;
    //dictionary of joints in the armature that correspond to connecction areas on the table
    Dictionary<TableConnectionArea,Transform> areaToJoint;
    Dictionary<Transform, TableConnectionArea> jointToArea;

    //!!!!!!!!!!! WILL DON'T COMMENT THESE OUT !!!!!!!!!!!!//
    //instead comment out the "#define USING_IK" at the top of this file
#if USING_IK
    FABRIKRoot mFABRIKRoot;
    List<FABRIK> fabrikChains;
    List<TransformChain> transformChains;
#endif

    //total number of joints, conneciton areas, and targets
    int count;

    //axis data
    [SerializeField]
    BoneAxis boneAxisDict = default;
    [SerializeField]
    BoneAxis tableAxisDict = default;

    private void ResetAssembler()
    {
        emptyArmature = FindObjectOfType<TableManager>().EmptyArmature.transform;
        areaToJoint = new Dictionary<TableConnectionArea, Transform>();
        jointToArea = new Dictionary<Transform, TableConnectionArea>();

#if USING_IK
        mFABRIKRoot = emptyArmature.GetComponentInChildren<FABRIKRoot>();
        fabrikChains = new List<FABRIK>();
#endif

        ConnectionAreasInit();

#if USING_IK
        transformChains = new List<TransformChain>();
        foreach (var fChain in fabrikChains)
        {
            var bones = fChain.solver.bones;
            Transform[] transforms = new Transform[bones.Length];
            for (int i = 0; i < bones.Length; i++)
            {
                transforms[i] = bones[i].transform;
            }
            transformChains.Add(
                new TransformChain(transforms,
                    fChain.solver.target.gameObject,
                    IsOffset(transforms[0].gameObject)
                ));
        }
#endif
    }

    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.AddEventMethod(typeof(GameInit), "End", ResetAssembler);
    }

    void ConnectionAreasInit()
    {
        connectionAreas = new List<TableConnectionArea>();

        //Queue for bredth first search. Both armature and connection transforms are stored in the same structure
        //values should always be enqueued / dequeued in pairs with aramature nodes coming first.
        Queue<Transform> checkNext = new Queue<Transform>();

        //method which does a bredth first search of the table higharchy and checks each node for a connection area
        //if an area is found that area and it's corresponding transform in the armature will be added to data structures
        void BFSearchInit(Transform armatureNode, Transform connectionNode)
        {
            TableConnectionArea area = null;

            //search all children for a connection area
            for (int i = 0, count = connectionNode.childCount; i < count; i++)
            {
                //check for connection area
                if (!area)
                {
                    area = connectionNode.GetChild(i).GetComponent<TableConnectionArea>();
                    //connection areas can't be children of connection areas so end recursion
                    if (area)
                        continue;
                }

                //if no connection area was found search deeper in the tree
                checkNext.Enqueue(armatureNode.GetChild(i));
                checkNext.Enqueue(connectionNode.GetChild(i));
            }

#if USING_IK
            FABRIK chain = armatureNode.GetComponent<FABRIK>();
            if (chain)
                fabrikChains.Add(chain);
#endif

            //empty nodes shouldn't be added to data structures
            if (!area)
                return;

            connectionAreas.Add(area);
            areaToJoint.Add(area, armatureNode);
            jointToArea.Add(armatureNode, area);

        }

        //check root values and search the entire tree
        BFSearchInit(emptyArmature, connectionColliders);
        while (checkNext.Count > 0)
        {
            BFSearchInit(checkNext.Dequeue(), checkNext.Dequeue());
        }

        count = connectionAreas.Count;
    }


    //assembles the skeleton
    public override IEnumerator Routine()
    {
        Begin();

        //early return if connection areas weren't initalized
        if (connectionAreas == null || areaToJoint == null)
        {
            End();
            yield break;
        }

        yield return StartCoroutine(MoveBonesToArmature());

        //init cat cat components
        RebuildIKChains();
        AddMovementComponents();

        End();
        yield break;
    }

    private void BoneCleanUp(Bone b)
    {
        Destroy(b);
        Destroy(b.GetComponent<CustomGravity>());
        Destroy(b.GetComponent<Rigidbody>());
        Destroy(b.GetComponent<BoneGroup>());
    }

    private IEnumerator MoveBonesToArmature()
    {
        //the current connection area
        TableConnectionArea connectionArea;
        //the current joint
        Transform armatureNode;

        //a list of bones that are attached to a table connection areaa
        List<Bone> bones;
        //used to make sure that bones are only a part of one connection area group
        HashSet<Bone> seenBones = new HashSet<Bone>();

        //loop though all connection areas in the table and move their bones into the armature
        for (int i = 0; i < count; i++)
        {
            connectionArea = connectionAreas[i];
            bones = connectionArea.GetAllBones();

            //check to see if bones are already a part of another group, if so remove them from this group
            List<Bone> toRemove = new List<Bone>();
            foreach (Bone b in bones)
            {
                if (seenBones.Contains(b))
                {
                    toRemove.Add(b);
                    continue;
                }
                else
                    seenBones.Add(b);
            }
            foreach (Bone b in toRemove)
            {
                bones.Remove(b);
            }
            //if there are no bones, continue and don't update this bone's chain
            //so that this space in the higharchy can be filled by lower bones
            if (bones.Count == 0)
            {
                continue;
            }

            armatureNode = areaToJoint[connectionArea];
#if USING_IK
            //check to see if there are empty nodes higher in the node's chain
            foreach (var tChain in transformChains)
            {
                if (tChain.Contains(armatureNode))
                {
                    armatureNode = tChain.MoveToFirstEmpty(armatureNode);
                    break;
                }
            }
#endif

            //move bones to their final position in the skeleton
            Vector3 totalOffset;         //how much the bone will need to move
            Bone bestBone = null;        //the bone that best aligns with the connection area
            int bestAxisIndex = default; //the axis of the bone that best alignes with the connection area
            if (tableAxisDict[connectionArea.gameObject.name] != null)
            {
                totalOffset = GetOffsetWithTableAxis(out bestBone, out bestAxisIndex, bones, connectionArea, jointToArea[armatureNode]);
                if (float.IsNaN(totalOffset.x) || float.IsNaN(totalOffset.y) || float.IsNaN(totalOffset.z))
                    Debug.Log("Invalid offset from offset with table axis");
            }
            else
            {
                totalOffset = GetOffsetWithJoint(bones, armatureNode);
                if (float.IsNaN(totalOffset.x) || float.IsNaN(totalOffset.y) || float.IsNaN(totalOffset.z))
                    Debug.Log("Invalid offset from offset with table joints");
            }
            yield return StartCoroutine(OffsetBonesOverTime(bones, totalOffset));

            //update armature to match the newly added bones
            if (bestBone != null)
            {
                SetJointPosition(armatureNode, bestBone, connectionArea, bestAxisIndex);
            }

            //make bones children of the armature
            foreach (Bone b in bones)
            {
                b.transform.parent = armatureNode;
                BoneCleanUp(b);
            }

            AudioManager.Instance.PlaySound("normal");
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator OffsetBonesOverTime(List<Bone> bones, Vector3 offset)
    {
        const float timePerBone = 0.2f; //how to reach the destination
        float timer = 0;
        Vector3 distMoved = new Vector3();
        while (timer < timePerBone)
        {
            timer += Time.deltaTime;
            Vector3 currentOffset = offset * Time.deltaTime / timePerBone;
            distMoved += currentOffset;
            foreach (Bone b in bones)
            {
                b.transform.position += currentOffset;
            }
            yield return null;
        }
        //ensure all bones end at propper location
        foreach (Bone b in bones)
        {
            b.transform.position += offset - distMoved;
        }
    }

    //returns the amount the bones need to move, which bone has the main axis, and the index of that axis
    //originalCA is the connection area that the bones were originally placed in, chainCA is the connection
    //area which corresponds to the bone's position in the chain
    private Vector3 GetOffsetWithTableAxis(out Bone bestBone, out int bestAxisIndex, List<Bone> bones, TableConnectionArea originalCA, TableConnectionArea chainCA)
    {
        //search for the bone axis which aligns best with the area it was placed in
        bestBone = null;
        float bestScore = float.NegativeInfinity;
        bestAxisIndex = 0;
        foreach (Bone b in bones)
        {
            float score = BoneScore(b, originalCA, out int tempIndex);
            if (score > bestScore)
            {
                bestBone = b;
                bestAxisIndex = tempIndex;
                bestScore = score;
            }
        }

        //get the world axis based on the axis search
        List<Vector3> bestAxis = boneAxisDict.GetWorldAxis(bestBone.AxisKey, bestBone.transform.localToWorldMatrix, bestAxisIndex);
        if (bestAxis == null || bestAxis.Count == 0)
        {
            Debug.LogError("Invalid bone axis for " + bestBone.name);
            return new Vector3();
        }

        //check neighbors of the for newAxis to attach to
        int toAttachToIndex= chainCA.chainNeighbors.FindIndex(ca => (ca && ca.newAxis != null && ca.newAxis.Count > 0));
        //if there are no chain neighbors then place the bone roughly in the middle of the corresponding joint in the armature
        if (toAttachToIndex == -1)
        {
            //move the midpoint of the bone to the midpoint of the armature
            Vector3 bestAxMidpoint = (bestAxis[0] + bestAxis[1]) / 2;
            Vector3 target = GetRoughJointMidpoint(areaToJoint[chainCA]);

            return target - bestAxMidpoint;
        }

        //if the object is down chain / farther from the root
        bool isDownChain = toAttachToIndex == 0;
        //if the object is down chain then connect to the point that's not at the origin (index 1) otherwise 
        //the bone is upchain and should connect to the other point (index 0)
        Vector3 toAttachTo = chainCA.chainNeighbors[toAttachToIndex].newAxis[isDownChain? 1 : 0];

        //indexes for closest points
        int p1 = 0;
        float minDistance = float.PositiveInfinity;
        //if there is an axis to attach to, set total offset so that the closest pair of points will connect
        for (int x = 0; x < 2; x++)
        {
            float dist = (bestAxis[x] - toAttachTo).magnitude;
            if (minDistance > dist)
            {
                p1 = x;
                minDistance = dist;
            }
        }

        return toAttachTo - bestAxis[p1];
    }

    private Vector3 GetRoughJointMidpoint(Transform joint)
    {
        int childCount = joint.childCount;
        Vector3 childAvg;
        if (childCount == 1)
        {
            childAvg = joint.GetChild(0).position;
        }
        else
        {
            childAvg = new Vector3();
            for (int c = 0; c < childCount; c++)
            {
                childAvg += joint.GetChild(c).position;
            }
            childAvg /= childCount;
        }
        return (joint.position + childAvg) / 2;
    }

    private Vector3 GetOffsetWithJoint(List<Bone> bones, Transform joint)
    {
        Vector3 boneAvg;
        if (bones.Count == 1)
        {
            boneAvg = bones[0].transform.position;
        }
        else
        {
            boneAvg = new Vector3();
            for (int i = 0; i < bones.Count; i++)
            {
                boneAvg += bones[i].transform.position;
            }
            boneAvg /= bones.Count;
        }

        //move the bones to the midpoint of the joint and it's children
        return GetRoughJointMidpoint(joint) - boneAvg;
    }

    //returns a score for how closely the bone is aligned with it's connectionArea
    private float BoneScore(Bone bone, TableConnectionArea connectionArea, out int axisIndex)
    {
        //half the distance of a spinechunk
        const float distancePenaltyBaseline = 0.04f;
        const float distanceWeight = 1;
        const float angleWeight = 1;

        float score = -10000;
        axisIndex = 0;

        List<Vector3> boneAxies;
        List<Vector3> tableAxies;

        boneAxies = boneAxisDict.GetAllWorldAxis(bone.AxisKey, bone.transform.localToWorldMatrix);
        if (boneAxies == null)
        {
            Debug.LogError("null bone axies for key :\"" + (bone.AxisKey == null ? "null" : bone.AxisKey) + "\" in bone " + bone.name);
        }
        tableAxies = tableAxisDict.GetAllWorldAxis(connectionArea.gameObject.name, connectionArea.transform.localToWorldMatrix);

        if (boneAxies ==null || tableAxies == null)
        {
            return score;
        }

        //remove y component to make calculations more precise
        for(int i = 0; i< boneAxies.Count; i++)
        {
            Vector3.Scale(boneAxies[i],new Vector3(1,0,1));
        }
        for(int i = 0; i < tableAxies.Count; i++)
        {
            Vector3.Scale(tableAxies[i], new Vector3(1, 0, 1));
        }

        float tempScore;
        for(int i = 0; i < Mathf.Min(boneAxies.Count, tableAxies.Count);i+=2)
        {
            tempScore = 0;
            
            //score for closeness to points, find the distance between the closest pairs of points
            tempScore += Mathf.Min(
                (tableAxies[0] - boneAxies[i]).magnitude + (tableAxies[1] - boneAxies[i + 1]).magnitude,
                (tableAxies[0] - boneAxies[i+1]).magnitude + (tableAxies[1] - boneAxies[i]).magnitude
                );
            tempScore = distanceWeight * (1 - (1 / distancePenaltyBaseline * tempScore));

            //score for alignment with axis
            float angle = Vector3.Angle(
                tableAxies[0] - tableAxies[1],
                boneAxies[i] - boneAxies[i + 1]
                );
            angle = angle > 90? 180 - angle : angle;
            tempScore += angleWeight * (1 - angle / 90);

            if(tempScore > score)
            {
                axisIndex = i;
                score = tempScore;
            }
        }

        return score;
    }

    private void SetJointPosition(Transform joint, Bone movedBone, TableConnectionArea connectionArea, int axisIndex)
    {
        int selectedAxisIndex;
        //points aren't organized when returned from the boneAxisDict, so they must be sorted
        var newAxis = boneAxisDict.GetWorldAxis(movedBone.AxisKey, movedBone.transform.localToWorldMatrix, axisIndex);

        int referenceAxis = connectionArea.chainNeighbors.FindIndex(ca => (ca && ca.newAxis != null && ca.newAxis.Count > 0));
        //if no referenceAxis is found choose the point that's highest in the body and closest to the head
        if (referenceAxis == -1)
        {
            if (newAxis[0].z - newAxis[0].x > newAxis[1].z - newAxis[1].x)
                selectedAxisIndex = 0;
            else
                selectedAxisIndex = 1;
        }
        //otherwise check the reference axis for which points are which
        else
        {
            //choose the point that's at the oppisite index of their axis array because if the bone is up chain then
            //we want our point to be closest to it's down chain point and vice versa
            Vector3 targetPoint = connectionArea.chainNeighbors[referenceAxis].newAxis[(referenceAxis + 1) % 2];
            //check distances to choose a point
            if((newAxis[0] - targetPoint).magnitude < (newAxis[1] - targetPoint).magnitude)
                selectedAxisIndex = 0;
            else
                selectedAxisIndex = 1;
        }

        connectionArea.newAxis = new List<Vector3> { newAxis[selectedAxisIndex], newAxis[(selectedAxisIndex+1) %2] };

        //move children to the oppisite end of the main axis
        joint.position = newAxis[selectedAxisIndex];
        for (int c = 0; c < joint.childCount; c++)
        {
            joint.GetChild(c).position = connectionArea.newAxis[1];
        }
    }

    void RebuildIKChains()
    {
#if USING_IK
        List<TransformChain> emptyChains = new List<TransformChain>();
        //update the ik chains to match the modified
        for(int i = 0; i < fabrikChains.Count; i++)
        {
            var fChain = fabrikChains[i];
            var bones = fChain.solver.bones;

            var tChain = transformChains[i];
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
        for (int fIndex = 0, tIndex = 0; fIndex < fabrikChains.Count; fIndex++)
        {
            bool isInvalid = emptyChains.Contains(transformChains[tIndex]);
            fabrikChains[fIndex].enabled = !isInvalid;
            if (isInvalid)
                transformChains.Remove(transformChains[tIndex]);
            else
                tIndex++;
        }
#endif
    }

    bool IsOffset(GameObject limbTransform)
    {
        return limbTransform.name.EndsWith("Offset");
    }

    void AddMovementComponents()
    {
#if USING_IK
        //limb data
        List<LimbEnd> limbEnds = new List<LimbEnd>();
        List<LimbEnd.LimbLocationTag> locations = new List<LimbEnd.LimbLocationTag>();
        List<LimbEnd.LimbTag> types = new List<LimbEnd.LimbTag>();

        //movement data
        List<Transform> spineAlignedTargets = new List<Transform>();
        //always add the root as a key target
        spineAlignedTargets.Add(emptyArmature);

        int frontIndex = -1; //holds the index of a front leg in the array
        int backIndex = -1;  //holds the index of a back leg in the array
        Transform chainTargets = emptyArmature.GetChild(1);
        foreach (var tChain in transformChains)
        {
            var transforms = tChain.GetList();
            var start = transforms[0].gameObject;

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

            GameObject target = tChain.Target;
            if (!target)
                continue; 
            var endGO = transforms[transforms.Count - 1].gameObject;
            target.transform.position = endGO.transform.position;
            var newLimbMarker = endGO.AddComponent<LimbEnd>();
            newLimbMarker.LimbInit(
                tChain.WorldLength(),
                target,
                start
                );
            limbEnds.Add(newLimbMarker);
        }

        Vector3 forward = emptyArmature.forward;
        int IsCloserToTail(Transform t1, Transform t2)
        {
            return System.Math.Sign(
                Vector3.Dot(forward, t1.position) - Vector3.Dot(forward, t2.position)
                );
        }
        spineAlignedTargets.Sort(IsCloserToTail);

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
            distances[i] = (spineAlignedTargets[i].transform.position - emptyArmature.transform.position).magnitude;
        }

        var behavior = emptyArmature.gameObject.AddComponent<CatBehavior>();
        behavior.BehaviorInit(
            limbEnds,
            spineAlignedTargets.ToArray(),
            distances
            );
#endif
    }

}