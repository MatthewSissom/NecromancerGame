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

    public void AssignmentInit()
    {
        bones = new List<Bone>();
        success = false;
    }

    // Depth First search of a parent node
    public void DFSearch(Transform parent)
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
            // If a child has a child itself, search more recursively
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
    public void AssignmentCheck()
    {
        foreach (AssignementDataBase.LimbRequrementData limbRequrement in currentAssignment.limbRequirements)
        {
            foreach (Bone bone in bones)
            {
                string boneName = bone.ToString();
                string limbReqName = limbRequrement.currentSelectedLimb.ToString() + " (Bone)";

                //Debug.Log("Bone found - " + boneName);

                //Debug.Log("test - " + limbReqName);

                if (limbReqName == boneName)
                {
                    success = true;
                    break;
                }
            }
        }

        //Debug.Log("Assignement Success = " + success);
    }
}