using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;

[CustomEditor(typeof(Bone))]
public class BoneEditor : Editor
{
    Bone script;

    private void OnEnable()
    {
        script = (Bone)target;
    }

    public override void OnInspectorGUI()
    {
        if(GUILayout.Button("Adjust Markers"))
        DrawDefaultInspector();
    }
}
