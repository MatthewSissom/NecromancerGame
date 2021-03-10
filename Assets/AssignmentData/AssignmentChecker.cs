using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentChecker : State
{
    public AssignementDataBase currentAssignment;   // Current assignment details
    private bool success;                           // Success of current assignment

    public bool Success
    { get { return success; } }

    // Initialize variables
    private void AssignmentInit()
    {
        GameObject root = GameObject.FindGameObjectWithTag("Root");

        root.SetActive(true);

        void ReEnableInit(GameObject current)
        {
            // If there are children loop through each child of the current node
            foreach (Transform child in current.transform)
            {
                child.gameObject.SetActive(true);
                ReEnableInit(child.gameObject);
            }
        }

        ReEnableInit(root);

        success = true;
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod("AssignmentChecker", "Begin", AssignmentInit);
        GameManager.Instance.AddEventMethod("AssignmentChecker", "End", PrintResults);
    }

    // Depth First search of a parent node for bones
    private bool SearchForBone(Transform current)
    {
        if (success == false)
            return false;

        //Debug.Log("Searching Depth");
        bool complete = false;

        //// if the current node has no children and no tableconnection area, return true because we have reach the end of the and it is complete
        //if (current.childCount == 0 && current.GetComponent<TableConnectionArea>() == null)
        //    return true;

        //// Check if there are children in the currnet table area before continuing
        //if (current.childCount == 0)
        //{
        //    // if there are no chidlren, return true if there were bones found before, false if there are no bones found before
        //    complete = complete & true;
        //}
        //else
        //{

        // if a tableconnectionarea was found, set complete to true if bones were found in that area or false if there are no bones found in that area
        if (current.GetComponent<TableConnectionArea>() != null)
        {
            // bone areas are complete when there are bones in the area and the current bone area's children
            complete = current.GetComponent<TableConnectionArea>().GetAllBones().Count != 0;
        }

        // If there are children loop through each child of the current node
        foreach (Transform child in current.transform)
            {
                // Checkfor the next table conneciton area
                complete = complete & SearchForBone(child);
            }
        //}

        // Priting for debugging
        if (complete)
            Debug.Log("FOUND Full bone at Name: " + current.name + " Tag: " + current.tag + "!");
        else
            Debug.Log("NOT FOUND bone's child is missing a bone at " + "Name: " + current.name + " Tag: " + current.tag);

        // Return complete if bones were found or not
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
                Debug.Log("HAVE WE FOUND A BONE?" + SearchForBone(boneLocation.transform));

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
                    // if the bone was not found, check if we were supposet to not exclude it
                    if (boneReqData.excludeLimb == false)
                    {
                        // if we were suppost to include a bone but a bone was not found, return false
                        success = false;
                        return;
                    }
                }
            }
            else
            {
                Debug.Log("No Bone Location Found");
                success = false;
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