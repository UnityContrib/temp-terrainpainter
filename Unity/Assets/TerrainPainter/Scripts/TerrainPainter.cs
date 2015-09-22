using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainPainter : MonoBehaviour
{
    public List<TerrainPainterRule> Rules = new List<TerrainPainterRule>();
}
