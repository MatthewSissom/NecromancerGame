using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager
{
    Queue<PartialScore> scoreQueue;
    private static ScoreManager instance;
    public static ScoreManager Instance
    {
        get
        {
            if (instance == null)
                instance = new ScoreManager();
            return instance;
        }
    }

    public void Add(PartialScore ps)
    {
        scoreQueue.Enqueue(ps);
    }

    public PartialScore Next()
    {
        if(scoreQueue.Count > 0)
        {
            return scoreQueue.Dequeue();
        }
        return null;
    }

    private ScoreManager()
    {
        scoreQueue = new Queue<PartialScore>();
    }
}
