using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class LevelManager : MonoBehaviour
{
    public static readonly int width = 5;
    public static readonly int height = 5;

    public Shape[] shapes;

    public static LevelManager Instance { get; private set; }

    public static bool IsValidPosition(Vector2Int position)
    {
        return (position.x >= 0
            && position.x < width
            && position.y >= 0
            && position.y < height);
    }

    public static bool IsValidPosition(int x, int y)
    {
        return IsValidPosition(new Vector2Int(x, y));
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    public State GenerateState()
    {
        return ShapeToState(shapes[Random.Range(0, shapes.Length)]);
    }

    private State ShapeToState(Shape shape)
    {
        State state = new State();
        Vector2Int startPosition = new Vector2Int(
            Random.Range(0, LevelManager.width - shape.width),
            Random.Range(0, LevelManager.height - shape.height));
        for (int i = 0; i < shape.positions.Length; ++i)
        {
            int index = State.PositionToIndex(startPosition + shape.positions[i]);
            state[index] = CellState.Active;
        }
        return state;
    }

    private void DrawGrid()
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