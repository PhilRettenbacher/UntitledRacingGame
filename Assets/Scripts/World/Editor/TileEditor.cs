using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Tile))]
public class TileEditor : Editor
{
    private Tile tile;

    private SerializedProperty m_mask;
    private SerializedProperty m_entryConnectionType;
    private SerializedProperty m_exitConnectionType;
    private SerializedProperty m_probability;

    private void OnEnable()
    {
        tile = target as Tile;
        tile.ScaleChildren();

        m_mask = serializedObject.FindProperty(nameof(Tile.mask));
        m_entryConnectionType = serializedObject.FindProperty(nameof(Tile.entryConnectionType));
        m_exitConnectionType = serializedObject.FindProperty(nameof(Tile.exitConnectionType));
        m_probability = serializedObject.FindProperty(nameof(Tile.probability));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawMaskGUI();
        
        DrawPathsGUI();

        EditorGUI.BeginChangeCheck();
        Undo.RecordObject(tile, "Change Model scale");

        tile.modelTileScale = EditorGUILayout.FloatField("Model scale", tile.modelTileScale);

        if (EditorGUI.EndChangeCheck())
        {
            tile.ScaleChildren();
        }

        Undo.RecordObject(tile, "Change Tile Probability");

        EditorGUILayout.PropertyField(m_probability);
        serializedObject.ApplyModifiedProperties();
    }

    void DrawMaskGUI()
    {
        EditorGUI.BeginChangeCheck();

        Undo.RecordObject(tile, "Set Mask");

        if (tile.mask == null)
        {
            tile.mask = new TileMask();
        }

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(m_mask);
            }
        }

        TileConnectionType newEntryConnection = tile.entryConnectionType;
        TileConnectionType newExitConnection = tile.exitConnectionType;

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(m_entryConnectionType);
            EditorGUILayout.PropertyField(m_exitConnectionType);
        }
        if (EditorGUI.EndChangeCheck())
        {
            //if(newEntryConnection != tile.entryConnectionType || newExitConnection != tile.exitConnectionType)
            //{
            //    tile.ResetPaths();
            //}


            SceneView.RepaintAll();
            //if(size != new Vector2Int(tile.mask.grid.GetLength(0), tile.mask.grid.GetLength(1)))
            //{
            //    tile.ResetPaths();
            //}
        }
    }

    public void DrawPathsGUI()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            GUILayout.Label("Borders:");
            if (GUILayout.Button("Reset Borders"))
            {
                Undo.RecordObject(tile, "Reset Borders");
                tile.ResetPaths();
            }

            Undo.RecordObject(tile, "Set Borders");
            EditorGUI.BeginChangeCheck();
            DrawPathGUI("Right Border", tile.rightBorder);
            EditorGUILayout.Space();
            DrawPathGUI("Left Border", tile.leftBorder);
            EditorGUILayout.Space();
            DrawPathGUI("Center Path", tile.centerPath);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(tile, "Set Borders");
                tile.rightBorder.RecalculatePoints();
                tile.leftBorder.RecalculatePoints();
                tile.centerPath.RecalculatePoints();

                SceneView.RepaintAll();
            }
        }
    }

    public Path DrawPathGUI(string label, Path path)
    {
        GUILayout.Label(label, EditorStyles.boldLabel);

        if (path.points.Count < 2)
            return path;

        if(path.points.Count == 2)
        {
            GUILayout.Label("No Points!");
        }
        GUILayout.Label($"Distance: {path.distance}");
        if (GUILayout.Button("Add Point"))
        {
            int newIdx = path.points.Count - 1;
            var newPoint = new PathPoint() { position = (path.points[newIdx-1].position + path.points[newIdx].position) / 2, hasRadius = true, radius = 1 };

            path.points.Insert(newIdx, newPoint);
        }

        for(int i = 1; i<path.points.Count-1; i++)
        {
            using (new GUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var point = path.points[i];

                point.position = EditorGUILayout.Vector2Field(i + ":", point.position);
                point.radius = EditorGUILayout.Slider("Radius: ", point.radius, 0.1f, 20);
                GUILayout.Label($"Calculated Radius: {point.calculatedRadius}");
                GUILayout.Label($"Calculated Distance To Transition: {point.calculatedDistanceToTransition}");
                GUILayout.Label($"Cumulative Distance: {point.cumulativeDistance}");

                using (new GUILayout.HorizontalScope())
                {
                    GUI.enabled = path.CanPointBeConstrained(i);
                    if (GUILayout.Button("Constraint Point"))
                    {
                        Vector2 newPos = new Vector2();

                        if(path.points.Count == 3)
                        {
                            newPos = MathUtils.ConstraintPointMulti(
                                path.points[0].position,
                                tile.entryDirection,
                                path.points[path.points.Count - 1].position,
                                tile.exitDirection
                                );
                        }
                        else if(i == 1)
                        {
                            newPos = MathUtils.ConstraintPoint(path.points[i].position, path.points[0].position, tile.entryDirection);
                        }
                        else
                        {
                            newPos = MathUtils.ConstraintPoint(path.points[i].position, path.points[path.points.Count-1].position, tile.exitDirection);
                        }

                        point.position = newPos;

                    }
                    GUI.enabled = true;

                    path.points[i] = point;

                    if (GUILayout.Button("Delete Point"))
                    {
                        path.points.RemoveAt(i);
                        i--;
                    }
                }
            }
        }

        return path;
    }

    TilePosition DrawTilePositionGUI(string label, TilePosition tilePos, Vector2Int max, Vector2Int min)
    {
        if(!string.IsNullOrEmpty(label))
        {
            GUILayout.Label(label);
        }

        tilePos.position = Vector2Int.Min(Vector2Int.Max(EditorGUILayout.Vector2IntField("Position", tilePos.position), min), max);

        tilePos.direction = new TileDirection(EditorGUILayout.Popup(tilePos.direction, TileDirection.names));

        return tilePos;
    }

    private void OnSceneGUI()
    {
        DrawMaskSceneGUI();

        DrawTilePositionSceneGUI(tile.mask.entryPosition, Color.green);
        DrawTilePositionSceneGUI(tile.mask.exitPosition, Color.red);

        DrawTilePathSceneGUI(tile.rightBorder, Color.green, Color.gray);
        DrawTilePathSceneGUI(tile.leftBorder, Color.green, Color.gray);
        DrawTilePathSceneGUI(tile.centerPath, Color.green, Color.gray);
    }

    void DrawMaskSceneGUI()
    {
        if (tile.mask == null)
            return;

        for(int x = 0; x<tile.mask.grid.GetLength(0); x++)
        {
            for(int y = 0; y<tile.mask.grid.GetLength(1); y++)
            {
                if(tile.mask.grid[x, y])
                    Handles.DrawWireCube(tile.TransformMaskPointToWorld(new Vector2Int(x, y)), new Vector3(WorldConstants.TileSize, 0.1f, WorldConstants.TileSize));
            }
        }
    }

    void DrawTilePositionSceneGUI(TilePosition tilePos, Color color)
    {
        if (tile.mask == null)
            return;
        var prevColor = Handles.color;

        Handles.color = color;

        Vector3 worldPoint = tile.TransformMaskPointToWorld(tilePos.position);

        Handles.DrawWireCube(worldPoint, new Vector3(WorldConstants.TileSize * 0.75f, 0.1f, WorldConstants.TileSize * 0.75f));

        Handles.DrawLine(worldPoint, tile.TransformMaskPointToWorld(tilePos.position + (Vector2)tilePos.direction.ToVector() / 2f));
        
        Handles.color = prevColor;
    }

    void DrawTilePathSceneGUI(Path path, Color color, Color secondary)
    {
        if (tile.mask == null)
            return;

        var newPath = tile.TransformMaskPathToWorld(path);

        EditorUtils.DrawPointPath(newPath, Vector3.zero, color, secondary);

        //for(int i = 0; i<path.points.Count-1; i++)
        //{
        //    var position = tile.TransformMaskPointToWorld(path.points[i].position);

        //    Handles.DrawWireCube(position, Vector3.one);
        //    Handles.DrawLine(position, tile.TransformMaskPointToWorld(path.points[i + 1].position));

        //    if (path.points[i].hasRadius)
        //    {
        //        var center = tile.TransformMaskPointToWorld(path.points[i].circleCenter);
        //        var entryPoint = tile.TransformMaskPointToWorld(path.points[i].startPos);

        //        var normal = Vector3.Cross(center - position, entryPoint - position);

        //        Handles.DrawWireArc(center, normal, entryPoint - center, path.points[i].calculatedAngle, path.points[i].calculatedRadius * WorldConstants.TileSize);
        //    }
        //}
        //Handles.DrawWireCube(tile.TransformMaskPointToWorld(path.points[path.points.Count-1].position), Vector3.one);
        //Handles.color = prevColor;
    }
}
