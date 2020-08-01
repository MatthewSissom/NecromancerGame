using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class rayPixelGrid : MonoBehaviour
{
    private List<List<rayPixel>> pixelGrid;
    private SpriteRenderer refrence;
    public int insideCount { get; private set; }
    public int outsideCount { get; private set; }

    [Header("Prefabs")]
    public GameObject exteriorPixel;
    public GameObject interiorPixel;

    [Header("Dimensions")]
    public int padding;
    private float distance;

    [Header("Refresh")]
    public float RefreshRate;
    public int groups;
    private float timer;
    private int cycle;

    [Header("UI")]
    private List<int> counts;
    public float UIRefreshRate;
    public TextMeshProUGUI inside;
    private string insideInital;
    public TextMeshProUGUI outside;
    private string outsideInital;
    public TextMeshProUGUI inital;
    public TextMeshProUGUI inital2;
    private string initalInital;
    public boneManager bManager;
    private float uiTimer = 0;
    private float uiCycles = 0;


    // Start is called before the first frame update
    void Start()
    {
        counts = new List<int>();
        counts.Add(0);
        counts.Add(0);

        outsideInital = outside.text;
        insideInital = inside.text;
        initalInital = inital.text;

        refrence = gameObject.GetComponent<SpriteRenderer>();
        Sprite image = refrence.sprite;
        Texture2D raw = image.texture;
        pixelGrid = new List<List<rayPixel>>();
        distance = transform.localScale.x;

        float width = raw.width;
        float height = raw.height;
        for (int x = 0; x < width; x++)
        {
            pixelGrid.Add(new List<rayPixel>());
            for(int y = 0; y < height; y++)
            {
                if(raw.GetPixel(x,y) != Color.white)
                {
                    pixelGrid[x].Add(
                        Instantiate(interiorPixel, transform.position + new Vector3(distance * (x - width/2), 0, distance * (y - height/2)), transform.rotation, transform)
                        .GetComponent<rayPixel>());
                }
                else
                {
                    pixelGrid[x].Add(
                        Instantiate(exteriorPixel, transform.position + new Vector3(distance * (x - width/2), 0 , distance * (y - height/2)), transform.rotation, transform)
                        .GetComponent<rayPixel>());
                }
            }
        }
        refrence.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (timer > RefreshRate && uiTimer > UIRefreshRate)
        {
            int activeCode = 0;
            foreach (var l in pixelGrid)
            {
                for(int i = cycle; i < l.Count; i += groups )
                {
                    activeCode = l[i].Refresh();
                    if(activeCode != -1)
                    {
                        counts[activeCode]++;
                    }
                }
            }
            timer = 0;
            cycle = (cycle + 1) % groups;
            uiCycles++;
            if(uiCycles == groups)
            {
                bManager.updateUI(counts[1] - counts[0]);
                inital.text = initalInital + (counts[1] - counts[0]).ToString();
                inital2.text = initalInital + (counts[1] - counts[0]).ToString();
                inside.text = insideInital + counts[1].ToString();
                outside.text = outsideInital + counts[0].ToString();
                uiTimer = UIRefreshRate;
                uiCycles = 0;
                counts[0] = 0;
                counts[1] = 0;
            }
        }
        else if(timer > RefreshRate)
        {
            foreach (var l in pixelGrid)
            {
                for (int i = cycle; i < l.Count; i += groups)
                {
                    l[i].Refresh();
                }
            }
            timer = 0;
            cycle = (cycle + 1) % groups;
        }
        timer += Time.deltaTime;
        uiTimer += Time.deltaTime;
    }
}
