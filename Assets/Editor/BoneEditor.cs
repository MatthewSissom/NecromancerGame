using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomEditor(typeof(Bone)), CanEditMultipleObjects]
public class BoneEditor : Editor
{
    SerializedProperty keyProp;

    private void OnEnable()
    {
        keyProp = serializedObject.FindProperty("axisKey");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawDefaultInspector();
        if (GUILayout.Button("Rest Axis Key"))
        {
            keyProp.stringValue = (target as Bone).gameObject.name;
            serializedObject.ApplyModifiedProperties();
        }
    }
}
