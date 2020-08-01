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
    List<TouchProxy> allProxies = new List<TouchProxy>();

    private void remove(int id)
    {
        Destroy(proxies[id].gameObject);
        allProxies.Remove(proxies[id]);
        proxies.Remove(id);
    }

    private TouchProxy isRotationTouch(TouchProxy tp)
    {
        foreach(TouchProxy t in allProxies)
        {
            if (t != tp && (t.transform.position - tp.transform.position).sqrMagnitude < rotationRadSquared)
                return t;
        }
        return null;
    }

    private TouchProxy isRotationTouch(Vector3 pos)
    {
        foreach (TouchProxy t in allProxies)
        {
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
            pos = Camera.main.ScreenToWorldPoint(pos);

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
                        allProxies.Add(newScript);
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
                        allProxies.Add(newScript);
                        newScript.Parent = rotationParent;
                    }
                    break;
                //update the proxy of existing touches
                case TouchPhase.Moved:
                    proxies[id].transform.position = pos;
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
}

