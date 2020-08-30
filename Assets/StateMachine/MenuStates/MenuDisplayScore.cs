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
        PartialScore ps = ScoreManager.Instance.Next();
        textBox.text = "";
        yield return new WaitForSeconds(0.5f);

        while (ps != null)
        {
            partialList += ps.Text();
            totalScore = ps.Apply(totalScore);
            textBox.text = partialList;

            ps = ScoreManager.Instance.Next();
            yield return new WaitForSeconds(0.5f);
        }

        textBox.text = partialList + "Total: " + totalScore.ToString();

        yield return new WaitUntil(() => { return buttonName == "Main"; });

        End();
        yield break;
    }

    public void ButtonPressed(string name)
    {
        buttonName = name;
    }

    private void Start()
    {
        MenuManager.Instance.AddStateBeginMethod("MenuMain", () => {
            textBox.text = "";
            canvas.SetActive(false); 
        });
        StateManager.Instance.AddStateEndMethod("CalculateScore", () => { canvas.SetActive(true); });
    }
}
