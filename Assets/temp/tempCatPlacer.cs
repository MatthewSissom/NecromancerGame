using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class tempCatPlacer : MonoBehaviour
{
    public GameObject EmptyArmature;
    [SerializeField]
    GameObject followTarget;
    [SerializeField]
    GameObject floor;

    private void TempRotateCat()
    {
        if (!EmptyArmature)
            return;
        EmptyArmature.transform.rotation = Quaternion.Euler(0, 45, 0);
        var behavior = EmptyArmature.GetComponent<CatBehavior>();
        if(behavior.transform.parent)
            behavior.transform.parent = null;
        behavior.followTarget = followTarget;
        behavior.GroundHeight = floor.transform.position.y;
        Vector3 cameraXZ = Camera.main.transform.position;
        cameraXZ.y = 0;
        behavior.transform.position += new Vector3(0, behavior.ChestHeight - behavior.transform.position.y, 0) + cameraXZ;

        /*void ChangeMatRecursive(Transform t)
        {
            for(int i = 0; i < t.childCount; i++)
            {
                if (t.TryGetComponent(out RendererGatherer rg))
                    rg.ChangeMat(0);
                else
                    ChangeMatRecursive(t.GetChild(i));
            }
        }
        ChangeMatRecursive(EmptyArmature.transform);*/
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod(typeof(PlayPenState), "Begin", TempRotateCat);
    }
}
