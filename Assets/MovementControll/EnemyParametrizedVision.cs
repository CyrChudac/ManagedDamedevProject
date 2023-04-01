using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class EnemyParametrizedVision : EnemyVision
{
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D lightSource;
    /// <summary>
    /// How far does the person see.
    /// </summary>
    [SerializeField] protected float minViewDistance = 1.5f;
	protected override float GetLengthForAngle(float angle) {
        var midCirc = new Vector2(lightSource.transform.position.x, lightSource.transform.position.y);
        var midVis = new Vector2(transform.position.x, transform.position.y);
		var v2 = Vector2Utils.VectorFromAngle(angle);
        var resCount = FindLineCircleIntersections(
            midCirc.x,
            midCirc.y,
            lightSource.pointLightOuterRadius,
            midVis,
            midVis + v2,
            out var i1,
            out var i2);
        Vector2 result;
        if (resCount != 2) {
            if(resCount == 0) {
                return minViewDistance;
            } else {
                result = i1;
            }
        }
        else if(v2.x < 0) {
            result = i1.x < midVis.x ? i1 : i2;
        } else {
            result = i1.x < midVis.x ? i2 : i1;
        }
        var dist = Vector2.Distance(midVis, result);
        //return Mathf.Min(viewDistance, dist);
        return Mathf.Max(dist, minViewDistance);
	}

	protected override float GetMaxLength() {
		return lightSource.pointLightOuterRadius;
	}

	// Find the points of intersection.
	private int FindLineCircleIntersections(
        float cx, float cy, float radius,
        Vector2 point1, Vector2 point2,
        out Vector2 intersection1, out Vector2 intersection2)
    {
        float dx, dy, A, B, C, det, t;

        dx = point2.x - point1.x;
        dy = point2.y - point1.y;

        A = dx * dx + dy * dy;
        B = 2 * (dx * (point1.x - cx) + dy * (point1.y - cy));
        C = (point1.x - cx) * (point1.x - cx) +
            (point1.y - cy) * (point1.y - cy) -
            radius * radius;

        det = B * B - 4 * A * C;
        if ((A <= 0.0000001) || (det < 0))
        {
            // No real solutions.
            intersection1 = new Vector2(float.NaN, float.NaN);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 0;
        }
        else if (det == 0)
        {
            // One solution.
            t = -B / (2 * A);
            intersection1 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            intersection2 = new Vector2(float.NaN, float.NaN);
            return 1;
        }
        else
        {
            // Two solutions.
            t = (float)((-B + Mathf.Sqrt(det)) / (2 * A));
            intersection1 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            t = (float)((-B - Mathf.Sqrt(det)) / (2 * A));
            intersection2 =
                new Vector2(point1.x + t * dx, point1.y + t * dy);
            return 2;
        }
    }
}
