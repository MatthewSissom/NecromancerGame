using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPenState : State
{
    bool exit;
    [SerializeField]
    GameObject arrow;

    public void SetSkeleton(GameObject playSkeleton)
    { 

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

    public void EndPlayPenState() { exit = true; }
}
