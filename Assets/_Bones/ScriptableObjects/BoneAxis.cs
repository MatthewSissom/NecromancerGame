using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneAxis : ScriptableObject
{
    [System.Serializable]
    struct AxisPair
    {
        public string key;
        public List<Vector3> values;
    }

    [SerializeField]
    //work around for serialization, shouldn't be accessed directly in code
    //use axies instead if at all possible
    List<AxisPair> axisPairs = default;

    //holds all axis for a given bone, stored by it's name
    private Dictionary<string, List<Vector3>> axis;

    //bracket accessor, acts like a dictionary
    public List<Vector3> this[string s]
    { 
        get 
        {
            List<Vector3> temp = null;
            if (axis == null)
                axis = new Dictionary<string, List<Vector3>>();
            axis.TryGetValue(s, out temp);
            return temp;
        }
        set 
        {
            Add(s, value);
        }
    }

    //for accesing axis pair arrays, returns the compliment of a given point
    public int GetComplimentIndex(int currentIndex)
    {
        return Mathf.FloorToInt(currentIndex / 2) * 2 + (currentIndex + 1) % 2;
    }

    public Vector3 GetCompliment(string key, int currentIndex)
    {
        return axis[key][GetComplimentIndex(currentIndex)];
    }

    
    public List<Vector3> GetWorldAxis(string key, Matrix4x4 localToWorld, int index)
    {
        if (index < 0)
            return null;

        int complimentIndex = GetComplimentIndex(index);
        //add one to compliment index because range is not inclusive
        return GetWorldAxisInRange(key, localToWorld, Mathf.Min(complimentIndex,index), Mathf.Max(complimentIndex, index)+1);
    }

    public List<Vector3> GetAllWorldAxis(string key, Matrix4x4 localToWorld)
    {
        return GetWorldAxisInRange(key, localToWorld, 0, int.MaxValue);
    }

    //get all world axis in a range, end is non inclusive
    private List<Vector3> GetWorldAxisInRange(string key, Matrix4x4 localToWorld, int start, int end)
    {
        if (key == null || !axis.ContainsKey(key) || axis[key].Count == 0)
            return null;

        if (end > axis[key].Count)
            end = axis[key].Count;

        List<Vector3> worldAxis = new List<Vector3>();
        List<Vector3> localAxis = axis[key];
        for(int i = start; i < end; i++)
        {
            worldAxis.Add(localToWorld.MultiplyPoint(localAxis[i]));
        }
        return worldAxis;
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
        if (axis.ContainsKey(key))
        {
            axis[key] = val;
        }
        else
        {
            axis.Add(key, val);
        }
    }

    public void Remove(string key, List<Vector3> toRemove)
    {
        int index = axisPairs.FindIndex(kvp => kvp.key == key);
        foreach (var removeThis in toRemove)
        {
            axisPairs[index].values.Remove(removeThis);
            axis[key].Remove(removeThis);
        }
    }

    private void OnEnable()
    {
        axis = new Dictionary<string, List<Vector3>>();
        foreach(var pair in axisPairs)
        {
            axis.Add(pair.key, pair.values);
        }
    }
}
