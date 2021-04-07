using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuIntroScreen : State
{
    public GameObject canvas;

    public override IEnumerator Routine()
    {
        Begin();
        canvas.SetActive(true);

        while (true)
        {
            if (Input.touches.Length > 0 || Input.GetMouseButtonDown(0))
            {
                End();
                canvas.SetActive(false);
                yield break;
            }
            yield return null;
        }
    }
}
