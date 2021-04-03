using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInstructions : State
{
    private bool exit;
    public GameObject canvas;

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
        MenuManager.Instance.GoToMenu("Main");
        exit = true;
    }

    private void Start()
    {
        MenuManager.Instance.AddEventMethod(typeof(MenuMain), "begin", () => { canvas.SetActive(true); });

        MenuManager.Instance.AddEventMethod(typeof(MenuShowAssignments), "begin", () => { canvas.SetActive(false); });

        MenuManager.Instance.AddEventMethod(typeof(MenuMusicSliders), "begin", () => { canvas.SetActive(false); });

        GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "end", () => { canvas.SetActive(false); });
    }
}
