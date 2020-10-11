using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExampleAudioManager : MonoBehaviour
{
    private FMOD.Studio.EventInstance musicInstance;

    //Once audio is attached to corresponding events no additional code is
    //needed, so all code is in start
    void Start()
    {
        //local functions are prefered over lambdas for event methods because of their better readablilty
        void startMusic()
        {
            musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/NecromancerAcademyDemo_Take4");
            musicInstance.start();
        }
        void stopMusic()
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        //attach methods to events. To see all events in a given manager, go to the manager file and look 
        //at their main enumerator. The same strings used for the "SetState" method are also used for AddEventMethod
        //All states have both a begin and end event, and more events can be added by overriding the state's "AddToEvent" method
        GameManager.Instance.AddEventMethod("GameInit", "begin", startMusic);
        GameManager.Instance.AddEventMethod("GameCleanUp", "end", stopMusic);
    }
}
