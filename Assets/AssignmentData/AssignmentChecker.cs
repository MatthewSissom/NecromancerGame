using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentChecker : State
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

    private void Start()
    {
        GameManager.Instance.AddEventMethod("AssignmentChecker", "Begin", AssignmentInit);
        GameManager.Instance.AddEventMethod("AssignmentChecker", "End", PrintResults);
    }

    // Depth First search of a parent node for bones
    private void SearchForBone(Transform parent)
    {
        //Debug.Log("Searching Depth");

        // Check if there is table connection area component 
        if (parent.GetComponent<TableConnectionArea>() != null)
        {
            // Get list of bones from a table connection area
            List<Bone> tableBones = parent.GetComponent<TableConnectionArea>().GetAllBones();

            if (tableBones.Count != 0)
            {
                // Display the bone in colsode log for testing purposes
                foreach (Bone bone in tableBones)
                {
                    bones.Add(bone);
                    //Debug.Log("Found Bone - " + bone);
                }
            }

        }

        // Loop trhough each child of the parent node
        foreach (Transform child in parent.transform)
        {
            // If a child has a child itself, search further recursively
            if (child.childCount > 0)
            {
                SearchForBone(child);
            }
            // Otherwise, return out of the function
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
            // Check if there is a bone in the specified areas
            // If there is, make success = true
            // If not, make success = false
        }
    }

    // Prints out results to chalkboard
    public void PrintResults()
    {
        ScoreManager.Instance.Add(new PartialScore("Assignment: " + currentAssignment.assignmentName));

        ScoreManager.Instance.Add(new PartialScore("Checking for:"));

        //foreach (AssignementDataBase.BoneRequrementData boneReqData in currentAssignment.boneRequirements)
        //{
        //    ScoreManager.Instance.Add(new PartialScore(boneReqData.requiredBone.ToString()));
        //}

        if (success)
            ScoreManager.Instance.Add(new PartialScore("\n" + "Passed Assignment!"));
        else
            ScoreManager.Instance.Add(new PartialScore("\n" + "Failed Assignment..."));
    }

    public override IEnumerator Routine()
    {
        Begin();

        //AssignmentInit();
        SearchForBone(GameObject.FindGameObjectWithTag("Root").transform);
        CheckConditions();
        //PrintResults();

        End();

        yield return new WaitForSeconds(1.5f);
    }
}