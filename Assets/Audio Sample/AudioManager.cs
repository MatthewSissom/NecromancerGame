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

    enum MusicTracks
    {
        PlayPen,
        Assembly,
        None
    }

    MusicTracks currentlyPlaying;

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

        currentlyPlaying = MusicTracks.None;
        PopulateDictionary();
    }

    void Start()
    {
        //attach methods to events. To see all events in a given manager, go to the manager file and look 
        //at their main enumerator. The same strings used for the "SetState" method are also used for AddEventMethod
        //All states have both a begin and end event, and more events can be added by overriding the state's "AddToEvent" method
        GameManager.Instance.AddEventMethod(typeof(GameInit), "Begin", StartAssemblyMusic);
        GameManager.Instance.AddEventMethod(typeof(PlayPenState), "Begin", StartPlaypenMusic);

        //start with playpen music
        StartPlaypenMusic();
    }

    void StartAssemblyMusic()
    {
        if (currentlyPlaying == MusicTracks.Assembly)
            return;

        StopMusic();    // stop music before starting new music
        musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/NecromancerAcademyDemo_Take4");
        musicInstance.start();
        currentlyPlaying = MusicTracks.Assembly;
    }

    void StartPlaypenMusic()
    {
        if (currentlyPlaying == MusicTracks.PlayPen)
            return;

        StopMusic();    // stop music before starting new music
        musicInstance = FMODUnity.RuntimeManager.CreateInstance("event:/Music/PlayPen_Music");
        musicInstance.start();
        currentlyPlaying = MusicTracks.PlayPen;
    }

    void StopMusic()
    {
        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        currentlyPlaying = MusicTracks.None;
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

    public void PlayMeow()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Vocals/General/Meows");
    }

    public void PlayMinorShock()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Vocals/General/Shock");
    }

    // Same as minor shock for testing, subject to change
    public void PlayMajorShock()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/Cats/Vocals/General/Shock");
    }

    public void PlayChalkboardSFX()
    {
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/ChalkboardSFX");
    }
}
