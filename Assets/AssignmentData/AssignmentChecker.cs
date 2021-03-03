using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentChecker : MonoBehaviour
{
    public AssignementDataBase currentAssignment;   // Current assignment details
    private List<BoneChunk> bones;                       // List of bones present at end of construction phase
    private bool success;                           // Success of current assignment

    public bool Success
    { get { return success; } }

    // Initialize variables
    private void AssignmentInit()
    {
        bones = new List<BoneChunk>();
        success = false;
    }

    // Depth First search of a parent node for bone chinks
    private void SearchForBoneChunk(Transform parent)
    {
        //Debug.Log("Searching Depth");

        // Check if there is table connection area component 
        if (parent.GetComponent<TableConnectionArea>() != null)
        {
            // Get list of bones from a table connection area
            List<BoneChunk> tableBones = parent.GetComponent<TableConnectionArea>().GetAllBoneChunks();

            // Display the bone in colsode log for testing purposes
            foreach (BoneChunk chunk in tableBones)
            {
                bones.Add(chunk);
                //Debug.Log("Found Bone - " + bone);
            }
        }

        // Loop trhough each child of the parent node
        foreach (Transform child in parent.transform)
        {
            // If a child has a child itself, search more recursively otherwise return out of function
            if (child.childCount > 0)
            {
                SearchForBoneChunk(child);
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
        foreach (AssignementDataBase.BoneRequrementData boneRequrement in currentAssignment.boneRequirements)
        {
            // Get string for the required limb name
            //string limbReqName = limbRequrement.currentSelectedLimb.ToString() + " (Bone)";
            //Debug.Log("Searching for - " + limbReqName);

            // loop through each bone in the found bones
            foreach (BoneChunk boneChunk in bones)
            {
                // get string name of the bone
                // string boneName = bone.ToString();

                Debug.Log("Bone found - " + boneChunk.boneType.ToString());

                // compare bone name to the name of the bone int he assignment and if we are not excluding the limb
                if (boneRequrement.requiredBone == boneChunk.boneType)
                {
                    //Debug.Log("FOUND BONE");
                    success = true;
                    break;
                }
                else
                {
                    //Debug.Log("No bone...");
                    success = false;
                }
            }
        }
    }

    // Prints out results to chalkboard
    public void PrintResults()
    {
        ScoreManager.Instance.Add(new PartialScore("Assignment: " + currentAssignment.assignmentName));

        ScoreManager.Instance.Add(new PartialScore("Checking for:"));

        foreach (AssignementDataBase.BoneRequrementData boneReqData in currentAssignment.boneRequirements)
        {
            ScoreManager.Instance.Add(new PartialScore(boneReqData.requiredBone.ToString()));
        }

        if (success)
            ScoreManager.Instance.Add(new PartialScore("\n" + "Passed Assignment!"));
        else
            ScoreManager.Instance.Add(new PartialScore("\n" + "Failed Assignment..."));
    }

    // Wrapper function for ease of access
    public void AssignmentCheck(Transform parent)
    {
        AssignmentInit();
        SearchForBoneChunk(parent);
        CheckConditions();
    }
}