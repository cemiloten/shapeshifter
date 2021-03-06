﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShiftStyle
{
    None = 0,
    Move,
    Grow
}

public class Shifter : MonoBehaviour
{
    private const float minTimeBetweenTouches = 0.2f;
    private const int maxTouchCount = 2;

    public GameObject blockPrefab;
    public bool registered = true;
    public float timeSinceTouchEnd = 0f; // change to private when not needed anymore

    public delegate void OnShiftHandler();
    public event OnShiftHandler OnShift;

    private State currState;
    private State nextState;
    private Block[] blocks;
    private ShiftStyle shiftStyle = ShiftStyle.Move;
    private Vector2 _swipeStart;
    private Vector2 _swipeEnd;
    private TouchInfo touchInfo;

    public ShiftStyle Style { get => shiftStyle; }

    public State State
    {
        get => currState;
        set
        {
            currState = value;
            ShowState(currState);
        }
    }

    private float MinSwipeDistance
    {
        get => 0.10f * Mathf.Min(Screen.width, Screen.height);
    }

    private Vector2 SwipeStart
    {
        get => _swipeStart;
        set
        {
            _swipeStart = value;
            GameManager.Instance.swipeStartSprite.transform.position = value;
        }
    }

    private Vector2 SwipeEnd
    {
        get => _swipeEnd;
        set
        {
            _swipeEnd = value;
            GameManager.Instance.potentialSwipeEndSprite.transform.position = value;
        }
    }

    private void OnEnable()
    {
        if (registered)
        {
            TouchManager.OnTouchStart += OnTouchStart;
            TouchManager.OnTouchHold += OnTouchHold;
            TouchManager.OnTouchMove += OnTouchMove;
            TouchManager.OnTouchEnd += OnTouchEnd;
        }
    }

    private void OnDisable()
    {
        if (registered)
        {
            TouchManager.OnTouchStart -= OnTouchStart;
            TouchManager.OnTouchHold -= OnTouchHold;
            TouchManager.OnTouchMove -= OnTouchMove;
            TouchManager.OnTouchEnd -= OnTouchEnd;
        }
    }

    private void Awake()
    {
        currState = new State();
        nextState = new State();
        currState[12] = CellState.Active;
        PrepareBlocks();
        ShowState(currState);
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

    private void UpdateShiftStyle(int count)
    {
        if (count == 1)
            shiftStyle = ShiftStyle.Move;
        else if (count == 2)
            shiftStyle = ShiftStyle.Grow;
        else
            shiftStyle = ShiftStyle.None;
    }

    private void OnTouchStart(TouchInfo touchInfo)
    {
        if (TouchManager.TouchCount >= maxTouchCount)
            return;
        this.touchInfo = touchInfo;
    }

    private void OnTouchHold(TouchInfo touchInfo) {}

    private void OnTouchMove(TouchInfo touchInfo)
    {
        if (touchInfo != this.touchInfo)
            return;
        State pState = CalculateState(touchInfo.Position - touchInfo.StartPosition);
        ShowPotentialState(pState);
    }

    private void OnTouchEnd(TouchInfo touchInfo)
    {
        if (touchInfo != this.touchInfo)
            return;
        Shift(touchInfo.Position - touchInfo.StartPosition);
        timeSinceTouchEnd = 0f;
    }

    private void Update()
    {
        if (!registered)
            return;

        timeSinceTouchEnd += Time.deltaTime;
    }

    private State EmptyToDirection(Direction direction)
    {
        State state = new State();
        if (direction == Direction.Right)
        {
            for (int y = 1; y < LevelManager.height - 1; ++y)
            {
                int index = State.PositionToIndex(new Vector2Int(0, y));
                state[index] = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(1, LevelManager.height / 2))] = CellState.Active;
        }
        else if (direction == Direction.Up)
        {
            for (int x = 1; x < LevelManager.width - 1; ++x)
            {
                int index = State.PositionToIndex(new Vector2Int(x, 0));
                state[index] = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(LevelManager.width / 2, 1))] = CellState.Active;
        }
        else if (direction == Direction.Left)
        {
            for (int y = 1; y < LevelManager.height - 1; ++y)
            {
                int index = State.PositionToIndex(new Vector2Int(LevelManager.width - 1, y));
                state[index] = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(LevelManager.width - 2, LevelManager.height / 2))] = CellState.Active;
        }
        else if (direction == Direction.Down)
        {
            for (int x = 1; x < LevelManager.width - 1; ++x)
            {
                int index = State.PositionToIndex(new Vector2Int(x, LevelManager.height - 1));
                state[index] = CellState.Active;
            }
            state[State.PositionToIndex(new Vector2Int(LevelManager.width / 2, LevelManager.height - 2))] = CellState.Active;
        }
        return state;
    }

    void Shift(Vector2 swipe)
    {
        State state = CalculateState(swipe);
        if (state == currState)
            return;

        nextState.Reset();
        currState = state;
        ShowState(currState);

        if (OnShift != null)
            OnShift();
    }


    private State CalculateState(Vector2 swipe)
    {
        Direction direction = Direction.None;
        if (swipe.magnitude > MinSwipeDistance)
            direction = DirectionMethods.ToDirection(swipe);

        if (direction == Direction.None)
            return currState;

        // get number of steps from swipe length
        int steps = 0;
        float mag = swipe.magnitude;
        while (mag > MinSwipeDistance)
        {
            mag -= MinSwipeDistance;
            ++steps;
        }

        // recursively calculate all states
        State state = new State(currState);
        for (int i = 0; i < steps; ++i)
        {
            state = CalculateNewState(state, direction);
        }
        return state;
    }

    private State CalculateNewState(State from, Direction direction)
    {
        State result = new State();
        if (from.IsEmpty())
        {
            return EmptyToDirection(direction);
        }

        for (int i = 0; i < State.Size; ++i)
        {
            if (from[i] == CellState.Active)
                SetCellStateToNext(result, i, direction);
        }
        return result;
    }

    private void ShowState(State state)
    {
        for (int i = 0; i < State.Size; ++i)
        {
            if (state[i] == CellState.Active)
            {
                blocks[i].Active = true;
                if (registered)
                    blocks[i].Color = Color.white;
            }
            else if (state[i] == CellState.Inactive)
                blocks[i].Active = false;
        }
    }

    private void ShowPotentialState(State state)
    {
        for (int i = 0; i < State.Size; ++i)
        {
            if (state[i] == CellState.Active)
                blocks[i].Active = true;

            if (currState[i] == CellState.Active)
            {
                if (state[i] == CellState.Active)
                {
                    // state will not change for this cell
                    blocks[i].Color = Color.white;
                }
                else if (state[i] == CellState.Inactive)
                {
                    // will disappear
                    blocks[i].Color = Color.red;
                }
            }
            else if (currState[i] == CellState.Inactive)
            {
                if (state[i] == CellState.Active)
                {
                    blocks[i].Color = Color.green;
                }
                if (state[i] == CellState.Inactive)
                {
                    blocks[i].Color = Color.white;
                    blocks[i].Active = false;
                }
            }
        }
    }

    void SetCellStateToNext(State state, int currIndex, Direction direction)
    {
        if (state[currIndex] == CellState.Inactive)
        {
            if (shiftStyle == ShiftStyle.Move)
                state[currIndex] = CellState.Inactive;
            else if (shiftStyle == ShiftStyle.Grow)
                state[currIndex] = CellState.Active;
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
                state[State.PositionToIndex(newPos)] = CellState.Active;
            }
        }
        else
            state[nextIndex] = CellState.Active;
    }
}
