using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header ("GameObjects")]
    public GameObject touchProxy;
    public GameObject rotationProxy;

    [Header ("Game Feel Values")]
    public float height;
    public float rotationRadSquared;


    Dictionary<int, TouchProxy> proxies = new Dictionary<int, TouchProxy>();
    List<int> allIDs = new List<int>();

    //removes a single proxie from all data structures
    private void remove(int id)
    {
        Destroy(proxies[id].gameObject);
        allIDs.Remove(id);
        proxies.Remove(id);
    }

    //removes all proxies
    private void clear()
    {
        foreach (int id in allIDs)
        {
            Destroy(proxies[id].gameObject);
        }
        proxies = new Dictionary<int, TouchProxy>();
        allIDs = new List<int>();
    }

    private TouchProxy isRotationTouch(Vector3 pos)
    {
        foreach (int i  in allIDs)
        {
            TouchProxy t = proxies[i];
            if ((t.transform.position - pos).sqrMagnitude < rotationRadSquared)
                return t;
        }
        return null;
    }

    // Update is called once per frame
    void Update()
    {
        foreach(Touch t in Input.touches)
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
                    TouchProxy rotationParent = isRotationTouch(pos);
                    //if not a rotation touch create a normal touch Proxy
                    if (!rotationParent)
                    {
                        var newScript = Instantiate(
                               touchProxy,
                               pos,
                               Quaternion.identity
                            ).GetComponent<TouchProxy>();
                        proxies.Add(id, newScript);
                        allIDs.Add(id);
                    }
                    //if rotating create a rotation proxy
                    else
                    {
                        var newScript = Instantiate(
                               rotationProxy,
                               pos,
                               Quaternion.identity
                            ).GetComponent<RotationProxy>();
                        proxies.Add(id, newScript);
                        allIDs.Add(id);
                        newScript.Parent = rotationParent;
                    }
                    break;
                //update the proxy of existing touches
                case TouchPhase.Moved:
                    proxies[id].transform.position = pos;
                    proxies[id].radius = rad;
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
    }

    private void Start()
    {
        StateManager.Instance.AddStateBeginMethod("Conveyor", () => { this.enabled = true; });
        StateManager.Instance.AddStateEndMethod("Conveyor", () => { this.enabled = false; clear(); });
        this.enabled = false;
    }
}

