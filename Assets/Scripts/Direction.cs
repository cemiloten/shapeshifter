using System.Collections.Generic;
using UnityEngine;

public enum Direction
{
    None = 0,
    Right,
    Up,
    Left,
    Down,
    UpRight,
    UpLeft,
    DownRight,
    DownLeft
}

public static class DirectionMethods
{
    public static readonly Dictionary<Direction, Vector2> DirectionToVector2 = new Dictionary<Direction, Vector2>()
    {
        { Direction.Right,     new Vector2( 1f,  0f) },
        { Direction.Up,        new Vector2( 0f,  1f) },
        { Direction.Left,      new Vector2(-1f,  0f) },
        { Direction.Down,      new Vector2( 0f, -1f) },
        { Direction.UpRight,   new Vector2( 0.707107f,  0.707107f) },
        { Direction.UpLeft,    new Vector2(-0.707107f,  0.707107f) },
        { Direction.DownRight, new Vector2( 0.707107f, -0.707107f) },
        { Direction.DownLeft,  new Vector2(-0.707107f, -0.707107f) }

    };

    private static readonly Dictionary<Direction, Vector2Int> DirectionToVector2Int = new Dictionary<Direction, Vector2Int>()
    {
        { Direction.Right,     new Vector2Int( 1,  0) },
        { Direction.Up,        new Vector2Int( 0,  1) },
        { Direction.Left,      new Vector2Int(-1,  0) },
        { Direction.Down,      new Vector2Int( 0, -1) },
        { Direction.UpRight,   new Vector2Int( 1,  1) },
        { Direction.UpLeft,    new Vector2Int(-1,  1) },
        { Direction.DownRight, new Vector2Int( 1, -1) },
        { Direction.DownLeft,  new Vector2Int(-1, -1) },
    };

    public static Vector2Int ToVector2Int(Direction dir)
    {
        return DirectionToVector2Int[dir];
    }

    public static Direction ToDirection(Vector2 vector)
    {
        Direction result = Direction.None;
        float dot = 0f;
        foreach (var dirToVector2 in DirectionToVector2)
        {
            float d = Vector2.Dot(vector.normalized, dirToVector2.Value);
            if (d > dot)
            {
                dot = d;
                result = dirToVector2.Key;
            }

            if (dot > 0.95f)
                break;
        }

        return result;
    }
}
