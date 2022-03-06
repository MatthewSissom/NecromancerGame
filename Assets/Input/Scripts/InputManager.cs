using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class InputManager : MonoBehaviour
{
    [Header("GameObjects")]
    public GameObject moveTouchPref;
    public GameObject rotationTouchPref;

    [Header("Game Feel Values")]
    public float height;
    public float rotationRadSquared;
    public float rotationVelocityThreshold;

    static public InputManager Instance;


#if (UNITY_EDITOR || PLATFORM_STANDALONE_WIN)
    // mouse input vars
    bool holdingMouseDown = false;
    bool rotating = false;
    bool activeTouchesFromFingersExist = false;
#endif

    //disables all proxies
    public void Clear()
    {
        while(activeTouches.Count > 0)
        {
            DisableTouch(activeTouches[0]);
        }
    }


    private void FindAndDisableUnused()
    {
#if UNITY_EDITOR
        if (!activeTouchesFromFingersExist)
            return;
#elif UNITY_STANDALONE_WIN
        return;
#endif

        foreach (TouchProxy tp in activeTouches)
        {
            if (!tp.moved)
            {
                if (toDeactivate == null)
                    toDeactivate = new List<TouchProxy>();
                toDeactivate.Add(tp);
            }
            tp.moved = false;
        }
        if (toDeactivate != null)
        {
            foreach (TouchProxy tp in toDeactivate)
            {
                DisableTouch(tp);
            }
            toDeactivate = null;
        }

#if UNITY_EDITOR
        if (activeTouches.Count == 0)
            activeTouchesFromFingersExist = false;
#endif
    }

    // Update is called once per frame
    void FixedUpdate()
    {
#region touchInput
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
            if (!mProxy)
            {
                if (!movingTouch.isActiveAndEnabled)
                {
                    mProxy = NewMoveTouch(pos, id);
                }
                else if(!rotationTouch.isActiveAndEnabled)
                {
                    mProxy = NewRotationTouch(pos, id, movingTouch);
                }
            }
            if(mProxy)
                mProxy.Move(pos, rad);
#if UNITY_EDITOR
            activeTouchesFromFingersExist = true;
#endif
        }
        FindAndDisableUnused();
        #endregion

        #region mouseInput
        if(UseMouseInputThisFrame())
        { 
            Vector3 pos = Input.mousePosition;
            pos.z = Camera.main.transform.position.y - height;
            Vector3 radVec = pos + new Vector3(5, 0, 0);

            pos = Camera.main.ScreenToWorldPoint(pos);
            float rad = (Camera.main.ScreenToWorldPoint(radVec) - pos).magnitude;
            int id = 0;

            if (Input.GetKeyDown(KeyCode.Escape))
                Application.Quit();
            if (Input.GetKey(KeyCode.Space))
            {
                if (!rotating && holdingMouseDown)
                {
                    rotating = true;
                    NewRotationTouch(pos, 1, activeTouches[0] as BoneMovingTouch);
                }
                id = 1;
                Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(5, 0, 0));
                Vector3 cameraSpaceVec = pos - center;
                Vector3 projection = Vector3.Project(cameraSpaceVec, Camera.main.transform.forward);
                pos = center + (cameraSpaceVec - projection).normalized;

                Vector3 angularVelocity = (activeTouches[0] as BoneMovingTouch).activeWatch.Rb.angularVelocity;
                (activeTouches[0] as BoneMovingTouch).activeWatch.Rb.angularVelocity = Vector3.Project(angularVelocity, Camera.main.transform.forward);
            }
            else if (rotating)
            {
                rotating = false;
                Vector3 center = Camera.main.ScreenToWorldPoint(new Vector3(5, 0, 0));
                Vector3 cameraSpaceVec = pos - center;
                Vector3 projection = Vector3.Project(cameraSpaceVec, Camera.main.transform.forward);
                pos = center + (cameraSpaceVec - projection).normalized;
                DisableTouch(pos);
            }
            else if (Input.GetMouseButton(0))
            {
                if (!holdingMouseDown)
                {
                    holdingMouseDown = true;
                    NewMoveTouch(pos, id);
                }
            }
            else if (holdingMouseDown)
            {
                holdingMouseDown = false;
                DisableTouch(pos);
            }

            if (rotating || holdingMouseDown)
                activeTouches[id].Move(pos, rad);

            const float rotationSpeed = -5f;
            float rotation = 0;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            {
                rotation += rotationSpeed;
            }
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            {
                rotation -= rotationSpeed;
            }
            if (rotation != 0)
            {
                BoneMovingTouch active = activeTouches[0] as BoneMovingTouch;
                if (active != null)
                {
                    Vector3 axis = Vector3.up;
                    //Vector3 pos = active.activeBone.transform.root.position;
                    //active.applyToAll((Bone toApply, FunctionArgs e) =>
                    //{
                    //    toApply.transform.RotateAround(pos, axis ,rotation);
                    //});
                }
            }
        }
#endregion
    }

    private bool UseMouseInputThisFrame()
    {
#if UNITY_EDITOR
        return DebugModes.UseMouseInput && !activeTouchesFromFingersExist;
#elif UNITY_STANDALONE_WIN
        return true;
#else
        return false;
#endif
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

        MenuManager.Instance.AddEventMethod(typeof(MenuMain), "Begin", () =>
        {
            enabled = false;
        });
        GameManager.Instance.AddEventMethod(typeof(PlayPenState), "Begin", () =>
        {
            enabled = false;
        });
        GameManager.Instance.AddEventMethod(typeof(GameInit), "End", () =>
        {
            enabled = true;

            // Set handedness, 1 for right 0 for left. Default to 1
            ChangeHandedness(PlayerPrefs.GetInt("handedness", 1) == 1);
        });
    }
}

