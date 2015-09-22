using System;

[Serializable]
public class TerrainPainterRule
{
    public bool UseHeightRange;
    public float MinimumHeight;
    public float MaximumHeight;

    public bool UseSlopeRange;
    public float MinimumSlope;
    public float MaximumSlope;

    public int SplatIndex;
}
