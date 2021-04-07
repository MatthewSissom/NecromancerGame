//#define USING_TOUCH

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPenInput : MonoBehaviour
{
    Camera camera;
    [SerializeField]
    GameObject followTarget;

    private void Start()
    {
        camera = Camera.main;
        enabled = false;
        GameManager.Instance.AddEventMethod(typeof(PlayPenState), "Begin", () => { enabled = true; });
        MenuManager.Instance.AddEventMethod(typeof(MenuMain), "Begin", () => { enabled = false; });
    }

    // Update is called once per frame
    void Update()
    {
#if USING_TOUCH
        var touches = Input.touches;
        if (touches.Length == 0)
            return;

        var t = touches[0];
        //pos represents the point in world space at the specified height
        Vector3 pos = t.position;
        pos.z = camera.transform.position.y - 1;
        pos = camera.ScreenToWorldPoint(pos);

#else
        Vector3 pos = Input.mousePosition;
        pos.z = camera.transform.position.y - 1;
        pos = camera.ScreenToWorldPoint(pos);
#endif
        Vector3 rayDirection = (camera.transform.position - pos).normalized;
        Ray ray = new Ray(camera.transform.position, rayDirection);

        if(Physics.Raycast(ray,out RaycastHit info))
        {
            followTarget.transform.position = info.point;
        }
    }
}
