using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[SelectionBase]
public class Tile : MonoBehaviour
{
    [HideInInspector]
    public TileMask mask = new TileMask();
    [Range(0.1f, 20)]
    public float probability = 1f;
    [SerializeField]
    public float modelTileScale = 1f;
    [SerializeField]
    public Path rightBorder;
    [SerializeField]
    public Path leftBorder;
    [SerializeField]
    public Path centerPath;

    [SerializeField]
    public TileConnectionType entryConnectionType;
    [SerializeField]
    public TileConnectionType exitConnectionType;

    public TileConnection entryConnection { get => TileConnection.GetTileConnection(entryConnectionType); }
    public TileConnection exitConnection { get => TileConnection.GetTileConnection(exitConnectionType); }

    public Vector2 entryDirection { get => ((Vector2)mask.entryPosition.direction.ToVector()).Rotate(entryConnection.angle); }
    public Vector2 exitDirection { get => ((Vector2)mask.exitPosition.direction.ToVector()).Rotate(exitConnection.angle); }

    public Vector2 localCenter { get => new Vector2((mask.grid.GetLength(0) - 1) / 2f, (mask.grid.GetLength(1) - 1) / 2f); } //Center/Origin Position in Tilespace

    // Start is called before the first frame update
    void Start()
    {
        ScaleChildren();
    }

    public Vector3 TransformMaskPointToWorld(Vector2 pos)
    {
        Vector2 localPos = pos - localCenter;
        return transform.TransformPoint(new Vector3(localPos.x, 0, localPos.y) * WorldConstants.TileSize);
    }

    public Path TransformMaskPathToWorld(Path path)
    {
        var result = new Path();
        result.points = path.points.Select(x => TransformMaskPathPointToWorld(x)).ToList();
        result.RecalculatePoints();
        return result;
    }
    public PathPoint TransformMaskPathPointToWorld(PathPoint point)
    {
        return new PathPoint()
        {
            position = TransformMaskPointToWorld(point.position).XZ(),
            radius = point.radius * WorldConstants.TileSize,
            hasRadius = point.hasRadius
        };
    }

    //Transforms point in Tilespace to local space
    public Vector3 TransformMaskPointToLocal(Vector2 pos)
    {
        Vector2 localPos = pos - localCenter;
        return new Vector3(localPos.x, 0, localPos.y) * WorldConstants.TileSize;
    }

    public void ResetPaths()
    {
        List<PathPoint> centerPoints = new List<PathPoint>();
        List<PathPoint> rightPoints = new List<PathPoint>();
        List<PathPoint> leftPoints = new List<PathPoint>();

        Vector2 startPos = mask.entryPosition.position - ((Vector2)mask.entryPosition.direction.ToVector()) * 0.5f;
        Vector2 rightDir = mask.entryPosition.direction.Turn(1).ToVector();

        centerPoints.Add(new PathPoint() { position = startPos });
        rightPoints.Add(new PathPoint() { position = startPos + rightDir * entryConnection.rightBorderDistance });
        leftPoints.Add(new PathPoint() { position = startPos - rightDir * entryConnection.leftBorderDistance });

        Vector2 exitPos = mask.exitPosition.position + ((Vector2)mask.exitPosition.direction.ToVector()) * 0.5f;
        Vector2 exitDir = mask.exitPosition.direction.Turn(1).ToVector();

        centerPoints.Add(new PathPoint() { position = exitPos });
        rightPoints.Add(new PathPoint() { position = exitPos + exitDir * exitConnection.rightBorderDistance });
        leftPoints.Add(new PathPoint() { position = exitPos - exitDir * exitConnection.leftBorderDistance });

        centerPath.points = centerPoints;
        rightBorder.points = rightPoints;
        leftBorder.points = leftPoints;
    }

    public void ScaleChildren()
    {
        if (modelTileScale != 0)
        {
            foreach (Transform child in transform)
            {
                child.localScale = Vector3.one * WorldConstants.TileSize / modelTileScale;
            }
        }
    }
}
