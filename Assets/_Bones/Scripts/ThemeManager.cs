using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThemeManager : MonoBehaviour
{
    public enum BoneTypes
    {
        Normal,
        Robot
    }

    private List<string> themeNames;
    private Dictionary<string, string> themeSoundsDictionary;
    private const string testSound = "event:/SFX/TestSound";

    // Start is called before the first frame update
    void Start()
    {
        themeNames = new List<string>();
        themeSoundsDictionary = new Dictionary<string, string>();

        themeNames.Add("normal");
        themeNames.Add("robot");

        themeSoundsDictionary.Add("normal", "event:/SFX/Bones/BoneConnections");
        themeSoundsDictionary.Add("robot", testSound);
    }

    public string GetThemeSoundPath(string themeName)
    {
        return themeSoundsDictionary["themeName"];
    }
}
