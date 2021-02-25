﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEngine.SceneManagement;

[EditorTool("Bone Axis Editor",typeof(Bone))]
public class BoneAxisTool : EditorTool
{
    //bone data
    Bone script;
    Transform boneTrans;
    BoneAxis savedAxies;

    //manipulators
    List<Vector3> axisManipulators;
    int activeManipulator = -1;
    int activeManipulatorCompliment = -1;

    List<Vector3> newAxisIndiactors;
    Vector3 newAxisCenter;
    Quaternion newAxisOrientation;

    //toolbar
    GUIStyle mStyle;
    const float baseHandleSize = 0.005f;
    float handleScale = 1;
    bool newAxisMode = false;

    //saving / loading values
    float timeSaved;
    const string boneAxiesPath = "Assets/_Bones/ScriptableObjects/BoneAxis.asset";
    const string backUpPath = "Assets/_Bones/ScriptableObjects/BoneAxisBackups/";

    //set the active manipulator and it's compliment
    private void SetActiveManipulator(int index)
    {
        activeManipulator = index;
        activeManipulatorCompliment = savedAxies.GetComplimentIndex(index);
    }

    private void OnEnable()
    {
        savedAxies = AssetDatabase.LoadAssetAtPath(boneAxiesPath, typeof(BoneAxis)) as BoneAxis;

        mStyle = new GUIStyle();
        mStyle.padding = new RectOffset(10,10,10,10);
        mStyle.margin = new RectOffset(10,10,0,0);
        mStyle.normal.background = AssetDatabase.LoadAssetAtPath("Assets/Editor/Images/background.png", typeof(Texture2D)) as Texture2D;

        EditorTools.activeToolChanged += OnActiveToolChange;
        EditorTools.activeToolChanging += OnActiveToolWillChange;
        Selection.selectionChanged += OnSelcecionChange;

        Load(target as Bone);
    }

    private void OnDisable()
    {
        EditorTools.activeToolChanged -= OnActiveToolChange;
        EditorTools.activeToolChanging -= OnActiveToolWillChange;
        Selection.selectionChanged -= OnSelcecionChange;
    }

    void OnActiveToolChange()
    {
        if (!EditorTools.IsActiveTool(this))
            return;

        ResetTool();
    }

    private void OnActiveToolWillChange()
    {
        if (!EditorTools.IsActiveTool(this))
            return;

        Save(script);
        script = null;
    }

    void OnSelcecionChange()
    {
        ResetTool();
    }

    void Save(Bone toSave)
    {
        if (!toSave)
            return;
        savedAxies[toSave.gameObject.name] = axisManipulators;
    }

    void Load(Bone toLoad)
    {
        if (!toLoad)
            return;
        script = toLoad;
        if(toLoad.AxisKey != null && toLoad.AxisKey != "")
            axisManipulators = savedAxies[script.AxisKey];
        else
            axisManipulators = savedAxies[script.gameObject.name];
        if(axisManipulators == null)
            axisManipulators = new List<Vector3>();
    }

    void ResetTool()
    {
        Bone newTarget = target as Bone;

        if (script == null)
            Load(newTarget);
        else if (script != newTarget)
        {
            Save(script);
            Load(newTarget);
        }

        boneTrans = newTarget.transform;
        ResetManipulatorTool();
        ResetNewAxisTool();
        CheckForBackUp();
    }

    void ResetManipulatorTool()
    {
        activeManipulator = -1;
        activeManipulatorCompliment = -1;
    }

    void ResetNewAxisTool()
    {
        newAxisCenter = script.transform.position;
        newAxisOrientation = Quaternion.identity;
        newAxisIndiactors = new List<Vector3>();
        newAxisIndiactors.Add(new Vector3());
        newAxisIndiactors.Add(new Vector3());
    }

    void CreateDefaultAxis()
    {
        List<Vector3> offsets = new List<Vector3>();
        offsets.Add(new Vector3(0,0,1));
        offsets.Add(new Vector3(0,0,-1));
        offsets.Add(new Vector3(0,1,0));
        offsets.Add(new Vector3(0,-1,0));
        offsets.Add(new Vector3(1,0,0));
        offsets.Add(new Vector3(-1,0,0));
        for(int i = 0; i < offsets.Count; i++)
        {
            //offsets[i] *= 0.1f;
        }

        axisManipulators = new List<Vector3>();
        RaycastHit hitData;
        bool hit = false;
        Vector3 offset ;

        PhysicsScene scriptScene = PhysicsSceneExtensions.GetPhysicsScene(script.gameObject.scene);
        for (int i = 0; i < offsets.Count; i++)
        {
            offset = offsets[i];
            hit = scriptScene.Raycast(script.transform.position + offset, offset * -1, out hitData);
            if (hit)
                axisManipulators.Add(boneTrans.worldToLocalMatrix.MultiplyPoint(hitData.point));
        }

        Undo.RecordObject(savedAxies, "Created Default Axies");
        Save(script);
    }

    void DeleteSelectedPair()
    {
        if (activeManipulator == -1)
            return;
        Undo.RegisterCompleteObjectUndo(savedAxies, "Deleated Bone Axis");
        List<Vector3> toRemove = new List<Vector3>();
        toRemove.Add(axisManipulators[activeManipulator]);
        toRemove.Add(axisManipulators[activeManipulatorCompliment]);
        savedAxies.Remove(script.gameObject.name,toRemove);

        ResetTool();
    }

    void CalculateNewAxisIndicators()
    {
        PhysicsScene scriptScene = PhysicsSceneExtensions.GetPhysicsScene(script.gameObject.scene);
        Vector3 direction = newAxisOrientation * new Vector3(0, 0, 1);
        RaycastHit data;
        if (scriptScene.Raycast(newAxisCenter - direction, direction, out data))
        {
            newAxisIndiactors[0] = data.point;
        }

        direction *= -1;
        if (scriptScene.Raycast(newAxisCenter - direction, direction, out data))
        {
            newAxisIndiactors[1] = data.point;
        }
    }

    public override void OnToolGUI(EditorWindow window)
    {
        //init vars
        var evnt = Event.current;

        //toolbar layout
        Handles.BeginGUI();
        GUILayout.BeginHorizontal(mStyle);

        if (!newAxisMode)
        {
            //switch to new axis mode
            if (GUILayout.Button("New Axis", GUILayout.Width(150)))
            {
                newAxisMode = true;
                ResetNewAxisTool();
                CalculateNewAxisIndicators();
            }

            if (GUILayout.Button("Delete axis", GUILayout.Width(150)))
            {
                DeleteSelectedPair();
            }

            if (GUILayout.Button("Reset to default axis", GUILayout.Width(150)))
            {
                CreateDefaultAxis();
            }
        }
        else
        {
            if (GUILayout.Button("Finalize", GUILayout.Width(150)))
            {
                newAxisMode = false;
                axisManipulators.Add(script.transform.worldToLocalMatrix.MultiplyPoint(newAxisIndiactors[0]));
                axisManipulators.Add(script.transform.worldToLocalMatrix.MultiplyPoint(newAxisIndiactors[1]));
                Undo.RegisterCompleteObjectUndo(savedAxies, "Added New Axis");
                Save(script);
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(150)))
            {
                newAxisMode = false;
            }
        }
        
        GUILayout.EndHorizontal();

        Handles.EndGUI();

        EditorGUI.BeginChangeCheck();
        if (!newAxisMode)
        {
            for (int i = 0, count = axisManipulators.Count; i < count; i++)
            {
                if (i != activeManipulator)
                {
                    Handles.color = Color.white;
                    if (i == activeManipulatorCompliment)
                        Handles.color = new Color(.5f, 1f, .5f);

                    //check if the user clicked on another manipulator
                    if (Handles.Button(
                        script.transform.localToWorldMatrix.MultiplyPoint(axisManipulators[i]),
                        Quaternion.identity,
                        baseHandleSize,
                        baseHandleSize,
                        Handles.SphereHandleCap
                    ))
                    {
                        SetActiveManipulator(i);
                    }
                }
                else
                {
                    Handles.color = new Color(.1f, 1f, .1f);
                    Handles.SphereHandleCap(0,
                        script.transform.localToWorldMatrix.MultiplyPoint(axisManipulators[i]),
                        Quaternion.identity,
                        baseHandleSize,
                        evnt.type);
                    axisManipulators[i] = 
                        script.transform.worldToLocalMatrix.MultiplyPoint(
                            Handles.PositionHandle(
                                script.transform.localToWorldMatrix.MultiplyPoint(axisManipulators[i]), 
                                Quaternion.identity)
                        );
                }
            }
            if (activeManipulator != -1)
            {
                Handles.color = new Color(.5f, 1f, .5f);
                Handles.DrawLine(
                    script.transform.localToWorldMatrix.MultiplyPoint(axisManipulators[activeManipulator]),
                    script.transform.localToWorldMatrix.MultiplyPoint(axisManipulators[activeManipulatorCompliment])
                    );
            }
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RegisterCompleteObjectUndo(savedAxies, "Moved Axis Handle");
                Save(script);
            }
        }
        else
        {
            Handles.TransformHandle(ref newAxisCenter, ref newAxisOrientation);
            Handles.color = new Color(.5f, 1f, .5f);
            Handles.SphereHandleCap(0, newAxisIndiactors[0], Quaternion.identity, baseHandleSize, EventType.Repaint);
            Handles.SphereHandleCap(0, newAxisIndiactors[1], Quaternion.identity, baseHandleSize, EventType.Repaint);
            Handles.DrawLine(newAxisIndiactors[0], newAxisIndiactors[1]);
            if(EditorGUI.EndChangeCheck())
            {
                CalculateNewAxisIndicators();
            }
        }
    }

    //checks the time since the last back up was made, if it was greater than five minutes
    //the script will duplicate the current scriptable object
    private void CheckForBackUp()
    {
        #if UNITY_EDITOR
        //init time if needed
        if (timeSaved == -1)
        {
            timeSaved = Time.realtimeSinceStartup;
            return;
        }
        //only save every five minutes
        if (Time.realtimeSinceStartup - timeSaved < 5 * 60)
        {
            return;
        }

        //create a copy of the scriptable object as a backup
        string fileName = DateTime.Now.ToString("MM_dd_hh_mm_Backup") + ".asset";
        AssetDatabase.CopyAsset(boneAxiesPath, backUpPath + fileName);
        timeSaved = Time.realtimeSinceStartup;
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        #endif
    }
}
