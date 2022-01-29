using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuShowAssignments : State
{
    private bool exit;                              // Bool for exiting state
    public GameObject canvas;                       // Canvas for displaying assignmetn data

    // fields for assignemnt displays
    public AssignmentDisplay assignment1;
    public AssignmentDisplay assignment2;
    public AssignmentDisplay assignment3;

    public AssignmentChecker assignmentChecker;     // Reference to assignment checker
    public AssignementDataBase selectedAssignment;  // The selected assignment to check for when building

    // for testing - will be replaced when more assignments are made
    public AssignementDataBase data1;
    public AssignementDataBase data2;
    public AssignementDataBase data3;

    private void Start()
    {
        canvas.SetActive(false);
        //Toggle canvas when switching between states
        MenuManager.Instance.AddEventMethod(typeof(MenuMain), "begin", () => { canvas.SetActive(false); });
        MenuManager.Instance.AddEventMethod(typeof(MenuShowAssignments), "begin", () => { canvas.SetActive(true); });

        GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "end", () => { canvas.SetActive(false); });
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

    // Method for button presses
    public void ButtonPressed(string name)
    {
        // If we want to play the game, set the current assignment first
        if (name == "Play")
        {
            assignmentChecker.currentAssignment = selectedAssignment;
        }

        MenuManager.Instance.GoToMenu(name);
        exit = true;
    }

    // Gets assignments and attaches them to assignment template prefabs
    private void GetAssignments()
    {
        // Test code, will need something different for when more assignments are
        assignment1.assignmentData = data1;
        assignment2.assignmentData = data2;
        assignment3.assignmentData = data3;

        assignment1.Init();
        assignment2.Init();
        assignment3.Init();
    }

    // Note: Still need systems for selecting from more than 3 assignments, sorting assignemnts, and deciding which ones to display
}
