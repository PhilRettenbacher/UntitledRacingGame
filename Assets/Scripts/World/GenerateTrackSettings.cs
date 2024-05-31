using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTrackSettings
{
    public TilePosition entryTile;
    public TileDirection[] exitDirections;
    public TilePosition exitPosition;
    public bool[,] chunkGrid;

    public GenerateTrackSettings(TilePosition entryTile, TileDirection[] exitDirections, int chunkSize)
    {
        this.entryTile = entryTile;
        this.exitDirections = exitDirections;
        chunkGrid = new bool[chunkSize, chunkSize];
    }

    public GenerateTrackSettings(TilePosition entryTile, TilePosition exitPosition, bool[,] chunkGrid)
    {
        this.entryTile = entryTile;
        this.exitPosition = exitPosition;
        this.chunkGrid = chunkGrid;
    }
}
