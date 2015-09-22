using UnityEditor;
using UnityEngine;

public static class HandlesEx
{
    public static void DrawClosedPolyLine(params Vector3[] points)
    {
        Handles.DrawPolyLine(points);
        Handles.DrawLine(points[0], points[points.Length - 1]);
    }

    public static void DrawClosedAAPolyLine(params Vector3[] points)
    {
        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, points);
        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, points[points.Length - 1], points[0]);
    }
}
