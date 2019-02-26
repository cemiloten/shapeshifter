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
    private struct TouchInfo
    {
        public int ID;
        public Vector2 position;
        public Vector2 startPosition;

        public TouchInfo(int ID, Vector2 position, Vector2 startPosition)
        {
            this.ID = ID;
            this.position = position;
            this.startPosition = startPosition;
        }
    }

    public GameObject blockPrefab;
    public GameObject touchStartSprite;
    public GameObject potentialSwipeEndSprite;
    public bool registered = true;

    public delegate void OnShiftHandler();
    public event OnShiftHandler OnShift;

    private const float minTimeBetweenTouches = 0.2f;
    private const int maxTouchCount = 2;

    public float timeSinceTouchEnd = 0f; // change to private when not needed anymore

    private State currState;
    private State nextState;
    private Block[] blocks;
    private ShiftStyle shiftStyle = ShiftStyle.Move;
    private List<TouchInfo> touchInfos = new List<TouchInfo>();
    private Vector2 _swipeStart;
    private Vector2 _swipeEnd;

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

    private int TouchCount => touchInfos.Count;

    private Vector2 SwipeStart
    {
        get => _swipeStart;
        set
        {
            _swipeStart = value;
            touchStartSprite.transform.position = value;
        }
    }

    private Vector2 SwipeEnd
    {
        get => _swipeEnd;
        set
        {
            _swipeEnd = value;
            potentialSwipeEndSprite.transform.position = value;
        }
    }

    private void OnEnable()
    {
        if (registered)
        {
            TouchManager.OnTouchStart += OnStartTouch;
            TouchManager.OnTouchHold += OnHoldTouch;
            TouchManager.OnTouchMove += OnMoveTouch;
            TouchManager.OnTouchEnd += OnEndTouch;
        }
    }

    private void OnDisable()
    {
        if (registered)
        {
            TouchManager.OnTouchStart -= OnStartTouch;
            TouchManager.OnTouchHold -= OnHoldTouch;
            TouchManager.OnTouchMove -= OnMoveTouch;
            TouchManager.OnTouchEnd -= OnEndTouch;
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

    private void UpdateShiftStyle(int count)
    {
        if (count == 1)
            shiftStyle = ShiftStyle.Move;
        else if (count == 2)
            shiftStyle = ShiftStyle.Grow;
        else
            shiftStyle = ShiftStyle.None;
    }

    private void OnStartTouch(int fingerID, Vector2 startPosition)
    {
        if (TouchCount >= maxTouchCount)
            return;

        Vector2 pos = Camera.main.ScreenToWorldPoint(startPosition);

        if (TouchCount == 0)
            SwipeStart = pos;

        touchInfos.Add(new TouchInfo(fingerID, pos, pos));
        UpdateShiftStyle(TouchCount);
    }

    private void OnHoldTouch(int fingerID, Vector2 position)
    {
        if (TouchCount == 1)
        {
            SwipeStart = touchInfos[0].startPosition;
        }
    }

    private void OnMoveTouch(int fingerID, Vector2 newPosition)
    {
        int index;
        if (!GetTouchInfoIndex(fingerID, out index))
            return; // Ignore touch if not recorded.

        Vector2 pos = Camera.main.ScreenToWorldPoint(newPosition);

        // Update touch's current position
        TouchInfo touchInfo = touchInfos[index];
        touchInfo.position = pos;
        touchInfos[index] = touchInfo;

        if (index != 0)
            return;

        Vector2 pSwipe = pos - touchInfo.startPosition;
        State pState = CalculateNextState(pSwipe);
        ShowPotentialState(pState);
        SwipeEnd = pos;
    }

    private void OnEndTouch(int fingerID, Vector2 endPosition)
    {
        int index;
        if (!GetTouchInfoIndex(fingerID, out index))
            return; // Ignore touch if not recorded.

        touchInfos.RemoveAt(index);
        timeSinceTouchEnd = 0f;
        UpdateShiftStyle(TouchCount);

        if (TouchCount == 1)
        {
            return;
        }

        // If we released a finger slighlty before, consider that it was a two finger swipe.
        if (timeSinceTouchEnd < minTimeBetweenTouches)
            shiftStyle = ShiftStyle.Grow;

        Shift(SwipeEnd - SwipeStart);
    }

    private void Update()
    {
        if (!registered)
            return;

        timeSinceTouchEnd += Time.deltaTime;
    }

    private bool GetTouchInfoIndex(int fingerID, out int index)
    {
        for (int i = 0; i < TouchCount; ++i)
        {
            if (touchInfos[i].ID == fingerID)
            {
                index = i;
                return true;
            }
        }

        index = -1;
        return false;
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
        State state = CalculateNextState(swipe);
        if (state == currState)
            return;

        nextState.Reset();
        currState = state;
        ShowState(currState);

        if (OnShift != null)
            OnShift();
    }


    private State CalculateNextState(Vector2 swipe, int distance = 0)
    {
        Direction direction = Direction.None;
        if (swipe.magnitude > MinSwipeDistance)
            direction = DirectionMethods.ToDirection(swipe);

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
}
