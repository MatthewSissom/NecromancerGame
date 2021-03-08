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
    private bool SearchForBone(Transform current)
    {
        //Debug.Log("Searching Depth");
        bool complete;

        //// if the current node has no children and no tableconnection area, return true because we have reach the end of the and it is complete
        //if (current.childCount == 0 && current.GetComponent<TableConnectionArea>() == null)
        //    return true;

        // if a tableconnectionarea was found, set complete to true if bones were found in that area or false if there are no bones found in that area
        if (current.GetComponent<TableConnectionArea>() != null)
            complete = current.GetComponent<TableConnectionArea>().GetAllBones().Count != 0;
        else
            complete = false;

        // Loop through each child of the current node
        foreach (Transform child in current.transform)
        {
            complete = complete | SearchForBone(child);

            /*
            //// Check if there is table connection area component 
            //if (current.GetComponent<TableConnectionArea>() != null)
            //{
            //    // Get list of bones from a table connection area
            //    List<Bone> tableBones = current.GetComponent<TableConnectionArea>().GetAllBones();
            //    complete = tableBones.Count != 0;

            //    // loop through all of the bone's children to determine if bone is compelte
            //    foreach (Bone bone in tableBones)
            //    {
            //        complete = complete & SearchForBone(current);
            //    }

            //    return complete;
            //}

            //// If a child has a child itself, search further recursively
            //if (child.childCount > 0)
            //{
            //    SearchForBone(child);
            //}
            // Otherwise, return out of the function
            //else
            //{
            //    Debug.Log("No further children bones found at " + current.tag + "!");

            //    complete = true;
            //}
            */
        }

        if (complete)
            Debug.Log("FOUND Full bone at " + current.tag + "!");
        else
            Debug.Log("NOT FOUND bone's child is missing a bone at " + current.tag);

        return complete;
    }

    // Checks if the bones at the end of construction phase satisfy the current assignment
    private void CheckConditions()
    {
        success = true;

        // loop through each of the bone requirement data objects
        foreach(AssignementDataBase.BoneRequrementData boneReqData in currentAssignment.boneRequirements)
        {
            GameObject boneLocation = GameObject.FindGameObjectWithTag(boneReqData.requiredBone.ToString());

            if (boneLocation != null)
            {
                // check if the bone that is required is completed
                if (SearchForBone(boneLocation.transform))
                {
                    // if the bone is found, check if the bone is suppost to be excluded
                    if (boneReqData.excludeLimb == true)
                    {
                        // if there is one failure with the assignemnt, the whole assignment is failed so return out to save the time from checking further
                        success = false;
                        return;
                    }
                }
                else
                {
                    success = false;
                    return;
                }
            }
            else
            {
                Debug.Log("No Bone Location Found");
            }
        }
    }

    // Prints out results to chalkboard
    public void PrintResults()
    {
        ScoreManager.Instance.Add(new PartialScore("Assignment: " + currentAssignment.assignmentName));

        ScoreManager.Instance.Add(new PartialScore("Assignment Detail:"));

        foreach (AssignementDataBase.BoneRequrementData boneReqData in currentAssignment.boneRequirements)
        {
            if (boneReqData.excludeLimb == false)
                ScoreManager.Instance.Add(new PartialScore("Include " + boneReqData.requiredBone.ToString()));
            else
                ScoreManager.Instance.Add(new PartialScore("Exclude " + boneReqData.requiredBone.ToString()));
        }

        if (success)
            ScoreManager.Instance.Add(new PartialScore("\n" + "Passed Assignment!"));
        else
            ScoreManager.Instance.Add(new PartialScore("\n" + "Failed Assignment..."));
    }

    public override IEnumerator Routine()
    {
        Begin();

        //AssignmentInit();
        //SearchForBone(GameObject.FindGameObjectWithTag("Root").transform);
        CheckConditions();
        //PrintResults();

        End();

        yield return new WaitForSeconds(1.5f);
    }
}