using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MenuDisplayScore : State
{
    public TextMeshProUGUI textBox;
    public GameObject canvas;

    string buttonName = "";

    public override IEnumerator Routine()
    {
        Begin();

        string partialList = "";
        float totalScore = 0;
        int lineCount = 0;
        const int maxLines = 5;
        PartialScore ps = ScoreManager.Instance.Next();
        textBox.text = "";
        yield return new WaitForSeconds(0.5f);
        FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/ChalkboardSFX");

        while (ps != null)
        {
            partialList += ps.text;
            totalScore = ps.Apply(totalScore);
            if (lineCount != maxLines)
                lineCount++;
            else
            {
                partialList = partialList.Substring(partialList.IndexOf("\n") + 1);
            }
            textBox.text = partialList;

            ps = ScoreManager.Instance.Next();
            yield return new WaitForSeconds(0.5f);
            FMODUnity.RuntimeManager.PlayOneShot("event:/SFX/UI/ChalkboardSFX");
        }

        //textBox.text = partialList + "\nTotal: " + totalScore.ToString();

        yield return new WaitUntil(() => { return buttonName == "Main"; });
        MenuManager.Instance.GoToMenu(buttonName);
        buttonName = "";

        End();
        yield break;
    }

    public void ButtonPressed(string name)
    {
        buttonName = name;
    }

    private void Start()
    {
        MenuManager.Instance.AddEventMethod("MenuMain", "begin", () => {
            textBox.text = "";
            canvas.SetActive(false); 
        });
        GameManager.Instance.AddEventMethod("CalculateScore", "end", () => { canvas.SetActive(true); });
    }
}
