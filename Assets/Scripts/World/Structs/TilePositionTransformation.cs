using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TilePositionTransformation
{
    TilePosition global;
    TilePosition local;

    Vector2Int xMat;
    Vector2Int yMat;

    int rotation;
    public TilePositionTransformation(TilePosition global, TilePosition local)
    {
        this.global = global;
        this.local = local;

        rotation = TileDirection.GetAngle(local.direction, global.direction);

        int sign = rotation < 2 ? 1 : -1;

        xMat = new Vector2Int(rotation % 2 == 0 ? sign : 0, rotation % 2 == 1 ? -sign : 0);
        yMat = new Vector2Int(rotation % 2 == 1 ? sign : 0, rotation % 2 == 0 ? sign : 0);
    }

    /// <summary>
    /// Transforms local Tileposition to global
    /// </summary>
    /// <param name="localPoint"></param>
    /// <returns></returns>
    public TilePosition TransformTilePosition(TilePosition localPoint)
    {
        return new TilePosition(TransformPoint(localPoint.position), TransformDirection(localPoint.direction));
    }

    /// <summary>
    /// Transforms global Tileposition to local
    /// </summary>
    /// <param name="globalPoint"></param>
    /// <returns></returns>
    public TilePosition InverseTransformTilePosition(TilePosition globalPoint)
    {
        return new TilePosition(InverseTransformPoint(globalPoint.position), InverseTransformDirection(globalPoint.direction));
    }

    /// <summary>
    /// Transforms local Point to global
    /// </summary>
    /// <param name="localPoint"></param>
    /// <returns></returns>
    public Vector2Int TransformPoint(Vector2Int localPoint)
    {
        Vector2Int relativePos = localPoint - local.position;
        return global.position + relativePos.x * xMat + relativePos.y * yMat;
    }

    /// <summary>
    /// Transforms global Point to local
    /// </summary>
    /// <param name="globalPoint"></param>
    /// <returns></returns>
    public Vector2Int InverseTransformPoint(Vector2Int globalPoint)
    {
        var pos = globalPoint - global.position;
        return -pos.x * xMat + -pos.y * yMat + local.position;
    }

    /// <summary>
    /// Transforms local Direction to global
    /// </summary>
    /// <param name="localDir"></param>
    /// <returns></returns>
    public TileDirection TransformDirection(TileDirection localDir)
    {
        return localDir.Turn(rotation);
    }

    /// <summary>
    /// Transforms global Direction to local
    /// </summary>
    /// <param name="globalDir"></param>
    /// <returns></returns>
    public TileDirection InverseTransformDirection(TileDirection globalDir)
    {
        return globalDir.Turn(-rotation);
    }
}
