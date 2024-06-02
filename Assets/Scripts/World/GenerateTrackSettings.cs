using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateTrackSettings
{
    public TilePosition entryTilePosition;
    public TileDirection exitDirection;
    public TilePosition exitPosition;
    public bool useExitPosition;
    public bool[,] chunkGrid;
    public List<Tile> startingTiles;

    public GenerateTrackSettings(TilePosition entryTile, TileDirection exitDirection, int chunkSize, List<Tile> startingTiles = null)
    {
        this.entryTilePosition = entryTile;
        this.exitDirection = exitDirection;
        chunkGrid = new bool[chunkSize, chunkSize];
        this.startingTiles = startingTiles;
        useExitPosition = false;
    }

    public GenerateTrackSettings(TilePosition entryTile, TilePosition exitPosition, int chunkSize, List<Tile> startingTiles = null)
    {
        this.entryTilePosition = entryTile;
        this.exitPosition = exitPosition;
        chunkGrid = new bool[chunkSize, chunkSize];
        this.startingTiles = startingTiles;
        useExitPosition = true;
    }
}
