using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TileMeshGenerator))]
public class TileMeshGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var generator = target as TileMeshGenerator;

        if(GUILayout.Button("Generate Tile Mesh"))
        {
            Undo.RecordObject(generator, "Generate track");
            generator.GenerateTileMesh();
        }
        if (GUILayout.Button("Clear Tile Mesh"))
        {
            Undo.RecordObject(generator, "Clear track");
            generator.DeleteTileMesh();
        }
    }
}
