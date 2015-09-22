using UnityEditor;
using System.Linq;
using UnityEngine;

/// <summary>
/// Editor for <see cref="T:AreaPolygon"/>.
/// </summary>
[CanEditMultipleObjects]
[CustomEditor(typeof(AreaPolygon))]
public class AreaPolygonEditor : Editor
{
    /// <summary>
    /// Creates a new <see cref="T:UnityEngine.GameObject"/> with a attached <see cref="T:AreaPolygon"/> component.
    /// </summary>
    [MenuItem("GameObject/Create Other/Area Polygon")]
    private static void CreatePolygon()
    {
        var gameObject = new GameObject("Area Polygon", typeof(AreaPolygon));
        gameObject.transform.parent = Selection.activeTransform;
        Undo.RegisterCreatedObjectUndo(gameObject, "area created polygon");
        Selection.activeGameObject = gameObject;
    }

    /// <summary>
    /// Draws the polygon and it's handles.
    /// </summary>
    private void OnSceneGUI()
    {
        var areaPolygon = this.target as AreaPolygon;
        if (areaPolygon == null)
        {
            return;
        }

        var e = Event.current;
        if (e.type == EventType.ValidateCommand && e.commandName == "UndoRedoPerformed")
        {
            this.Repaint();
            return;
        }

        Handles.color = areaPolygon.color;
        Handles.DrawAAPolyLine(EditorGUIUtility.whiteTexture, areaPolygon.points);

        if (e.shift)
        {
            this.DrawMoveAllHandle(areaPolygon);
        }
        else if (EditorGUI.actionKey) // pc ctrl or mac command key
        {
            this.DrawRemoveHandles(areaPolygon);
        }
        else
        {
            this.DrawAddHandles(areaPolygon);
            this.DrawMoveHandles(areaPolygon);
        }
    }

    private void DrawMoveAllHandle(AreaPolygon areaPolygon)
    {
        Handles.color = Color.white;
        var newPosition = Handles.FreeMoveHandle(
            areaPolygon.transform.position,
            Quaternion.identity,
            0.5f,
            Vector3.zero,
            Handles.DotCap
        );
        newPosition.y = 0;

        if (newPosition != areaPolygon.transform.position)
        {
            var delta = newPosition - areaPolygon.transform.position;
            Undo.RecordObject(areaPolygon, "moved polygon");
            areaPolygon.transform.position = newPosition;
            for (var index = 0; index < areaPolygon.points.Length; index++)
            {
                areaPolygon.points[index] += delta;
            }
            areaPolygon.RecalculateBoundaries();
            EditorUtility.SetDirty(areaPolygon);
            return;
        }
    }

    /// <summary>
    /// Draws the handles used for moving points on the polygon.
    /// </summary>
    /// <param name="areaPolygon">
    /// The current <see cref="T:AreaPolygon"/> being edited.
    /// </param>
    private void DrawMoveHandles(AreaPolygon areaPolygon)
    {
        Handles.color = Color.white;
        for (var index = 0; index < areaPolygon.points.Length - 1; index++)
        {
            var point = areaPolygon.points[index];

            var newPoint = Handles.FreeMoveHandle(
                point,
                Quaternion.identity,
                0.25f,
                Vector3.zero,
                Handles.DotCap
            );
            newPoint.y = 0;

            if (newPoint != point)
            {
                Undo.RecordObject(areaPolygon, "moved point");
                areaPolygon.points[index] = newPoint;
                if (index == 0)
                {
                    areaPolygon.points[areaPolygon.points.Length - 1] = newPoint;
                }
                areaPolygon.RecalculateBoundaries();
                EditorUtility.SetDirty(areaPolygon);
                return;
            }
        }
    }

    /// <summary>
    /// Draws the handles used for adding new points to the polygon.
    /// </summary>
    /// <param name="areaPolygon">
    /// The current <see cref="T:AreaPolygon"/> being edited.
    /// </param>
    private void DrawAddHandles(AreaPolygon areaPolygon)
    {
        var length = areaPolygon.points.Length;
        Handles.color = Color.green;
        for (var index = 0; index < length - 1; index++)
        {
            var n = (index + 1) % length;
            var p1 = areaPolygon.points[index];
            var p2 = areaPolygon.points[n];
            var mid = (p1 + p2) * 0.5f;
            if (Handles.Button(mid, Quaternion.identity, 0.1f, 0.5f, Handles.DotCap))
            {
                Undo.RecordObject(areaPolygon, "added point");
                areaPolygon.points = areaPolygon.points.Insert(n, mid);
                areaPolygon.RecalculateBoundaries();
                EditorUtility.SetDirty(areaPolygon);
                break;
            }
        }
    }

    /// <summary>
    /// Draws the handles used for removing existing points from the polygon.
    /// </summary>
    /// <param name="areaPolygon">
    /// The current <see cref="T:AreaPolygon"/> being edited.
    /// </param>
    private void DrawRemoveHandles(AreaPolygon areaPolygon)
    {
        if (areaPolygon.points.Length < 5)
        {
            return;
        }

        Handles.color = Color.red;
        for (var index = 0; index < areaPolygon.points.Length - 1; index++)
        {
            if (Handles.Button(areaPolygon.points[index], Quaternion.identity, 0.25f, 1.0f, Handles.DotCap))
            {
                Undo.RecordObject(areaPolygon, "removed point");
                areaPolygon.points = areaPolygon.points.Remove(index);
                if (index == 0)
                {
                    areaPolygon.points[areaPolygon.points.Length - 1] = areaPolygon.points[0];
                }
                areaPolygon.RecalculateBoundaries();
                EditorUtility.SetDirty(areaPolygon);
                break;
            }
        }
    }
}
