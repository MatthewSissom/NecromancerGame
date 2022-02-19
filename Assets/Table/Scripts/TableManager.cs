using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TableManager : MonoBehaviour
{
    static TableManager Instance;
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
    [SerializeField]
    private List<GameObject> outlineRenders = default;
    private List<SpriteRenderer> outlines;

    private List<TableConnectionArea> allAreas;
    private List<List<TableConnectionArea>> deliveryAreas;

    [SerializeField]
    Color disabledColor;
    [SerializeField]
    Color enabledColor;

    void Awake()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;

        allAreas = new List<TableConnectionArea>();
        deliveryAreas = new List<List<TableConnectionArea>>();
        outlines = new List<SpriteRenderer>(); 
        foreach(var go in outlineRenders)
        {
            outlines.Add(go.GetComponent<SpriteRenderer>());
        }

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
        //disable all outlines
        foreach(Renderer outline in outlines)
        {
            outline.enabled = false;
        }
    }
    
    //called at the begining of the next shipment
    private void NextShipment()
    {
        ++shipmentNumber;
        IEnumerator WaitForFade()
        {
            //enable new group
            yield return StartCoroutine(FadeColor(outlines[shipmentNumber], disabledColor, enabledColor, 1));
            ToggleGroupActive(shipmentNumber);
        }
        StartCoroutine(WaitForFade());
    }

    public void ReadyNextShipment()
    {
        if (shipmentNumber + 1 < outlines.Count)
        {
            SpriteRenderer toReady = outlines[shipmentNumber + 1];
            toReady.enabled = true;
            Color newColor = disabledColor;
            newColor.a = 0;
            //fade up from nothing to current color
            StartCoroutine(FadeColor(toReady, newColor, disabledColor, 1)); 
        }

        if (shipmentNumber >= 0)
        {
            SpriteRenderer toUnready = outlines[shipmentNumber];
            toUnready.enabled = true;
            Color newColor = toUnready.color;
            newColor.a = 0;
            //fade down to nothing from current color

            IEnumerator WaitForFade()
            {
                //enable new group
                yield return StartCoroutine(FadeColor(toUnready, toUnready.color, newColor, 1));
                ToggleGroupActive(shipmentNumber);
            }
            StartCoroutine(WaitForFade());
        }
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
            area.ApplyToAll((Bone b, FunctionArgs args) => { b.connecting = isActive; });
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

        foreach (TableConnectionArea ta in allAreas)
            ta.ResetArea();
        foreach (var outline in outlines)
            outline.enabled = false;
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod(typeof(GhostManager), "Begin", ReadyNextShipment);
        GameManager.Instance.AddEventMethod(typeof(GhostManager), "End", NextShipment);
        GameManager.Instance.AddEventMethod(typeof(GameInit), "Begin", ResetTable);
    }
}
