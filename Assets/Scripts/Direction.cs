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

public static class __ExtensionMethodsDirection
{
    public static Vector2Int ToVector2Int(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Right     : return new Vector2Int( 1,  0);
            case Direction.Up        : return new Vector2Int( 0,  1);
            case Direction.Left      : return new Vector2Int(-1,  0);
            case Direction.Down      : return new Vector2Int( 0, -1);
            case Direction.UpRight   : return new Vector2Int( 1,  1);
            case Direction.UpLeft    : return new Vector2Int(-1,  1);
            case Direction.DownRight : return new Vector2Int( 1, -1);
            case Direction.DownLeft  : return new Vector2Int(-1, -1);
            default:
                Debug.LogErrorFormat("Direction {0} not implemented", direction);
                return new Vector2Int(0, 0);
        }
    }

    public static Vector2 ToVector2(this Direction direction)
    {
        switch (direction)
        {
            case Direction.Right     : return new Vector2( 1f,  0f);
            case Direction.Up        : return new Vector2( 0f,  1f);
            case Direction.Left      : return new Vector2(-1f,  0f);
            case Direction.Down      : return new Vector2( 0f, -1f);
            case Direction.UpRight   : return new Vector2( 0.707107f,  0.707107f);
            case Direction.UpLeft    : return new Vector2(-0.707107f,  0.707107f);
            case Direction.DownRight : return new Vector2( 0.707107f, -0.707107f);
            case Direction.DownLeft  : return new Vector2(-0.707107f, -0.707107f);
            case Direction.None:
            default:
                return new Vector2(0f, 0f);
        }
    }
}