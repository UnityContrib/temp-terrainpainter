using UnityEngine;

public static class TerrainEx
{
    public static float SampleWorldHeight(this Terrain terrain, Vector3 worldPosition)
    {
        var height = terrain.SampleHeight(worldPosition) + terrain.transform.position.y;
        return height;
    }
}
