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
        behavior.transform.parent = null;
        behavior.followTarget = followTarget;
        behavior.GroundHeight = floor.transform.position.y;
        behavior.transform.position += new Vector3(0, behavior.ChestHeight - behavior.transform.position.y, 0);
    }

    private void Start()
    {
        GameManager.Instance.AddEventMethod(typeof(BoneAssembler), "End", TempRotateCat);
    }
}
