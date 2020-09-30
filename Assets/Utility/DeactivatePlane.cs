using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeactivatePlane : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnCollisionEnter(Collision collision)
    {
        bone b;
        if ((b = collision.gameObject.GetComponent<bone>()) != null)
        {
            boneManager.Instance.DeactivateBone(b);
        }
    }
}
