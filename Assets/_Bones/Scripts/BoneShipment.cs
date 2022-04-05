using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoneShipment : MonoBehaviour
{
    public List<GameObject> bones;

    public List<GameObject> specialBones;

    public List<GameObject> BoneShuffle(bool special)
    {
        System.Random _random = new System.Random();

        GameObject myGO;

        int n = bones.Count;
        for (int i = 0; i < n; i++)
        {
            int r = i + (int)(_random.NextDouble() * (n - i));
            myGO = bones[r];
            bones[r] = bones[i];
            bones[i] = myGO;
        }

        if (special)
        {
            int randomIndex = Random.Range(0, n - 1);
            int specialIndex = Random.Range(0, specialBones.Count - 1);

            bones[randomIndex] = specialBones[specialIndex];
        }
        return bones;
    }
}
