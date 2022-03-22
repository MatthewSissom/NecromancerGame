using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneRain : MonoBehaviour
{
    public float delay;
    public float timeBetweenDrop;

    private float timer;
    private bool dropping;
    // Start is called before the first frame update
    void Start()
    {
        ApplyToAllChildren(ChildInit);
        timer = 0;
        dropping = false;
    }

    void ChildInit(Transform child)
    {
        Rigidbody rb = child.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeAll;
        rb.useGravity = false;
    }

    void ReleaseChild(Transform child)
    {
        Rigidbody rb = child.GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.None;
        rb.useGravity = true;

        child.parent = null;
    }

    Transform GetRandomChild()
    {
        if (transform.childCount == 0)
            return null;

        int index = Random.Range(0, transform.childCount);
        return transform.GetChild(index);
    }

    void ApplyToAllChildren(System.Action<Transform> action)
    {
        for (int i = 0, cnt = transform.childCount; i < cnt; i++)
        {
            action(transform.GetChild(i));
        }
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;

        if (!dropping)
        {
            if (timer > delay)
            {
                dropping = true;
                timer -= delay;
            }
            else
                return;
        }

        if (timer > timeBetweenDrop)
        {
            timer -= timeBetweenDrop;

            Transform randChild = GetRandomChild();
            if (randChild == null)
                return;

            ReleaseChild(randChild);
        }
    }
}
