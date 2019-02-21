using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShiftStyle
{
    None = 0,
    Move,
    Grow
}

public class ShapeShifter : MonoBehaviour
{
    public GameObject blockPrefab;
    public bool register = true;

    // public float shiftDuration = 0.25f;
    // public Vector3 maxScale = new Vector3(0.9f, 0.9f, 0.2f);
    // public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

    public delegate void OnShiftHandler();
    public event OnShiftHandler OnShift;

    private State currState;
    private State nextState;
    private Block[] blocks;

    // private bool shifting = false;
    // private float shiftTimer = 0f;

    private ShiftStyle shiftStyle = ShiftStyle.Move;

    public State State
    {
        get => currState;
        set
        {
            currState = value;
            ShowCurrentState();
        }
    }

    private void OnEnable()
    {
        if (register)
            TouchController.OnSwipe += OnSwipe;
    }

    private void OnDisable()
    {
        if (register)
            TouchController.OnSwipe -= OnSwipe;
    }

    private void OnSwipe(Direction direction, ShiftStyle style)
    {
        shiftStyle = style;
        Shift(direction);

        if (OnShift != null)
        {
            OnShift();
        }
    }

    private void Start()
    {
        currState = new State();
        nextState = new State();
        currState[12] = CellState.Active;
        PrepareBlocks();
        ShowCurrentState();
    }

    private void PrepareBlocks()
    {
        blocks = new Block[LevelManager.width * LevelManager.height];
        for (int y = 0; y < LevelManager.height; ++y)
        {
            for (int x = 0; x < LevelManager.width; ++x)
            {
                GameObject go = Instantiate(blockPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                go.transform.parent = transform;
                Block block = go.GetComponent<Block>();
                block.Position = new Vector2Int(x, y);
                blocks[x + y * LevelManager.width] = block;
            }
        }
    }

    void Update()
    {
        // if (shifting)
        // {
        //     UpdateShape();
        //     return;
        // }


        // if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Space))
        // {
        //     StartShifting();
        // }
    }

    // private void StartShifting()
    // {
    //     if (shifting)
    //         return;

    //     for (int i = 0; i < nextShape.Length; ++i)
    //     {
    //         if (nextShape[i])
    //             blocks[i].Active = true;
    //     }
    //     shifting = true;
    // }

    // private void EndShifting()
    // {
    //     shifting = false;
    //     Array.Clear(currentShape, 0, currentShape.Length);
    //     shiftTimer -= shiftDuration;
    //     currentShape = nextShape;
    //     SetBlocksToCurrentShape();
    //     Array.Clear(nextShape, 0, nextShape.Length);
    // }

    private State EmptyToDirection(Direction direction)
    {
        State state = new State();
        if (direction == Direction.Right)
        {
            for (int y = 0; y < LevelManager.height; ++y)
            {
                int index = State.PositionToIndex(new Vector2Int(0, y));
                state[index]  = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(1, LevelManager.height / 2))]  = CellState.Active;
        }
        else if (direction == Direction.Up)
        {
            for (int x = 0; x < LevelManager.width; ++x)
            {
                int index = State.PositionToIndex(new Vector2Int(x, 0));
                state[index]  = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(LevelManager.width / 2, 1))]  = CellState.Active;
        }
        else if (direction == Direction.Left)
        {
            for (int y = 0; y < LevelManager.height; ++y)
            {
                int index = State.PositionToIndex(new Vector2Int(LevelManager.width - 1, y));
                state[index]  = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(LevelManager.width - 2, LevelManager.height / 2))]  = CellState.Active;
        }
        else if (direction == Direction.Down)
        {
            for (int x = 0; x < LevelManager.width; ++x)
            {
                int index = State.PositionToIndex(new Vector2Int(x, LevelManager.height - 1));
                state[index]  = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(LevelManager.width / 2, LevelManager.height - 2))]  = CellState.Active;
        }
        return state;
    }

    void Shift(Direction direction)
    {
        nextState.Reset();

        if (currState.IsEmpty())
        {
            nextState = EmptyToDirection(direction);
        }
        else
        {
            for (int i = 0; i < State.Size; ++i)
            {
                if (currState[i] == CellState.Active)
                    SetCellStateToNext(i, direction);
            }
        }
        currState = new State(nextState);
        ShowCurrentState();

        if (OnShift != null)
            OnShift();
    }

    private void ShowCurrentState()
    {
        for (int i = 0; i < State.Size; ++i)
        {
            if (currState[i] == CellState.Active)
                blocks[i].gameObject.SetActive(true);
            else if (currState[i] == CellState.Inactive)
                blocks[i].gameObject.SetActive(false);
        }
    }

    // void UpdateShape()
    // {
    //     float t = shiftTimer / shiftDuration;
    //     for (int i = 0; i < blocks.Length; ++i)
    //     {
    //         if (currentShape[i])
    //         {
    //             if (!nextShape[i])
    //                 blocks[i].transform.localScale = Vector3.Lerp(maxScale, minScale, t);
    //             else
    //                 blocks[i].transform.localScale = maxScale;
    //         }
    //         else if (nextShape[i])
    //         {
    //             blocks[i].transform.localScale = Vector3.Lerp(minScale, maxScale, t);
    //         }
    //     }

    //     if (shiftTimer < shiftDuration)
    //         shiftTimer += Time.deltaTime;
    //     else
    //     {
    //         EndShifting();
    //     }
    // }

    void SetCellStateToNext(int currIndex, Direction direction)
    {
        if (nextState[currIndex] == CellState.Inactive)
        {
            if (shiftStyle == ShiftStyle.Move)
                nextState[currIndex] = CellState.Inactive;
            else if (shiftStyle == ShiftStyle.Grow)
                nextState[currIndex] = CellState.Active;
        }

        Vector2Int currPosition = State.IndexToPosition(currIndex);
        Vector2Int directionVector = DirectionMethods.ToVector2Int(direction);
        int nextIndex = State.PositionToIndex(currPosition + directionVector);
        if (nextIndex < 0)
        {
            if (shiftStyle == ShiftStyle.Grow)
            {
                int x = (currPosition.x + directionVector.x) % LevelManager.width;
                if (x < 0)
                    x = LevelManager.width + x;
                int y = (currPosition.y + directionVector.y) % LevelManager.height;
                if (y < 0)
                    y = LevelManager.width + y;
                Vector2Int newPos = new Vector2Int(x, y);
                nextState[State.PositionToIndex(newPos)] = CellState.Active;
            }
        }
        else
            nextState[nextIndex] = CellState.Active;
    }

}
