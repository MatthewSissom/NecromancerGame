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
            myScript.gameObject.AddComponent(typeof(CameraTransition));
        }
    }
}
