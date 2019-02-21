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

        if (Input.GetMouseButtonDown(0))
        {
            touchOrigin = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            touchEnd = Input.mousePosition;
            Direction direction = DirectionMethods.ToDirection(touchEnd - touchOrigin);
            OnSwipe(direction, shiftStyle);
            Debug.Log(direction);
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
                Vector2 swipe = new Vector2(touchEnd - touchOrigin);
                if (swipe.magnitude < SwipeDistance)
                    return;

                Direction dir = DirectionMethods.ToDirection(swipe);
                if (OnSwipe != null && dir != Direction.None)
                {
                    OnSwipe(dir, shiftStyle);
                }
            }
        }
#endif
    }
}