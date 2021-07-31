using System.Collections.Generic;
using UnityEngine;
using static ZeppelinGames.GlyphRecogniser;

public class DrawGlyph : MonoBehaviour
{
    public Camera cam;
    public float maxPointDist = 0.1f;
    public bool debugMode = false;

    private List<Vector2> glyphPoints = new List<Vector2>();
    private List<GlyphSO> glyphs = new List<GlyphSO>();
    private LineRenderer lr;

    private void Start()
    {
        lr = GetComponent<LineRenderer>();

        GlyphSO[] loadedGlyphs = Resources.LoadAll<GlyphSO>("Glyphs/");
        Debug.Log("Loaded " + loadedGlyphs.Length + " glyphs");
        glyphs.AddRange(loadedGlyphs);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (glyphPoints.Count < 1)
            {
                glyphPoints.Add((Vector2)Input.mousePosition);
            }
            else
            {
                if (Vector2.Distance(glyphPoints[glyphPoints.Count - 1], Input.mousePosition) > maxPointDist)
                {
                    glyphPoints.Add((Vector2)Input.mousePosition);
                }
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            List<GlyphReturnData> allMatchData = new List<GlyphReturnData>();
            foreach (GlyphSO matchGlyph in glyphs)
            {
                GlyphReturnData glyphData = MatchGlyph(glyphPoints.ToArray(), matchGlyph);
                Debug.Log(glyphData.glyphName + ": " + glyphData.matchPercent + ", " + glyphData.keyPointsPercent);
                /*   if(glyphData.keyPointsPercent >= matchGlyph.minKeyPointMatchPercentage && glyphData.matchPercent >= matchGlyph.minMatchPercentage)
                   {
                       allMatchData.Add(glyphData);
                   }*/
                if (glyphData.keyPointsPercent + glyphData.matchPercent > matchGlyph.minMatchPercent)
                {
                    allMatchData.Add(glyphData);
                }
            }

            GlyphReturnData bestMatch = null;
            if (allMatchData.Count > 0)
            {
                float highestPercent = 0;
                foreach (GlyphReturnData data in allMatchData)
                {
                    float avgPer = (data.keyPointsPercent + data.matchPercent) / 2;
                    if (avgPer > highestPercent)
                    {
                        highestPercent = avgPer;
                        bestMatch = data;
                    }
                }
            }

            if(bestMatch != null)
            {
                //Run commmands
                Debug.Log(bestMatch.glyphName);
            }

            glyphPoints.Clear();
        }

        lr.positionCount = glyphPoints.Count;
        for (int n = 0; n < glyphPoints.Count; n++)
        {
            Vector3 worldP = cam.ScreenToWorldPoint(glyphPoints[n]);
            worldP.z = 0;
            lr.SetPosition(n, worldP);
        }
    }
}
