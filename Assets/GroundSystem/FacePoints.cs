using System;
using System.Collections.Generic;
using UnityEngine;

namespace CoolGroundGenerator
{
    public struct FacePoint {
        public readonly Vector2 p;
        public float groundness;

        public float x { get { return p.x; } }
        public float y { get { return p.y; } }

        public FacePoint(Vector2 p, float groundness = -1.0f) {
            this.p = p;
            this.groundness = groundness;
        }
    }

    public struct ShapePoints {
        public FacePoint[] edge;
        public FacePoint[] interior;
    }

    static public class FacePoints
    {
        // Duplicated from Walker.cs
        static bool normalIsGround(Vector2 n) { return n.y >=  Mathf.Cos(Mathf.PI / 4); }

        static public ShapePoints getPointsForShape(Vector2[] outline)
        {
            const float D = 1.0f;
            const float CUTOFF = D / 2;
            const float SQR_CUTOFF = CUTOFF * CUTOFF;

            var edge = new List<FacePoint>();
            for (int i = 0; i < outline.Length; ++i) {
                var ptPrev = outline[(i+outline.Length-1) % outline.Length];
                var ptCurr = outline[i];
                var ptNext = outline[(i+1) % outline.Length];

                var dNext = ptNext - ptCurr;
                var normNext = dNext.Rotate(-90).normalized;
                var groundNext = normalIsGround(normNext);
                var dPrev = ptCurr - ptPrev;
                var normPrev = dPrev.Rotate(-90).normalized;
                var groundPrev = normalIsGround(normPrev);

                var vexGroundCond = (groundNext && groundPrev)
                    || (groundNext && normPrev.x > 0 || groundPrev && normNext.x < 0);

                edge.Add(new FacePoint(ptCurr, 0f)); //vexGroundCond ? 1f : 0f));

                var segs = (int)(dNext.magnitude / D);
                for (int j = 1; j < segs; ++j) {
                    var newPt = Vector2.Lerp(ptCurr, ptNext, (float)j / segs);
                    newPt += normNext * UnityEngine.Random.value * 0.3f;
                    edge.Add(new FacePoint(newPt, 0f)); // groundNext ? 1f : 0f));
                }
            }

            var bounds = boundsOfPoints(outline);
            var interior = new List<FacePoint>();
            for (float x = bounds.xMin; x <= bounds.xMax; x += D) {
                for (float y = bounds.yMin; y <= bounds.yMax; y += D) {
                    var p = new Vector2(
                        x + UnityEngine.Random.value * 0.8f*D - 0.4f*D,
                        y + UnityEngine.Random.value * 0.8f*D - 0.4f*D
                    );
                    if (p.InsidePolygon(outline)) {
                        var degenerate = false;
                        var ground = 0.0f;

                        foreach (var pt in edge) {
                            var d2 = (pt.p - p).sqrMagnitude;

                            if (d2 < SQR_CUTOFF) {
                                degenerate = true;
                                break;
                            }

                            // TODO check that the closest edge point is within some angle
                            // before assigning groundness to it.  Grass leaks out a bit
                            // on interior corners.
                            if (pt.y > p.y && pt.groundness > 0.99f) {
                                var newGround = 1f / Mathf.Sqrt(d2);
                                if (newGround > 1f) newGround = 1f;
                                if (newGround > ground) {
                                    ground = newGround;
                                }
                            }
                        }

                        if (!degenerate) {
                            interior.Add(new FacePoint(p, ground));
                        }
                    }
                }
            }

            return new ShapePoints {
                edge = edge.ToArray(),
                interior = interior.ToArray()
            };
        }

        static Rect boundsOfPoints(Vector2[] pts)
        {
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);

            foreach (var pt in pts) {
                if (pt.x < min.x) min.x = pt.x;
                else if (pt.x > max.x) max.x = pt.x;
                if (pt.y < min.y) min.y = pt.y;
                else if (pt.y > max.y) max.y = pt.y;
            }

            return new Rect(min.x, min.y, max.x - min.x, max.y - min.y);
        }
    }
}
