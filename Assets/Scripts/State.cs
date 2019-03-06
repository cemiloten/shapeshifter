using System;
using UnityEngine;

public enum CellState
{
    Inactive = 0,
    Active
}

public class State
{
    private CellState[] cellStates;

    public static int Size { get => LevelManager.width * LevelManager.height; }

    public static Vector2Int IndexToPosition(int index)
    {
        return new Vector2Int(index % LevelManager.width, index / LevelManager.width);
    }

    public static int PositionToIndex(Vector2Int position)
    {
        if (!LevelManager.IsValidPosition(position))
            return -1;

        return position.x + LevelManager.width * position.y;
    }

    public static void Copy(State sourceState, ref State destState)
    {
        for (int i = 0; i < Size; ++i)
        {
            sourceState[i] = destState[i];
        }
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

    public State(State other)
    {
        cellStates = new CellState[LevelManager.width * LevelManager.height];
        for (int i = 0; i < Size; ++i)
        {
            cellStates[i] = other[i];
        }
    }

    public void Reset()
    {
        for (int i = 0; i < cellStates.Length; ++i)
        {
            cellStates[i] = CellState.Inactive;
        }
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < cellStates.Length; ++i)
            if (cellStates[i] == CellState.Active)
                return false;
        return true;
    }

    public bool Equals(State other)
    {
        if (other == null)
        {
            return false;
        }

        for (int i = 0; i < Size; ++i)
        {
            if (other[i] != cellStates[i])
            {
                return false;
            }
        }
        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return false;
        return obj as State == null ? false : Equals(obj);
    }

    public override int GetHashCode() { return base.GetHashCode(); }

    public static bool operator ==(State state1, State state2)
    {
        if ((object)state1 == null || (object)state2 == null)
            return object.Equals(state1, state2);

        return state1.Equals(state2);
    }

    public static bool operator !=(State state1, State state2)
    {
        if ((object)state1 == null || (object)state2 == null)
            return !object.Equals(state1, state2);

        return !(state1.Equals(state2));
    }

    public override string ToString()
    {
        string result = "";
        int index = Size - LevelManager.width;
        for (int i = 0; i < LevelManager.height; ++i)
        {
            for (int j = 0; j < LevelManager.width; ++j)
            {
                string s = cellStates[index + j] == CellState.Active ? "1  " : "0  ";
                result += s;
            }
            result += "\n";
            index -= LevelManager.width;
        }
        return result;
    }
}