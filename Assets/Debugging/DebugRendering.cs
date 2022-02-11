using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugRendering : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    private GameObject rendererTemplate;

    private Dictionary<DebugModes.DebugPathFlags, LineRenderer> pathRenderers;


    private static DebugRendering instance;

    public static void RenderModeChanged()
    {
        foreach (var key in instance.pathRenderers.Keys)
        {
            instance.pathRenderers[key].enabled = RendererIsVisible(key);
        }
    }

    public static void UpdatePath(DebugModes.DebugPathFlags type, List<Vector3> points)
    {
        instance.UpdatePathInternal(type,points.ToArray());
    }

    public static void UpdatePath(DebugModes.DebugPathFlags type, Vector3[] points)
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

        pathRenderers = new Dictionary<DebugModes.DebugPathFlags, LineRenderer>();
        rendererTemplate.GetComponent<LineRenderer>().enabled = false;
    }

    private void Start()
    {
        AddRenderer(DebugModes.DebugPathFlags.NavMeshPath, Color.red);
        AddRenderer(DebugModes.DebugPathFlags.ModifiedNavMeshPath, Color.green);
        AddRenderer(DebugModes.DebugPathFlags.TruePath, Color.cyan);
    }

    private static bool RendererIsVisible(DebugModes.DebugPathFlags type)
    {
        return (DebugModes.SkeletonPathMode & type) != 0;
    }

    private void AddRenderer(DebugModes.DebugPathFlags mode, Color color)
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

    private void UpdatePathInternal(DebugModes.DebugPathFlags type, Vector3[] points)
    {
        LineRenderer renderer = pathRenderers[type];
        renderer.positionCount = points.Length;
        renderer.SetPositions(points);
    }
#endif
}
