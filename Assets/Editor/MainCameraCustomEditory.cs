using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainCamera))]
public class MainCameraCustomEditory : Editor
{
    MainCamera myScript;

    private void OnEnable()
    {
        myScript = (MainCamera)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if(GUILayout.Button("New Transition"))
        {
            //if the name given is the same name as an older transition
            //add an (old) to the end of the other script's name
            foreach(CameraTransition c in myScript.gameObject.GetComponents<CameraTransition>())
            {
                if(c.transitionName == myScript.NewStateName)
                {
                    c.transitionName += " (old)";
                }
            }

            CameraTransition newTrans = (CameraTransition)myScript.gameObject.AddComponent(typeof(CameraTransition));
            newTrans.pos = newTrans.transform.position;
            newTrans.up = newTrans.transform.up;
            newTrans.forward = newTrans.transform.forward;
            newTrans.transitionName = myScript.NewStateName;
            newTrans.time = 1;
        }
    }
}
