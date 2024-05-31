using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct TileDirection : IEquatable<TileDirection>
{
    public static readonly string[] names = new[] { "North", "East", "South", "West" };

    [SerializeField]
    private int value;
    public TileDirection(int value)
    {
        this.value = value;
    }

    public static TileDirection North { get => new TileDirection(0); }
    public static TileDirection East { get => new TileDirection(1); }
    public static TileDirection South { get => new TileDirection(2); }
    public static TileDirection West { get => new TileDirection(3); }

    public TileDirection Opposite { get => new TileDirection((value + 2) % 4); }

    /// <summary>
    /// Rotates the direction in clockwise direction by the amount of steps.
    /// </summary>
    /// <param name="rotations">Amount of steps</param>
    /// <returns></returns>
    public TileDirection Turn(int rotations) 
    {
        int fullValue = value + rotations;

        return new TileDirection(((fullValue % 4) + 4) % 4);
    }

    public Vector2Int ToVector()
    {
        return new Vector2Int((2 - value) % 2, (1 - value) % 2);
    }
    
    /// <summary>
    /// Returns the rotation between 2 Directions in clockwise direction.
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <returns></returns>
    public static int GetAngle(TileDirection from, TileDirection to)
    {
        int value = to.value - from.value;
        return (value + 4) % 4;
    }

    public static Vector2 RotateVector(TileDirection rotation, Vector2 vec)
    {
        switch (rotation)
        {
            case 0: //North
                return vec;
            case 1: //East
                return new Vector2(vec.y, -vec.x);
            case 2: //South
                return -vec;
            case 3: //West
                return new Vector2(-vec.y, vec.x);
        }
        return Vector2.zero;
    }

    public override string ToString()
    {
        return names[value];
    }
    public override bool Equals(object obj)
    {
        if (!(obj is TileDirection))
            return false;

        return Equals((TileDirection)obj);
    }
    public override int GetHashCode()
    {
        return value.GetHashCode();
    }
    public bool Equals(TileDirection other)
    {
        return other.value == value;
    }
    public static implicit operator int(TileDirection dir)
    {
        return dir.value;
    }
    public static implicit operator TileDirection(int dir)
    {
        return new TileDirection(dir % 4);
    }
    public static bool operator ==(TileDirection lhs, TileDirection rhs)
    {
        return lhs.Equals(rhs);
    }
    public static bool operator !=(TileDirection lhs, TileDirection rhs)
    {
        return !lhs.Equals(rhs);
    }
}
