using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class BoneAssembler : State
{
    public static BoneAssembler Instance { get; private set; }

    [SerializeField]
    private GameObject rootBone;
    private GameObject skeleton;
    private List<IAssemblyStage> assemblyStages;


    public void Start()
    {
        SetPipeline(rootBone,
            new RemoveExcessBones(),
            new CalcResidualData(),
            new MakeShoulderRoot(),
            new ReassignParents(),
            new RemoveGrabbableInfo(),
            new InsertIntoArmature(),
            //Save goes here
            new IkInit(),
            new MovementDataInit(),
            //TODO: step to remove residualbonedata from bones (not necessary but feels cleaner)
            //  new RemoveResidualData(),
            new BehaviourInit()
        );

        //Loaded cat pipeline:
        /* new BuildGameObjectsFromData (builds bone structure in an empty armature, 
         *   with proper parent-child relations and no grabbable info)
         * new IKInit()
         * 
         */
    }

    override protected void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }

    public override IEnumerator Routine()
    {
        Debug.Log("starting bone assembly");
        yield return RunPipeline();
    }

    void SetPipeline(GameObject startSkeleton, params IAssemblyStage[] assemblySequence)
    {
        skeleton = startSkeleton;
        assemblyStages = new List<IAssemblyStage>(assemblySequence);
    }
    IEnumerator RunPipeline()
    {
        yield break;

        IAssemblyStage previousStage = null;
        foreach (IAssemblyStage stage in assemblyStages)
        {
#if UNITY_EDITOR
            if (DebugModes.AdditionalAssemblerInfo)
                Debug.Log("Running assembly phase \'" + stage.GetType().ToString() + "\'");
#endif
            yield return stage.Execute(skeleton,previousStage);
            if (stage is IAssemblySkeletonChanger)
                skeleton = (stage as IAssemblySkeletonChanger).NewSkeleton;
            if(!(stage is TempDebugPause))
                previousStage = stage;
        }
    }

#if UNITY_EDITOR
    public void SetTestPipeline(GameObject prefab)
    {
        GameObject go = Instantiate(prefab);
        SetPipeline(go,
            new CalcResidualDataForPrefab(),
            new MakeShoulderRoot(),
            new ReassignParents(),
            new RemoveGrabbableInfo(),
            new InsertIntoArmature(),
            new IkInit(),
            new MovementDataInit(),
            new BehaviourInit()
        );
    }
#endif
    /*
    [SerializeField]
    //root gameObject for connection colliders, the higharchy of this gameObject and the
    //empty armature must be identical except for objects with the TableConnectionArea component
    Transform connectionColliders = default;
    [SerializeField]
    Transform[] ribTransforms;
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

        ConnectionAreasInit();

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

        End();
        yield break;
    }

    private void BoneCleanUp(Bone b)
    {
        Destroy(b.GetComponent<CustomGravity>());
        Destroy(b.GetComponent<Rigidbody>());
        Destroy(b.Group);
        Destroy(b);
    }

    private IEnumerator MoveBonesToArmature()
    {
        //the current connection area
        TableConnectionArea connectionArea;
        //the current joint
        Transform armatureNode;

        //a list of bones that are attached to a table connection areaa
        List<Bone> bones = new List<Bone>();
        //used to make sure that bones are only a part of one connection area group
        HashSet<Bone> seenBones = new HashSet<Bone>();

        //loop though all connection areas in the table and move their bones into the armature
        for (int i = 0; i < count; i++)
        {
            connectionArea = connectionAreas[i];

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

            //move bones to their final position in the skeleton
            Vector3 totalOffset;         //how much the bone will need to move
            Bone bestBone = null;        //the bone that best aligns with the connection area
            int bestAxisIndex = default; //the axis of the bone that best alignes with the connection area
            float angleToRotate = 0;
            Vector3 localPointOfRotation = default;
            //if table area has an axis, use it to find the best bone and get it's offset
            if (tableAxisDict[connectionArea.gameObject.name] != null)
            {
                totalOffset = GetOffsetWithTableAxis(out bestBone, out bestAxisIndex, out angleToRotate, out localPointOfRotation, bones, connectionArea, jointToArea[armatureNode]);
            }
            //if table area has no axis, check the table area which corresponds to it's new spot in the transform chain in case it was moved
            else if (tableAxisDict[jointToArea[armatureNode].gameObject.name] != null)
            {
                totalOffset = GetOffsetWithTableAxis(out bestBone, out bestAxisIndex, out angleToRotate, out localPointOfRotation, bones, jointToArea[armatureNode], jointToArea[armatureNode]);
            }
            //fallback for no axies on table
            else
            {
                totalOffset = GetOffsetWithJoint(bones, armatureNode);
            }
            if (float.IsNaN(totalOffset.x) || float.IsNaN(totalOffset.y) || float.IsNaN(totalOffset.z))
                Debug.Log("Invalid offset from offset with table joints");

            //move bones
            if(angleToRotate != 0 && bestBone != null)
                StartCoroutine(RotateBonesOverTime(bones, angleToRotate, localPointOfRotation, bestBone.transform));
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

    #region bone moving routines

    const float timePerBone = 0.2f; //how to reach the destination
    private IEnumerator OffsetBonesOverTime(List<Bone> bones, Vector3 offset)
    {
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

    private IEnumerator RotateBonesOverTime(List<Bone> bones, float theta, Vector3 localRotationPoint, Transform mainTransform)
    {
        List<Transform> transforms = new List<Transform>();
        foreach (var bone in bones)
            transforms.Add(bone.transform);

        float timer = 0;
        while(timer < timePerBone)
        {
            float time = Time.deltaTime;
            Vector3 point = mainTransform.localToWorldMatrix.MultiplyPoint(localRotationPoint);

            timer += time;
            float toRotate = theta * time / timePerBone;
            foreach (var trans in transforms)
                trans.RotateAround(point, Vector3.up, toRotate);

            yield return null;
        }

        Vector3 finalPoint = mainTransform.localToWorldMatrix.MultiplyPoint(localRotationPoint);
        float finalToRotate = theta * timePerBone - timer;
        foreach (var trans in transforms)
            trans.RotateAround(finalPoint, new Vector3(0,1,0), finalToRotate);

        yield break;
    }

    #endregion

    //returns the amount the bones need to move, which bone has the main axis, and the index of that axis
    //originalCA is the connection area that the bones were originally placed in, chainCA is the connection
    //area which corresponds to the bone's position in the chain
    private Vector3 GetOffsetWithTableAxis(out Bone bestBone, out int bestAxisIndex, out float angle, out Vector3 locatPointOfRotation, List<Bone> bones, TableConnectionArea originalCA, TableConnectionArea chainCA)
    {
        angle = 0;
        locatPointOfRotation = default;
        bestAxisIndex = 0;

        //search for the bone axis which aligns best with the area it was placed in
        bestBone = null;
        float bestScore = float.NegativeInfinity;
        foreach (Bone b in bones)
        {
            float score = BoneScore(b, originalCA, out int tempIndex,out float tempAngle);
            if (score > bestScore)
            {
                bestBone = b;
                bestAxisIndex = tempIndex;
                bestScore = score;
                angle = tempAngle;
            }
        }

        //get the world axis based on the axis search
        List<Vector3> bestAxis = boneAxisDict.GetWorldAxis(bestBone.AxisKey, bestBone.transform.localToWorldMatrix, bestAxisIndex);
        if (bestAxis == null || bestAxis.Count == 0)
        {
            Debug.LogError("Invalid bone axis for " + bestBone.name);
            return new Vector3();
        }
        locatPointOfRotation = boneAxisDict.GetLocalMidPoint(bestBone.AxisKey, bestAxisIndex);

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

        angle = GetRotationWithTableAxis(out Vector3 offsetError, bestBone.transform.localToWorldMatrix.MultiplyPoint(locatPointOfRotation), bestAxis[p1], angle);

        return toAttachTo - bestAxis[p1] - offsetError;
    }

    //returns the angle to rotate in DEGREES 
    //
    //documentation: https://www.desmos.com/calculator/uxmhykr4w9
    private float GetRotationWithTableAxis(out Vector3 offsetError, Vector3 worldPointOfRotation, Vector3 connectionPoint, float angleFromTable)
    {
        const float curveCoefficent = 2f;
        const float errorMin = 10;
        const float errorMax = 45;

        float originBoundAlignmentRotation(float x)
        {
            return Mathf.Pow(x, curveCoefficent) / Mathf.Pow(errorMax, curveCoefficent - 1);
        }

        float getAdjustmentAngle(float currentAngle)
        {
            return currentAngle;

            float sign = Mathf.Sign(currentAngle);
            currentAngle = Mathf.Abs(currentAngle);
            if (currentAngle < errorMin)
                return -currentAngle;
            return sign * originBoundAlignmentRotation((currentAngle-errorMin)*errorMax/(errorMax-errorMin));
        }

        float adjustment = getAdjustmentAngle(angleFromTable);
        worldPointOfRotation = new Vector3(worldPointOfRotation.x, 0, worldPointOfRotation.z);
        connectionPoint = new Vector3(connectionPoint.x, 0, connectionPoint.z) - worldPointOfRotation;

        float theta = Mathf.Atan2(connectionPoint.z, connectionPoint.x) + adjustment* Mathf.Deg2Rad;
        Vector3 newConnectionPointPos = new Vector3(Mathf.Cos(theta), 0, Mathf.Sin(theta)) * connectionPoint.magnitude;
        offsetError =  connectionPoint - newConnectionPointPos;
        return adjustment;
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
    //angle = signed angle from table to bone
    private float BoneScore(Bone bone, TableConnectionArea connectionArea, out int axisIndex, out float angle)
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
            angle = 0;
            return score;
        }

        //remove y component to make calculations more precise
        for(int i = 0; i< boneAxies.Count; i++)
        {
            boneAxies[i] = new Vector3(boneAxies[i].x, 0, boneAxies[i].z);
        }
        for(int i = 0; i < tableAxies.Count; i++)
        {
            tableAxies[i] = new Vector3(tableAxies[i].x, 0, tableAxies[i].z);
        }

        float tempScore = 0;
        angle = 0;
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
            Vector3 tableVec = tableAxies[0] - tableAxies[1];
            Vector3 boneVec = boneAxies[i] - boneAxies[i + 1];
            float tempAngle = Vector3.Angle(tableVec,boneVec);
            bool angleToLarge = tempAngle > 90;
            tempAngle = angleToLarge? 180 - tempAngle : tempAngle;
            tempScore += angleWeight * (1 - tempAngle / 90);

            if(tempScore > score)
            {
                axisIndex = i;
                score = tempScore;
                //convert angle to signed angle by checking which side of the table vector the bone is on
                float dotWithPerp = -boneVec.z * tableVec.x + boneVec.x * tableVec.z;
                angle = tempAngle * ((angleToLarge)? 1 : -1) * Mathf.Sign(dotWithPerp);
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
    }*/
}
