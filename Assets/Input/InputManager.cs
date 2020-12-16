﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{
    //TEMP stats for input
    bool recordingVelocities = false;
    List<float> touchVelocities = new List<float>();

    [Header ("GameObjects")]
    public GameObject moveTouchPref;
    public GameObject rotationTouchPref;

    [Header ("Game Feel Values")]
    public float height;
    public float rotationRadSquared;
    public float rotationVelocityThreshold;

    static public InputManager Instance;

    //temp mouse input var
    bool holdingMouseDown = false;

    //disables all proxies
    public void Clear()
    {
        while(activeTouches.Count > 0)
        {
            DisableTouch(activeTouches[0]);
        }
    }

    private BoneMovingTouch isRotationTouch(Vector3 pos)
    {
        for (int i = 0; i < activeTouches.Count; i++)
        {
            BoneMovingTouch ft = activeTouches[i] as BoneMovingTouch;
            if (ft &&
                //ft.velocity.magnitude < rotationVelocityThreshold &&
                (ft.transform.position - pos).sqrMagnitude < rotationRadSquared)
            {
                return ft;
            }
        }
        return null;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        #region mouseInput
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
        if (Input.GetMouseButton(0))
        {
            Vector3 pos = Input.mousePosition;
            pos.z = Camera.main.transform.position.y - height;
            Vector3 radVec = pos + new Vector3(5, 0, 0);

            pos = Camera.main.ScreenToWorldPoint(pos);
            float rad = (Camera.main.ScreenToWorldPoint(radVec) - pos).magnitude;

            int id = 0;


            if (!holdingMouseDown)
            {
                holdingMouseDown = true;
                NewMoveTouch(pos,id);
            }
            else
            {
                activeTouches[id].Move(pos, rad);
                if (recordingVelocities)
                {
                    FloatingTouch ft = activeTouches[id] as FloatingTouch;
                    if (ft)
                        touchVelocities.Add(ft.speed);
                }
            }
        }
        else if(holdingMouseDown)
        {
            holdingMouseDown = false;
            Vector3 pos = Input.mousePosition;
            pos.z = Camera.main.transform.position.y - height;
            Vector3 radVec = pos + new Vector3(5, 0, 0);

            pos = Camera.main.ScreenToWorldPoint(pos);
            DisableTouch(pos);
        }
        if(Input.mouseScrollDelta.y != 0)
        {
            BoneMovingTouch active = activeTouches[0] as BoneMovingTouch;
            if (active != null)
            {
                float toRotate = -30* Input.mouseScrollDelta.y;
                Vector3 axis = Vector3.up;
                Vector3 pos = active.activeBone.transform.root.position;
                active.applyToAll((Bone toApply, FunctionArgs e) =>
                {
                    toApply.transform.RotateAround(pos, axis ,toRotate);
                });
            }
        }
        #endregion
        



        foreach (Touch t in Input.touches)
        {
            //pos represents the point in world space at the specified height
            Vector3 pos = t.position;
            pos.z = Camera.main.transform.position.y - height;
            Vector3 radVec = pos + new Vector3(t.radius, 0, 0);

            pos = Camera.main.ScreenToWorldPoint(pos);
            float rad = (Camera.main.ScreenToWorldPoint(radVec) - pos).magnitude;

            int id = t.fingerId;
            TouchProxy mProxy = activeTouches.Find(a => a.id == id);
            if(!mProxy)
            {
                BoneMovingTouch parent = isRotationTouch(pos);
                if (parent)
                {
                    mProxy = NewRotationTouch(pos, id,parent);
                }
                else
                {
                    mProxy = NewMoveTouch(pos, id);
                }
            }
            mProxy.Move(pos, rad);
        }

        foreach(TouchProxy tp in activeTouches)
        {
            if(!tp.moved)
            {
                if (toDeactivate == null)
                    toDeactivate = new List<TouchProxy>();
                toDeactivate.Add(tp);
            }
            tp.moved = false;
        }
        if(toDeactivate != null)
        {
            foreach(TouchProxy tp in toDeactivate)
            {
                DisableTouch(tp);
            }
            toDeactivate = null;
        }
    }

    private void Awake()
    {
        ArrayInit();
    }

    private void Start()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;

        MenuManager.Instance.AddEventMethod("MenuMain", "Begin", () =>
        {
            enabled = false;
        });
        GameManager.Instance.AddEventMethod("GameInit", "End", () =>
        {
            enabled = true;
        });

        //TEMP stats for input
        void RecordStats()
        {
            touchVelocities = new List<float>();
            recordingVelocities = true;
        }
        void CalcStats()
        {
            touchVelocities.Sort();
            Debug.Log(string.Format("Median: {0}\n" +
                "80 per over {1}\n" +
                "80 per under {3}\n",
                touchVelocities[Mathf.FloorToInt(touchVelocities.Count/2)],
                touchVelocities[Mathf.FloorToInt(touchVelocities.Count * .2f)],
                touchVelocities[Mathf.FloorToInt(touchVelocities.Count * .8f)]
                ));
            recordingVelocities = false;
        }
        //GameManager.Instance.AddEventMethod("GhostManager", "begin", RecordStats);
        //GameManager.Instance.AddEventMethod("TableTrans", "begin", CalcStats);
    }
}

