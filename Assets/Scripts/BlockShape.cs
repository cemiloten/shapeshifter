using System;
using UnityEngine;

public abstract class BlockShape : MonoBehaviour
{
    public GameObject blockPrefab;

    protected int width;
    protected int height;
    protected Block[] blocks;
    protected bool[] currentShape;
    protected bool[] nextShape;

    private void Start()
    {
        width = GameManager.Instance.width;
        height = GameManager.Instance.height;

        nextShape = new bool[GameManager.Instance.width * GameManager.Instance.height];
        currentShape = new bool[GameManager.Instance.width * GameManager.Instance.height];
        PrepareBlocks();
        ShowCurrentShape();
    }

    private void PrepareBlocks()
    {
        blocks = new Block[width * height];
        for (int y = 0; y < height; ++y)
        {
            for (int x = 0; x < width; ++x)
            {
                GameObject go = Instantiate(blockPrefab, new Vector3(x, y, 0f), Quaternion.identity);
                go.transform.parent = transform;
                Block block = go.GetComponent<Block>();
                block.Position = new Vector2Int(x, y);
                blocks[x + y * width] = block;
            }
        }
    }

    protected void ShowCurrentShape()
    {
        for (int i = 0; i < blocks.Length; ++i)
        {
            blocks[i].Active = currentShape[i];
        }
    }

    protected bool IsEmpty()
    {
        for (int i = 0; i < currentShape.Length; ++i)
            if (currentShape[i])
                return false;
        return true;
    }

    protected Block BlockInDirection(Block start, Direction direction)
    {
        return BlockAt(start.Position + direction.ToVector2Int());
    }

    protected Block BlockAt(Vector2Int pos)
    {
        if (!IsInShape(pos))
            return null;
        return blocks[pos.x + pos.y * width];
    }

    protected bool IsInShape(Vector2Int pos)
    {
        return (pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height);
    }

    protected int PositionToIndex(Vector2Int position)
    {
        if (!IsInShape(position))
            return -1;

        return Array.IndexOf(blocks, BlockAt(position));
    }
}

