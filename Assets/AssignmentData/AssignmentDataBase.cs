using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/AssignementDataBase", order = 1)]
public class AssignementDataBase : ScriptableObject
{
    public string assignmentName;   // The name of the assignemnt
    public Bone assignmentReward;   // The reward(s) the player will get after completing the assignment

    public List<LimbRequrementData> limbRequirements;   // The limbs required to complete the assignement
    public List<ThemeRequirements> themeRequirements;   // The themes reuiqred to complete the assignent

    // Struct for limbs required to complete assignment

    [System.Serializable]
    public class LimbRequrementData
    {
        public LimbName currentSelectedLimb;   // Limb selected to check for
        public bool excludeLimb;               // Exclude one of the limbs from LimbName from being checked to complete the assignment

        // The names of possible limbs in assignments
        public enum LimbName
        {
            Leg,
            Arm,
            Rib,
            Skull,
            Tail,
            Spine
        }
    }

    [System.Serializable]
    // Struct for themes required to complete assigment
    public class ThemeRequirements
    {
        public ThemeNames currentSelectedTheme;    // Theme selected to check for
        public bool excludeTheme;                  // Exclude onf ot he themes from ThemeName from being checked to complete the assignment

        // The names of possible themes in assigments
        public enum ThemeNames
        {
            NoTheme,
            Normal,
            Robo,
            Tea
        }
    }

}
