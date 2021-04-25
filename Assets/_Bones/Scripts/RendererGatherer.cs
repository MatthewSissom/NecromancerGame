using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RendererGatherer : MonoBehaviour
{
    [SerializeField]
    List<Material> materials;
    List<Renderer> renderers;


    // Start is called before the first frame update
    void Start()
    {
        renderers = new List<Renderer>();
        if (materials == null)
            return;
        void FindRenderers(Transform toCheck)
        {
            Renderer renderer = toCheck.GetComponent<Renderer>();
            if (renderer)
                renderers.Add(renderer);

            int children = toCheck.childCount;
            for (int i = 0; i < children; i++)
            {
                FindRenderers(toCheck.GetChild(i));
            }
        }
        FindRenderers(transform);
    }

    public void ChangeMat(int index)
    {
        if (index < 0 || index >= materials.Count)
            return;
        foreach(var renderer in renderers)
        {
            renderer.material = materials[index];
        }
    }
}
