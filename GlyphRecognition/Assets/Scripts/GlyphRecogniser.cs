using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZeppelinGames
{
    public class GlyphRecogniser
    {
        public static GlyphReturnData MatchGlyph(Vector2[] points, GlyphSO glyph)
        {
            Vector2[] nPoints = normalisePoints(points);
            Vector2[] nGlyphPoints = normalisePoints(glyph.points);

            int pointsInside = 0;
            foreach (Vector2 nP in nPoints)
            {
                bool pointIn = false;
                for (int n = 0; n < nGlyphPoints.Length - 1; n++)
                {
                    if (!pointIn)
                    {
                        if (pointNearLine(nP, nGlyphPoints[n], nGlyphPoints[n + 1], 0.1f))
                        {
                            pointIn = true;
                            pointsInside++;
                        }
                    }
                }
            }

            float pointsInLine = (float)(nPoints.Length - nGlyphPoints.Length) / (float)nGlyphPoints.Length;
            int pointsFound = 0;
            int accPoints = 0;
            for (int n = 0; n < nGlyphPoints.Length - 1; n++)
            {
                Vector2[] cPs = subDivLine(nGlyphPoints[n], nGlyphPoints[n + 1], Mathf.FloorToInt(pointsInLine));
                accPoints += cPs.Length;
                foreach (Vector2 cP in cPs)
                {
                    bool hasPoint = false;
                    foreach (Vector2 p in nPoints)
                    {
                        if (!hasPoint)
                        {
                            if (pointInCircle(p, cP, 0.02f))
                            {
                                hasPoint = true;
                                pointsFound++;
                            }
                        }
                    }
                }
            }

            float inPer = ((float)pointsInside / (float)nPoints.Length) * 100;
            float keyPer = ((float)pointsFound / (float)accPoints) * 100;

            GlyphReturnData glyphReturnData = new GlyphReturnData(glyph.name, inPer, keyPer, accPoints);
            return glyphReturnData;
        }

        public static bool isGlyph(Vector2[] points, GlyphSO glyph)
        {
            Vector2[] nPoints = normalisePoints(points);
            Vector2[] nGlyphPoints = normalisePoints(glyph.points);

            if (nPoints.Length >= nGlyphPoints.Length)
            {
                int pointsInside = 0;
                foreach (Vector2 nP in nPoints)
                {
                    bool pointIn = false;
                    for (int n = 0; n < nGlyphPoints.Length - 1; n++)
                    {
                        if (!pointIn)
                        {
                            if (pointNearLine(nP, nGlyphPoints[n], nGlyphPoints[n + 1], 0.1f))
                            {
                                pointIn = true;
                                pointsInside++;
                            }
                        }
                    }
                }

                float pointsInLine = (float)(nPoints.Length - nGlyphPoints.Length) / (float)nGlyphPoints.Length;
                int pointsFound = 0;
                int accPoints = 0;
                for (int n = 0; n < nGlyphPoints.Length - 1; n++)
                {
                    Vector2[] cPs = subDivLine(nGlyphPoints[n], nGlyphPoints[n + 1], Mathf.FloorToInt(pointsInLine));
                    accPoints += cPs.Length;
                    foreach (Vector2 cP in cPs)
                    {
                        bool hasPoint = false;
                        foreach (Vector2 p in nPoints)
                        {
                            if (!hasPoint)
                            {
                                if (pointInCircle(p, cP, 0.02f))
                                {
                                    hasPoint = true;
                                    pointsFound++;
                                }
                            }
                        }
                    }
                }

                if (pointsFound < (accPoints / nGlyphPoints.Length) - nGlyphPoints.Length)
                {
                    return false;
                }

                float inPer = (pointsInside / (float)nPoints.Length) * 100;
                if (inPer > 75)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool pointInRect(Vector2 point, Rectangle rect)
        {
            float pointArea = triangleArea(rect.pos + rect.p1, point, rect.pos + rect.p4) +
                triangleArea(rect.pos + rect.p4, point, rect.pos + rect.p3) +
                triangleArea(rect.pos + rect.p3, point, rect.pos + rect.p2) +
                triangleArea(point, rect.pos + rect.p2, rect.pos + rect.p1);
            float rectArea = triangleArea(rect.pos + rect.p1, rect.pos + rect.p2, rect.pos + rect.p3) + triangleArea(rect.pos + rect.p1, rect.pos + rect.p3, rect.pos + rect.p4);

            return pointArea > rectArea ? false : true;
        }

        public static bool pointNearLine(Vector2 point, Vector2 lp1, Vector2 lp2, float lineDist)
        {
            float d = Mathf.Abs(((lp2.x - lp1.x) * (lp1.y - point.y)) - ((lp1.x - point.x) * (lp2.y - lp1.y))) / Mathf.Sqrt(Mathf.Pow(lp2.x - lp1.x, 2) + Mathf.Pow(lp2.y - lp1.y, 2));
            if (d < lineDist)
            {
                return true;
            }

            return false;
        }

        public static Vector2[] subDivLine(Vector2 lp1, Vector2 lp2, int subDivs)
        {
            List<Vector2> subDivPoints = new List<Vector2>();
            float k = Vector2.Distance(lp2, lp1) / subDivs;
            for (int n = 0; n < subDivs; n++)
            {
                float kn = k * n;
                subDivPoints.Add(new Vector2(lp1.x + (kn * (lp2.x - lp1.x)), lp1.y + (kn * (lp2.y - lp1.y))));
            }
            return subDivPoints.ToArray();
        }

        public static bool pointInCircle(Vector2 point, Vector2 circlePos, float circleRadius)
        {
            return Vector2.Distance(point, circlePos) < circleRadius ? true : false;
        }

        public static float triangleArea(Vector2 a, Vector2 b, Vector2 c)
        {
            return Mathf.Abs((b.x * a.y - a.x * b.y) + (c.x * b.y - b.x * c.y) + (a.x * c.y - c.x * a.y)) / 2;
        }

        public static Vector2[] normalisePoints(Vector2[] points)
        {
            Vector2[] normalPoints = new Vector2[points.Length];

            Vector2 minPoint = points[0];
            Vector2 maxPoint = points[0];

            foreach (Vector2 p in points)
            {
                if (p.x < minPoint.x) { minPoint.x = p.x; }
                if (p.y < minPoint.y) { minPoint.y = p.y; }
                if (p.x > maxPoint.x) { maxPoint.x = p.x; }
                if (p.y > maxPoint.y) { maxPoint.y = p.y; }
            }

            for (int n = 0; n < points.Length; n++)
            {
                Vector2 newPoint = points[n];
                newPoint.x = 1 / (maxPoint.x - minPoint.x) * (newPoint.x - minPoint.x);
                newPoint.y = 1 / (maxPoint.y - minPoint.y) * (newPoint.y - minPoint.y);

                normalPoints[n] = newPoint;
            }

            return normalPoints;
        }

        public class Rectangle
        {
            public Vector2 pos;
            public Vector2 p1, p2, p3, p4;

            public Rectangle(Vector2 pos, Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
            {
                this.pos = pos;
                this.p1 = p1;
                this.p2 = p2;
                this.p3 = p3;
                this.p4 = p4;
            }
        }

        public class GlyphReturnData
        {
            /// <summary>
            /// Name of the glyph
            /// </summary>
            public string glyphName;
            /// <summary>
            /// Percentage of points matched with glyph shape
            /// </summary>
            public float matchPercent;
            /// <summary>
            /// Percentage of points that hit key points
            /// </summary>
            public float keyPointsPercent;
            /// <summary>
            /// Amount of key points measured against
            /// </summary>
            public int keyPoints;

            public GlyphReturnData(string glyphName, float matchPercent, float keyPointsPercent, int keyPoints)
            {
                this.glyphName = glyphName;
                this.matchPercent = matchPercent;
                this.keyPointsPercent = keyPointsPercent;
                this.keyPoints = keyPoints;
            }
        }
    }
}