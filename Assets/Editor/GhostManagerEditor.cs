//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEditor;

//[CustomEditor(typeof(GhostManager))]
//public class GhostManagerEditor : Editor
//{
//    GhostManager gm;
//    int shipments;
//    List<int> boneCounts;
//    bool timing = false;
//    float timer;

//    private void OnEnable()
//    {
//        gm = (GhostManager)target;
//        gm.boneShipments = gm.boneShipments ?? new List<List<GameObject>>();
//        boneCounts = new List<int>();
//        EditorApplication.delayCall += UpdateData;
//    }

//    void ResizeList<T>(List<T> myList, int desiredCount) where T : new()
//    {
//        int count = myList.Count;
//        if (desiredCount > count)
//        {
//            myList.AddRange(Enumerable.Repeat<T>(new T(), desiredCount - count));
//        }
//        else if (desiredCount < count)
//        {
//            myList.RemoveRange(desiredCount, count - shipments);
//        }
//    }

//    void UpdateData()
//    {

//        gm.boneShipments = gm.boneShipments ?? new List<List<GameObject>>();
//        if (shipments != gm.boneShipments.Count)
//        {
//            ResizeList(gm.boneShipments, shipments);
//            ResizeList(boneCounts, shipments);
//        }
//    }

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        shipments = EditorGUILayout.IntField("Shipments", shipments);
//        EditorGUILayout.LabelField("Boneshipment count" + gm.boneShipments.Count.ToString());
//        for (int shipNum= 0; shipNum < gm.boneShipments.Count; shipNum++)
//        {
//            EditorGUILayout.LabelField("Shipment " + shipNum.ToString());
//        }

//        if (GUI.changed)
//        {
//            timing = true;
//            timer = Time.realtimeSinceStartup;
//        }
//        if(timing)
//        {
//            if(Time.realtimeSinceStartup - timer > 3)
//            {
//                Debug.Log("2nd " + Time.realtimeSinceStartup);
//                timing = false;
//                UpdateData();
//            }
//        }
//    }
//}
