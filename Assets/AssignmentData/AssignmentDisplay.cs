using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AssignmentDisplay : MonoBehaviour
{
    public TextMeshProUGUI assignmentName;
    public TextMeshProUGUI assignmentDetail;
    public AssignementDataBase assignmentData;
    public MenuShowAssignments showAssignments;

    // Start is called before the first frame update
    public void Init()
    {
        assignmentName.text = "";
        assignmentDetail.text = "";

        assignmentName.text = assignmentData.assignmentName;

        foreach (AssignementDataBase.BoneRequrementData boneReqData in assignmentData.boneRequirements)
        {
            if (boneReqData.excludeLimb)
                assignmentDetail.text += "Exclude " + boneReqData.requiredBone.ToString();
            else
                assignmentDetail.text += "Include " + boneReqData.requiredBone.ToString();
        }
    }

    public void SelectAssignment()
    {
        showAssignments.selectedAssignment = assignmentData;

        showAssignments.ButtonPressed("Play");
    }
}
