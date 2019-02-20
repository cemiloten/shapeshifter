using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TouchController
{
    public delegate void OnSwipeHandler(Direction direction, ShiftStyle style);
    public static event OnSwipeHandler OnSwipe;

    private static Vector2 touchOrigin;
    private static Vector2 touchEnd;
    private static Touch shift;
    private static ShiftStyle shiftStyle = ShiftStyle.None;
    private static float SwipeDistance
    {
        get
        {
            // 10% of smallest screen border
            return 0.1f * Mathf.Min(Screen.width, Screen.height);
        }
    }

    public static void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.LeftShift))
            shiftStyle = ShiftStyle.Duplicate;
        else
            shiftStyle = ShiftStyle.Move;

        if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.E))
        {
            OnSwipe(Direction.Up, shiftStyle);
        }

        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.F))
        {
            OnSwipe(Direction.Right, shiftStyle);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.S))
        {
            OnSwipe(Direction.Left, shiftStyle);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.C))
        {
            OnSwipe(Direction.Down, shiftStyle);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            OnSwipe(Direction.UpRight, shiftStyle);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            OnSwipe(Direction.UpLeft, shiftStyle);
        }
        if (Input.GetKeyDown(KeyCode.V))
        {
            OnSwipe(Direction.DownRight, shiftStyle);
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            OnSwipe(Direction.DownLeft, shiftStyle);
        }
#else
        if (Input.touchCount > 0)
        {
            shift = Input.GetTouch(0);
            shiftStyle = Input.touchCount > 1 ? ShiftStyle.Duplicate : ShiftStyle.Move;

            if (shift.phase == TouchPhase.Began)
            {
                touchOrigin = shift.position;
            }
            else if (shift.phase == TouchPhase.Ended)
            {
                touchEnd = shift.position;
                Vector2 swipe = new Vector2(touchEnd.x - touchOrigin.x, touchEnd.y - touchOrigin.y);
                if (swipe.magnitude < SwipeDistance)
                    return;
                Direction[] directions = System.Enum.GetValues(typeof(Direction)) as Direction[];
                Vector2[] directionVectors = new Vector2[directions.Length];
                for (int i = 0; i < directions.Length; ++i)
                {
                    directionVectors[i] = directions[i].ToVector2();
                }

                Direction dir = Direction.None;
                float dot = 0f;
                for (int i = 0; i < directions.Length; ++i)
                {
                    float d = Vector2.Dot(swipe.normalized, directionVectors[i]);
                    if (d > dot)
                    {
                        dot = d;
                        dir = directions[i];
                    }

                    if (dot > 0.95f)
                        break;
                }

                if (OnSwipe != null && dir != Direction.None)
                {
                    OnSwipe(dir, shiftStyle);
                }
            }
        }
#endif
    }
}