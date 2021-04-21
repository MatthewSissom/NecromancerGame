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

    public void Enable()
    {
        enabled = true;
        rb.useGravity = false;
    }

    public void Disable()
    {
        enabled = false;
        rb.velocity = new Vector3(0, rb.velocity.y, 0);
        rb.useGravity = true;
    }

    private void Start()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        if (!rb)
            Destroy(this);
        enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        rb.AddForce((rb.transform.position - origin.position).normalized * gravitationalConst);
    }
}
