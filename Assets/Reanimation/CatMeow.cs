using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatMeow : MonoBehaviour
{
    private float offset;
    private IEnumerator catMeows;

    public float minSecondsCooldown;
    public float maxSecondsCooldown;

    // Start is called before the first frame update
    void Start()
    {
        offset = Random.Range(.5f, 1.0f);
        catMeows = DoCatMeow(offset);

        minSecondsCooldown = 20.0f;
        maxSecondsCooldown = 45.0f;

        StartCoroutine(catMeows);
    }

    IEnumerator DoCatMeow(float offset)
	{
        while (true)
        {
            //Debug.Log("Playing meow");
            AudioManager.Instance.PlayCatMeow();

            //Debug.Log("Generating new offset");
            float newOffset = Random.Range(minSecondsCooldown, maxSecondsCooldown);

            //Debug.Log("Waiting for callback");
            yield return new WaitForSeconds(newOffset);
        }
	}
}
