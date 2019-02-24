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

public class Shifter : MonoBehaviour
{
    public GameObject blockPrefab;
    public GameObject touchStartSprite;
    public GameObject potentialTouchEndSprite;
    public bool registered = true;

    public delegate void OnShiftHandler();
    public event OnShiftHandler OnShift;

    private State currState;
    private State nextState;
    private Block[] blocks;
    private ShiftStyle shiftStyle = ShiftStyle.Move;

    private Vector2 touchStart;
    private Vector2 potentialTouchEnd;
    private float minTimeBetweenTouches = 0.25f;
    private float timeSinceTouchEnd = 0f;

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
        get => 0.05f * Mathf.Min(Screen.width, Screen.height);
    }

    private void OnEnable()
    {
        if (registered)
        {
            TouchManager.OnTouchCountChanged += OnTouchCountChanged;
            TouchManager.OnStartTouch += OnStartTouch;
            TouchManager.OnMoveTouch += OnMoveTouch;
            TouchManager.OnEndTouch += OnEndTouch;
        }
    }

    private void OnDisable()
    {
        if (registered)
        {
            TouchManager.OnTouchCountChanged -= OnTouchCountChanged;
            TouchManager.OnStartTouch -= OnStartTouch;
            TouchManager.OnMoveTouch -= OnMoveTouch;
            TouchManager.OnEndTouch -= OnEndTouch;
        }
    }

    private void Start()
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

    private void OnTouchCountChanged()
    {
        if (TouchManager.TouchCount == 1)
        {
            shiftStyle = ShiftStyle.Move;
        }
        else if (TouchManager.TouchCount > 1)
        {
            shiftStyle = ShiftStyle.Grow;
        }
        else
        {
            shiftStyle = ShiftStyle.None;
        }

        // if (shiftStyle != ShiftStyle.None)
        // {
        //     Vector2 pSwipe = potentialTouchEnd - touchStart;
        //     Direction direction = Direction.None;
        //     if (pSwipe.magnitude > MinSwipeDistance)
        //         direction = DirectionMethods.ToDirection(pSwipe);
        //     State pState = CalculateNextState(direction);
        //     ShowPotentialState(pState);
        // }
    }

    private void OnStartTouch(Vector2 startPosition)
    {
        if (timeSinceTouchEnd < minTimeBetweenTouches)
            return;

        touchStartSprite.transform.position = startPosition;
        touchStart = startPosition;
    }

    private void OnMoveTouch(Vector2 newPosition)
    {
        if (timeSinceTouchEnd < minTimeBetweenTouches)
            return;

        potentialTouchEndSprite.transform.position = newPosition;
        potentialTouchEnd = newPosition;
        Vector2 pSwipe = potentialTouchEnd - touchStart;

        Direction direction = Direction.None;
        if (pSwipe.magnitude > MinSwipeDistance)
            direction = DirectionMethods.ToDirection(pSwipe);
        State pState = CalculateNextState(direction);
        ShowPotentialState(pState);
    }

    private void OnEndTouch(Vector2 endPosition)
    {
        if (timeSinceTouchEnd < minTimeBetweenTouches)
            return;
        Vector2 swipe = endPosition - touchStart;
        Direction direction = Direction.None;
        if (swipe.magnitude > MinSwipeDistance)
        {
            direction = DirectionMethods.ToDirection(swipe);
            Shift(direction);
            timeSinceTouchEnd = 0f;
        }
    }

    private void Update()
    {
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

    void Shift(Direction direction)
    {
        nextState.Reset();
        currState = CalculateNextState(direction);
        ShowState(currState);

        if (OnShift != null)
            OnShift();
    }

    private State CalculateNextState(Direction direction)
    {
        if (direction == Direction.None)
            return currState;

        State state = new State();
        if (currState.IsEmpty())
        {
            state = EmptyToDirection(direction);
        }
        else
        {
            for (int i = 0; i < State.Size; ++i)
            {
                if (currState[i] == CellState.Active)
                    SetCellStateToNext(state, i, direction);
            }
        }
        return state;
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

    private void OnDrawGizmos()
    {
        // if (touchInfos.Count > 0)
        // {
        //     Vector3 a = Camera.main.ScreenToWorldPoint(touchInfos[0].startPosition);
        //     Vector3 b = Camera.main.ScreenToWorldPoint(potentialTouchEnd);
        //     a.z = -3f;
        //     b.z = -3f;
        //     Debug.Log(a);
        //     Debug.Log(b);
        //     Debug.DrawLine(a, b);
        // }
    }
}
