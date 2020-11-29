using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CountDown : State
{
    static CountDown instance;
    public TextMeshProUGUI text;
    string eventName = ""; 
    string eventText = "";
    int time = 0;


    protected override void Awake()
    {
        base.Awake();
        if (instance)
            Destroy(this);
        else
            instance = this;
    }

    public override IEnumerator Routine()
    {
        Begin();

        text.gameObject.SetActive(true);
        text.text = "Next Event: " + name;
        yield return new WaitForSeconds(time - 3);

        for (int i = 3; i > 0; i--)
        {
            text.text = name + "in " + i.ToString();
            yield return new WaitForSeconds(1);
        }
        text.text = "Go!";
        yield return new WaitForSeconds(.5f);
        text.gameObject.SetActive(false);

        End();
        yield return null;
    }

    public static void SetParams(string eventName, string eventText, int time)
    {
        instance.eventName = eventName;
        instance.eventText = eventText;
        instance.time = time;
    }
}
