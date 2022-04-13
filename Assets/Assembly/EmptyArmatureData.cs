using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EmptyArmatureData : MonoBehaviour
{
    [field: SerializeField]
    public SkeletonTransforms Transforms { get; private set; }
}
