using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuMain : State
{
    private bool exit;

    public override IEnumerator Routine()
    {
        base.Begin();

        exit = false;
        while (!exit)
        {
            yield return null;
        }

        base.End();
        yield break;
    }

    public void ButtonPressed(string name)
    {
        exit = true;
        MenuManager.Instance.GoToMenu(name);
    }
}
