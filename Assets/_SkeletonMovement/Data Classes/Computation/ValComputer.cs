using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lazily computes values and stores the result
abstract public class ValComputer<T>
{
    public T Val { get => computed ? val : val = ComputeVal(); }

    private bool computed = false;
    private T val = default;

    protected virtual T ComputeVal()
    {
        computed = true;
        return default;
    }
    public static implicit operator T(ValComputer<T> valComputer) => valComputer.Val;
    public static implicit operator ValComputer<T>(T val) => new TrivalComputer<T>(val);
}

public class TrivalComputer<T> : ValComputer<T>
{
    T mVal;
    public TrivalComputer(T val) { mVal = val; }
    protected override T ComputeVal() { return mVal; }
}