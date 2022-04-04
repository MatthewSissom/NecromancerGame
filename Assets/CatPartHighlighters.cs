using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CatPartHighlighters : MonoBehaviour
{
    public static CatPartHighlighters instance;

    public Material headMat;
    public Material shoulderSpineMat;
    public Material hipSpineMat;
    public Material shoulderMat;
    public Material hipMat;
    public Material tailMat;
    public Material footMat;
    public Material leftLegStartMat;
    public Material rightLegStartMat;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }
}
