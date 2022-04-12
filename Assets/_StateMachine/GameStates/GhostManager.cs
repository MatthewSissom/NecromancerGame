﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//State in the main game loop, brings bones to the table
public class GhostManager : State
{
    public bool done { get; private set; }
    static public GhostManager Instance;
    static public GhostCollisionHandler Collision { get; private set; }

    [Header("Bones")]
    [SerializeField]
    private List<GameObject> boneShipments;
    [SerializeField]
    private List<GameObject> tutorialShipments;
    [SerializeField]
    float timePerShipment = default;
    [SerializeField]
    public float timeBetweenShipments = default;
    private int currentShipment = 0;

    [Header("Ghosts")]
    public GameObject ghostPref;
    private List<GhostBehavior> ghosts;

    public int tutorialShipmentId;
    public System.Func<bool> TutorialPhaseFinished { get; set; } = null;

    //Starting the ghost manager, 
    public void ManagerInit()
    {
        currentShipment = 0;
        done = false;
        ghosts = new List<GhostBehavior>();
    }

    //Destroys all ghosts in the ghosts list
    public void DestroyAll()
    {
        if (ghosts != null)
        {
            int ghostCount = ghosts.Count;
            while (ghostCount > 0)
            {
                Destroy(ghosts[ghostCount - 1]);
                ghosts.RemoveAt(ghostCount - 1);
                ghostCount -= 1;
            }
        }
        ghosts = new List<GhostBehavior>();
    }

    /// <summary>
    /// Creates a new ghost and puts it on a predefined path
    /// </summary>
    /// <param name="path">Path for the ghost to travel upon</param>
    /// <returns></returns>
    public GhostBehavior CreateGhost(List<GameObject> path)
    {
        GhostBehavior newGhost = Instantiate(ghostPref, path[0].transform.position, path[0].transform.rotation).GetComponent<GhostBehavior>();

        ghosts.Add(newGhost);
        for (int i = 0; i < path.Count; i++)
        {
            newGhost.AddToPath(path[i], i == path.Count - 1);
        }
        newGhost.FollowPath();
        return newGhost;
    }

    public bool GhostsHaveNoBones()
    {
        bool haveBones = false;
        foreach (GhostBehavior b in ghosts)
        {
            haveBones = haveBones || b.mBone != null;
        }
        return !haveBones;
    }

    /// <summary>
    /// Creates all the ghosts required for a given round
    /// </summary>
    /// <param name="boneShipment">an object which holds all the paths for ghosts to travel allong in a given round.</param>
    private void InitBoneShipmentObjects(GameObject boneShipment)
    {
        // Don't shuffle bones when playing the tutorial
        List<GameObject> bones = boneShipment.GetComponent<BoneShipment>().BoneShuffle(false && !GameManager.Instance.PlayingTutorial);
#if UNITY_EDITOR
        if (boneShipment.transform.childCount != bones.Count)
        {
            Debug.LogError("Number of bones is different than number of paths on" + boneShipment.name);
            return;
        }
#endif
        Transform pathRoot;
        List<GameObject> path;
        // create a ghost for each bone in the delivery
        for (int i = 0, size = bones.Count; i < size; i++)
        {
            path = new List<GameObject>();
            pathRoot = boneShipment.transform.GetChild(i);
            // add all path nodes in the delivery to the ghost's path
            for (int p = 0; p < pathRoot.childCount; p++)
            {
                path.Add(pathRoot.GetChild(p).gameObject);
            }
            GhostBehavior ghost = CreateGhost(path);
            GameObject newBone = Instantiate(bones[i]);
            ghost.mBone = newBone.GetComponent<GrabbableGroup>();
            newBone.GetComponent<GrabbableGroup>().mGhost = ghost;
            TableManager.Instance.boneObjects.Add(newBone.GetComponent<GrabbableGroup>());
        }
    }

    /// <summary>
    /// Destroys any given ghost
    /// </summary>
    /// <param name="toRemove">Ghost to destroy</param>
    public void DestroyGhost(GhostBehavior toRemove)
    {
        ghosts.Remove(toRemove);
        //TODO: remove the bone from existence
        Destroy(toRemove.transform.root.gameObject);
    }

    /// <summary>
    /// Recalls all ghosts back to their original start point
    /// </summary>
    /// <param name="lifeSpan">How long the ghosts have until they're destroyed</param>
    public void RecallGhosts(float lifeSpan)
    {
        foreach (GhostBehavior b in ghosts)
        {
            b.Recall(lifeSpan);
        }
    }

    /// <summary>
    /// All ghosts not already at the table will go directly to it.
    /// Note for B: This is unfinished code, sorry for the confusion.
    ///             The intent is for this loop to have each ghost go 
    ///             directly to the table, likely speeding up and skipping
    ///             unneeded path points.
    /// </summary>
    public void FocusGhostsOnTable()
    {
        foreach (GhostBehavior b in ghosts)
        {
            //TODO
        }
    }


    private float getLength(GameObject bone)
    {
        return bone.GetComponent<Renderer>().bounds.size.x;
    }

    /// <summary>
    /// Routine to send a group of ghosts carrying bones to the table.
    /// Will be called repeatedly by GameManager until there are no more shipments in the list of bone shipments
    /// </summary>
    /// <returns></returns>
    private IEnumerator BoneShipment()
    {
        // Bone shipment will be called multiple times during construction, use number 
        // of times called to chose a boneShipment
        if (GameManager.Instance.PlayingTutorial)
            InitBoneShipmentObjects(tutorialShipments[tutorialShipmentId]);
        else
            InitBoneShipmentObjects(boneShipments[currentShipment]);

        // update 'done' so GameManager will know when to move past the assembly stage
        currentShipment++;
        done = currentShipment == boneShipments.Count;

        // Plays sound when cats fist spawn for bone shipment
        AudioManager.Instance.PlaySound("catTest");

        // Check for the end condition of this shipment. Typically a countdown, but could be a tutorial phase.
        if (GameManager.Instance.PlayingTutorial)
        {
            Assert.IsNotNull(TutorialPhaseFinished);
            yield return StartCoroutine(TutorialHelper.DelayedWaitUntil(TutorialPhaseFinished));
        }
        else
        {
            CountDown.SetParams("Grab Bones", timePerShipment);
            yield return StartCoroutine(CountDown.instance.Routine());
        }

        RecallGhosts(timeBetweenShipments - 2);
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
        Assert.IsNotNull(tutorialShipments);
        Assert.IsNotNull(boneShipments);

        base.Awake();
        if (Instance)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            Collision = new GhostCollisionHandler();
        }
    }

#if UNITY_EDITOR
    public void ShortenDeliveryTimes()
    {
        timeBetweenShipments = 1;
        timePerShipment = .01f;
    }
#endif
}