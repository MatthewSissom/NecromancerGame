using System.Collections;
using System.Collections.Generic;

public class PartialScore
{
    public float value { get; private set; }
    public string name { get; private set; }
    public string text { get; private set; }

    public virtual float Apply(float currentTotal)
    {
        return currentTotal + value;
    }

    public string Text()
    {
        return "Has" + (value > 0 ? " a " : " no ") + name + " , +" + value.ToString() + "!\n";
    }

    // Will - For only printing out text, no score values
    public string TextOnly()
    {
        return name + "\n";
    }

    public PartialScore(string name, float value = 0, string text = "")
    {
        this.name = name;
        this.value = value;
        if (text != "")
            this.text = text;
        else
            this.text = Text();
    }

    // Will - new contructor for only printing text
    public PartialScore(string name)
    {
        this.name = name;
        this.text = TextOnly();
    }
}
