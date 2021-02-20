using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BoneAxies")]
public class BoneAxies : ScriptableObject
{
    [System.Serializable]
    struct AxisPair
    {
        public string key;
        public List<Vector3> values;
    }

    [SerializeField]
    //work around for serialization, shouldn't be accessed directly
    //use axies instead if at all possible
    List<AxisPair> axisPairs;

    //holds all axies for a given bone, stored by it's name
    private Dictionary<string, List<Vector3>> axies;

    //bracket accessor, acts like a dictionary
    public List<Vector3> this[string s]
    { 
        get 
        {
            List<Vector3> temp = null;
            if (axies == null)
                axies = new Dictionary<string, List<Vector3>>();
            axies.TryGetValue(s, out temp);
            return temp;
        }
        set 
        {
            Add(s, value);
        }
    }

    void ForceSerialization()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
        #endif
    }

    //updates data structures
    private void Add(string key, List<Vector3> val)
    {
        //update list
        AxisPair newPair = new AxisPair();
        newPair.values = val;
        newPair.key = key;

        int index = axisPairs.FindIndex(kvp => kvp.key == key);
        if(index == -1)
        {
            axisPairs.Add(newPair);
        }
        else
        {
            axisPairs[index] = newPair;
        }
        ForceSerialization();

        //update dictionary
        if (axies.ContainsKey(key))
        {
            axies[key] = val;
        }
        else
        {
            axies.Add(key, val);
        }
    }

    public void Remove(string key, List<Vector3> toRemove)
    {
        int index = axisPairs.FindIndex(kvp => kvp.key == key);
        foreach (var removeThis in toRemove)
        {
            axisPairs[index].values.Remove(removeThis);
            axies[key].Remove(removeThis);
        }
    }

    private void OnEnable()
    {
        axies = new Dictionary<string, List<Vector3>>();
        foreach(var pair in axisPairs)
        {
            axies.Add(pair.key, pair.values);
        }
    }
}
