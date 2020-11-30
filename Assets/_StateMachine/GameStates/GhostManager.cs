using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//State in the main game loop, brings bones to the table
public class GhostManager : State
{
    public bool done { get; private set; }
    static public GhostManager Instance;

    [Header("Bones")]
    public List<GameObject> boneShipments;
    [SerializeField]
    float timePerShipment;
    private int currentShipment = 0;

    [Header("Ghosts")]
    public GameObject ghostPref;
    private List<GhostBehavior> ghosts;

    public GhostBehavior CreateGhost(List<GameObject> path)
    {
        GhostBehavior newGhost = Instantiate(ghostPref, path[0].transform.position, path[0].transform.rotation).GetComponent<GhostBehavior>();
        ghosts.Add(newGhost);
        for(int i = 0; i < path.Count; i++)
        {
            newGhost.AddToPath(path[i], i == path.Count - 1);
        }
        newGhost.FollowPath();
        return newGhost;
    }

    private void InitObjects(GameObject boneShipment)
    {
        List<GameObject> bones = boneShipment.GetComponent<BoneShipment>().bones;
    #if UNITY_EDITOR
        if (boneShipment.transform.childCount != bones.Count)
            Debug.LogError("Number of bones is different than number of paths on" + boneShipment.name);
#endif
        Transform pathRoot;
        List<GameObject> path;
        for(int i = 0, size = bones.Count; i< size; i++)
        {
            pathRoot = boneShipment.transform.GetChild(i);
            path = new List<GameObject>();
            for(int p = 0; p < pathRoot.childCount; p++)
            {
                path.Add(pathRoot.GetChild(p).gameObject);
            }
            GhostBehavior ghost = CreateGhost(path);
            BoneManager.Instance.NewBone(bones[i], 
                new Vector3(0, 0, 100), 
                bones[i].transform.rotation, 
                ghost
            );
        }
    }

    public void DestroyGhost(GhostBehavior toRemove)
    {
        ghosts.Remove(toRemove);
        if(toRemove.mBone)
        {
            BoneManager.Instance.DestroyBone(toRemove.mBone);
        }
        Destroy(toRemove.transform.root.gameObject);
    }

    public void RecallGhosts()
    {
        foreach(GhostBehavior b in ghosts)
        {
            b.Recall();
        }
    }

    public void FocusGhostsOnTable()
    {
        foreach (GhostBehavior b in ghosts)
        {
            b.Recall();
        }
    }


    private float getLength(GameObject bone)
    {
        return bone.GetComponent<Renderer>().bounds.size.x;
    }

    private IEnumerator BoneShipment()
    {
        InitObjects(boneShipments[currentShipment]);
        done = ++currentShipment == boneShipments.Count;
        yield return new WaitForSeconds(timePerShipment);
        RecallGhosts();
        yield break;
    }

    public override IEnumerator Routine()
    {
        Begin();

        yield return BoneShipment();

        End();
        yield break;
    }

    override protected void Awake()
    {
        base.Awake();
        if(Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        ghosts = new List<GhostBehavior>();
    }
}



