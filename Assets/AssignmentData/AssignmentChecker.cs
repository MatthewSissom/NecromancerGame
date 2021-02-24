using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentChecker : MonoBehaviour
{
    // Loops through the table connection areas on a gameObject and returns true/false if they have bones
    private bool GetBoneData(Transform node)
    {
        // Get list of bones from a table connection area
        List<Bone> bones = GetComponent<TableConnectionArea>().GetAllBones();

        // set hasBones to true/false if there are bones in the table connection area
        bool hasBones = bones != null && bones.Count > 0;

        // loop through children of the parent node and get bone data from them
        foreach (Transform child in node.transform)
        {
            hasBones = GetBoneData(child) || hasBones;
        }

        // Display the bone in colsode log for testing purposes
        foreach (Bone bone in bones)
        {
            Debug.Log("Found Bone - " + bone);
        }

        // return hasBones for recursive search purposes
        return hasBones;
    }

    // Depth First search of a parent node
    public void DFSearch(Transform parent)
    {
        // Check if there is table connection area component 
        if (parent.GetComponent<TableConnectionArea>() != null)
        {
            // Get bone data if there is a table connection area
            GetBoneData(parent);
        }

        // Loop trhough each child of the parent node
        foreach (Transform child in parent)
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
}