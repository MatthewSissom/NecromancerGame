using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPenState : State
{
    bool exit;
    [SerializeField]
    GameObject arrow;
    [SerializeField]
    GameObject skeletonSpawnPoint;

    public static PlayPenState Instance { get; private set; }

    public void SetSkeleton(GameObject playSkeleton)
    {
        if (!playSkeleton)
        {
            Debug.LogError("Playpen state entered with no skeleton set! Try enabling IK or setting a skeleton prefab in debug options");
            return;
        }

        var behavior = playSkeleton.GetComponent<SkeletonBehaviour>();
        if (!behavior)
            return;

        if (behavior.transform.parent)
            behavior.transform.parent = null;

        // set ai vars
        behavior.followTarget = PlayPenInput.Instance.FollowTarget;

        // orient skeleton
        playSkeleton.transform.forward = Vector3.right;
        playSkeleton.transform.up = Vector3.up;
        Vector3 spawnPoint = skeletonSpawnPoint.transform.position;
        playSkeleton.transform.position = new Vector3(spawnPoint.x, 0, spawnPoint.z);

        // set bone colors back to white if needed
        void ChangeMatRecursive(Transform t)
        {
            for (int i = 0; i < t.childCount; i++)
            {
                if (t.TryGetComponent(out RendererGatherer rg))
                    rg.ChangeMat(0);
                else
                    ChangeMatRecursive(t.GetChild(i));
            }
        }
        ChangeMatRecursive(playSkeleton.transform);
    }

    public override IEnumerator Routine()
    {
        Begin();

        arrow.SetActive(true);
        exit = false;

        while (!exit)
            yield return null;

        arrow.SetActive(false);
        End();
        yield return null;
    }

    private void Start()
    {
        arrow.SetActive(false);
    }

    override protected void Awake()
    {
        base.Awake();
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }

    public void EndPlayPenState() { exit = true; }
}
