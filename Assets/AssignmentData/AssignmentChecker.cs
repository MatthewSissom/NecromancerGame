using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentChecker : MonoBehaviour
{
    public AssignementDataBase currentAssignment;   // Current assignment details
    private List<Bone> bones;                       // List of bones present at end of construction phase
    private bool success;                           // Success of current assignment

    public bool Success
    { get { return success; } }

    // Initialize variables
    private void AssignmentInit()
    {
        bones = new List<Bone>();
        success = false;
    }

    // Depth First search of a parent node
    private void DFSearch(Transform parent)
    {
        //Debug.Log("Searching Depth");

        // Check if there is table connection area component 
        if (parent.GetComponent<TableConnectionArea>() != null)
        {
            // Get list of bones from a table connection area
            List<Bone> tableBones = parent.GetComponent<TableConnectionArea>().GetAllBones();

            // Display the bone in colsode log for testing purposes
            foreach (Bone bone in tableBones)
            {
                bones.Add(bone);
                //Debug.Log("Found Bone - " + bone);
            }
        }

        // Loop trhough each child of the parent node
        foreach (Transform child in parent.transform)
        {
            // If a child has a child itself, search more recursively otherwise return out of function
            if (child.childCount > 0)
            {
                DFSearch(child);
            }
            else 
            {
                return;
            }
        }
    }

    // Checks if the bones at the end of construction phase satisfy the current assignment
    private void CheckConditions()
    {
        // loop through each assignemt's limb requirement data in the current assignment
        foreach (AssignementDataBase.LimbRequrementData limbRequrement in currentAssignment.limbRequirements)
        {
            // Get string for the required limb name
            string limbReqName = limbRequrement.currentSelectedLimb.ToString() + " (Bone)";
            Debug.Log("Searching for - " + limbReqName);

            // loop through each bone in the found bones
            foreach (Bone bone in bones)
            {
                // get string name of the bone
                string boneName = bone.ToString();

                Debug.Log("Bone found - " + boneName);

                // compare bone name to the name of the bone int he assignment and if we are not excluding the limb
                if ((limbReqName == boneName) && (!limbRequrement.excludeLimb))
                {
                    // if a match was found and the bone is not excluded, change success to true
                    Debug.Log("FOUND BONE");
                    success = true;
                    break;
                }
                else
                {
                    success = false;
                    Debug.Log("No bone...");
                }
            }
        }
    }

    // Prints out results to chalkboard
    public void PrintResults()
    {
        ScoreManager.Instance.Add(new PartialScore("Assignment: " + currentAssignment.assignmentName));

        ScoreManager.Instance.Add(new PartialScore("Checking for:"));

        foreach (AssignementDataBase.LimbRequrementData limbReqData in currentAssignment.limbRequirements)
        {
            ScoreManager.Instance.Add(new PartialScore(limbReqData.currentSelectedLimb.ToString()));
        }

        if (success)
            ScoreManager.Instance.Add(new PartialScore("Passed Assignment!"));
        else
            ScoreManager.Instance.Add(new PartialScore("Failed Assignment..."));
    }

    // Wrapper function for ease of access
    public void AssignmentCheck(Transform parent)
    {
        AssignmentInit();
        DFSearch(parent);
        CheckConditions();
    }
}