using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRendering : MonoBehaviour
{
    [SerializeField]
    private GameObject rendererTemplate;

    private Dictionary<DebugModes.SkeletonPathFlags, LineRenderer> pathRenderers;


    private static DebugRendering instance;

    public static void RenderModeChanged()
    {
        foreach (var key in instance.pathRenderers.Keys)
        {
            instance.pathRenderers[key].enabled = RendererIsVisible(key);
        }
    }

    public static void UpdatePath(DebugModes.SkeletonPathFlags type, List<Vector3> points)
    {
        instance.UpdatePathInternal(type,points.ToArray());
    }

    public static void UpdatePath(DebugModes.SkeletonPathFlags type, Vector3[] points)
    {
        instance.UpdatePathInternal(type,points);
    }

    // Start is called before the first frame update
    void Awake()
    {
        if (instance)
            Destroy(this);
        else
            instance = this;

        pathRenderers = new Dictionary<DebugModes.SkeletonPathFlags, LineRenderer>();
        rendererTemplate.GetComponent<LineRenderer>().enabled = false;
    }

    private void Start()
    {
        AddRenderer(DebugModes.SkeletonPathFlags.NavMeshPath, Color.red);
        AddRenderer(DebugModes.SkeletonPathFlags.ModifiedNavMeshPath, Color.green);
        AddRenderer(DebugModes.SkeletonPathFlags.TruePath, Color.cyan);
    }

    private static bool RendererIsVisible(DebugModes.SkeletonPathFlags type)
    {
        return (DebugModes.SkeletonPathMode & type) != 0;
    }

    private void AddRenderer(DebugModes.SkeletonPathFlags mode, Color color)
    {
        GameObject newRenderer = Instantiate(rendererTemplate);
        newRenderer.transform.parent = gameObject.transform;
        newRenderer.name = mode.ToString() + "Renderer";

        LineRenderer renderer = newRenderer.GetComponent<LineRenderer>();
        renderer.startColor = color;
        renderer.endColor = color;
        renderer.enabled = RendererIsVisible(mode);
        pathRenderers.Add(mode, renderer);

        UpdatePathInternal(mode,new Vector3[0]);
    }

    private void UpdatePathInternal(DebugModes.SkeletonPathFlags type, Vector3[] points)
    {
        LineRenderer renderer = pathRenderers[type];
        renderer.positionCount = points.Length;
        renderer.SetPositions(points);
    }
}
