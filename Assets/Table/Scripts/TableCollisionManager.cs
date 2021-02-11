using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableCollisionManager : MonoBehaviour
{
    private List<TableConnectionArea> allAreas;
    void Awake()
    {
        allAreas = new List<TableConnectionArea>();
        TableConnectionArea temp;
        void FindAreasRecursive(Transform toCheck)
        {
            temp = toCheck.GetComponent<TableConnectionArea>();
            if (temp)
                allAreas.Add(temp);
            for (int i = 0; i < toCheck.childCount; i++)
            {
                FindAreasRecursive(toCheck.GetChild(i));
            }
        }
        FindAreasRecursive(transform);

    }

    private void Start()
    {
    BoneManager.Instance.CreateLimbTags(allAreas);
    }
}
