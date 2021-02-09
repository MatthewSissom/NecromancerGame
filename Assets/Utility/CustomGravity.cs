using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    const float gravitationalConst = 5f;

    private static Transform origin;
    private Rigidbody rb;

    public static void SetOrigin(Transform origin)
    {
        CustomGravity.origin = origin;
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (!rb)
            Destroy(this);
        enabled = rb.useGravity;
        rb.useGravity = false;
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce((rb.transform.position - origin.position).normalized * gravitationalConst);
    }
}
