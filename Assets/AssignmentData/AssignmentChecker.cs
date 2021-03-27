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
        GameManager.Instance.AddEventMethod(typeof(AssignmentChecker), "Begin", AssignmentInit);
        GameManager.Instance.AddEventMethod(typeof(AssignmentChecker), "End", PrintResults);
    }

    // Depth First search of a parent node for bones
    private bool SearchForBone(Transform current)
    {
        // if we have already failed the assignment, there should be no need to check further
        if (success == false)
            return false;

        //Debug.Log("Searching Depth");
        //bool complete = false;

        // if a tableconnectionarea was found, set complete to true if bones were found in that area or false if there are no bones found in that area
        if (current.GetComponent<TableConnectionArea>() != null)
        {
            // bone areas are complete when there are bones in the area and the current bone area's children
            bool complete = current.GetComponent<TableConnectionArea>().GetAllBones().Count != 0;

            Debug.Log("Bone completeness at Name: " + current.name + " Tag: " + current.tag + " = " + complete);

            // reutrn complete because there's no need to check a table connection area's children
            return complete;
        }
        // if there are no table connection areas, search for children
        else
        {
            // If there are children loop through each child of the current node and search for their bones
            foreach (Transform child in current.transform)
            {
                SearchForBone(child);
            }

            // Return false because at this point, no bones are found
            return false;
        }
    }

    // Checks if the bones at the end of construction phase satisfy the current assignment
    private void CheckConditions()
    {
        // By default, set success to true
        success = true;

        // loop through each of the bones/limbs requirement data objects
        foreach (AssignementDataBase.BoneRequrementData boneReqData in currentAssignment.boneRequirements)
        {
            GameObject boneLocation = GameObject.FindGameObjectWithTag(boneReqData.requiredBone.ToString());

            if (boneLocation != null)
            {
                //Debug.Log("HAVE WE FOUND A BONE?" + SearchForBone(boneLocation.transform));

                // check if the bone that is required is completed
                if (SearchForBone(boneLocation.transform))
                {
                    // if the bone is found, check if the bone is suppost to be excluded
                    if (boneReqData.excludeLimb == true)
                    {
                        // If a bone was found but we were suppost to exclude it, reutrn false
                        success = false;

                        // if there is one failure with the assignemnt, the whole assignment is failed so return out to save the time from checking further
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