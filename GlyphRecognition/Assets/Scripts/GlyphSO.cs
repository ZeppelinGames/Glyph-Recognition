using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

[Serializable]
[CreateAssetMenu(fileName = "New Glyph", menuName = "Create Glyph")]
public class GlyphSO : ScriptableObject
{
    [Range(0,200)] [Tooltip("Added match percent and key point percent")]
    public float minMatchPercent = 120;
    [SerializeField] public Vector2[] points = new Vector2[0];
}

[CustomEditor(typeof(GlyphSO))]
public class GlyphSOEditor : Editor
{
    private GlyphSO glyphSO;

    private void OnEnable()
    {
        glyphSO = (GlyphSO)target;
    }

    public override void OnInspectorGUI()
    {
        GUILayout.ExpandHeight(true);
        GUILayout.Height(1000);
        EditorGUILayout.Space(Screen.width, true);
        EditorGUILayout.EditorToolbar();

        SerializedObject so = new SerializedObject(target);
        SerializedProperty minMatchProperty = so.FindProperty("minMatchPercent");
        SerializedProperty pointsProperty = so.FindProperty("points");
        
        EditorGUI.PropertyField(new Rect(10, 10, Screen.width-20, 20), minMatchProperty);
        EditorGUI.PropertyField(new Rect(10, 50, Screen.width-20, 100), pointsProperty, true);

        float height = EditorGUI.GetPropertyHeight(pointsProperty, true) + 50;
        bool normaliseBTN = GUI.Button(new Rect(0, height, Screen.width, 50), "Normalise Points");

        if (normaliseBTN) { NormalisePoints(); }

        float size = Screen.height - (height + 500) - 50;
        //EditorGUI.DrawRect(new Rect(0, height + 50, size, size), Color.white);

        //Should really use drawTexture and generate texture here
        int texSize = Mathf.RoundToInt(Screen.width / 2);
        Texture2D tex = new Texture2D(texSize,texSize);

        Vector2 pointOffset = new Vector2(tex.width/4, tex.height/4);
        if (glyphSO.points.Length > 1)
        {
            for (int n = 0; n < glyphSO.points.Length - 1; n++)
            {
                Vector2 gP1 = glyphSO.points[n] * (tex.width/2) + pointOffset;
                Vector2 gP2 = glyphSO.points[n + 1] * (tex.height/2) + pointOffset;

                DrawLine(tex, gP1, gP2, Color.red, 5);
            }
        }
        tex.Apply();
        EditorGUI.DrawPreviewTexture(new Rect((Screen.width/2) - (tex.width/2), height + 50, tex.width, tex.height), tex);

        so.ApplyModifiedProperties();
    }

    void DrawLine(Texture2D tex, Vector2 p1, Vector2 p2, Color col, int lineWidth = 1)
    {
        bool steep = Mathf.Abs(p2.y - p1.y) > Mathf.Abs(p2.x - p1.x);

        if (steep)
        {
            p1 = new Vector2(p1.y, p1.x);
            p2 = new Vector2(p2.y, p2.x);
        }

        if (p1.x > p2.x)
        {
            Vector2 tempP1 = p1;
            p1 = p2;
            p2 = tempP1;
        }

        float dx = p2.x - p1.x;
        float dy = p2.y - p1.y;
        float gradient = dy / dx;

        if (dx == 0)
        {
            gradient = 1;
        }

        float xEnd = Mathf.Round(p1.x);
        float yEnd = p1.y + gradient * (xEnd - p1.x);
        float xGap = (p1.x + 0.5f) - Mathf.Floor(p1.x + 0.5f);

        float xpxl1 = xEnd;
        float ypxl1 = Mathf.Floor(yEnd);

        if (steep)
        {
            PlotPixelOnTexture(tex, new Vector2(ypxl1, xpxl1), col);
            PlotPixelOnTexture(tex, new Vector2(ypxl1 + 1, xpxl1), col);
        }
        else
        {
            PlotPixelOnTexture(tex, new Vector2(xpxl1, ypxl1), col);
            PlotPixelOnTexture(tex, new Vector2(xpxl1, ypxl1 + 1), col);
        }

        float yInter = yEnd + gradient;

        xEnd = Mathf.Round(p2.x);
        yEnd = p2.y + gradient * (xEnd - p2.x);
        xGap = (p2.x + 0.5f) - Mathf.Floor(p2.x + 0.5f);

        float xpxl2 = xEnd;
        float ypxl2 = Mathf.Floor(yEnd);

        if (steep)
        {
            PlotPixelOnTexture(tex, new Vector2(ypxl2, xpxl2), col);
            PlotPixelOnTexture(tex, new Vector2(ypxl2 + 1, xpxl2), col);
        }
        else
        {
            PlotPixelOnTexture(tex, new Vector2(xpxl2, ypxl2), col);
            PlotPixelOnTexture(tex, new Vector2(xpxl2, ypxl2 + 1), col);
        }

        if (steep)
        {
            for (int x = (int)xpxl1; x < xpxl2; x++)
            {
                PlotPixelOnTexture(tex, new Vector2(Mathf.Round(yInter), x), col);
                for (int n = -Mathf.FloorToInt(lineWidth / 2); n < Mathf.Ceil(lineWidth / 2); n++)
                {
                    PlotPixelOnTexture(tex, new Vector2(Mathf.Round(yInter) + n, x), col);
                }
                yInter = yInter + gradient;
            }
        }
        else
        {
            for (int x = (int)xpxl1; x < xpxl2; x++)
            {
                PlotPixelOnTexture(tex, new Vector2(x, Mathf.Round(yInter)), col);
                for (int n = -Mathf.FloorToInt(lineWidth / 2); n < Mathf.Ceil(lineWidth / 2); n++)
                {
                    PlotPixelOnTexture(tex, new Vector2(x, Mathf.Round(yInter) + 1), col);
                }

                yInter = yInter + gradient;
            }
        }
    }

    void PlotPixelOnTexture(Texture2D tex, Vector2 pos, Color col)
    {
        if (pos.x > 0 && pos.x < tex.width)
        {
            if (pos.y > 0 && pos.y < tex.height)
            {
                tex.SetPixel((int)pos.x, (int)pos.y, col);
            }
        }
    }

    void NormalisePoints()
    {
        Vector2 minPoint = glyphSO.points[0];
        Vector2 maxPoint = glyphSO.points[0];

        foreach (Vector2 p in glyphSO.points)
        {
            if (p.x < minPoint.x) { minPoint.x = p.x; }
            if (p.y < minPoint.y) { minPoint.y = p.y; }
            if (p.x > maxPoint.x) { maxPoint.x = p.x; }
            if (p.y > maxPoint.y) { maxPoint.y = p.y; }
        }

        for (int n = 0; n < glyphSO.points.Length; n++)
        {
            Vector2 newPoint = glyphSO.points[n];
            newPoint.x = 1/(maxPoint.x - minPoint.x) * (newPoint.x - minPoint.x);
            newPoint.y = 1/(maxPoint.y - minPoint.y) * (newPoint.y - minPoint.y);

            glyphSO.points[n] = newPoint;
        }
    }
}