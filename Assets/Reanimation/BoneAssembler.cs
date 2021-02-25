using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RootMotion.FinalIK;

public class BoneAssembler : State
{
    [SerializeField]
    //root gameObject for connection colliders, the higharchy of this gameObject and the
    //empty armature must be identical except for objects with the TableConnectionArea component
    Transform connectionColliders = default;
    [SerializeField]
    Transform emptyArmature = default;

    //list of all connection areas
    List<TableConnectionArea> connectionAreas;
    //dictionary of joints in the armature that correspond to connecction areas on the table
    Dictionary<TableConnectionArea,Transform> joints;

    FABRIKRoot mFABRIKRoot;
    List<FABRIK> chains;

    //total number of joints, conneciton areas, and targets
    int count;

    //void temp()
    //{
    //    gameObject.GetComponent<RootMotion.FinalIK.FABRIK>().solver.SetChain();
    //}

    //axis data
    [SerializeField]
    BoneAxis boneAxisDict;
    [SerializeField]
    BoneAxis tableAxisDict;

    // Start is called before the first frame update
    void Start()
    {
        connectionAreas = new List<TableConnectionArea>();
        joints = new Dictionary<TableConnectionArea, Transform>();
        mFABRIKRoot = emptyArmature.GetComponentInChildren<FABRIKRoot>();
        chains = new List<FABRIK>();

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

            //empty nodes shouldn't be added to data structures
            if (!area)
                return;

            connectionAreas.Add(area);
            joints.Add(area,armatureNode);

            FABRIK chain = armatureNode.GetComponent<FABRIK>();
            if (chain)
                chains.Add(chain);
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
        Start();

        //early return if start was never called
        if (connectionAreas == null || joints == null)
        {
            End();
            yield break;
        }

        //how long a bone has been moving for
        float timer;
        //how long a bone should move for to reach it's destination
        const float timePerBone = 0.2f;
        //the current connection area
        TableConnectionArea connectionArea;
        Transform armatureNode;

        //a list of bones that are attached to a table connection areaa
        List<Bone> bones;
        //used to make sure that bones are only a part of one connection area group
        HashSet<Bone> seenBones = new HashSet<Bone>();


        for (int i = 0; i < count; i++)
        {
            connectionArea = connectionAreas[i];
            armatureNode = joints[connectionArea];
            //init vars for the group of bones moving in to the skeleton
            timer = 0;
            bones = connectionArea.GetAllBones();

            //temp, check to see if bones are a part of another group
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

            //if there are no bones, don't modify this joint
            if (bones.Count == 0)
            {
                continue;
            }

            //how much the bone will need to move from it's starting position to reach it's final destination 
            Vector3 totalOffset;
            //values calculated by GetOffsetWithTableAxis
            Bone bestBone = null;
            int bestAxisIndex = default;

            if(tableAxisDict[connectionArea.gameObject.name] != null)
            {
                totalOffset = GetOffsetWithTableAxis(out bestBone, out bestAxisIndex, bones, connectionArea);
            }
            else
            {
                totalOffset = GetOffsetWithJoint(bones, armatureNode);
            }

            //move bones in to position over time
            while (timer < timePerBone)
            {
                timer += Time.deltaTime;
                Vector3 offset = totalOffset * (Time.deltaTime / timePerBone);
                foreach (Bone b in bones)
                {
                    b.transform.position += offset;
                }
                yield return null;
            }

            //if the bone is part of a chain move it's joint position to one end of the bone axis
            if (bestBone != null)
            { 
                SetJointPosition(armatureNode, bestBone, connectionArea, bestAxisIndex);
            }

            //make bones children of the armature
            foreach (Bone b in bones)
            {
                b.transform.parent = armatureNode;
            }

            //bone clean up
            foreach (Bone b in bones)
            {
                Destroy(b);
                Destroy(b.GetComponent<CustomGravity>());
                Destroy(b.GetComponent<Rigidbody>());
                Destroy(b.GetComponent<BoneGroup>());

                // Plays a sound when the bones have reached their final position
                // This is not cursed.
                // When themes are implemented, can use theme field as parameter for PlaySound
                // Ex. AudioManager.Instance.PlaySound(b.theme);
                AudioManager.Instance.PlaySound("normal");
            }
        }

        Debug.Break();
        yield return new WaitForSeconds(1);

        RebuildIKChains();

        yield return new WaitForSeconds(1);

        Debug.Break();

        End();
        yield break;
    }

    private Vector3 GetOffsetWithTableAxis(out Bone bestBone, out int bestAxisIndex, List<Bone> bones, TableConnectionArea connectionArea)
    {
        //search for the bone axis which has the best score
        bestBone = null;
        float bestScore = float.NegativeInfinity;
        bestAxisIndex = 0;
        foreach (Bone b in bones)
        {
            float score = BoneScore(b, connectionArea, out int tempIndex);
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

        //check neighbors for newAxis to attach to
        int toAttachToIndex= connectionArea.chainNeighbors.FindIndex(ca => (ca && ca.newAxis != null && ca.newAxis.Count > 0));
        //if there are no chain neighbors then place the bone roughly in the middle of their spot in the armature
        if (toAttachToIndex == -1)
        {
            //move the midpoint of the bone to the midpoint of the armature
            Vector3 bestAxMidpoint = (bestAxis[0] + bestAxis[1]) / 2;
            Vector3 target = GetRoughJointMidpoint(joints[connectionArea]);

            return target - bestAxMidpoint;
        }

        //if the object is down chain / farther from the root
        bool isDownChain = toAttachToIndex == 0;
        //if the object is down chain then connect to the point that's not at the origin (index 1) otherwise 
        //the bone is upchain and should connect to the other point (index 0)
        Vector3 toAttachTo = connectionArea.chainNeighbors[toAttachToIndex].newAxis[isDownChain? 1 : 0];

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

        joint.position = newAxis[selectedAxisIndex];
        for (int c = 0; c < joint.childCount; c++)
        {
            joint.GetChild(c).position = connectionArea.newAxis[1];
        }
    }

    void RebuildIKChains()
    {
        foreach (var chain in mFABRIKRoot.solver.chains)
        {
            var bones = chain.ik.solver.bones;
            Transform[] transforms = new Transform[bones.Length];
            for(int i = 0; i < bones.Length; i++)
            {
                transforms[i] = bones[i].transform;
            }
            chain.ik.solver.SetChain(transforms, chain.ik.solver.GetRoot());
        }

        mFABRIKRoot.enabled = true;
        foreach (var chain in chains)
            chain.enabled = true;
    }
}