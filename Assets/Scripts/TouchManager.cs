using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TouchManager
{
    public delegate void OnTouchCountChangedHandler();
    public static event OnTouchCountChangedHandler OnTouchCountChanged;

    public delegate void OnStartTouchHandler(Vector2 startPosition);
    public static event OnStartTouchHandler OnStartTouch;

    public delegate void OnHoldTouchHandler(Vector2 newPosition);
    public static event OnHoldTouchHandler OnHoldTouch;

    public delegate void OnMoveTouchHandler(Vector2 newPosition);
    public static event OnMoveTouchHandler OnMoveTouch;

    public delegate void OnEndTouchHandler(Vector2 endPosition);
    public static event OnEndTouchHandler OnEndTouch;

    public static int TouchCount { get; private set; }

    public static void Update()
    {
        if (Input.touchCount != TouchCount && OnTouchCountChanged != null)
        {
            TouchCount = Input.touchCount;
            OnTouchCountChanged();
        }

        if (Input.touchCount > 0)
        {
            Touch touch = Input.touches[0];
            if (touch.phase == TouchPhase.Began && OnStartTouch != null)
            {
                OnStartTouch(touch.position);
            }
            else if (touch.phase == TouchPhase.Stationary && OnHoldTouch != null)
            {
                OnHoldTouch(touch.position);
            }
            else if (touch.phase == TouchPhase.Moved && OnMoveTouch != null)
            {
                OnMoveTouch(touch.position);
            }
            else if (touch.phase == TouchPhase.Ended && OnEndTouch != null)
            {
                OnEndTouch(touch.position);
            }
        }
    }
}