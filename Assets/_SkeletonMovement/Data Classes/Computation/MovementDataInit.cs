using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class MovementDataInit : IAssemblyStage, IBehaviourDataProvider
{
    public SkeletonLayoutData LayoutData { get; private set; }

    private LimbTunables limbTunables;

    public MovementDataInit SetTunables(LimbTunables limbTunables)
    {
        this.limbTunables = limbTunables;
        return this;
    }

    public IEnumerator Execute(GameObject skeleton, IAssemblyStage previous)
    {
        //--- Create base data ---//
        IikTransformProvider chainStarts = previous as IikTransformProvider;
        Assert.IsNotNull(chainStarts, "Assembly pipline is set up incorrectly: " + previous.GetType() + " comes before " + GetType() + " but does not implement " + typeof(IikTransformProvider).ToString());
        IikTargetProvider chainTargets = previous as IikTargetProvider;
        Assert.IsNotNull(chainTargets, "Assembly pipline is set up incorrectly: " + previous.GetType() + " comes before " + GetType() + " but does not implement " + typeof(IikTargetProvider).ToString());

        Transform root = skeleton.transform;
        while (root.parent != null)
            root = root.parent;
        SkeletonBehaviour skeletonBehaviour = root.GetComponent<SkeletonBehaviour>();
        Assert.IsNotNull(skeletonBehaviour, "Assembly pipline is set up incorrectly: Skeleton has no skeleton behavior component attached.");
        
        float speed = skeletonBehaviour.Speed;
        SetTunables(skeletonBehaviour.LimbTunables);
        LabeledLimbData<Transform> limbStarts = chainStarts.Transforms.LimbData;
        LabeledLimbData<Transform> limbTargets = chainTargets.Targets.LimbData;
        LabeledSpineData<Transform> spineTransforms = chainStarts.Transforms.SpineData;
        LabeledSpineData<Transform> spineTargets = chainTargets.Targets.SpineData;

        //--- Create computers ---//
        LabeledSpineData<SpineLenght> spineLengths = new LabeledSpineData<SpineLenght>();
        SpinePointPositions spinePositions = new SpinePointPositions();
        SpineHeight spineHeight = new SpineHeight();

        LimbOffset limbOffset = new LimbOffset();
        LabeledLimbData<LimbLength> limbLengths = new LabeledLimbData<LimbLength>();
        LabeledLimbData<StrideLenght> strideLenghts = new LabeledLimbData<StrideLenght>();
        LabeledLimbData<StepHeight> stepHeights = new LabeledLimbData<StepHeight>();

        //--- Set relationships between computers ---//
        spineLengths.Head.To = spineTransforms.Shoulder;
        spineLengths.Head.From = spineTransforms.Head;
        spineLengths.Shoulder.To = spineTransforms.Shoulder;
        spineLengths.Shoulder.From = spineTransforms.Hip;
        spineLengths.Hip.To = spineTransforms.Shoulder;
        spineLengths.Hip.From = spineTransforms.Hip;
        spineLengths.Tail.To = spineTransforms.Tail;
        spineLengths.Tail.From = spineTransforms.Hip;
        spinePositions.SpineLengths = spineLengths;
        spinePositions.LimbLenghts = limbLengths;
        spineHeight.SpinePositions = spinePositions;
        limbOffset.AllLimbLenghts = limbLengths;
        limbLengths.Modify(limbStarts, (LimbLength ll, Transform start) => ll.LimbStart = start);
        stepHeights.Modify(limbLengths, (StepHeight sh, LimbLength ll) => sh.LimbLength = ll);
        stepHeights.Modify(strideLenghts, (StepHeight sh, StrideLenght sl) => sh.LimbLength = sl);

        // ok to compute spineHeight before full assignment
        LabeledSpineData<ValComputer<float>> computedSpineHeights = spineHeight;
        LabeledLimbData<ValComputer<float>> limbOriginHeight = new LabeledLimbData<ValComputer<float>>(computedSpineHeights.Shoulder,computedSpineHeights.Shoulder, computedSpineHeights.Hip, computedSpineHeights.Hip);
        strideLenghts.Modify(limbOriginHeight, (StrideLenght sl, ValComputer<float> sh) => sl.SpinePointHeight = sh);
        strideLenghts.Modify(limbLengths, (StrideLenght sl, LimbLength ll) => sl.LimbLength = ll);

        //--- Perform simple conversions ---//
        LabeledSpineData<float> spineDelays = ((LabeledSpineData<Vector2>)spinePositions).Convert((Vector2 pos) => pos.x / speed);
        LabeledLimbData<float> limbDelays = new LabeledLimbData<float>(spineDelays.Shoulder, spineDelays.Shoulder, spineDelays.Hip, spineDelays.Hip);
        LabeledLimbData<float> stepTimeInfrontOfSpine = strideLenghts.Convert((StrideLenght length) => length / 2 / speed);


        //--- Assign values to data containers ---//

        //Limbs//

        LabeledLimbData<OpenLimbTransforms> openLimbTransforms = new LabeledLimbData<OpenLimbTransforms>();
        openLimbTransforms.Modify(limbStarts, (OpenLimbTransforms olt, Transform t) => olt.LimbStart = t);
        openLimbTransforms.Modify(limbTargets, (OpenLimbTransforms olt, Transform t) => olt.Target = t);

        LabeledLimbData<OpenLimbMeasurements> openLimbMeasurements = new LabeledLimbData<OpenLimbMeasurements>();
        openLimbMeasurements.Modify(stepHeights, (OpenLimbMeasurements olm, StepHeight sh) => olm.StepHeight = sh);
        openLimbMeasurements.Modify(limbLengths, (OpenLimbMeasurements olm, LimbLength ll) => olm.TotalLength = ll);
        openLimbMeasurements.Modify(limbOffset, (OpenLimbMeasurements olm, LimbOffset lo) => olm.OffsetFromSpine = lo);
        openLimbMeasurements.Modify(strideLenghts, (OpenLimbMeasurements olm, StrideLenght sl) => olm.StrideLength = sl);

        LabeledLimbData<OpenLimbTracingData> openLimbTracingData = new LabeledLimbData<OpenLimbTracingData>();
        openLimbTracingData.Modify(limbDelays, (OpenLimbTracingData oltd, float delay) => oltd.Delay = delay);
        openLimbTracingData.Modify(limbTunables, (OpenLimbTracingData oltd, LimbTunables tunables) => oltd.Tunables = tunables);
        openLimbTracingData.Modify(stepTimeInfrontOfSpine, (OpenLimbTracingData oltd, float time) => oltd.StepTimeInfrontOfSpinePoint = time);

        LabeledLimbData<OpenLimbIdentityData> openLimbIds = new LabeledLimbData<OpenLimbIdentityData>(
            new OpenLimbIdentityData( true, true ),
            new OpenLimbIdentityData( true, false ),
            new OpenLimbIdentityData( false, true ),
            new OpenLimbIdentityData( false, false)
        );

        LabeledLimbData<OpenLimbData> labeledOpenLimbData = new LabeledLimbData<OpenLimbData>(new OpenLimbData(),new OpenLimbData(),new OpenLimbData(),new OpenLimbData());
        labeledOpenLimbData.Modify(openLimbIds, (OpenLimbData old, OpenLimbIdentityData olid) => old.IdentityData= olid);
        labeledOpenLimbData.Modify(openLimbMeasurements, (OpenLimbData old, OpenLimbMeasurements olm) => old.Measurements = olm);
        labeledOpenLimbData.Modify(openLimbTransforms, (OpenLimbData old, OpenLimbTransforms olt) => old.Transforms = olt);
        labeledOpenLimbData.Modify(limbTunables, (OpenLimbData old, LimbTunables tunables) => old.Tunables = tunables);
        labeledOpenLimbData.Modify(openLimbTracingData, (OpenLimbData old, OpenLimbTracingData oltd) => old.TracingData = oltd);

        //Spine//
        LabeledSpineData<OpenSpineIdentityData> spineIdData = new LabeledSpineData<OpenSpineIdentityData>(
            new OpenSpineIdentityData(),
            new OpenSpineIdentityData(true,false),
            new OpenSpineIdentityData(false,true),
            new OpenSpineIdentityData()
            );


        LabeledSpineData<OpenSpinePointData> openSpineData = new LabeledSpineData<OpenSpinePointData>(new OpenSpinePointData(), new OpenSpinePointData(), new OpenSpinePointData(), new OpenSpinePointData());
        openSpineData.Modify(spineTargets, (OpenSpinePointData ospd, Transform target) => ospd.Target = target);
        openSpineData.Modify(spineDelays, (OpenSpinePointData ospd, float delay) => ospd.Delay = delay);
        openSpineData.Modify(spineIdData, (OpenSpinePointData ospd, OpenSpineIdentityData sid) => ospd.Identity = sid);
        openSpineData.Modify(spineHeight, (OpenSpinePointData ospd, ValComputer<float> height) => ospd.BaseHeight = height);

        // Create final data containers
        LabeledLimbData<LimbData> labeledLimbData = labeledOpenLimbData.Convert((OpenLimbData openData) => new LimbData().Init(openData));
        LabeledSpineData<SpinePointData> labeledSpineData = openSpineData.Convert((OpenSpinePointData openData) => new SpinePointData(openData));
        
        LimbData[] limbEnds = labeledLimbData.ToList().ToArray();
        SpinePointData[] spinePoints = labeledSpineData.ToList().ToArray();
        float totalLength = 0;
        foreach (var length in spineLengths.ToList())
            totalLength += length;
        LayoutData = new SkeletonLayoutData(limbEnds, spinePoints, totalLength);

        yield break;
    }

    public void EditorInit(LimbData[] limbEnds , SkeletonTransforms transforms, SkeletonTransforms targets, SkeletonPathTunables tunables)
    {
        Transform[] orderedTransforms = new Transform[4];
        orderedTransforms[0] = targets.Head;
        orderedTransforms[1] = targets.Shoulder;
        orderedTransforms[2] = targets.Hip;
        orderedTransforms[3] = targets.Tail;

        float[] distances = new float[4];
        distances[0] = 0;
        distances[1] = (orderedTransforms[0].transform.position - orderedTransforms[1].transform.position).magnitude;
        distances[2] = (orderedTransforms[1].transform.position - orderedTransforms[2].transform.position).magnitude;
        distances[3] = (orderedTransforms[2].transform.position - orderedTransforms[3].transform.position).magnitude;
        float totalDistance = distances[1] + distances[2] + distances[3];

        Transform GetMatchingTransform(SkeletonTransforms findFrom, LimbIdentityData identityData)
        {
            if(identityData.IsFront)
            {
                if (identityData.IsRight)
                {
                    return findFrom.FrontRightLeg;
                }
                return findFrom.FrontLeftLeg;
            }
            else
            {

                if (identityData.IsRight)
                {
                    return findFrom.BackRightLeg;
                }
                return findFrom.BackLeftLeg;
            }
        }

        // Create data
        SpinePointData[] spinePoints = new SpinePointData[orderedTransforms.Length];
        float cumulativeDelay = 0;
        for (int i = 0; i < spinePoints.Length; i++)
        {
            // simple delay calculation
            cumulativeDelay += distances[i] / tunables.Speed;

            SpineIdentityData idData = new SpineIdentityData(i == 1, i == 2);

            spinePoints[i] = new SpinePointData(
                orderedTransforms[i],
                cumulativeDelay,
                //temp
                .7f * .2f,
                idData
            );
        }

        // set limb delays to match their corresponding spine point (shoulders for front legs, hips for back legs)
        SkeletonBehaviour behaviour = GameObject.FindObjectOfType<SkeletonBehaviour>();
        LimbTunables limbTunables = null;
        if (behaviour != null)
            limbTunables = behaviour.LimbTunables;
        foreach (LimbData limbEnd in limbEnds)
        {
            Transform start = GetMatchingTransform(transforms, limbEnd.IdentityData);
            Transform target = GetMatchingTransform(targets, limbEnd.IdentityData);
            LimbTransforms limbGo = new LimbTransforms(start, target);

            if (limbEnd.IdentityData.IsFront)
            {
                limbEnd.EditorInit(limbGo, limbTunables, new LimbTracingData(spinePoints[1].Delay, limbTunables, .1f/tunables.Speed));
            }
            else
            {
                limbEnd.EditorInit(limbGo, limbTunables, new LimbTracingData(spinePoints[2].Delay, limbTunables, .1f/tunables.Speed));
            }
        }

        LayoutData = new SkeletonLayoutData(limbEnds, spinePoints, totalDistance);
    }
}

