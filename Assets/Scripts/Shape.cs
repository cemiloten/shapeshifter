using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Shape", menuName = "ShapeShifter/Shape")]
public class Shape : ScriptableObject
{
    public Vector2Int[] positions;

    // private Vector2Int GetMin()
    // {
    //     Vector2Int min = positions[0];
    //     for (int i = 0; i < positions.Length; ++i)
    //     {
    //         if (positions[i] < m
    //     }
    // }

    public static bool Equals(Shape s1, Shape s2)
    {
        if (s1.positions.Length != s2.positions.Length)
        {
            return false;
        }

        for (int i = 0; i < s1.positions.Length; ++i)
        {
            if (s1.positions[i] != s2.positions[i])
                return false;
        }
        return true;
    }
}