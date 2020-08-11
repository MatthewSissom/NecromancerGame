using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : State
{
    public
    TextMeshProUGUI text;

    public override IEnumerator Routine()
    {
        Begin();

        yield return new WaitForSeconds(.2f);
        text.text = "3";
        yield return new WaitForSeconds(.5f);
        text.text = "2";
        yield return new WaitForSeconds(.5f);
        text.text = "1";
        yield return new WaitForSeconds(.5f);
        text.text = "Start!";
        yield return new WaitForSeconds(.5f);
        text.gameObject.SetActive(false);

        End();
        yield return null;
    }
}
