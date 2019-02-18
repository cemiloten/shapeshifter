﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Direction = UnityEngine.Vector2Int;

public class ShapeShifter : MonoBehaviour
{
    private enum ShiftStyle
    {
        Move,
        Duplicate
    }

    public GameObject blockPrefab;
    public int width = 5;
    public int height = 5;
    public float shiftDuration = 0.25f;
    public Vector3 maxScale = new Vector3(0.9f, 0.9f, 0.2f);
    public Vector3 minScale = new Vector3(0.1f, 0.1f, 0.1f);

    private bool shifting = false;
    private float shiftTimer = 0f;
    private Block[] blocks;
    private bool[] currentShape;
    private bool[] nextShape;
    private ShiftStyle shiftStyle = ShiftStyle.Move;

    void Start()
    {
        Camera.main.transform.position = new Vector3((width - 1) / 2f, (height - 1) / 2f, -6f);
        SetupBlocks();
        currentShape = new bool[width * height];
        currentShape[0] = true;
        currentShape[1] = true;
        SetBlocksToCurrentShape();

        nextShape = new bool[width * height];
    }

    private void SetBlocksToCurrentShape()
    {
        for (int i = 0; i < blocks.Length; ++i)
        {
            blocks[i].Active = currentShape[i];
        }
    }

    void SetupBlocks()
    {
        blocks = new Block[width * height];
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                GameObject go = Instantiate(blockPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                Block block = go.GetComponent<Block>();
                block.Position = new Vector2Int(x, y);
                blocks[x + y * width] = block;
            }
        }
    }

    void Update()
    {
        if (shifting)
        {
            UpdateShape();
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            shiftStyle = ShiftStyle.Duplicate;
        }
        else
        {
            shiftStyle = ShiftStyle.Move;
        }

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.E))
        {
            if (IsEmpty())
            {
                currentShape = EmptyToDirection(Directions.Up);
            }
            else
            {
                Shift(Directions.Up);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.F))
        {
            if (IsEmpty())
            {
                currentShape = EmptyToDirection(Directions.Right);
            }
            else
                Shift(Directions.Right);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.S))
        {
            Shift(Directions.Left);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.C))
        {
            Shift(Directions.Down);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            Shift(Directions.UpRight);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            Shift(Directions.UpLeft);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            Shift(Directions.DownRight);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            Shift(Directions.DownLeft);
        }

        if (Input.anyKeyDown && !Input.GetKeyDown(KeyCode.Space))
        {
            StartShifting();
        }
    }

    private void StartShifting()
    {
        if (shifting)
            return;

        for (int i = 0; i < nextShape.Length; ++i)
        {
            if (nextShape[i])
                blocks[i].Active = true;
        }
        shifting = true;
    }

    private void EndShifting()
    {
        shifting = false;
        Array.Clear(currentShape, 0, currentShape.Length);
        shiftTimer -= shiftDuration;
        currentShape = nextShape;
        SetBlocksToCurrentShape();

        Array.Clear(nextShape, 0, nextShape.Length);
    }

    private bool[] EmptyToDirection(Direction direction)
    {
        bool[] state = new bool[width * height];
        if (direction == Directions.Right)
        {
            for (int y = 0; y < height; ++y)
            {
                int index = PositionToIndex(new Vector2Int(0, y));
                state[index] = true;
            }
            state[PositionToIndex(new Vector2Int(1, height / 2))] = true;
        }
        else if (direction == Directions.Up)
        {
            for (int x = 0; x < width; ++x)
            {
                int index = PositionToIndex(new Vector2Int(x, 0));
                state[index] = true;
            }
            state[PositionToIndex(new Vector2Int(width / 2, 1))] = true;
        }
        return state;
    }

    void Shift(Direction direction)
    {
        Array.Clear(nextShape, 0, nextShape.Length);

        for (int i = 0; i < blocks.Length; ++i)
        {
            if (blocks[i].Active)
                ShiftBlock(blocks[i], direction);
        }
    }

    void UpdateShape()
    {
        float t = shiftTimer / shiftDuration;

        for (int i = 0; i < blocks.Length; ++i)
        {
            if (currentShape[i])
            {
                if (!nextShape[i])
                    blocks[i].transform.localScale = Vector3.Lerp(maxScale, minScale, t);
                else
                    blocks[i].transform.localScale = maxScale;
            }
            else if (nextShape[i])
            {
                blocks[i].transform.localScale = Vector3.Lerp(minScale, maxScale, t);
            }
        }


        if (shiftTimer < shiftDuration)
            shiftTimer += Time.deltaTime;
        else
        {
            EndShifting();
        }
    }

    void ShiftBlock(Block block, Direction direction)
    {
        int index = PositionToIndex(block.Position);
        if (nextShape[index] == false)
        {
            if (shiftStyle == ShiftStyle.Move)
                nextShape[index] = false;
            else if (shiftStyle == ShiftStyle.Duplicate)
                nextShape[index] = true;
        }

        int nextIndex = PositionToIndex(block.Position + direction);
        if (nextIndex < 0)
        {
            if (shiftStyle == ShiftStyle.Duplicate)
            {
                int x = (block.Position.x + direction.x) % width;
                if (x < 0)
                    x = width + x;
                int y = (block.Position.y + direction.y) % height;
                if (y < 0)
                    y = width + y;
                Vector2Int newPos = new Vector2Int(x, y);
                nextShape[PositionToIndex(newPos)] = true;
            }
        }
        else
            nextShape[nextIndex] = true;
    }

    private bool IsEmpty()
    {
        for (int i = 0; i < currentShape.Length; ++i)
            if (currentShape[i])
                return false;
        return true;
    }

    Block BlockInDirection(Block start, Direction direction)
    {
        return BlockAt(start.Position + direction);
    }

    public Block BlockAt(Vector2Int pos)
    {
        if (!IsInShape(pos))
            return null;
        return blocks[pos.x + pos.y * width];
    }

    public bool IsInShape(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height);
    }

    private int PositionToIndex(Vector2Int position)
    {
        if (!IsInShape(position))
            return -1;

        return Array.IndexOf(blocks, BlockAt(position));
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
