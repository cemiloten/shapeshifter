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

public class ShapeShifter : BlockShape
{

    // public float shiftDuration = 0.25f;
    // public Vector3 maxScale = new Vector3(0.9f, 0.9f, 0.2f);
    // public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);
    public delegate void OnShiftHandler();
    public static event OnShiftHandler OnShift;

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
        Shift(direction);
    }

    void Update()
    {
        // if (shifting)
        // {
        //     UpdateShape();
        //     return;
        // }

        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift))
        {
            shiftStyle = ShiftStyle.Duplicate;
        }
        else
        {
            shiftStyle = ShiftStyle.Move;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.E))
        {
            Shift(Direction.Up);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.F))
        {
            Shift(Direction.Right);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Shift(Direction.Left);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.C))
        {
            Shift(Direction.Down);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Shift(Direction.UpRight);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Shift(Direction.UpLeft);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Shift(Direction.DownRight);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Shift(Direction.DownLeft);
        }

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

    private bool[] EmptyToDirection(Direction direction)
    {
        bool[] state = new bool[width * height];
        if (direction == Direction.Right)
        {
            for (int y = 0; y < height; ++y)
            {
                int index = PositionToIndex(new Vector2Int(0, y));
                state[index] = true;
            }
            state[PositionToIndex(new Vector2Int(1, height / 2))] = true;
        }
        else if (direction == Direction.Up)
        {
            for (int x = 0; x < width; ++x)
            {
                int index = PositionToIndex(new Vector2Int(x, 0));
                state[index] = true;
            }
            state[PositionToIndex(new Vector2Int(width / 2, 1))] = true;
        }
        else if (direction == Direction.Left)
        {
            for (int y = 0; y < height; ++y)
            {
                int index = PositionToIndex(new Vector2Int(width - 1, y));
                state[index] = true;
            }
            state[PositionToIndex(new Vector2Int(width - 2, height / 2))] = true;
        }
        else if (direction == Direction.Down)
        {
            for (int x = 0; x < width; ++x)
            {
                int index = PositionToIndex(new Vector2Int(x, height - 1));
                state[index] = true;
            }
            state[PositionToIndex(new Vector2Int(width / 2, height - 2))] = true;
        }
        return state;
    }

    void Shift(Direction direction)
    {
        Array.Clear(nextShape, 0, nextShape.Length);

        if (IsEmpty())
        {
            nextShape = EmptyToDirection(direction);
        }
        else
        {
            for (int i = 0; i < blocks.Length; ++i)
            {
                if (blocks[i].Active)
                    SetBlockToNext(blocks[i], direction);
            }
        }

        Array.Copy(nextShape, currentShape, width * height);
        ShowCurrentShape();

        if (OnShift != null)
            OnShift();
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

    void SetBlockToNext(Block block, Direction direction)
    {
        int index = PositionToIndex(block.Position);
        if (nextShape[index] == false)
        {
            if (shiftStyle == ShiftStyle.Move)
                nextShape[index] = false;
            else if (shiftStyle == ShiftStyle.Duplicate)
                nextShape[index] = true;
        }

        Vector2Int directionVector = direction.ToVector2Int();
        int nextIndex = PositionToIndex(block.Position + directionVector);
        if (nextIndex < 0)
        {
            if (shiftStyle == ShiftStyle.Duplicate)
            {
                int x = (block.Position.x + directionVector.x) % width;
                if (x < 0)
                    x = width + x;
                int y = (block.Position.y + directionVector.y) % height;
                if (y < 0)
                    y = width + y;
                Vector2Int newPos = new Vector2Int(x, y);
                nextShape[PositionToIndex(newPos)] = true;
            }
        }
        else
            nextShape[nextIndex] = true;
    }

    private void OnDrawGizmos()
    {
        Vector3 widthLine = Vector3.right * width;
        Vector3 heightLine = Vector3.up * height;

        for (int z = 0; z <= height; ++z)
        {
            Vector3 start = Vector3.up * z;
            start.x -= 0.5f;
            start.y -= 0.5f;
            Debug.DrawLine(start, start + widthLine, Color.black);

            for (int x = 0; x <= width; ++x)
            {
                start = Vector3.right * x;
                start.x -= 0.5f;
                start.y -= 0.5f;
                Debug.DrawLine(start, start + heightLine, Color.black);
            }
        }
    }
}
