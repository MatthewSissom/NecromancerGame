using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    // Instance of audio manager singleton
    public static AudioManager Instance { get; private set; }

    private FMOD.Studio.EventInstance musicInstance;

    // Dictionary of themes (keys) for audio file paths (value)
    public Dictionary<string, string> themeSoundsDictionary;

    // create instance of singleton
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }
        else
        {
            Instance = this;
        }

        PopulateDictionary();
    }

    void Start()
    {
        //local functions are prefered over lambdas for event methods because of their better readablilty
        void startAssemblyMusic()
        {
            musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/NecromancerAcademyDemo_Take4");
            stopMusic();    // stop music before starting new music
            musicInstance.start();
        }

        void startPlayPenMusic()
        {
            musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/PlayPen_Music");
            stopMusic();    // stop music before starting new music
            musicInstance.start();
        }

        void stopMusic()
        {
            musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        }

        //attach methods to events. To see all events in a given manager, go to the manager file and look 
        //at their main enumerator. The same strings used for the "SetState" method are also used for AddEventMethod
        //All states have both a begin and end event, and more events can be added by overriding the state's "AddToEvent" method

        /*
        GameManager.Instance.AddEventMethod("GameInit", "begin", startMusic);
        GameManager.Instance.AddEventMethod("GameCleanUp", "end", stopMusic);
        */

        // if (in main menu/assembly state)
        startAssemblyMusic();

        // else if (in play pen state)
        //startPlayPenMusic();
    }

    // Populates the Dictionary with key value pairs
    private void PopulateDictionary()
    {
        // When the singleton is created, create a new dictionary and populate it with the proper themes/sounds
        themeSoundsDictionary = new Dictionary<string, string>();

        themeSoundsDictionary.Add("normal", "event:/SFX/Bones/BoneConnections");
        themeSoundsDictionary.Add("catTest", "event:/SFX/Cats/CatGhostTEST");
        themeSoundsDictionary.Add("test", "event:/SFX/TestSound");
    }

    // Plays a sound according to a bone's theme
    public void PlaySound(string themeName)
    {
        FMODUnity.RuntimeManager.PlayOneShot(themeSoundsDictionary[themeName]);
    }

    // For testing purposes, plays a test sound
    public void PlayTestSound()
    {
        FMODUnity.RuntimeManager.PlayOneShot(themeSoundsDictionary["test"]);
    }
}
