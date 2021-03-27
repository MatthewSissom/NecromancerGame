using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class destroyRoot : MonoBehaviour
{
    [SerializeField]
    string activateOnState = default;
    [SerializeField]
    int skips = default;

    public void RemoveChildren()
    {
        if (skips > 0)
        {
            skips--;
            return;
        }
        gameObject.SetActive(true);
        //IEnumerator Routine()
        //{
        //    while (transform.childCount != 0)
        //    {
        //        int count = transform.childCount;
        //        while (count > 0)
        //        {
        //            int index = Random.Range(0, count);
        //            transform.GetChild(index).gameObject.SetActive(true);
        //            transform.GetChild(index).parent = null;
        //            count = transform.childCount;
        //            yield return new WaitForSeconds(0.2f);
        //        }
        //        yield return null;
        //    }
        //}
        //StartCoroutine(Routine());
    }

    private void Start()
    {
        //GameManager.Instance.AddEventMethod(activateOnState, "End", RemoveChildren);
        gameObject.SetActive(false);
    }
}
