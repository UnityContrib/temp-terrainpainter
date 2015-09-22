using UnityEngine;
using System.Linq;

/// <summary>
/// Defines a polygon with a color.
/// </summary>
public class AreaPolygon : MonoBehaviour
{
    /// <summary>
    /// The points defining the shape of the polygon.
    /// </summary>
    public Vector3[] points = new Vector3[]
    {
        (Vector3.right + Vector3.back) * 10.0f,
        (Vector3.left + Vector3.back) * 10.0f,
        Vector3.forward * 10.0f,
        (Vector3.right + Vector3.back) * 10.0f,
    };

    /// <summary>
    /// The minimum and maximum X position.
    /// </summary>
    public RangeF xBoundaries;

    /// <summary>
    /// The minimum and maximum Z position.
    /// </summary>
    public RangeF zBoundaries;

    /// <summary>
    /// The color of the polygon.
    /// </summary>
    public Color color = Color.blue;

    private void Update()
    {
        //var hit = RaycastFromMouse.Hits.FirstOrDefault();
        //var contained = this.Contains(hit.point);
        //Debug.Log(contained);
    }

    /// <summary>
    /// Returns a value indicating if the specified <paramref name="test"/> point is within the convex or concave polygon.
    /// </summary>
    /// <param name="areaPolygon">
    /// The polygon to test against.
    /// </param>
    /// <param name="test">
    /// The point to test is within the polygon.
    /// </param>
    /// <returns>
    /// true if the polygon is inside the polygon; otherwise false.
    /// </returns>
    public bool Contains(Vector3 test)
    {
        var points = this.points;

        if (test.x < this.xBoundaries.minimum)
        {
            return false;
        }
        if (test.z < this.zBoundaries.minimum)
        {
            return false;
        }
        if (test.x > this.xBoundaries.maximum)
        {
            return false;
        }
        if (test.z > this.zBoundaries.maximum)
        {
            return false;
        }

        int i, j;
        var result = false;

        for (i = 0, j = points.Length - 1; i < points.Length; j = i++)
        {
            if (((points[i].z > test.z) != (points[j].z > test.z)) && (test.x < (points[j].x - points[i].x) * (test.z - points[i].z) / (points[j].z - points[i].z) + points[i].x))
            {
                result = !result;
            }
        }

        return result;
    }

    /// <summary>
    /// Calculates the rectangle boundaries of the polygon and stores the values in the polygon.
    /// </summary>
    public void RecalculateBoundaries()
    {
        var xBoundaries = new RangeF(float.MaxValue, float.MinValue);
        var zBoundaries = new RangeF(float.MaxValue, float.MinValue);

        foreach (var point in this.points)
        {
            if (point.x < xBoundaries.minimum)
            {
                xBoundaries.minimum = point.x;
            }
            if (point.x > xBoundaries.maximum)
            {
                xBoundaries.maximum = point.x;
            }
            if (point.z < zBoundaries.minimum)
            {
                zBoundaries.minimum = point.z;
            }
            if (point.z > zBoundaries.maximum)
            {
                zBoundaries.maximum = point.z;
            }
        }

        this.transform.position = new Vector3(
            (xBoundaries.minimum + xBoundaries.maximum) * 0.5f,
            0.0f,
            (zBoundaries.minimum + zBoundaries.maximum) * 0.5f
            );

        this.xBoundaries = xBoundaries;
        this.zBoundaries = zBoundaries;
    }

    public float CalculateAreaSize()
    {
        var area = 0.0f;
        var j = this.points.Length - 1;
        for (var i = 0; i < this.points.Length; i++)
        {
            area += (this.points[j].x + this.points[i].x) * (this.points[j].z - this.points[i].z);
            j = i;
        }
        return area * 0.5f;
    }
}