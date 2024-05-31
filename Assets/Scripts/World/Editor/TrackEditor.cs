using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Track))]
public class TrackEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var track = target as Track;

        if(GUILayout.Button("Generate Track"))
        {
            Undo.RecordObject(track, "Generate track");
            track.Generate();
        }
        if (GUILayout.Button("Clear Track"))
        {
            Undo.RecordObject(track, "Clear track");
            track.ClearTrack();
        }
    }

    public void OnSceneGUI()
    {
        var track = target as Track;

        foreach(var chunk in track.chunks)
        {
            DrawChunkSceneGUI(track, chunk);
        }
    }

    void DrawChunkSceneGUI(Track track, Chunk chunk)
    {
        Handles.DrawWireCube(chunk.gameObject.transform.TransformPoint(new Vector3(0.5f, 0, 0.5f) * track.chunkSize * WorldConstants.TileSize) - new Vector3(0.5f, 0, 0.5f) * WorldConstants.TileSize, new Vector3(track.chunkSize * WorldConstants.TileSize, 1, track.chunkSize * WorldConstants.TileSize));

        EditorUtils.DrawPointPath(chunk.leftBorder, chunk.transform.position, Color.red, Color.gray);
        EditorUtils.DrawPointPath(chunk.rightBorder, chunk.transform.position, Color.blue, Color.gray);
        EditorUtils.DrawPointPath(chunk.centerPath, chunk.transform.position, Color.green, Color.gray);

        EditorUtils.DrawPathSubdivisions(chunk.centerPath, chunk.transform.position, Color.gray, Color.gray);
    }
}
