using System;
using UnityEngine;

public enum CellState
{
    Inactive = 0,
    Active
}

public class State
{
    public CellState[] cellStates;

    public static int Size { get => LevelManager.width * LevelManager.height; }

    public static Vector2Int IndexToPosition(int index)
    {
        return new Vector2Int(index % LevelManager.width, index / LevelManager.width);
    }

    public static int PositionToIndex(Vector2Int position)
    {
        return position.x + LevelManager.width * position.y;
    }

    public CellState this[int i]
    {
        get => cellStates[i];
        set => cellStates[i] = value;
    }


    public State()
    {
        cellStates = new CellState[LevelManager.width * LevelManager.height];
    }

    private bool IsEmpty()
    {
        for (int i = 0; i < cellStates.Length; ++i)
            if (cellStates[i] == CellState.Active)
                return false;
        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
            return false;

        State other = (State)obj;
        for (int i = 0; i < LevelManager.width * LevelManager.height; ++i)
        {
            if (other.cellStates[i] != cellStates[i])
                return false;
        }
        return true;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}