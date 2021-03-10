using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssignmentDisplay : MonoBehaviour
{
    public TextMeshProUGUI assignmentName;      // Name of the assignment currently displaying
    public TextMeshProUGUI assignmentDetail;    // String description of assignment data
    public AssignementDataBase assignmentData;  // Requirements of the assignments
    public MenuShowAssignments showAssignments; // Reference to showAssignments state

    // Initializes assignment display text
    public void Init()
    {
        assignmentName.text = "";
        assignmentDetail.text = "";

        // Displays assignment name
        assignmentName.text = assignmentData.assignmentName;

        // Loop through each assignment data requirements and display them in text
        foreach (AssignementDataBase.BoneRequrementData boneReqData in assignmentData.boneRequirements)
        {
            if (boneReqData.excludeLimb)
                assignmentDetail.text += "Exclude " + boneReqData.requiredBone.ToString();
            else
                assignmentDetail.text += "Include " + boneReqData.requiredBone.ToString();
        }
    }

    // Method for button presses, selects an assignment and uses it when building cats
    public void SelectAssignment()
    {
        showAssignments.selectedAssignment = assignmentData;

        showAssignments.ButtonPressed("Play");
    }
}
