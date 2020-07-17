using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class boneManager : MonoBehaviour
{
    private int numGroups;
    public int NumGroups
    {
        get { return numGroups;  }
        set { numGroups = value; }
    }
    public TextMeshProUGUI groupCount;
    public TextMeshProUGUI total;
    private string startingString;
    private string startingStringTotal;

    // Start is called before the first frame update
    void Start()
    {
        startingString = groupCount.text;
        startingStringTotal = total.text;
    }

    public void updateUI(int initalScore)
    {
        groupCount.text = startingString + numGroups.ToString();
        total.text = startingStringTotal + (initalScore / numGroups).ToString();
    }
}