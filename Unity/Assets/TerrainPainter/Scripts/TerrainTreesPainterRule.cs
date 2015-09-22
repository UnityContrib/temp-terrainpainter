using System;

[Serializable]
public class TerrainTreesPainterRule
{
    public bool UseHeightRange;
    public float MinimumHeight;
    public float MaximumHeight;

    public bool UseSlopeRange;
    public float MinimumSlope;
    public float MaximumSlope;

    public int TreeIndex;

    public float Density;

    public AreaPolygon[] AreaPolygons = new AreaPolygon[1];

    public TerrainTreesPainterRule Clone()
    {
        return new TerrainTreesPainterRule()
        {
            UseHeightRange = this.UseHeightRange,
            MinimumHeight = this.MinimumHeight,
            MaximumHeight = this.MaximumHeight,
            UseSlopeRange = this.UseSlopeRange,
            MinimumSlope = this.MinimumSlope,
            MaximumSlope = this.MaximumSlope,
            TreeIndex = this.TreeIndex,
            Density = this.Density,
            AreaPolygons = this.AreaPolygons.Clone() as AreaPolygon[],
        };
    }
}
