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
    public bool registered = true;

    public delegate void OnShiftHandler();
    public event OnShiftHandler OnShift;

    private State currState;
    private State nextState;
    private Block[] blocks;
    private ShiftStyle shiftStyle = ShiftStyle.Move;

    private struct TouchInfo
    {
        public Touch touch;
        public Vector2 startPosition;

        public TouchInfo(Touch touch, Vector2 startPosition)
        {
            this.touch = touch;
            this.startPosition = startPosition;
        }
    }
    private List<TouchInfo> touchInfos = new List<TouchInfo>(2);
    private Vector2 potentialTouchEnd = new Vector2();

    public ShiftStyle Style { get => shiftStyle; }

    public State State
    {
        get => currState;
        set
        {
            currState = value;
            ShowCurrentState();
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
            TouchController.OnTouchCountChanged += OnTouchCountChanged;
            TouchController.OnStartTouch += OnStartTouch;
            TouchController.OnEndTouch += OnEndTouch;
        }
    }

    private void OnDisable()
    {
        if (registered)
        {
            TouchController.OnTouchCountChanged -= OnTouchCountChanged;
            TouchController.OnStartTouch -= OnStartTouch;
            TouchController.OnEndTouch -= OnEndTouch;
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

    private void OnTouchCountChanged()
    {
        if (TouchController.TouchCount == 1)
        {
            shiftStyle = ShiftStyle.Move;
        }
        else if (TouchController.TouchCount > 1)
        {
            shiftStyle = ShiftStyle.Grow;
        }
        else
        {
            shiftStyle = ShiftStyle.None;
        }
    }

    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    private void OnStartTouch(Touch touch)
    {
        if (touchInfos == null || touchInfos.Count > 1)
        {
            return;
        }

        touchInfos.Add(new TouchInfo(touch, touch.position));
    }

    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    private void OnEndTouch(Touch touch)
    {
        if (touchInfos.Count == 1)
        // We are going to remove last touch
        {
            Vector2 swipe = touchInfos[0].touch.position - touchInfos[0].startPosition;
            Direction direction = Direction.None;
            if (swipe.magnitude > MinSwipeDistance)
            {
                direction = DirectionMethods.ToDirection(swipe);
                Shift(direction);
                if (OnShift != null)
                {
                    OnShift();
                }
            }
        }

        for (int i = 0; i < touchInfos.Count; ++i)
        {
            if (touchInfos[i].touch.fingerId == touch.fingerId)
            {
                touchInfos.RemoveAt(i);
                break;
            }
        }

    }

    private void Update()
    {
        // handle current touches
        if (touchInfos.Count > 0)
        {
            Vector3 world = Camera.main.ScreenToWorldPoint(touchInfos[0].touch.position);
            world.z = 0f;
            Debug.Log(world);
            Debug.DrawLine(new Vector3(2f, 2f, 0f), world, Color.red);
            potentialTouchEnd = touchInfos[0].touch.position;
        }
    }

    // private void OnSwipe(Direction direction, ShiftStyle style)
    // {
    //     shiftStyle = style;
    // }

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
        ShowCurrentState();

        if (OnShift != null)
            OnShift();
    }

    private State CalculateNextState(Direction direction)
    {
        if (direction == Direction.None)
            throw new ArgumentException("[direction] cannot be Direction.None");

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

    private void ShowCurrentState()
    {
        for (int i = 0; i < State.Size; ++i)
        {
            if (currState[i] == CellState.Active)
            {
                blocks[i].Color = Color.white;
                blocks[i].gameObject.SetActive(true);
            }
            else if (currState[i] == CellState.Inactive)
                blocks[i].gameObject.SetActive(false);
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
