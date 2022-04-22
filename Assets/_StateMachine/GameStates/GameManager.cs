using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : StateManagerBase
{
    public static GameManager Instance;
    public bool PlayingTutorial { get; private set; } = false;

    protected override void Awake()
    {
        if (Instance)
        {
            Destroy(this);
        }
        else
            Instance = this;

        base.Awake();
    }

    protected override bool IsMyState(State state)
    {
        string name = state.Name;
        string managerKey = name.Substring(0, 4);
        return base.IsMyState(state)
            &&(managerKey != "StMn" && managerKey != "Menu");
    }

    public IEnumerator Game()
    {
#if (UNITY_EDITOR)
        if (DebugModes.StateMode != DebugModes.EStateDebugMode.None)
        {
            yield return StartCoroutine(DebugLoops());
            yield break;
        }
#endif
        int playTutorial = PlayerPrefs.GetInt("playTutorial");
        if (playTutorial == 1)
        {
            yield return StartCoroutine(Tutorial());
        }
        else
        {
            yield return StartCoroutine(MainGameLoop());
        }
    }

    public IEnumerator MainGameLoop()
    {
        yield return SetState(typeof(GameInit));

        //bone delivery loop
        while (!GhostManager.Instance.done)
        {
            yield return CameraTransition("GhostTrans");
            yield return SetState(typeof(GhostManager));
            yield return CameraTransition("TableTrans");
            CountDown.SetParams("Assemble Cat", GhostManager.Instance.timeBetweenShipments);
            yield return SetState(typeof(CountDown));
        }

        yield return SetState(typeof(BoneAssembler));        
        
        yield return CameraTransition("ToPlayPenMid");
        yield return CameraTransition("PlayPen");
        yield return SetState(typeof(PlayPenState));

        yield return SetState(typeof(GameCleanUp));

        yield break;
    }

    public IEnumerator Tutorial()
    {
        yield return SetState(typeof(GameInit));
        PlayingTutorial = true;

        // Repeat first delivery phase until the player has connected 3 bones
        IEnumerator FirstPhase(bool firstTry)
        {
            // Show moving bones instruction
            if (firstTry)
            {
                TutorialHelper.PrepareNextInstruction();
                yield return SetState(MenuManager.Instance.InMenus("Instructions"));
                GhostManager.Instance.tutorialShipmentId = 0;
                // Move on after all bones have been picked up
                GhostManager.Instance.TutorialPhaseFinished = () => GhostManager.Instance.GhostsHaveNoBones() && TableManager.Instance.BonesAreNotBeingHeld();
            }
            // Show failure screen if this isn't the player's first try
            else
            {
                TutorialHelper.PrepareNextInstruction(false);
                yield return SetState(MenuManager.Instance.InMenus("Instructions"));
            }

            // Deliver bones
            yield return CameraTransition("GhostTrans");
            yield return SetState(typeof(GhostManager));

            // Show attaching instruction
            if (firstTry)
            {
                TutorialHelper.PrepareNextInstruction();
                yield return SetState(MenuManager.Instance.InMenus("Instructions"));
            }

            // Don't continue until bones are attached or attaching is possible
            yield return CameraTransition("TableTrans");
            yield return StartCoroutine(
                TutorialHelper.DelayedWaitUntil(TableManager.Instance.BonesAreConnectedOrGrounded)
            );

            // Loop if not enough bones were attached
        }
        yield return TutorialHelper.RepeatCoroutineUntil(
            FirstPhase,
            () => TableManager.Instance.ConnectedBoneCnt() >= 3
        );

        // Show rotation instruction
        TutorialHelper.PrepareNextInstruction();
        yield return SetState(MenuManager.Instance.InMenus("Instructions"));

        // No failure case, just wait for skeleton to be incomplete then complete again
        yield return CameraTransition("TableTrans");
        yield return new WaitWhile(TableManager.Instance.BonesAreConnectedOrGrounded);
        yield return StartCoroutine(
            TutorialHelper.DelayedWaitUntil(TableManager.Instance.BonesAreConnectedOrGrounded)
        );

        // Repeat second phase until the player has connected 3 more bones
        int initalConnectedCount = TableManager.Instance.ConnectedBoneCnt();
        IEnumerator SecondPhase(bool firstTry)
        {
            // Show phases instructions, if not the first try show failure screen instead
            TutorialHelper.PrepareNextInstruction(firstTry);
            yield return SetState(MenuManager.Instance.InMenus("Instructions"));

            // Second bone delivery
            GhostManager.Instance.tutorialShipmentId = 1;
            GhostManager.Instance.TutorialPhaseFinished = () => GhostManager.Instance.GhostsHaveNoBones() && TableManager.Instance.BonesAreNotBeingHeld();
            yield return CameraTransition("GhostTrans");
            yield return SetState(typeof(GhostManager));

            yield return CameraTransition("TableTrans");
            yield return StartCoroutine(
                TutorialHelper.DelayedWaitUntil(TableManager.Instance.BonesAreConnectedOrGrounded)
            );
        }
        yield return TutorialHelper.RepeatCoroutineUntil(
            SecondPhase,
            () => TableManager.Instance.ConnectedBoneCnt() >= initalConnectedCount + 3
        );

        // Show stopwatch instruction
        TutorialHelper.PrepareNextInstruction();
        yield return SetState(MenuManager.Instance.InMenus("Instructions"));

        // Run through a standard delivery phase with the stopwatch
        GhostManager.Instance.tutorialShipmentId = 2;
        GhostManager.Instance.TutorialPhaseFinished = null;
        yield return CameraTransition("GhostTrans");
        yield return SetState(typeof(GhostManager));
        yield return CameraTransition("TableTrans");
        CountDown.SetParams("Assemble Cat", GhostManager.Instance.timeBetweenShipments);
        yield return SetState(typeof(CountDown));

        // Show playpen instruction
        //TutorialHelper.PrepareNextInstruction();
        //yield return SetState(MenuManager.Instance.InMenus("Instructions"));

        // Tutorial is finished, default to not playing it after this
        PlayerPrefs.SetInt("playTutorial", 0);
        PlayerPrefs.Save();

        yield return SetState(typeof(BoneAssembler));

        yield return CameraTransition("ToPlayPenMid");
        yield return CameraTransition("PlayPen");
        yield return SetState(typeof(PlayPenState));

        yield return SetState(typeof(GameCleanUp));

        yield break;
    }

#if UNITY_EDITOR
    public IEnumerator DebugLoops()
    {
        switch (DebugModes.StateMode)
        {
            case DebugModes.EStateDebugMode.PlaypenOnly:
                yield return SetState(typeof(GameInit));

                yield return CameraTransition("ToPlayPenMid");
                yield return new WaitForSeconds(0.2f);
                yield return CameraTransition("PlayPen");

                yield return SetState(typeof(PlayPenState));
                yield return SetState(typeof(GameCleanUp));
                break;

            case DebugModes.EStateDebugMode.Tutorial:
                yield return StartCoroutine(Tutorial());
                break;

            case DebugModes.EStateDebugMode.AssemblyTest:
                yield return SetState(typeof(GameInit));

                BoneAssembler.Instance.SetTestPipeline(DebugModes.AssemblyTestPrefab);
                yield return SetState(typeof(BoneAssembler));

                yield return CameraTransition("ToPlayPenMid");
                yield return CameraTransition("PlayPen");
                yield return SetState(typeof(PlayPenState));
                yield return SetState(typeof(GameCleanUp));
                break;

            default:
                yield return StartCoroutine(MainGameLoop());
                break;
        }
    }
#endif
}
