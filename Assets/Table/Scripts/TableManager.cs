using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TableManager : MonoBehaviour
{
    private int shipmentNumber;

    [SerializeField]
    //transforms in the collider higharchy used to enable and disable colliders during shipments
    //colliders are owned by the most specific transform given, and enabled during the shipment
    //given in the shipment numbers array
    private List<Transform> shipmentAreas;
    [SerializeField]
    private List<int> areaShipmentNumbers;

    //outlines in the order that they appear for shipments
    [SerializeField]
    private List<GameObject> outlineRenders;
    private List<SpriteRenderer> outlines;

    private List<TableConnectionArea> allAreas;
    private List<List<TableConnectionArea>> deliveryAreas;

    void Awake()
    {
        shipmentNumber = -1;
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
        void ToggleGroupActive(int deliveryNumber)
        {
            if (shipmentNumber < 0 || shipmentNumber == deliveryAreas.Count)
                return;
            //toggle areas
            foreach (TableConnectionArea area in deliveryAreas[deliveryNumber])
            {
                area.gameObject.SetActive(!area.isActiveAndEnabled);
            }
            //toggle outline
            outlines[deliveryNumber].enabled = !outlines[deliveryNumber].enabled;
        }

        //disable currently active group
        ToggleGroupActive(shipmentNumber);
        //enable new group
        ToggleGroupActive(++shipmentNumber);
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod("TableTrans", "Begin", NextShipment);
    }


}
