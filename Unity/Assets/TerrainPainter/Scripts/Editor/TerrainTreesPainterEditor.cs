using UnityEngine;
using System.Linq;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(TerrainTreesPainter))]
public class TerrainTreesPainterEditor : Editor
{
    private TerrainTreesPainter painter;
    private Terrain terrain;
    private TerrainData data;

    private void OnEnable()
    {
        this.painter = this.target as TerrainTreesPainter;
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

        if (GUILayout.Button("Add Rule"))
        {
            this.painter.Rules.Add(new TerrainTreesPainterRule());
        }
        GUI.enabled = this.data.treePrototypes.Length > 0;
        if (GUILayout.Button("Paint All"))
        {
            this.ApplyAll();
        }
        GUI.enabled = true;

        GUILayout.Space(20.0f);

        var rules = this.painter.Rules;

        for (var ruleIndex = 0; ruleIndex < rules.Count; ruleIndex++)
        {
            var rule = rules[ruleIndex];

            Common.Horizontal(() =>
            {
                Common.Vertical(() =>
                {
                    Common.Horizontal(() =>
                    {
                        EditorGUILayout.LabelField("Tree", GUILayout.Width(100.0f));
                        var newTreeIndex = EditorGUILayout.Popup(rule.TreeIndex, this.data.treePrototypes.Select(s => s.prefab.name).ToArray());
                        if (newTreeIndex != rule.TreeIndex)
                        {
                            rule.TreeIndex = newTreeIndex;
                            return;
                        }
                    });

                    Common.Horizontal(() =>
                    {
                        EditorGUILayout.LabelField("Density", GUILayout.Width(100.0f));
                        rule.Density = EditorGUILayout.FloatField(rule.Density);
                    });

                    Common.Horizontal(() =>
                    {
                        EditorGUILayout.LabelField("Area Polygon", GUILayout.Width(100.0f));
                        if (GUILayout.Button("+", GUILayout.Width(20.0f)))
                        {
                            rule.AreaPolygons = rule.AreaPolygons.Insert(0, null);
                            return;
                        }
                        Common.Vertical(() =>
                        {
                            for (var areaIndex = 0; areaIndex < rule.AreaPolygons.Length; areaIndex++)
                            {
                                Common.Horizontal(() =>
                                {
                                    GUI.enabled = rule.AreaPolygons.Length > 1;
                                    if (GUILayout.Button("X", GUILayout.Width(20.0f)))
                                    {
                                        rule.AreaPolygons = rule.AreaPolygons.Remove(areaIndex);
                                        return;
                                    }
                                    GUI.enabled = true;
                                    rule.AreaPolygons[areaIndex] = EditorGUILayout.ObjectField(rule.AreaPolygons[areaIndex], typeof(AreaPolygon), allowSceneObjects: true) as AreaPolygon;
                                });
                            }
                        });
                    });

                    Common.Horizontal(() =>
                    {
                        EditorGUILayout.LabelField("Height range", GUILayout.Width(100.0f));
                        rule.UseHeightRange = EditorGUILayout.Toggle(rule.UseHeightRange, GUILayout.Width(20.0f));
                        rule.MinimumHeight = EditorGUILayout.FloatField(rule.MinimumHeight);
                        rule.MaximumHeight = EditorGUILayout.FloatField(rule.MaximumHeight);
                    });

                    Common.Horizontal(() =>
                    {
                        EditorGUILayout.LabelField("Slope range", GUILayout.Width(100.0f));
                        rule.UseSlopeRange = EditorGUILayout.Toggle(rule.UseSlopeRange, GUILayout.Width(20.0f));
                        rule.MinimumSlope = EditorGUILayout.FloatField(rule.MinimumSlope);
                        rule.MaximumSlope = EditorGUILayout.FloatField(rule.MaximumSlope);
                    });

                    Common.Horizontal(() =>
                    {
                        GUI.enabled = ruleIndex > 0;
                        if (GUILayout.Button("Up"))
                        {
                            this.painter.Rules.Swap(ruleIndex, ruleIndex - 1);
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                        GUI.enabled = ruleIndex < rules.Count - 1;
                        if (GUILayout.Button("Down"))
                        {
                            this.painter.Rules.Swap(ruleIndex, ruleIndex + 1);
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                        GUI.enabled = true;
                        if (GUILayout.Button("Clone"))
                        {
                            this.painter.Rules.Insert(ruleIndex, rule.Clone());
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                        if (GUILayout.Button("Remove"))
                        {
                            this.painter.Rules.Remove(rule);
                            EditorUtility.SetDirty(this.target);
                            return;
                        }
                        if (GUILayout.Button("Paint"))
                        {
                            this.Apply(rule.TreeIndex);
                        }
                    });
                });
                Common.Vertical(() =>
                {
                    if (rule.TreeIndex < this.data.treePrototypes.Length)
                    {
                        var thumbnail = AssetPreview.GetAssetPreview(this.data.treePrototypes[rule.TreeIndex].prefab);
                        GUILayout.Label(thumbnail, GUILayout.Width(64.0f), GUILayout.Height(64.0f));
                    }
                });
            });

            if (ruleIndex < rules.Count - 1)
            {
                Common.Separator();
            }
        }
    }

    private void ApplyAll()
    {
        for (var treeIndex = 0; treeIndex < this.data.treePrototypes.Length; treeIndex++)
        {
            this.Apply(treeIndex);
        }
    }

    private void Apply(int treeIndex)
    {
        this.data.treeInstances = this.data.treeInstances.RemoveAll(i => i.prototypeIndex == treeIndex);

        var rules = this.painter.Rules.Where(r => r.TreeIndex == treeIndex);
        foreach (var rule in rules)
        {
            foreach (var area in rule.AreaPolygons)
            {
                var areaSize = area.CalculateAreaSize();
                Debug.Log(areaSize);
            }
        }
    }
}
