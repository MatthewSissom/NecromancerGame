using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomGravity : MonoBehaviour
{
    private static float gravitationalConst;
    private static Transform origin;
    private Rigidbody rb;

    public static void SetOrigin(Transform origin)
    {
        CustomGravity.origin = origin;
        gravitationalConst = Mathf.Abs(Physics.gravity.y);
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

    void FixedUpdate()
    {
        rb.AddForce((rb.transform.position - origin.position).normalized * gravitationalConst * Time.fixedDeltaTime,ForceMode.VelocityChange);
    }
}
