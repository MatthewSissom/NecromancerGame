using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MainCamera))]
public class MainCameraCustomEditor : Editor
{
    MainCamera myScript;

    private void OnEnable()
    {
        myScript = (MainCamera)target;
    }

    public override void OnInspectorGUI()
    {
        if (GUILayout.Button("New Transition at Camera Pos"))
        {
            changeOldNames();

            setTransitionPos(myScript.transform);
        }
        if (GUILayout.Button("Move Camera to Scene View"))
        {
            var sceneCameraTransform = SceneView.lastActiveSceneView.camera.transform;
            myScript.transform.position = sceneCameraTransform.position;
            myScript.transform.rotation = sceneCameraTransform.rotation;
        }
        DrawDefaultInspector();
        if (GUILayout.Button("Set To Start"))
        {
            setCameraPos("Start");
        }
    }

    private void changeOldNames()
    {
        //if the name given is the same name as an older transition
        //add an (old) to the end of the other script's name
        foreach (CameraTransition c in myScript.gameObject.GetComponents<CameraTransition>())
        {
            if (c.transitionName == myScript.NewStateName)
            {
                c.transitionName += " (old)";
            }
        }
    }

    private void setTransitionPos(Transform newPos)
    {
        CameraTransition newTrans = (CameraTransition)myScript.gameObject.AddComponent(typeof(CameraTransition));
        newTrans.pos = newPos.position;
        newTrans.up = newPos.up;
        newTrans.forward = newPos.forward;
        newTrans.transitionName = myScript.NewStateName;
        newTrans.time = 1;
    }

    private void setCameraPos(string stateName)
    {
        Transform cameraTrans = myScript.transform;
        CameraTransition cameraTransition = Array.Find(cameraTrans.gameObject.GetComponents<CameraTransition>(), c => c.transitionName.ToLowerInvariant() == stateName.ToLowerInvariant());
        cameraTrans.localPosition = cameraTransition.pos;
        cameraTrans.rotation = Quaternion.LookRotation(cameraTransition.forward, cameraTransition.up);
    }
}
