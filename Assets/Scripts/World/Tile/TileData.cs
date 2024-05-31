using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile", menuName = "Track/Tile")]
public class TileData : ScriptableObject
{
    public TileMask mask = new TileMask();

    public GameObject model;
    [Range(0.1f, 20)]
    public float probability = 1f;

    public Path rightBorder;
    public Path leftBorder;
    public Path centerPath;
    public TileConnectionType entryConnectionType;
    public TileConnectionType exitConnectionType;

    public TileConnection entryConnection { get => TileConnection.GetTileConnection(entryConnectionType); }
    public TileConnection exitConnection { get => TileConnection.GetTileConnection(exitConnectionType); }

    public Vector2 entryDirection { get => ((Vector2)mask.entryPosition.direction.ToVector()).Rotate(entryConnection.angle); }
    public Vector2 exitDirection { get => ((Vector2)mask.exitPosition.direction.ToVector()).Rotate(exitConnection.angle); }

    public void ResetPaths()
    {
        Debug.Log(entryDirection);
        Debug.Log(exitDirection);

        Vector2 entryPosition = mask.entryPosition.position - ((Vector2)mask.entryPosition.direction.ToVector()) * 0.5f;
        Vector2 entryNormalDirection = mask.entryPosition.direction.Turn(1).ToVector();
        Vector2 exitPosition = mask.exitPosition.position + ((Vector2)mask.exitPosition.direction.ToVector()) * 0.5f;
        Vector2 exitNormalDirection = mask.exitPosition.direction.Turn(1).ToVector();

        rightBorder = new Path(entryPosition + entryNormalDirection * entryConnection.rightBorderDistance,
            entryDirection,
            exitPosition + exitNormalDirection * exitConnection.rightBorderDistance,
            exitDirection);

        leftBorder = new Path(entryPosition - entryNormalDirection * entryConnection.leftBorderDistance,
            entryDirection,
            exitPosition - exitNormalDirection * exitConnection.leftBorderDistance,
            exitDirection);

        centerPath = new Path(entryPosition, entryDirection, exitPosition, exitDirection);
    }

    void Reset()
    {
        ResetPaths();
    }
}
