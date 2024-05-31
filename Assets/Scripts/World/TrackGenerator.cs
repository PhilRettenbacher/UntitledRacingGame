using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrackGenerator
{
    int chunkSize;
    int maxMissSteps;
    int maxAttempts;
    int minDepth = 30;
    float minSubdivisionPathDistance = 15;

    List<Tile> tiles;
    Dictionary<TileConnectionType, List<Tile>> tileConnectionTable;
    /// <summary>
    /// 
    /// </summary>
    /// <param name="chunkSize">Sidelength of the chunk</param>
    /// <param name="maxAttempts">maximum retrys before failing</param>
    /// <param name="maxMissSteps">maximum wrong 'guesses' before restarting</param>
    public TrackGenerator(int chunkSize, int maxAttempts, int maxMissSteps, List<Tile> tiles)
    {
        this.chunkSize = chunkSize;
        this.maxMissSteps = maxMissSteps;
        this.maxAttempts = maxAttempts;
        this.tiles = tiles;

        GenerateTileConnectionTable();
    }

    /// <summary>
    /// Generates Track from entryTile to (when set) exitPosition, or otherwise to the specified exitDirections
    /// </summary>
    /// <param name="entryTile"></param>
    /// <param name="exitDirections"></param>
    /// <param name="exitPosition">The Tracks exit node should be here</param>
    /// <returns></returns>
    public GeneratedTrackResult GenerateTrack(GenerateTrackSettings genSettings)
    {
        int attempts = 0;

        bool success = false;

        GeneratedTrackResult res = null;


        while (attempts < maxAttempts && !success)
        {
            res = TryGenerateTrack(genSettings);
            if (res != null)
                success = true;
            
            attempts++;
        }

        if (success)
        {
            Debug.Log("Map generated, ends with: " + res.segments.Last().pos);
        }
        else
        {
            Debug.LogError($"Could not generate Track after {attempts} attempts!");
        }
        return res;
    }

    public GeneratedTrackResult TryGenerateTrack(GenerateTrackSettings genSettings)
    {   
        int missSteps = 0;
        bool[,] chunkGrid = genSettings.chunkGrid;

        List<TrackSegment> currStack = new List<TrackSegment>();

        System.Random random = new System.Random();

        if (!PlaceTile(chunkGrid, currStack, genSettings.entryTile, 0, genSettings, TileConnectionType.RoadStraight, random, ref missSteps))
        {
            return null;

        }
        return new GeneratedTrackResult(currStack,
            GenerateTrackPath(x => x.leftBorder, currStack, true),
            GenerateTrackPath(x => x.rightBorder, currStack, false),
            GenerateTrackPath(x => x.centerPath, currStack, false));
    }

    /// <summary>
    /// Generates Track from List of Tiles, backwards from exitTile
    /// </summary>
    /// <param name="tiles"></param>
    /// <param name="exitTile"></param>
    public GeneratedTrackResult GenerateTrackFromTileList(List<Tile> tiles, TilePosition entryPosition)
    {
        List<TrackSegment> currentStack = new List<TrackSegment>();

        TilePosition currentEntryPosition = entryPosition;
        for (int i = 0; i < tiles.Count; i++)
        {
            var nextTilePos = tiles[i].mask.FindNextTilePosition(currentEntryPosition);

            currentStack.Add(new TrackSegment() { tile = tiles[i], pos = currentEntryPosition, next = nextTilePos });

            currentEntryPosition = nextTilePos;
        }

        return new GeneratedTrackResult(currentStack,
            GenerateTrackPath(x => x.leftBorder, currentStack, true),
            GenerateTrackPath(x => x.rightBorder, currentStack, false),
            GenerateTrackPath(x => x.centerPath, currentStack, false));
    }

    public Path GenerateTrackPath(System.Func<Tile, Path> getPath, List<TrackSegment> track, bool normalFacesRight)
    {
        var points = track.SelectMany(x => GetTrackPoints(getPath, x)).ToList();

        points = points.Where((x, i) => i == 0 || i == points.Count - 1 || x.hasRadius).ToList(); //Remove straight line points

        Path path = new Path();
        path.points = points;
        path.normalFacesRight = normalFacesRight;
        path.RecalculatePoints();
        path.CalculateSubdivisionPoints(minSubdivisionPathDistance);

        return path;
    }

    private List<PathPoint> GetTrackPoints(System.Func<Tile, Path> getPath, TrackSegment segment)
    {
        var points = getPath(segment.tile).points;

        points = points.
            Select(x => GetTilePathPoint(x, segment)).ToList(); //Transform points to Chunk Space

        return points;

    }

    private PathPoint GetTilePathPoint(PathPoint point, TrackSegment segment)
    {
        point.position = segment.TransformTileToChunkPosition4(point.position).XZ();
        point.radius = point.calculatedRadius * WorldConstants.TileSize;
        return point;
    }

    public bool PlaceTile(bool[,] grid, List<TrackSegment> currStack, TilePosition entryPosition, int currDepth, GenerateTrackSettings genSettings, TileConnectionType connectionType, System.Random random, ref int missSteps)
    {
        
        var shuffledTiles = tileConnectionTable[connectionType].OrderByDescending(x => random.NextDouble() * x.probability).ToList();

        for (int i = 0; i < shuffledTiles.Count; i++)
        {
            if (missSteps > maxMissSteps)
                return false;


            if (shuffledTiles[i].mask.CanFillGrid(grid, entryPosition, out TilePosition nextPos))
            {
                //Exit Condition:
                bool canExit = false;
                if (nextPos.position.x < 0 || nextPos.position.y < 0 || nextPos.position.x >= grid.GetLength(0) || nextPos.position.y >= grid.GetLength(1))
                {
                    if (genSettings.exitDirections.Contains(nextPos.direction) && currDepth >= minDepth)
                    {
                        Debug.Log(nextPos);
                        //Exit
                        canExit = true;
                    }
                    else
                    {
                        //Wrong direction
                        missSteps++;
                        continue;
                    }
                }

                shuffledTiles[i].mask.FillGrid(grid, entryPosition, true);

                currStack.Add(new TrackSegment() { tile = shuffledTiles[i], pos = entryPosition, next = nextPos });

                if (canExit || PlaceTile(grid, currStack, nextPos, currDepth + 1, genSettings, shuffledTiles[i].exitConnectionType, random, ref missSteps))
                {
                    return true;
                }
                else
                {
                    currStack.RemoveAt(currStack.Count - 1);
                    missSteps++;
                    shuffledTiles[i].mask.FillGrid(grid, entryPosition, false);
                    continue;
                }
            }
        }
        missSteps++;
        return false;
    }

    private void GenerateTileConnectionTable()
    {
        tileConnectionTable = tiles.GroupBy(x => x.entryConnectionType).ToDictionary(x => x.Key, x => x.ToList());
    }
}

public struct TrackSegment
{
    public Tile tile;
    public TilePosition pos; //Entry Position
    public TilePosition next;

    [System.Obsolete]
    public Vector3 TransformTileToChunkPosition(Vector2 tilePos)
    {
        return ((Vector2)pos.position).X0Z() *
            WorldConstants.TileSize -
            Quaternion.Euler(0, pos.direction * 90, 0) *
            tile.TransformMaskPointToLocal(tilePos) + new Vector3(WorldConstants.TileSize, 0, WorldConstants.TileSize) / 2;
    }
    public Vector3 TransformTileToChunkPosition2(Vector2 tilePos)
    {
        return ((Vector2)pos.position).X0Z() *
            WorldConstants.TileSize -
            Quaternion.Euler(0, pos.direction * 90, 0) *
            (tile.TransformMaskPointToLocal(tile.mask.entryPosition.position) -
            tile.TransformMaskPointToLocal(tilePos)) +
            new Vector3(WorldConstants.TileSize, 0, WorldConstants.TileSize) / 2;
    }

    //Transforms Position in Tilespace (eg. entryposition) to chunk space (world space)
    public Vector3 TransformTileToChunkPosition3(Vector2 tilePos)
    {
        var entryVec = tile.TransformMaskPointToLocal(tilePos);

        TileDirection angle = TileDirection.GetAngle(tile.mask.entryPosition.direction, pos.direction);

        var rotatedEntryVec = TileDirection.RotateVector(angle, entryVec.XZ());

        return ((Vector2)pos.position).X0Z() *
            WorldConstants.TileSize - rotatedEntryVec.X0Z();
    }
    //Transforms Position in Tilespace (eg. entryposition) to chunk space (world space)
    public Vector3 TransformTileToChunkPosition4(Vector2 tilePos)
    {
        var entryVec = tile.TransformMaskPointToLocal(tile.mask.entryPosition.position) - tile.TransformMaskPointToLocal(tilePos);

        TileDirection angle = TileDirection.GetAngle(tile.mask.entryPosition.direction, pos.direction);

        var rotatedEntryVec = TileDirection.RotateVector(angle, entryVec.XZ());

        return ((Vector2)pos.position).X0Z() *
            WorldConstants.TileSize - rotatedEntryVec.X0Z();
    }
}
