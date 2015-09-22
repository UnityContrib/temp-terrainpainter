using UnityEngine;
using System.Collections;
using System;

[Serializable]
public class TerrainDetailsPainterRule
{
    public bool UseHeightRange;
    public float MinimumHeight;
    public float MaximumHeight;

    public bool UseSlopeRange;
    public float MinimumSlope;
    public float MaximumSlope;

    public int DetailIndex;

    public float Chance;
}
