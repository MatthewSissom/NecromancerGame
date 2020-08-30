using System.Collections;
using System.Collections.Generic;

public class PartialScore
{
    public float value { get; private set; }
    public string name { get; private set; }

    public virtual float Apply(float currentTotal)
    {
        return currentTotal + value;
    }

    public string Text()
    {
        return "Has" + (value > 0 ? " a " : " no ") + name + " , +" + value.ToString() + "\n";
    }

    public PartialScore(string name, float value = 0)
    {
        this.name = name;
        this.value = value;
    }
}
