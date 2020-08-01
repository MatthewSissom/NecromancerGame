using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : State
{
    private bool buttonPressed = false;

    public override IEnumerator Routine()
    {
        base.Begin();

        while (!buttonPressed)
        {
            yield return null;
        }

        base.End();
        yield break;
    }

    public void playPressed()
    {
        buttonPressed = true;
    }

    public MainMenu()
    {
        Name = "MainMenu";
    }
}
