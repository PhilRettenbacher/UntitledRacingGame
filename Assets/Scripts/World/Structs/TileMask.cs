using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileMask : ISerializationCallbackReceiver
{
    public bool[,] grid;

    [SerializeField, HideInInspector]
    private bool[] serializableGrid = new bool[0];
    [SerializeField, HideInInspector]
    private int maskX;

    public TilePosition entryPosition;
    public TilePosition exitPosition;

    public TileMask()
    {
        this.grid = new bool[5, 1];
        this.entryPosition = new TilePosition();
        this.exitPosition = new TilePosition();
    }
    public TileMask(bool[,] grid, TilePosition entryPosition, TilePosition exitPosition)
    {
        this.grid = grid;
        this.entryPosition = entryPosition;
        this.exitPosition = exitPosition;
    }

    public TilePosition FindNextTilePosition(TilePosition gridEntryPosition)
    {
        TilePositionTransformation trans = new TilePositionTransformation(gridEntryPosition, entryPosition);

        return trans.TransformTilePosition(new TilePosition(exitPosition.position + exitPosition.direction.ToVector(), exitPosition.direction));
    }

    /// <summary>
    /// Checks if this mask fits on a point in the grid.
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="gridEntryPosition">The global position of the local entryPosition</param>
    /// <returns></returns>
    public bool CanFillGrid(bool[,] grid, TilePosition gridEntryPosition, out TilePosition nextTilePosition)
    {
        TilePositionTransformation trans = new TilePositionTransformation(gridEntryPosition, entryPosition);

        nextTilePosition = FindNextTilePosition(gridEntryPosition);

        //Bounds Check
        if (!FitsInBounds(new Vector2Int(grid.GetLength(0), grid.GetLength(1)), trans))
            return false;

        //Collision Check
        for(int x = 0; x< this.grid.GetLength(0); x++)
        {
            for (int y = 0; y< this.grid.GetLength(1); y++)
            {
                if(this.grid[x, y])
                {
                    var transPoint = trans.TransformPoint(new Vector2Int(x, y));

                    if (grid[transPoint.x, transPoint.y])
                        return false;
                }
            }
        }

        return true;
    }

    public bool FitsInBounds(Vector2Int gridSize, TilePositionTransformation trans)
    {
        Vector2Int v1 = trans.TransformPoint(Vector2Int.zero);
        Vector2Int v2 = trans.TransformPoint(new Vector2Int(grid.GetLength(0) - 1, grid.GetLength(1) - 1));

        Vector2Int min = Vector2Int.Min(v1, v2);
        Vector2Int max = Vector2Int.Max(v1, v2);

        if (Vector2Int.Max(min, Vector2Int.zero) != min || Vector2Int.Min(max, new Vector2Int(gridSize.x-1, gridSize.y-1)) != max)
            return false;
        return true;
    }

    public void FillGrid(bool[,] grid, TilePosition gridEntryPosition, bool value)
    {
        TilePositionTransformation trans = new TilePositionTransformation(gridEntryPosition, entryPosition);

        for (int x = 0; x < this.grid.GetLength(0); x++)
        {
            for (int y = 0; y < this.grid.GetLength(1); y++)
            {
                if (this.grid[x, y])
                {
                    var transPoint = trans.TransformPoint(new Vector2Int(x, y));

                    grid[transPoint.x, transPoint.y] = value;
                }
            }
        }
    }

    public void OnBeforeSerialize()
    {
        if (grid == null)
        {
            serializableGrid = null;
            return;
        }
        maskX = grid.GetLength(0);

        if (serializableGrid.Length != grid.Length)
            serializableGrid = new bool[grid.Length];


        for(int i = 0; i< serializableGrid.Length; i++)
        {
            serializableGrid[i] = grid[i % maskX, i / maskX];
        }
    }

    public void OnAfterDeserialize()
    {
        if (serializableGrid == null)
        {
            grid = null;
            return;
        }

        grid = new bool[maskX, serializableGrid.Length != 0 ? serializableGrid.Length / maskX : 0];
        for (int i = 0; i < serializableGrid.Length; i++)
        {
            grid[i % maskX, i / maskX] = serializableGrid[i];
        }
    }
}