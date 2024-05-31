using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Chunk : MonoBehaviour
{
    public List<Tile> track;
    public Vector2Int offset;
    public TilePosition nextTilePosition;
    public Path rightBorder;
    public Path leftBorder;
    public Path centerPath;
    public Bounds bounds;

    public float centerPathStartingDistance;
    public float centerPathEndDistance { get => centerPath.distance + centerPathStartingDistance; }

    public void Clear()
    {
        if (Application.isEditor)
            UnityEngine.Object.DestroyImmediate(gameObject);
        else
            UnityEngine.Object.Destroy(gameObject);
    }

    public Vector3 GetPositionByDistance(float distance) //Full Track Distance
    {
        if (distance < centerPathStartingDistance || distance > centerPathEndDistance)
        {
            Debug.LogError("Parameter (distance) outside of chunk Range!");
            return Vector3.zero;
        }
        return GetPositionByChunkDistance(distance - centerPathStartingDistance);
    }
    public Vector3 GetPositionByChunkDistance(float chunkDistance)
    {
        return transform.TransformPoint(centerPath.GetPositionByDistance(chunkDistance).X0Z());
    }
}
