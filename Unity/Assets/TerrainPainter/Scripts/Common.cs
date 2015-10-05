using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class Common
{
    public const float TwoPi = Mathf.PI * 2.0f;
    public const float PiOver2 = Mathf.PI * 0.5f;
    public const float PiOver4 = Mathf.PI * 0.25f;

    public static void Separator()
    {
        Common.Vertical(() =>
        {
            GUILayout.Space(7.0f);
            GUILayout.Box(EditorGUIUtility.whiteTexture, GUILayout.Height(1.0f), GUILayout.ExpandWidth(true));
            GUILayout.Space(7.0f);
        });
    }

    public static void Horizontal(Action content)
    {
        GUILayout.BeginHorizontal();
        content();
        GUILayout.EndHorizontal();
    }

    public static void Horizontal(Action content, GUIStyle style)
    {
        GUILayout.BeginHorizontal(style);
        content();
        GUILayout.EndHorizontal();
    }

    public static void Vertical(Action content)
    {
        GUILayout.BeginVertical();
        content();
        GUILayout.EndVertical();
    }

    public static void Vertical(Action content,params GUILayoutOption[] options)
    {
        GUILayout.BeginVertical(options);
        content();
        GUILayout.EndVertical();
    }

    public static void Padding(float padding, Action content)
    {
        Horizontal(() =>
            {
                GUILayout.Space(padding);
                Vertical(() =>
                {
                    GUILayout.Space(padding);
                    content();
                    GUILayout.Space(padding);
                });
                GUILayout.Space(padding);
            });
    }

    public static bool Contains(this string text, string value, StringComparison comparisonType)
    {
        return text.IndexOf(value, comparisonType) > -1;
    }

    public static int IndexOf<T>(this IEnumerable<T> source, Predicate<T> predicate)
    {
        var index = 0;
        foreach (var element in source)
        {
            if (predicate(element))
            {
                return index;
            }
            index++;
        }
        return -1;
    }

    public static float CalculateSlopeRad(this Vector3 normal)
    {
        return Mathf.Abs(Vector3.Dot(Vector3.up, normal));
        //return Mathf.Acos(Mathf.Clamp(normal.y, -1.0f, 1.0f));
    }

    public static float CalculateSlopeDeg(this Vector3 normal)
    {
        return normal.CalculateSlopeRad() * Mathf.Rad2Deg;
    }

    public static Vector3 HeightmapToWorldPosition(this Terrain terrain, int x, int y)
    {
        var heightmapWidth = (float)terrain.terrainData.heightmapWidth;
        var heightmapHeight = (float)terrain.terrainData.heightmapHeight;
        return new Vector3(
            terrain.terrainData.size.x / heightmapWidth * x + terrain.transform.position.x,
            0.0f,
            terrain.terrainData.size.z / heightmapHeight * y + terrain.transform.position.z
            );
    }

    public static Vector3 AlphamapToWorldPosition(this Terrain terrain, int x, int y)
    {
        var alphamapWidth = (float)terrain.terrainData.alphamapWidth;
        var alphamapHeight = (float)terrain.terrainData.alphamapHeight;
        return new Vector3(
            terrain.terrainData.size.x / alphamapWidth * y + terrain.transform.position.x,
            0.0f,
            terrain.terrainData.size.z / alphamapHeight * x + terrain.transform.position.z
            );
    }

    public static Vector2 AlphamapToHeightmapPosition(this Terrain terrain, float x, float y)
    {
        var normalizedPosition = terrain.AlphamapToNormalizedPosition(x, y);
        var heightmapWidth = (float)terrain.terrainData.heightmapWidth;
        var heightmapHeight = (float)terrain.terrainData.heightmapHeight;
        return new Vector2(
            normalizedPosition.x * heightmapWidth,
            normalizedPosition.y * heightmapHeight
            );
    }

    public static void Swap<T>(this List<T> source, int i1, int i2)
    {
        var t = source[i1];
        source[i1] = source[i2];
        source[i2] = t;
    }

    public static void Swap<T>(this T[] source, int i1, int i2)
    {
        var t = source[i1];
        source[i1] = source[i2];
        source[i2] = t;
    }

    public static Vector2 WorldToAlphamapPosition(this Terrain terrain, Vector3 worldPosition)
    {
        var alphamapWidth = (float)terrain.terrainData.alphamapWidth;
        var alphamapHeight = (float)terrain.terrainData.alphamapHeight;
        return new Vector2(
            alphamapWidth / terrain.terrainData.size.x * (worldPosition.x - terrain.transform.position.x),
            alphamapHeight / terrain.terrainData.size.z * (worldPosition.z - terrain.transform.position.z)
            );
    }

    public static string GetMostUsedTextureNameAt(this Terrain terrain, Vector3 worldPosition)
    {
        var alphamapPosition = terrain.WorldToAlphamapPosition(worldPosition);
        var alphamaps = terrain.terrainData.GetAlphamaps((int)alphamapPosition.x, (int)alphamapPosition.y, 1, 1);
        var texture = terrain.terrainData.splatPrototypes[0].texture;
        var value = alphamaps[0, 0, 0];
        for (var s = 1; s < terrain.terrainData.splatPrototypes.Length; s++)
        {
            var value2 = alphamaps[0, 0, s];
            if (value2 > value)
            {
                texture = terrain.terrainData.splatPrototypes[s].texture;
                value = value2;
            }
        }

        return texture.name;
    }

    public static Vector2 AlphamapToNormalizedPosition(this Terrain terrain, float x, float y)
    {
        var alphamapWidth = (float)terrain.terrainData.alphamapWidth;
        var alphamapHeight = (float)terrain.terrainData.alphamapHeight;
        return new Vector2(
            x / alphamapWidth,
            y / alphamapHeight
            );
    }

    public static Vector2 WorldToNormalizedPosition(this Terrain terrain, Vector3 worldPosition)
    {
        return new Vector2(
            (worldPosition.x - terrain.transform.position.x) / terrain.terrainData.size.x,
            (worldPosition.z - terrain.transform.position.z) / terrain.terrainData.size.z
            );
    }

    public static Vector3 DetailsToWorldPosition(this Terrain terrain, float x, float y)
    {
        var normalizedPosition = terrain.DetailsToNormalizedPosition(x, y);
        return new Vector3(
            terrain.terrainData.size.x * normalizedPosition.x + terrain.transform.position.x,
            0.0f,
            terrain.terrainData.size.z * normalizedPosition.y + terrain.transform.position.z
            );
    }

    public static Vector2 DetailsToNormalizedPosition(this Terrain terrain, float x, float y)
    {
        var detailsWidth = (float)terrain.terrainData.detailWidth;
        var detailsHeight = (float)terrain.terrainData.detailHeight;
        return new Vector2(
            y / detailsWidth,
            x / detailsHeight
            );
    }

    public static Vector3 RandomVector3(float radius)
    {
        return UnityEngine.Random.insideUnitSphere * radius;
    }

    public static float GetSteepnessFromWorldPosition(this Terrain terrain, Vector3 worldPosition)
    {
        var normalizedPosition = terrain.WorldToNormalizedPosition(worldPosition);
        return terrain.terrainData.GetSteepness(normalizedPosition.x, normalizedPosition.y);
    }

    public static T GetComponent2<T>(this GameObject gameObject)
    {
        return (T)(object)gameObject.GetComponent(typeof(T));
    }

    public static Texture2D ToTexture2D(this RenderTexture renderTexture)
    {
        var active = RenderTexture.active;
        RenderTexture.active = renderTexture;
        var texture = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, renderTexture.useMipMap);
        Debug.Log(renderTexture.format + " " + texture.format);
        texture.ReadPixels(
            new Rect(0.0f, 0.0f, renderTexture.width, renderTexture.height),
            0, 0
            );
        texture.alphaIsTransparency = false;
        texture.filterMode = renderTexture.filterMode;
        texture.Apply();
        var pixels = texture.GetPixels32();
        for (var index = 0; index < pixels.Length; index++)
        {
            var color = pixels[index];
            if (color.r == 255 && color.g == 0 && color.b == 255)
            {
                color.a = 0;
                pixels[index] = color;
            }
            if (color.a > 0)
            {
                color.a = 255;
                pixels[index] = color;
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        RenderTexture.active = active;
        return texture;
    }

    public static void DrawPolygon(this Vector3[] points, Color color)
    {
        Handles.color = color;
        for (var index = 0; index < 10; index++)
        {
            Handles.DrawAAPolyLine(10.0f, points);
            Handles.DrawAAPolyLine(10.0f, points[0], points[points.Length - 1]);
        }
    }

    public static float[] GetDistanceToVerticies(this Vector3[] verticies, Vector3 position)
    {
        var distances = new float[verticies.Length];
        for(var index = 0; index < verticies.Length; index++)
        {
            distances[index] = Vector3.Distance(position, verticies[index]);
        }
        return distances;
    }

    public static Nullable<int> GetNearestVertex(this Vector3[] verticies, Vector3 position, float[] distances, float maxDistance)
    {
        Nullable<int> nearestIndex = null;
        var nearestDistance = float.MaxValue;

        for (var currentIndex = 0; currentIndex < verticies.Length; currentIndex++)
        {
            //var currentPoint = verticies[currentIndex];
            var currentDistance = distances[currentIndex];
            if (currentDistance > maxDistance)
            {
                continue;
            }
            if (currentDistance > nearestDistance)
            {
                continue;
            }

            nearestDistance = currentDistance;
            nearestIndex = currentIndex;
        }

        return nearestIndex;
    }

    public static T[] RemoveAll<T>(this T[] source, Func<T, bool> predicate)
    {
        return source.Where(predicate).ToArray();
    }
}
