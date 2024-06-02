using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TilePosition : IEquatable<TilePosition>
{
    public Vector2Int position;
    public TileDirection direction;
    public TilePosition(Vector2Int position, TileDirection direction)
    {
        this.position = position;
        this.direction = direction;
    }

    /// <summary>
    /// Gets the next tileposition in the direction of this tilePosition with the same direction
    /// </summary>
    /// <returns></returns>
    public TilePosition NextInDirection(int steps = 1)
    {
        return new TilePosition(position + direction.ToVector() * steps, direction);
    }
    public override string ToString()
    {
        return position.ToString() + " " + direction.ToString();
    }
    public override bool Equals(object obj)
    {
        if (!(obj is TilePosition))
            return false;

        return Equals((TilePosition)obj);
    }
    public bool Equals(TilePosition other)
    {
        return other.direction == direction && other.position == position;
    }
    public override int GetHashCode()
    {
        return position.GetHashCode() ^ direction.GetHashCode();
    }
    public static bool operator ==(TilePosition lhs, TilePosition rhs)
    {
        return lhs.Equals(rhs);
    }
    public static bool operator !=(TilePosition lhs, TilePosition rhs)
    {
        return !lhs.Equals(rhs);
    }
}
