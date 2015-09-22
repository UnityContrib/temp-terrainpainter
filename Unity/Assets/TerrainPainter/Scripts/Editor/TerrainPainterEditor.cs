using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TerrainPainter))]
public class TerrainPainterEditor : Editor
{
    private TerrainPainter painter;
    private Terrain terrain;
    private TerrainData data;

    private void OnEnable()
    {
        this.painter = this.target as TerrainPainter;
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

        GUILayout.Space(20.0f);

        // existing rules
        for (var index = 0; index < this.painter.Rules.Count; index++)
        {
            var rule = this.painter.Rules[index];

            Common.Horizontal(() =>
                {
                    Common.Vertical(() =>
                        {
                            rule.SplatIndex = EditorGUILayout.Popup(rule.SplatIndex, this.data.splatPrototypes.Select(s => s.texture.name).ToArray());

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
                    if (rule.SplatIndex < this.data.splatPrototypes.Length)
                    {
                        GUILayout.Label(this.data.splatPrototypes[rule.SplatIndex].texture, GUILayout.Width(64.0f), GUILayout.Height(64.0f));
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
            this.painter.Rules.Add(new TerrainPainterRule());
        }

        // paint button
        GUI.enabled = this.data.splatPrototypes.Length > 0;
        if (GUILayout.Button("Paint"))
        {
            this.AutoApplyTexture();
        }
        GUI.enabled = true;
    }

    private void AutoApplyTexture()
    {
        var maps = new float[this.data.alphamapWidth, this.data.alphamapHeight, this.data.splatPrototypes.Length];

        for (var z = 0; z < this.data.alphamapHeight; z++)
        {
            for (var x = 0; x < this.data.alphamapWidth; x++)
            {
                foreach (var rule in this.painter.Rules)
                {
                    if (!rule.UseHeightRange && !rule.UseSlopeRange)
                    {
                        maps[x, z, rule.SplatIndex] = 1.0f;
                        break;
                    }

                    if (rule.UseHeightRange)
                    {
                        var worldPosition = terrain.AlphamapToWorldPosition(x, z);
                        var worldY = terrain.SampleHeight(worldPosition);
                        if (worldY < rule.MinimumHeight || worldY >= rule.MaximumHeight)
                        {
                            continue;
                        }
                    }

                    if (rule.UseSlopeRange)
                    {
                        var normalizedPosition = terrain.AlphamapToNormalizedPosition(x, z);
                        var slope = this.data.GetSteepness(normalizedPosition.y, normalizedPosition.x);
                        if (slope < rule.MinimumSlope || slope >= rule.MaximumSlope)
                        {
                            continue;
                        }
                    }

                    maps[x, z, rule.SplatIndex] = 1.0f;
                    break;
                }
            }
        }

        for (var z = 0; z < this.data.alphamapHeight; z++)
        {
            for (var x = 0; x < this.data.alphamapWidth; x++)
            {
                for (var s = 0; s < this.data.splatPrototypes.Length; s++)
                {
                    var value = maps[x, z, s];
                    if (value != 1.0f)
                    {
                        continue;
                    }

                    if (x < this.data.alphamapWidth - 1)
                    {
                        for (var s2 = 0; s2 < this.data.splatPrototypes.Length; s2++)
                        {
                            var value2 = maps[x + 1, z, s2];
                            if (value2 != 1.0f)
                            {
                                continue;
                            }
                            if (s == s2)
                            {
                                continue;
                            }

                            maps[x, z, s2] = 1.0f;
                            break;
                        }
                    }
                    if (z < this.data.alphamapHeight - 1)
                    {
                        for (var s2 = 0; s2 < this.data.splatPrototypes.Length; s2++)
                        {
                            var value2 = maps[x, z + 1, s2];
                            if (value2 != 1.0f)
                            {
                                continue;
                            }
                            if (s == s2)
                            {
                                continue;
                            }

                            maps[x, z, s2] = 1.0f;
                            break;
                        }
                    }
                }
            }
        }

        //blend
        for (var z = 0; z < this.data.alphamapHeight; z++)
        {
            for (var x = 0; x < this.data.alphamapWidth; x++)
            {
                var count = 0.0f;
                for (var s = 0; s < this.data.splatPrototypes.Length; s++)
                {
                    if (maps[x, z, s] == 1.0f)
                    {
                        count++;
                    }
                }

                for (var s = 0; s < this.data.splatPrototypes.Length; s++)
                {
                    if (maps[x, z, s] == 1.0f)
                    {
                        maps[x, z, s] = 1.0f / count;
                    }
                }
            }
        }

        //add dirt
        for (var z = 0; z < this.data.alphamapHeight; z++)
        {
            for (var x = 0; x < this.data.alphamapWidth; x++)
            {
                var r = UnityEngine.Random.Range(0.9f, 1.0f);
                for (var s = 0; s < this.data.splatPrototypes.Length; s++)
                {
                    maps[x, z, s] *= r;
                }
            }
        }

        this.data.SetAlphamaps(0, 0, maps);
        Debug.Log(DateTime.Now + ": DONE");
    }
}
