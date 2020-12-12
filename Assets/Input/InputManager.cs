using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    //TEMP stats for input
    bool recordingVelocities = false;
    List<float> touchVelocities = new List<float>();

    [Header ("GameObjects")]
    public GameObject touchProxy;
    public GameObject searchingTouch;
    public GameObject floatingTouch;
    public GameObject rotationProxy;

    [Header ("Game Feel Values")]
    public float height;
    public float rotationRadSquared;
    public float rotationVelocityThreshold;

    static public InputManager Instance;

    TouchProxy[] proxies = new TouchProxy[5];

    //temp mouse input var
    bool holdingMouseDown = false;

    //removes a single proxie from all data structures
    private void remove(int id)
    {
        ReplaceWith<TouchProxy>(proxies[id]);
    }

    //replaces the given touch with another touch and return the replacment
    public T ReplaceWith<T>(TouchProxy toReplace, Vector3? pos = null) where T : TouchProxy
    {
        //find id
        int id = -1;
        for(int possibleId = 0; possibleId < 5; possibleId++)
        {
            if (proxies[possibleId] == toReplace)
            {
                id = possibleId;
                break;
            }
        }
        if (id == -1) return null;

        return ReplaceWith<T>(id, pos);
    }

    public T ReplaceWith<T>(int id, Vector3? pos) where T : TouchProxy
    {
        var toReplace = proxies[id];
        if (!toReplace)
            return null;

        if (pos == null)
            pos = toReplace.transform.position;


        //create new proxy
        T newProxy;
        if (typeof(T) == typeof(TouchProxy))
            newProxy = Instantiate(touchProxy, (Vector3)pos, Quaternion.identity).GetComponent<T>();
        else if (typeof(T) == typeof(FloatingTouch))
            newProxy = Instantiate(floatingTouch, (Vector3)pos, Quaternion.identity).GetComponent<T>();
        else if (typeof(T) == typeof(SearchingTouch))
            newProxy = Instantiate(searchingTouch, (Vector3)pos, Quaternion.identity).GetComponent<T>();
        else if (typeof(T) == typeof(RotationProxy))
            newProxy = Instantiate(rotationProxy, (Vector3)pos, Quaternion.identity).GetComponent<T>();
        else
            throw new System.Exception("ReplaceWith<T> does not support type \"" + typeof(T) + "\"");

        proxies[id] = newProxy;
        Destroy(toReplace.gameObject);
        return newProxy;
    }

    //removes all proxies
    public void clear()
    {
        for (int id = 0; id < 5; id++)
        {
            Destroy(proxies[id].gameObject);
        }
        proxies = new TouchProxy[5];
        for (int i = 0; i < 5; i++)
        {
            proxies[i] = Instantiate(touchProxy).GetComponent<TouchProxy>();
        }
    }

    private FloatingTouch isRotationTouch(Vector3 pos)
    {
        for (int id = 0; id < 5; id++)
        {
            FloatingTouch ft = proxies[id] as FloatingTouch;
            if (ft &&
                //ft.velocity.magnitude < rotationVelocityThreshold &&
                (ft.transform.position - pos).sqrMagnitude < rotationRadSquared)
            {
                return ft;
            }
        }
        return null;
    }

    void CreateTouchProxy(int id, Vector3 pos, FloatingTouch rotationParent = null)
    {
        if(rotationParent)
        {
            ReplaceWith<RotationProxy>(id,pos).Parent = rotationParent;
            return;
        }
        ReplaceWith<SearchingTouch>(id, pos);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        #region mouseInput
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
                CreateTouchProxy(id, pos, isRotationTouch(pos));
            }
            else
            {
                proxies[id].Move(pos, rad);
                if (recordingVelocities)
                {
                    FloatingTouch ft = proxies[id] as FloatingTouch;
                    if (ft)
                        touchVelocities.Add(ft.speed);
                }
            }
        }
        else if(holdingMouseDown)
        {
            holdingMouseDown = false;
            remove(0);
        }
        if(Input.mouseScrollDelta.y != 0)
        {
            FloatingTouch active = proxies[0] as FloatingTouch;
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
            //try
            {
                //pos represents the point in world space at the specified height
                Vector3 pos = t.position;
                pos.z = Camera.main.transform.position.y - height;
                Vector3 radVec = pos + new Vector3(t.radius, 0, 0);

                pos = Camera.main.ScreenToWorldPoint(pos);
                float rad = (Camera.main.ScreenToWorldPoint(radVec) - pos).magnitude;

                int id = t.fingerId;

                switch (t.phase)
                {
                    //new touch, create a proxy at the touch location and add it to dictionary
                    case TouchPhase.Began:
                        CreateTouchProxy(id, pos, isRotationTouch(pos));
                        break;
                    //update the proxy of existing touches
                    case TouchPhase.Moved:
                        proxies[id].Move(pos, rad);
                        if (recordingVelocities)
                        {
                            FloatingTouch ft = proxies[id] as FloatingTouch;
                            if (ft)
                                touchVelocities.Add(ft.speed);
                        }
                        break;
                    //destroy any proxies for ended touches
                    case TouchPhase.Ended:
                        remove(id);
                        break;
                    case TouchPhase.Canceled:
                        remove(id);
                        break;
                    default:
                        break;
                }
            }
            //catch (System.Exception e )
            {
               // proxies = null;
            }
        }
    }

    private void Awake()
    {
        proxies = new TouchProxy[5];
        for (int i = 0; i < 5; i++)
        {
            proxies[i] = Instantiate(touchProxy).GetComponent<TouchProxy>();
        }
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

