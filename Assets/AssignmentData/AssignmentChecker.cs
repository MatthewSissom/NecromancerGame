using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AssignmentChecker : MonoBehaviour
{
    private bool GetTransformData(Transform node)
    {
        List<Bone> bones = GetComponent<TableConnectionArea>().GetAllBones();
        bool hasBones = bones != null && bones.Count > 0;
        foreach (Transform child in gameObject.transform)
        {
            hasBones = GetTransformData(child) || hasBones;
        }

        foreach (Bone bone in bones)
        {
            Debug.Log("Found Bone - " + bone);
        }

        return hasBones;
    }

    public void DFSearch(Transform parent)
    {
        foreach (Transform child in parent)
        {
            if (child.childCount > 0)
            {
                if (child.GetComponent<TableConnectionArea>() != null)
                {
                    GetTransformData(child);
                }
                DFSearch(child);
            }
            else 
            {
                return;
            }
        }
    }
}