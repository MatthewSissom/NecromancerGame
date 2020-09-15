using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor (typeof(CameraTransition))]
public class CameraTransitionEditor : Editor
{
    CameraTransition myScript;

    private void OnEnable()
    {
        myScript = (CameraTransition)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("Set Camera To Location"))
        {
            Transform cameraTrans = myScript.transform;
            cameraTrans.position = myScript.pos;
            cameraTrans.up = myScript.up;
            cameraTrans.forward = myScript.forward;
        }
        DrawDefaultInspector();
    }
}
