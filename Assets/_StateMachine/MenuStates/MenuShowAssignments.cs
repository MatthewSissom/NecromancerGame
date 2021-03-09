using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuShowAssignments : State
{
    private bool exit;
    public GameObject canvas;
    public AssignmentDisplay assignment1;
    public AssignmentDisplay assignment2;
    public AssignmentDisplay assignment3;
    public AssignmentChecker assignmentChecker;
    public AssignementDataBase selectedAssignment;

    // for testing
    public AssignementDataBase data1;
    public AssignementDataBase data2;
    public AssignementDataBase data3;

    private void Start()
    {
        MenuManager.Instance.AddEventMethod("MenuMain", "begin", () => { canvas.SetActive(false); });

        MenuManager.Instance.AddEventMethod("MenuShowAssignments", "begin", () => { canvas.SetActive(true); });

        GameManager.Instance.AddEventMethod("GameCleanUp", "end", () => { canvas.SetActive(false); });

        //MenuManager.Instance.AddEventMethod("MenuShowAssignments", "begin", () => { canvas.SetActive(true); });
        //GameManager.Instance.AddEventMethod("MenuShowAssignments", "end", () => { canvas.SetActive(false); });
    }

    public override IEnumerator Routine()
    {
        Begin();

        GetAssignments();

        exit = false;
        while (!exit)
        {
            yield return null;
        }

        End();
        yield break;
    }

    public void ButtonPressed(string name)
    {
        if (name == "Play")
        {
            assignmentChecker.currentAssignment = selectedAssignment;
        }

        MenuManager.Instance.GoToMenu(name);
        exit = true;
    }

    // Gets assignments and attached them to assignment template prefabs
    private void GetAssignments()
    {
        assignment1.assignmentData = data1;
        assignment2.assignmentData = data2;
        assignment3.assignmentData = data3;

        assignment1.Init();
        assignment2.Init();
        assignment3.Init();
    }
}
