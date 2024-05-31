using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileData))]
public class TileDataEditor : Editor
{
    private TileData tileData;

    private SerializedProperty m_mask;
    private SerializedProperty m_entryConnectionType;
    private SerializedProperty m_exitConnectionType;
    private SerializedProperty m_probability;

    private Vector3 midPoint { get => new Vector3(tileData.mask.grid.GetLength(0) - 1, 0, tileData.mask.grid.GetLength(1) - 1) / 2; }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        DrawMaskGUI();

        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        {
            EditorGUILayout.PropertyField(m_entryConnectionType);
            EditorGUILayout.PropertyField(m_exitConnectionType);
        }

        if (GUILayout.Button("Reset Paths"))
        {
            Undo.RecordObject(tileData, "Reset Paths");
            tileData.ResetPaths();
            SceneView.RepaintAll();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void OnSceneGUI(SceneView obj)
    {
        DrawMaskSceneGUI();

        DrawTilePositionSceneGUI(tileData.mask.entryPosition, Color.green);
        DrawTilePositionSceneGUI(tileData.mask.exitPosition, Color.red);

        DrawTilePathSceneGUI(tileData.rightBorder, Color.green, Color.gray);
        DrawTilePathSceneGUI(tileData.leftBorder, Color.green, Color.gray);
        DrawTilePathSceneGUI(tileData.centerPath, Color.blue, Color.gray);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUI;
        tileData = target as TileData;

        m_mask = serializedObject.FindProperty(nameof(TileData.mask));
        m_entryConnectionType = serializedObject.FindProperty(nameof(TileData.entryConnectionType));
        m_exitConnectionType = serializedObject.FindProperty(nameof(TileData.exitConnectionType));
        m_probability = serializedObject.FindProperty(nameof(TileData.probability));
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    void DrawMaskGUI()
    {
        using (new GUILayout.VerticalScope(EditorStyles.helpBox))
        using (new EditorGUI.IndentLevelScope())
        using (var changeCheckScope = new EditorGUI.ChangeCheckScope())
        {
            EditorGUILayout.PropertyField(m_mask);
            if (changeCheckScope.changed)
            {
                SceneView.RepaintAll();
            }
        }
    }

    void DrawMaskSceneGUI()
    {
        if (tileData.mask == null)
            return;

        for (int x = 0; x < tileData.mask.grid.GetLength(0); x++)
        {
            for (int y = 0; y < tileData.mask.grid.GetLength(1); y++)
            {
                if (tileData.mask.grid[x, y])
                    Handles.DrawWireCube((new Vector3(x, 0, y) - midPoint) * WorldConstants.TileSize, new Vector3(WorldConstants.TileSize, 0.1f, WorldConstants.TileSize));
            }
        }
    }
    void DrawTilePositionSceneGUI(TilePosition tilePos, Color color)
    {
        if (tileData.mask == null)
            return;
        var prevColor = Handles.color;

        Handles.color = color;

        Vector3 worldPoint = (((Vector2)tilePos.position).X0Z() - midPoint) * WorldConstants.TileSize;

        Handles.DrawWireCube(worldPoint, new Vector3(WorldConstants.TileSize * 0.75f, 0.1f, WorldConstants.TileSize * 0.75f));

        Handles.DrawLine(worldPoint, worldPoint + ((Vector2)tilePos.direction.ToVector()).X0Z() * WorldConstants.TileSize / 2f);

        Handles.color = prevColor;
    }
    void DrawTilePathSceneGUI(Path path, Color color, Color secondary)
    {
        if (tileData.mask == null)
            return;

        EditorUtils.DrawPointPath(path, -midPoint * WorldConstants.TileSize, color, secondary, WorldConstants.TileSize);
    }
}
