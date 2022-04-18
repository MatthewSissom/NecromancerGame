using System.Collections;
using System.Collections.Generic;
using System;
using System.Reflection;
using UnityEngine;

[Serializable]
public class LabeledLimbData<T>
{
    // Leg transforms should correspond to the transform coming off of the shoulder or the hips excluding offsets
    [field: SerializeField]
    public T FrontLeftLeg { get; private set; }
    [field: SerializeField]
    public T FrontRightLeg { get; private set; }
    [field: SerializeField]
    public T BackLeftLeg { get; private set; }
    [field: SerializeField]
    public T BackRightLeg { get; private set; }

    public LabeledLimbData<U> Convert<U>(Func<T, U> converter)
    {
        return new LabeledLimbData<U>(
            converter(FrontLeftLeg),
            converter(FrontRightLeg),
            converter(BackLeftLeg),
            converter(BackRightLeg)
        );
    }

    public LabeledLimbData<F> Combine<U, F>(LabeledLimbData<U> uData, Func<T, U, F> combiner)
    {
        return new LabeledLimbData<F>(
            combiner(FrontLeftLeg, uData.FrontLeftLeg),
            combiner(FrontRightLeg, uData.FrontRightLeg),
            combiner(BackLeftLeg, uData.BackLeftLeg),
            combiner(BackRightLeg, uData.BackRightLeg)
        );
    }

    public void Modify<U>(LabeledLimbData<U> uData, Action<T, U> func)
    {
        func(FrontLeftLeg, uData.FrontLeftLeg);
        func(FrontRightLeg, uData.FrontRightLeg);
        func(BackLeftLeg, uData.BackLeftLeg);
        func(BackRightLeg, uData.BackRightLeg);
    }

    public void Modify<U>(U uData, Action<T, U> func)
    {
        func(FrontLeftLeg,  uData);
        func(FrontRightLeg, uData);
        func(BackLeftLeg,   uData);
        func(BackRightLeg, uData);
    }

    public bool Contains(T val)
    {
        return  FrontLeftLeg.Equals(val) ||
                FrontRightLeg.Equals(val) ||
                BackLeftLeg.Equals(val) ||
                BackRightLeg.Equals(val);
    }

    public List<T> ToList()
    {
        List<T> list = new List<T>();
        list.Add(FrontLeftLeg);
        list.Add(FrontRightLeg);
        list.Add(BackLeftLeg);
        list.Add(BackRightLeg);
        return list;
    }

    public LabeledLimbData(
        T frontLeftLeg,
        T frontRightLeg,
        T backLeftLeg,
        T backRightLeg
        )
    {
        FrontLeftLeg = frontLeftLeg;
        FrontRightLeg = frontRightLeg;
        BackLeftLeg  = backLeftLeg;
        BackRightLeg = backRightLeg;
    }

    public LabeledLimbData()
    {
        ConstructorInfo[] ctors = typeof(T).GetConstructors();
        ConstructorInfo defaultCtor = null;
        foreach (var ctor in ctors)
        {
            if (ctor.GetParameters().Length == 0)
                defaultCtor = ctor;
        }
        if (defaultCtor != null)
        {
             FrontLeftLeg  =  (T)defaultCtor.Invoke(new object[] { });
             FrontRightLeg =  (T)defaultCtor.Invoke(new object[] { });
             BackLeftLeg   =  (T)defaultCtor.Invoke(new object[] { });
            BackRightLeg = (T)defaultCtor.Invoke(new object[] { });
        }
        else
        {
            FrontLeftLeg = default;
            FrontRightLeg = default;
            BackLeftLeg = default;
            BackRightLeg = default;
        }
    }
}

[Serializable]
public class LabeledSpineData<T>
{
    // Spine transforms should be at the END of their gameObject, tip of the nose, tip of the tail, etc.
    // Values should be null if 
    [field: SerializeField]
    public T Head { get; private set; }
    [field: SerializeField]
    public T Shoulder { get; private set; }
    [field: SerializeField]
    public T Hip { get; private set; }
    [field: SerializeField]
    public T Tail { get; private set; }

    public LabeledSpineData<U> Convert<U>(Func<T, U> converter)
    {
        return new LabeledSpineData<U>(
            converter(Head),
            converter(Shoulder),
            converter(Hip),
            converter(Tail)
        );
    }

    public LabeledSpineData<F> Combine<U, F>(LabeledSpineData<U> uData, Func<T, U, F> combiner)
    {
        return new LabeledSpineData<F>(
            combiner(Head, uData.Head),
            combiner(Shoulder, uData.Shoulder),
            combiner(Hip, uData.Hip),
            combiner(Tail, uData.Tail)
        );
    }

    public void Modify<U>(LabeledSpineData<U> uData, Action<T, U> func)
    {
        func(Head, uData.Head);
        func(Shoulder, uData.Shoulder);
        func(Hip, uData.Hip);
        func(Tail, uData.Tail);
    }

    public bool Contains(T val)
    {
        return Head.Equals(val) ||
                Shoulder.Equals(val) ||
                Hip.Equals(val) ||
                Tail.Equals(val);
    }

    public List<T> ToList()
    {
        List<T> list = new List<T>();
        list.Add(Head);
        list.Add(Shoulder);
        list.Add(Hip);
        list.Add(Tail);
        return list;
    }

    public LabeledSpineData(
        T head,
        T shoulder,
        T hip,
        T tail
        )
    {
        Head     = head;
        Shoulder = shoulder;
        Hip      = hip;
        Tail     = tail;
    }

    public LabeledSpineData()
    {
        ConstructorInfo[] ctors = typeof(T).GetConstructors();
        ConstructorInfo defaultCtor = null;
        foreach (var ctor in ctors)
        {
            if (ctor.GetParameters().Length == 0)
                defaultCtor = ctor;
        }
        if (defaultCtor != null)
        {
            Head     = (T)defaultCtor.Invoke(new object[] { });
            Shoulder = (T)defaultCtor.Invoke(new object[] { });
            Hip      = (T)defaultCtor.Invoke(new object[] { });
            Tail = (T)defaultCtor.Invoke(new object[] { });
        }
        else
        {
            Head     = default;
            Shoulder = default;
            Hip      = default;
            Tail = default;
        }
    }
}

[Serializable]
public class LabledSkeletonData<T>
{
    [field: SerializeField]
    public LabeledSpineData<T> SpineData { get; private set; }
    [field: SerializeField]
    public LabeledLimbData<T> LimbData { get; private set; }

    // Spine transforms should be at the END of their gameObject, tip of the nose, tip of the tail, etc.
    // Values should be null if 
    public T Head { get => SpineData.Head; }
    public T Shoulder { get => SpineData.Shoulder; }
    public T Hip { get => SpineData.Hip; } 
    public T Tail { get => SpineData.Tail; }

    // Leg transforms should correspond to the transform coming off of the shoulder or the hips excluding offsets
    public T FrontLeftLeg { get => LimbData.FrontLeftLeg ; }
    public T FrontRightLeg { get => LimbData.FrontRightLeg ; }
    public T BackLeftLeg { get => LimbData.BackLeftLeg ; }
    public T BackRightLeg { get => LimbData.BackRightLeg ; }

    public LabledSkeletonData<U> Convert<U>(Func<T, U> converter)
    {
        return new LabledSkeletonData<U>(
            converter(Head         ),
            converter(Shoulder     ),
            converter(Hip          ),
            converter(Tail         ),
            converter(FrontLeftLeg ),
            converter(FrontRightLeg),
            converter(BackLeftLeg  ),
            converter(BackRightLeg )
        );
    }

    public LabledSkeletonData<F> Combine<U,F> (LabledSkeletonData<U> uData, Func<T, U, F> combiner) 
    {
        return new LabledSkeletonData<F>(
            combiner(Head          , uData.Head         ),
            combiner(Shoulder      , uData.Shoulder     ),
            combiner(Hip           , uData.Hip          ),
            combiner(Tail          , uData.Tail         ),
            combiner(FrontLeftLeg  , uData.FrontLeftLeg ),
            combiner(FrontRightLeg , uData.FrontRightLeg),
            combiner(BackLeftLeg   , uData.BackLeftLeg  ),
            combiner(BackRightLeg  , uData.BackRightLeg )
        );
    }

    public void Modify<U>(LabledSkeletonData<U> uData, Action<T,U> func)
    {
        func(Head, uData.Head)                  ;
        func(Shoulder, uData.Shoulder)          ;
        func(Hip, uData.Hip)                    ;
        func(Tail, uData.Tail)                  ;
        func(FrontLeftLeg, uData.FrontLeftLeg)  ;
        func(FrontRightLeg, uData.FrontRightLeg);
        func(BackLeftLeg, uData.BackLeftLeg)    ;
        func(BackRightLeg, uData.BackRightLeg)  ;
    }

    public bool Contains(T val)
    {
        return  Head         .Equals(val) ||
                Shoulder     .Equals(val) ||
                Hip          .Equals(val) ||
                Tail         .Equals(val) ||
                FrontLeftLeg .Equals(val) ||
                FrontRightLeg.Equals(val) ||
                BackLeftLeg  .Equals(val) ||
                BackRightLeg .Equals(val);
    }

    public List<T> ToList()
    {
        List<T> list = new List<T>();
        list.Add(Head);
        list.Add(Shoulder     );
        list.Add(Hip          );
        list.Add(Tail         );
        list.Add(FrontLeftLeg );
        list.Add(FrontRightLeg);
        list.Add(BackLeftLeg  );
        list.Add(BackRightLeg );
        return list;
    }

    public LabledSkeletonData(
        T head,
        T shoulder,
        T hip,
        T tail,
        T frontLeftLeg,
        T frontRightLeg,
        T backLeftLeg,
        T backRightLeg
        )
    {
        SpineData = new LabeledSpineData<T>(head, shoulder, hip, tail);
        LimbData = new LabeledLimbData<T>(frontLeftLeg, frontRightLeg, backLeftLeg, backRightLeg);
    }
}

public class SkeletonTransforms : LabledSkeletonData<Transform>
{
    // Ordered from head to tail, don't include null values
    [field: SerializeField]
    public SkeletonTransforms(Transform head, Transform shoulder, Transform hip, Transform tail, Transform frontLeftLeg, Transform frontRightLeg, Transform backLeftLeg, Transform backRightLeg) : base(head, shoulder, hip, tail, frontLeftLeg, frontRightLeg, backLeftLeg, backRightLeg) {}
}


[Serializable]
public class SkeletonPathTunables
{
    [field: SerializeField]
    public float MinTurningRad { get; private set; }
    [field: SerializeField]
    public float Speed { get; private set; }

    public SkeletonPathTunables(float minTurningRad, float speed)
    {
        MinTurningRad = minTurningRad;
        Speed = speed;
    }
}

public class SkeletonPathData
{
    public float SkeletonDuration { get; private set; }
    public float DelayedPathLenght { get; private set; }

    public SkeletonPathData( SkeletonPathTunables tunables, SkeletonLayoutData layoutData)
    {
        SkeletonDuration = layoutData.SkeletonLenght / tunables.Speed + .25f;
        DelayedPathLenght = SkeletonDuration * tunables.Speed;
    }
}

public class SkeletonLayoutData
{
    public LimbData[] LimbEnds { get; private set; }
    public SpinePointData[] SpinePoints { get; private set; }
    public float SkeletonLenght { get; private set; }

    public SkeletonLayoutData(LimbData[] limbEnds, SpinePointData[] spinePoints, float skeletonLength)
    {
        LimbEnds = limbEnds;
        SpinePoints = spinePoints;
        SkeletonLenght = skeletonLength;
    }
}