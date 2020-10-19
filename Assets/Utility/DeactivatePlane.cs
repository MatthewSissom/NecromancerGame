using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivatePlane : MonoBehaviour
{
    // Start is called before the first frame update

    private void Start()
    {
        gameObject.GetComponent<MeshRenderer>().material.mainTextureScale = new Vector2(20, 20);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Bone b;
        if ((b = collision.gameObject.GetComponent<Bone>()) != null)
        {
            BoneManager.Instance.DeactivateBone(b);
        }
    }
}
