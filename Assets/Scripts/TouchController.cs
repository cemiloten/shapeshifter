using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TouchController
{
    public delegate void OnTouchCountChangedHandler();
    public static event OnTouchCountChangedHandler OnTouchCountChanged;

    public delegate void OnStartTouchHandler(Touch touch);
    public static event OnStartTouchHandler OnStartTouch;

    public delegate void OnEndTouchHandler(Touch touch);
    public static event OnEndTouchHandler OnEndTouch;

    public static int TouchCount { get; private set; }

    public static void Update()
    {
        if (Input.touchCount != TouchCount && OnTouchCountChanged != null)
        {
            TouchCount = Input.touchCount;
            OnTouchCountChanged();
        }

    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
    // TOUCHES ARE PASSED BY VALUE EVERYTHING IS WRONG
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touches.Length; ++i)
            {
                Touch touch = Input.touches[i];
                if (touch.phase == TouchPhase.Began && OnStartTouch != null)
                {
                    OnStartTouch(touch);
                }
                else if (touch.phase == TouchPhase.Ended && OnEndTouch != null)
                {
                    OnEndTouch(touch);
                }
            }
        }
    }
}