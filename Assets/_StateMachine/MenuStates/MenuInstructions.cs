using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuInstructions : State
{
    private bool exit;
    public GameObject canvas;

    public List<GameObject> instructionImages;
    public List<GameObject> hands;

    Vector3 initalHandOnePos;
    public float handOneEnd;
    public float handOneDuration;
    
    public float handTwoEnd;
    public float handTwoDuration;

    public float handThreeStart;
    public float handThreeEnd;
    public float handThreeDuration;

    Vector3 initalHandFourPos;
    Vector3 boneFourInitalPos;
    GameObject boneFour;
    public float handFourEnd;
    public float handFourDuration;

    Vector3 initalHandFivePos;
    GameObject boneFive;
    Vector3 boneFiveInitalPos;
    public float handFiveEnd;
    public float handFiveDuration;


    int instructionIndex;
    float handTime;

    public override IEnumerator Routine()
    {
        Begin();

        ResetVars();
        exit = false;
        while (!exit)
        {
            handTime += Time.deltaTime;
            AnimateHands();
            yield return null;
        }

        End();
        yield break;
    }


    public void ButtonPressed(string name)
    {
        MenuManager.Instance.GoToMenu("Main");
        exit = true;
    }

    public void NextInstruction()
    {
        if(instructionIndex < instructionImages.Count-1)
        {
            instructionImages[instructionIndex].SetActive(false);
            instructionIndex++;
            instructionImages[instructionIndex].SetActive(true);
        }


    }

    public void PreviousInstruction()
    {
        if (instructionIndex > 0)
        {
            instructionImages[instructionIndex].SetActive(false);
            instructionIndex--;
            instructionImages[instructionIndex].SetActive(true);
        }
    }

    private void AnimateHands()
    {
        switch (instructionIndex)
        {
            case 0:
                hands[0].transform.position = initalHandOnePos + new Vector3(Mathf.PingPong(handTime / handOneDuration * handOneEnd, handOneEnd),0,0);
                break;
            case 1:
                hands[1].transform.rotation = Quaternion.Euler(new Vector3(0, 0, Mathf.PingPong(handTime / handTwoDuration * Mathf.Abs(handTwoEnd), Mathf.Abs(handTwoEnd)) * Mathf.Sign(handTwoEnd)));
                break;
            case 2:
                break;
            case 3:
                hands[3].transform.position = initalHandFourPos - new Vector3(0, Mathf.PingPong(handTime / handFourDuration * handFourEnd, handFourEnd), 0);
                break;
            case 4:
                hands[4].transform.position = initalHandFivePos + new Vector3(Mathf.PingPong(handTime / handFiveDuration * handFiveEnd, handFiveEnd), 0, 0);
                break;
        }
    }

    private void ResetVars()
    {
        instructionImages[instructionIndex].SetActive(false);
        instructionIndex = 0;
        instructionImages[instructionIndex].SetActive(true);
        handTime = 0;
    }

    private void Start()
    {
        canvas.SetActive(false);

        //events
        MenuManager.Instance.AddEventMethod(typeof(MenuInstructions), "begin", () => { canvas.SetActive(true); });
        MenuManager.Instance.AddEventMethod(typeof(MenuMain), "begin", () => { canvas.SetActive(false); });
        MenuManager.Instance.AddEventMethod(typeof(MenuShowAssignments), "begin", () => { canvas.SetActive(false); });
        MenuManager.Instance.AddEventMethod(typeof(MenuMusicSliders), "begin", () => { canvas.SetActive(false); });
        GameManager.Instance.AddEventMethod(typeof(GameCleanUp), "end", () => { canvas.SetActive(false); });

        //array init
        hands = new List<GameObject>();
        for (int i = 0; i < instructionImages.Count; i++)
        {
            hands.Add(instructionImages[i].transform.GetChild(0).gameObject);
            instructionImages[i].SetActive(false);
        }
        
        initalHandOnePos = hands[0].transform.position;
        initalHandFourPos = hands[3].transform.position;
        initalHandFivePos = hands[4].transform.position;
        boneFour = hands[3].transform.GetChild(1).gameObject;
        boneFourInitalPos = boneFour.transform.position;
        boneFive = hands[4].transform.GetChild(1).gameObject;
        boneFiveInitalPos = boneFive.transform.position;

    }
}
