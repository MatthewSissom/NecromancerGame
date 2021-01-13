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
        if (GUILayout.Button("Set Camera"))
        {
            Transform cameraTrans = myScript.transform;
            cameraTrans.localPosition = myScript.pos;
            cameraTrans.rotation = Quaternion.LookRotation(myScript.forward, myScript.up);
        }
        if (GUILayout.Button("Set Scene View"))
        {
            GameObject temp = new GameObject();
            temp.transform.localPosition = myScript.pos;
            temp.transform.rotation = Quaternion.LookRotation(myScript.forward, myScript.up);
            SceneView.lastActiveSceneView.AlignViewToObject(temp.transform);
            DestroyImmediate(temp);
        }
        DrawDefaultInspector();
    }
}
