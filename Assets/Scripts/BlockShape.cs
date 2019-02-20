// using System;
// using UnityEngine;

// public abstract class BlockShape : MonoBehaviour
// {

//     private void Start()
//     {
//         nextShape = new bool[LevelManager.Instance.width * LevelManager.Instance.height];
//         currentShape = new bool[LevelManager.Instance.width * LevelManager.Instance.height];
//         PrepareBlocks();
//         ShowCurrentShape();
//     }


//     protected Block BlockInDirection(Block start, Direction direction)
//     {
//         return BlockAt(start.Position + direction.ToVector2Int());
//     }
