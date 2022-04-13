using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance;
    private int shipmentNumber;

    [SerializeField]
    GameObject emptyArmaturePrefab;
    public GameObject EmptyArmature { get; private set; }

    [SerializeField]
    //transforms in the collider higharchy used to enable and disable colliders during shipments
    //colliders are owned by the most specific transform given, and enabled during the shipment
    //given in the shipment numbers array
    private List<Transform> shipmentAreas = default;
    [SerializeField]
    private List<int> areaShipmentNumbers = default;


    //outlines in the order that they appear for shipments
    //[SerializeField]
    //private List<GameObject> outlineRenders = default;
    //private List<SpriteRenderer> outlines;

    private List<TableConnectionArea> allAreas;
    private List<List<TableConnectionArea>> deliveryAreas;

    [SerializeField]
    Color disabledColor;
    [SerializeField]
    Color enabledColor;


    [SerializeField]
    public List<BoneGroup> boneObjects;
    [SerializeField]
    public List<ResidualBoneData> residualBoneData;
    void Awake()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;

        allAreas = new List<TableConnectionArea>();
        deliveryAreas = new List<List<TableConnectionArea>>();

        TableConnectionArea temp;
        void FindAreasRecursive(Transform toCheck,int shipmentNumber)
        {
            //check for shipment number change
            int index = shipmentAreas.IndexOf(toCheck);
            if (index != -1)
            {
                shipmentNumber = areaShipmentNumbers[index];
            }

            //find area and add it to any data structures
            temp = toCheck.GetComponent<TableConnectionArea>();
            if (temp)
            {
                allAreas.Add(temp);

                //create lists if needed
                while (deliveryAreas.Count <= shipmentNumber)
                    deliveryAreas.Add(new List<TableConnectionArea>());

                deliveryAreas[shipmentNumber].Add(temp);
            }

            //recurse
            for (int i = 0; i < toCheck.childCount; i++)
            {
                FindAreasRecursive(toCheck.GetChild(i),shipmentNumber);
            }
        }

        FindAreasRecursive(transform,0);
        //disable all areas
        foreach(TableConnectionArea connectionArea in allAreas)
        {
            connectionArea.gameObject.SetActive(false);
        }
    }
    
    //called at the begining of the next shipment
    private void NextShipment()
    {
        ++shipmentNumber;
    }

    public void ReadyNextShipment()
    {

    }

    void ToggleGroupActive(int deliveryNumber)
    {
        if (shipmentNumber < 0 || shipmentNumber >= deliveryAreas.Count)
            return;
        //toggle areas
        foreach (TableConnectionArea area in deliveryAreas[deliveryNumber])
        {
            bool isActive = !area.isActiveAndEnabled;
            area.gameObject.SetActive(isActive);
        }
    }


    private IEnumerator FadeColor(SpriteRenderer renderer, Color start, Color end, float time)
    {
        float timer = 0;
        while(timer < time)
        {
            timer += Time.deltaTime;
            renderer.color = Color.Lerp(start, end, timer / time);
            yield return null;
        }
        renderer.color = end;
    }

    private void ResetTable()
    {
        shipmentNumber = -1;
        EmptyArmature = Instantiate(emptyArmaturePrefab, new Vector3(-.093f,.183f,.053f), Quaternion.Euler(0,-90,-90), transform);

        PlayPenState.Instance.SetSkeleton(EmptyArmature);
    }

    private void AlignAllColliders()
    {
        if (boneObjects != null)
        {
            foreach (var b in boneObjects)
                b.AlignAllCylindersToCamera();
        }
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod(typeof(GhostManager), "Begin", ReadyNextShipment);
        GameManager.Instance.AddEventMethod(typeof(GhostManager), "End", NextShipment);
        GameManager.Instance.AddEventMethod(typeof(GameInit), "Begin", ResetTable);
        GameManager.Instance.AddCamTransitionMethod("TableTrans", "End", AlignAllColliders);
        GameManager.Instance.AddCamTransitionMethod("GhostTrans", "End", AlignAllColliders);
    }

    //---Tutorial Queries---//

    private int GetMatchingBoneCount(System.Predicate<BoneGroup> predicate)
    {
        int cnt = 0;
        foreach (BoneGroup b in boneObjects)
        {
            cnt += predicate(b) ? 1 : 0;
        }
        return cnt;
    }

    public bool BonesAreConnectedOrGrounded()
    {
        return boneObjects.Count == GetMatchingBoneCount((BoneGroup b) => (b.isAttached || b.IsOnFloor) && !b.isBeingDragged);
    }

    public int ConnectedBoneCnt()
    {
        return GetMatchingBoneCount(b => b.isAttached);
    }
}
