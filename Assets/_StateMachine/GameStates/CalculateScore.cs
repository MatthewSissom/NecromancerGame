using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateScore : State
{

    [Header("Score Mods")]
    public int maxOutsideOutlinePenalty;

    [Header("Dimensions")]
    private float scale;
    private Vector2 imageDimensions;

    [Header("Fidelity")]
    public int step;

    [Header("Special Locations")]
    public List<Vector3> locationAndRad;
    public List<string> names;
    public List<int> value;
    private Dictionary<string,List<Vector2>> specialCircles;

    [Header("Refrences")]
    public SpriteRenderer scoreAreaRenderer;
    public GameObject small;
    private Texture2D scoreAreaImage;

    bool RayAtPixelLocation(Vector2 location)
    {
        return Physics.Raycast(
            new Ray(
                scoreAreaRenderer.transform.position + new Vector3(scale * (location.x - imageDimensions.x / 2), 0, scale * (imageDimensions.y / 2 - location.y)),
                Vector3.up
                )
            );
    }

    public override IEnumerator Routine()
    {
        Begin();

        //create circles of pixels for each location
        for (int i = 0; i < names.Count; i++)
        {
            foreach (Vector2 location in specialCircles[names[i]])
            {
                if (RayAtPixelLocation(location))
                {
                    ScoreManager.Instance.Add(new PartialScore(names[i], value[i]));
                    break;
                }
            }
        }

        yield return new WaitForEndOfFrame();
        
        //check for bones outside the outline
        int outCount = 0;
        for (int x = 0; x < imageDimensions.x; x += step)
        {
            for(int y = 0; y < imageDimensions.y; y += step)
            {
                if (scoreAreaImage.GetPixel(x, y) == Color.white && RayAtPixelLocation(new Vector2(x, y)))
                    outCount++;
            }
        }

        //normalize the raw count by accounting for step size
        outCount = (int)(outCount * step * step / (imageDimensions.x * imageDimensions.y) * maxOutsideOutlinePenalty);

        if (outCount > 0)
            ScoreManager.Instance.Add(new PartialScore("outPenalty", -outCount, "Bones outside the line " + (-outCount).ToString() + "\n"));

        ScoreManager.Instance.Add(BoneManager.Instance.ConnectionScore());
        ScoreManager.Instance.Add(BoneManager.Instance.LostBones());

        End();
        yield break;
    }

    // Start is called before the first frame update
    void Start()
    {
        Sprite image = scoreAreaRenderer.sprite;
        scoreAreaImage = image.texture;
        imageDimensions = new Vector2(scoreAreaImage.width, scoreAreaImage.height);
        scale = scoreAreaRenderer.transform.localScale.x / image.pixelsPerUnit;

        if (names.Count != value.Count || locationAndRad.Count != names.Count)
        {
            Debug.LogError("Size mismatch between names, values or circles in CalculateScore!");
            return;
        }

        specialCircles = new Dictionary<string, List<Vector2>>();
        for (int i = 0; i < value.Count; i++)
        {
            List<Vector2> circlePoints = new List<Vector2>();

            int rad = (int)(locationAndRad[i].z);
            Vector2 location = new Vector2(locationAndRad[i].x, locationAndRad[i].y);

            //Debug orbs
            //Instantiate(small, scoreAreaRenderer.transform.position + new Vector3(scale * (location.x - imageDimensions.x / 2), .1f, scale * (imageDimensions.y / 2 - location.y)), Quaternion.identity);

            for (int x = -rad; x <= rad; x++)
            {
                for (int y = -rad; y <= rad; y++)
                {
                    if (x * x + y * y <= rad * rad)
                    {
                        circlePoints.Add(location + new Vector2(x, y));
                    }
                }
            }

            specialCircles.Add(names[i], circlePoints);
        }

        scoreAreaRenderer.enabled = false;
    }
}
