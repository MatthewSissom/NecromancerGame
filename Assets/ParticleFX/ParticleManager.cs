using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    static ParticleManager Instance;

    //all particles
    public List<GameObject> PFX;
    private Dictionary<string, GameObject> directory;

    private void Awake()
    {
        if(Instance)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        directory = new Dictionary<string, GameObject>();
        foreach(GameObject g in PFX)
        {
            directory.Add(g.name, g);
        }
    }

    public static GameObject CreateEffect(string name, Vector3 pos)
    {
        if (!Instance.directory.ContainsKey(name))
            Debug.LogError("ParticleManager does not contain effect \'" + name + "\'");
        return Instantiate(Instance.directory[name], pos, Quaternion.identity);
    }
}
