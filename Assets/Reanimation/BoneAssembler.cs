using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneAssembler : State
{
    [SerializeField]
    //root gameObject for connection colliders, the higharchy of this gameObject and the
    //empty armature must be identical except for objects with the TableConnectionArea component
    Transform connectionColliders;

    //list of all connection areas, joints at the same index at their corresponding 
    List<TableConnectionArea> connectionAreas;

    //list of joints in the armature that have connection areas
    List<Transform> joints;

    //world offset from the position of the joint, which is the target for bones
    List<Vector3> targets;
    //total number of joints, conneciton areas, and targets
    int count;

    // Start is called before the first frame update
    void Start()
    {
        //init vars
        targets = new List<Vector3>();
        connectionAreas = new List<TableConnectionArea>();
        joints = new List<Transform>();

        void InitVarsRecursive(Transform armatureNode, Transform connectionNode)
        {
            TableConnectionArea area = null;

            for(int i = 0, count = connectionNode.childCount; i < count; i ++)
            {
                if (!area)
                {
                    area = connectionNode.GetChild(i).GetComponent<TableConnectionArea>();
                    if (area)
                        continue;
                }
                    
                InitVarsRecursive(armatureNode.GetChild(i), connectionNode.GetChild(i));
            }
            if (!area)
                return;

            connectionAreas.Add(area);
            joints.Add(armatureNode);

            //calculate targets by finding the average between it's position and the average of it's children
            int childCount = armatureNode.childCount;

            if (childCount == 0)
            {
                targets.Add(new Vector3());
                return;
            }
            Vector3 childAvg;
            if (childCount == 1)
            {
                childAvg = armatureNode.GetChild(0).position;
            }
            else
            {
                childAvg = new Vector3();
                for (int c = 0; c < childCount; c++)
                {
                    childAvg += armatureNode.GetChild(c).position;
                }
                childAvg /= childCount;
            }

            Vector3 target = (childAvg + armatureNode.position) / 2;
            targets.Add(target);
        }

        InitVarsRecursive(transform, connectionColliders);
        count = joints.Count;
    }

    //assembles the skeleton
    public override IEnumerator Routine()
    {
        Start();

        float timer;
        const float timePerBone = 0.2f;

        Vector3 finalOffset;

        List<Bone> bones;

        HashSet<Bone> seenBones = new HashSet<Bone>();

        for(int i = 0; i < count; i++)
        {
            //init vars for the group of bones moving in to the skeleton
            timer = 0;
            bones = connectionAreas[i].GetAllBones();
            finalOffset = new Vector3();

            //temp
            List<Bone> toRemove = new List<Bone>();

            //find the average of all bones as the inital pos
            foreach (Bone b in bones)
            {
                //temp
                if (seenBones.Contains(b))
                {
                    //Debug.LogError("Bone belongs to multiple areas!");
                    toRemove.Add(b);
                    continue;
                }
                else
                    seenBones.Add(b);

                finalOffset += b.transform.position;
            }
            foreach (Bone b in toRemove)
            {
                bones.Remove(b);
            }
            //foreach (Bone b in bones)
            //{
            //    finalOffset += b.transform.position;
            //}
            finalOffset /= bones.Count;
            //finalOffset = final pos - inital pos
            finalOffset = targets[i] - finalOffset;

            //move bones in to position over time
            while (timer < timePerBone)
            {
                timer += Time.deltaTime;
                Vector3 offset = finalOffset * (Time.deltaTime/timePerBone);
                foreach (Bone b in bones)
                {
                    b.transform.position += offset;
                }
                yield return null;
            }

            //make bones children of the armature
            foreach (Bone b in bones)
            {
                b.transform.parent = joints[i];
            }

            //bone clean up
            foreach (Bone b in bones)
            {
                Destroy(b);
                Destroy(b.GetComponent<CustomGravity>());
                Destroy(b.GetComponent<Rigidbody>());
                Destroy(b.GetComponent<BoneGroup>());
            }

        }

        yield return new WaitForSeconds(1);

        End();
        yield break;
    }
}
