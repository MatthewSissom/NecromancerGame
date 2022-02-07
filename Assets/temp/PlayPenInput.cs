using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayPenInput : MonoBehaviour
{
    Camera camera;
    [SerializeField]
    GameObject followTarget;
    public GameObject FollowTarget { get { return followTarget; } }

    public static PlayPenInput Instance { get; private set; }

    void Awake()
    {
        if (Instance)
            Destroy(this);
        else
            Instance = this;
    }

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
        var touches = Input.touches;
        if (touches.Length == 0)
            return;

        var t = touches[0];
        //pos represents the point in world space at the specified height
        Vector3 pos = t.position;
        pos.z = camera.transform.position.y - 1;
        pos = camera.ScreenToWorldPoint(pos);

#if UNITY_EDITOR
        if (DebugModes.UseMouseInput)
        {
            pos = Input.mousePosition;
            pos.z = camera.transform.position.y - 1;
            pos = camera.ScreenToWorldPoint(pos);
        }
#endif
        Vector3 rayDirection = (camera.transform.position - pos).normalized;
        Ray ray = new Ray(camera.transform.position, rayDirection);

        if(Physics.Raycast(ray,out RaycastHit info))
        {
            // check that the surface is horizontal - get angle between normal and up
            if(Mathf.Acos(Vector3.Dot(info.normal,Vector3.up)) < .1)
                followTarget.transform.position = info.point;
        }
    }
}
