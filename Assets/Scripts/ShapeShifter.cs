using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ShiftStyle
{
    None = 0,
    Move,
    Duplicate
}

public class ShapeShifter : MonoBehaviour
{
    public GameObject blockPrefab;

    // public float shiftDuration = 0.25f;
    // public Vector3 maxScale = new Vector3(0.9f, 0.9f, 0.2f);
    // public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

    public delegate bool OnShiftHandler();
    public static event OnShiftHandler OnShift;

    private State currState;
    private State nextState;
    private Block[] blocks;

    // private bool shifting = false;
    // private float shiftTimer = 0f;

    private ShiftStyle shiftStyle = ShiftStyle.Move;

    private void OnEnable()
    {
        TouchController.OnSwipe += OnSwipe;
    }

    private void OnDisable()
    {
        TouchController.OnSwipe -= OnSwipe;
    }

    private void OnSwipe(Direction direction, ShiftStyle style)
    {
        shiftStyle = style;
        // Shift(direction);
    }

    private void Start()
    {
        currState = new State();
        nextState = new State();
        PrepareBlocks();
        UpdateBlocks();
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

    private void UpdateBlocks()
    {
        for (int i = 0; i < blocks.Length; ++i)
        {
            if (currState[i] == CellState.Active)
                blocks[i].Active = true;
            else
                blocks[i].Active = false;
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

    // private bool[] EmptyToDirection(Direction direction)
    // {
    //     bool[] state = new bool[LevelManager.width * LevelManager.height];
    //     if (direction == Direction.Right)
    //     {
    //         for (int y = 0; y < LevelManager.height; ++y)
    //         {
    //             int index = PositionToIndex(new Vector2Int(0, y));
    //             state[index] = true;
    //         }
    //         state[PositionToIndex(new Vector2Int(1, LevelManager.height / 2))] = true;
    //     }
    //     else if (direction == Direction.Up)
    //     {
    //         for (int x = 0; x < LevelManager.width; ++x)
    //         {
    //             int index = PositionToIndex(new Vector2Int(x, 0));
    //             state[index] = true;
    //         }
    //         state[PositionToIndex(new Vector2Int(LevelManager.width / 2, 1))] = true;
    //     }
    //     else if (direction == Direction.Left)
    //     {
    //         for (int y = 0; y < LevelManager.height; ++y)
    //         {
    //             int index = PositionToIndex(new Vector2Int(LevelManager.width - 1, y));
    //             state[index] = true;
    //         }
    //         state[PositionToIndex(new Vector2Int(LevelManager.width - 2, LevelManager.height / 2))] = true;
    //     }
    //     else if (direction == Direction.Down)
    //     {
    //         for (int x = 0; x < LevelManager.width; ++x)
    //         {
    //             int index = PositionToIndex(new Vector2Int(x, LevelManager.height - 1));
    //             state[index] = true;
    //         }
    //         state[PositionToIndex(new Vector2Int(LevelManager.width / 2, LevelManager.height - 2))] = true;
    //     }
    //     return state;
    // }

    // void Shift(Direction direction)
    // {
    //     Array.Clear(nextShape, 0, nextShape.Length);

    //     if (IsEmpty())
    //     {
    //         nextShape = EmptyToDirection(direction);
    //     }
    //     else
    //     {
    //         for (int i = 0; i < blocks.Length; ++i)
    //         {
    //             if (blocks[i].Active)
    //                 SetBlockToNext(blocks[i], direction);
    //         }
    //     }

    //     Array.Copy(nextShape, currentShape, LevelManager.width * LevelManager.height);
    //     ShowCurrentShape();

    //     if (OnShift != null)
    //         OnShift();
    // }

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

    // void SetBlockToNext(Block block, Direction direction)
    // {
    //     int index = PositionToIndex(block.Position);
    //     if (nextShape[index] == false)
    //     {
    //         if (shiftStyle == ShiftStyle.Move)
    //             nextShape[index] = false;
    //         else if (shiftStyle == ShiftStyle.Duplicate)
    //             nextShape[index] = true;
    //     }

    //     Vector2Int directionVector = direction.ToVector2Int();
    //     int nextIndex = PositionToIndex(block.Position + directionVector);
    //     if (nextIndex < 0)
    //     {
    //         if (shiftStyle == ShiftStyle.Duplicate)
    //         {
    //             int x = (block.Position.x + directionVector.x) % LevelManager.width;
    //             if (x < 0)
    //                 x = LevelManager.width + x;
    //             int y = (block.Position.y + directionVector.y) % LevelManager.height;
    //             if (y < 0)
    //                 y = LevelManager.width + y;
    //             Vector2Int newPos = new Vector2Int(x, y);
    //             nextShape[PositionToIndex(newPos)] = true;
    //         }
    //     }
    //     else
    //         nextShape[nextIndex] = true;
    // }

    private void OnDrawGizmos()
    {
        Vector3 widthLine = Vector3.right * LevelManager.width;
        Vector3 heightLine = Vector3.up * LevelManager.height;

        for (int z = 0; z <= LevelManager.height; ++z)
        {
            Vector3 start = Vector3.up * z;
            start.x -= 0.5f;
            start.y -= 0.5f;
            Debug.DrawLine(start, start + widthLine, Color.black);

            for (int x = 0; x <= LevelManager.width; ++x)
            {
                start = Vector3.right * x;
                start.x -= 0.5f;
                start.y -= 0.5f;
                Debug.DrawLine(start, start + heightLine, Color.black);
            }
        }
    }
}
