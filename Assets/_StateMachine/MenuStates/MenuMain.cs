using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMain : State
{
    private string buttonName = null;

    public override IEnumerator Routine()
    {
        base.Begin();

        while (buttonName == null)
        {
            yield return null;
        }

        MenuManager.Instance.buttonName = buttonName;

        base.End();
        buttonName = null;
        yield break;
    }

    public void ButtonPressed(string name)
    {
        buttonName = name;
    }
}
