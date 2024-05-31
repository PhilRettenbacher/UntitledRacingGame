using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct TileConnection
{
    public readonly TileConnectionType type;
    public readonly float rightBorderDistance;
    public readonly float leftBorderDistance;
    public readonly float angle;

    public static TileConnection GetTileConnection(TileConnectionType type)
    {
        switch (type)
        {
            case TileConnectionType.RoadStraight:
                return new TileConnection(type, 2.3f, 2.3f, 0);
            case TileConnectionType.RoadAngleRight:
                return new TileConnection(type, 3.25f, 3.25f, -45);
            case TileConnectionType.RoadAngleLeft:
                return new TileConnection(type, 3.25f, 3.25f, 45);
            case TileConnectionType.RoadBankedRight:
                return new TileConnection(type, 2.3f, 2.3f, 0);
            case TileConnectionType.RoadBankedLeft:
                return new TileConnection(type, 2.3f, 2.3f, 0);
        }
        return new TileConnection(type, 0, 0, 0);
    }

    private TileConnection (TileConnectionType type, float rbd, float lbd, float a)
    {
        this.type = type;
        rightBorderDistance = rbd;
        leftBorderDistance = lbd;
        angle = a;
    }

}

public enum TileConnectionType
{
    RoadStraight,
    RoadAngleRight,
    RoadAngleLeft,
    RoadBankedRight,
    RoadBankedLeft
}
