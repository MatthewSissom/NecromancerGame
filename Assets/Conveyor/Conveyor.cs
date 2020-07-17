using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    [Header("Bones")]
    public List<GameObject> orderedBones;
    public List<int> count;
    public boneManager boneManager;

    [Header("Belt Stats")]
    public Vector3 velocity;
    public float boneSpacing;
    public Vector3 safeInstantiation;

    private conveyorGroup mGroup;

    private void Awake()
    {
        mGroup = gameObject.GetComponent<conveyorGroup>();
    }

    // Start is called before the first frame update
    void Start()
    {
        Vector3 offset = transform.position;
        Vector3 offsetDirection = -velocity.normalized;

        for(int i = 0; i < count.Count; i++)
        {
            for(int c = 1; c < count[i]; c++)
            {
                orderedBones.Add(orderedBones[i]);
            }
        }
        for(int i = 0; i < 50; i++)
        {
            int rand1 = Random.Range(0, orderedBones.Count);
            int rand2 = Random.Range(0, orderedBones.Count);
            GameObject temp = orderedBones[rand2];
            orderedBones[rand2] = orderedBones[rand1];
            orderedBones[rand1] = temp;
        }
        boneManager.NumGroups = orderedBones.Count;

        GameObject current;
        foreach(GameObject p in orderedBones)
        {
            current = Instantiate(p, safeInstantiation, Quaternion.Euler(90,0,0));
            bone currentBone = current.GetComponent<bone>();
            currentBone.boneManager = boneManager;
            if (currentBone)
            {
                mGroup.addChild(currentBone.Group);

                //set bone position
                float length = getLength(current);
                offset += offsetDirection * length / 2;
                current.transform.position = offset;
                offset += offsetDirection * (length / 2 + boneSpacing);
            }
            else
            {
                Destroy(current);
            }
            mGroup.GroupID = 0;
        }
    }

    private float getLength(GameObject bone)
    {
        return bone.GetComponent<Renderer>().bounds.size.x;
    }

    // Update is called once per frame
    void Update()
    {
        mGroup.applyToAll((bone toApply, FunctionArgs e) =>
        {
            toApply.transform.Translate(velocity * Time.deltaTime,Space.World);
        }, new FunctionArgs());
    }
}
