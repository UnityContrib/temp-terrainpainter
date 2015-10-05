using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainDetailsPainter))]
public class TerrainDetailsPainterEditor : Editor
{
    private TerrainDetailsPainter painter;
    private Terrain terrain;
    private TerrainData data;

    private void OnEnable()
    {
        this.painter = this.target as TerrainDetailsPainter;
        this.terrain = this.painter.GetComponent<Terrain>();
        this.data = this.terrain.terrainData;
    }

    public override void OnInspectorGUI()
    {
        // reference check
        if (this.terrain == null)
        {
            GUILayout.Label("Terrain not found.");
            return;
        }
        if (this.data == null)
        {
            GUILayout.Label("Data not found.");
            return;
        }

        GUILayout.Space(20.0f);

        // existing rules
        for (var index = 0; index < this.painter.Rules.Count; index++)
        {
            var rule = this.painter.Rules[index];

            Common.Horizontal(() =>
            {
                Common.Vertical(() =>
                {
                    rule.DetailIndex = EditorGUILayout.Popup(rule.DetailIndex, this.data.detailPrototypes.Select(s => s.usePrototypeMesh ? s.prototype.name : s.prototypeTexture.name).ToArray());

                    Common.Horizontal(() =>
                    {
                        EditorGUILayout.LabelField("Chance (0-1)");
                        rule.Chance = EditorGUILayout.FloatField(rule.Chance);
                    });

                    Common.Horizontal(() =>
                    {
                        rule.UseHeightRange = EditorGUILayout.Toggle("Height range", rule.UseHeightRange);
                        rule.MinimumHeight = EditorGUILayout.FloatField(rule.MinimumHeight);
                        rule.MaximumHeight = EditorGUILayout.FloatField(rule.MaximumHeight);
                    });

                    Common.Horizontal(() =>
                    {
                        rule.UseSlopeRange = EditorGUILayout.Toggle("Slope range", rule.UseSlopeRange);
                        rule.MinimumSlope = EditorGUILayout.FloatField(rule.MinimumSlope);
                        rule.MaximumSlope = EditorGUILayout.FloatField(rule.MaximumSlope);
                    });

                    Common.Horizontal(() =>
                    {
                        GUI.enabled = index > 0;
                        if (GUILayout.Button("Up"))
                        {
                            this.painter.Rules.Swap(index, index - 1);
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                        GUI.enabled = index < this.painter.Rules.Count - 1;
                        if (GUILayout.Button("Down"))
                        {
                            this.painter.Rules.Swap(index, index + 1);
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button("Remove"))
                        {
                            this.painter.Rules.RemoveAt(index);
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                    });
                });
                if (rule.DetailIndex < this.data.detailPrototypes.Length)
                {
                    GUILayout.Label(this.data.detailPrototypes[rule.DetailIndex].prototypeTexture, GUILayout.Width(64.0f), GUILayout.Height(64.0f));
                }
                else
                {
                    GUILayout.Label("Texture\nis\nmissing", GUILayout.Width(64.0f), GUILayout.Height(64.0f));
                }
            });

            GUILayout.Space(20.0f);
        }

        // add button
        if (GUILayout.Button("Add Rule"))
        {
            this.painter.Rules.Add(new TerrainDetailsPainterRule());
        }

        // paint button
        GUI.enabled = this.data.detailPrototypes.Length > 0;
        if (GUILayout.Button("Paint"))
        {
            this.Apply();
        }
        GUI.enabled = true;
    }

    private void Apply()
    {
        int[,] detailsLayer;
        var details = new int[this.data.detailPrototypes.Length][,];

        for (var y = 0; y < this.data.detailHeight; y++)
        {
            for (var x = 0; x < this.data.detailWidth; x++)
            {
                for (var ruleIndex = 0; ruleIndex < this.painter.Rules.Count; ruleIndex++)
                {
                    var rule = this.painter.Rules[ruleIndex];

                    var r = Random.Range(0.0f, 1.0f);
                    if (r > rule.Chance)
                    {
                        detailsLayer = details[rule.DetailIndex];
                        if (detailsLayer == null)
                        {
                            details[rule.DetailIndex] = detailsLayer = new int[this.data.detailWidth, this.data.detailHeight];
                        }
                        continue;
                    }

                    if (!rule.UseHeightRange && !rule.UseSlopeRange)
                    {
                        detailsLayer = details[rule.DetailIndex];
                        if (detailsLayer == null)
                        {
                            details[rule.DetailIndex] = detailsLayer = new int[this.data.detailWidth, this.data.detailHeight];
                        }
                        details[rule.DetailIndex][x, y] = Random.Range(1, 8);
                        continue;
                    }

                    if (rule.UseHeightRange)
                    {
                        var worldPosition = terrain.DetailsToWorldPosition(x, y);
                        var worldY = terrain.SampleHeight(worldPosition);
                        if (worldY < rule.MinimumHeight || worldY >= rule.MaximumHeight)
                        {
                            detailsLayer = details[rule.DetailIndex];
                            if (detailsLayer == null)
                            {
                                details[rule.DetailIndex] = detailsLayer = new int[this.data.detailWidth, this.data.detailHeight];
                            }
                            continue;
                        }
                    }

                    if (rule.UseSlopeRange)
                    {
                        var normalizedPosition = terrain.DetailsToNormalizedPosition(x, y);
                        var slope = this.data.GetSteepness(normalizedPosition.x, normalizedPosition.y);
                        if (slope < rule.MinimumSlope || slope >= rule.MaximumSlope)
                        {
                            detailsLayer = details[rule.DetailIndex];
                            if (detailsLayer == null)
                            {
                                details[rule.DetailIndex] = detailsLayer = new int[this.data.detailWidth, this.data.detailHeight];
                            }
                            continue;
                        }
                    }

                    detailsLayer = details[rule.DetailIndex];
                    if (detailsLayer == null)
                    {
                        details[rule.DetailIndex] = detailsLayer = new int[this.data.detailWidth, this.data.detailHeight];
                    }
                    details[rule.DetailIndex][x, y] = Random.Range(1, 8);
                    break;
                }
            }
        }

        for (var index = 0; index < this.data.detailPrototypes.Length; index++)
        {
            detailsLayer = details[index];
            if (detailsLayer != null)
            {
                this.data.SetDetailLayer(0, 0, index, details[index]);
            }
        }
    }
}
