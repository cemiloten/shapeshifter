using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TouchManager
{
    public delegate void OnTouchCountChangedHandler();
    public static event OnTouchCountChangedHandler OnTouchCountChanged;

    public delegate void OnTouchStartHandler(int fingerdID, Vector2 startPosition);
    public static event OnTouchStartHandler OnTouchStart;

    public delegate void OnTouchHoldHandler(int fingerdID, Vector2 newPosition);
    public static event OnTouchHoldHandler OnTouchHold;

    public delegate void OnTouchMoveHandler(int fingerdID, Vector2 newPosition);
    public static event OnTouchMoveHandler OnTouchMove;

    public delegate void OnTouchEndHandler(int fingerdID, Vector2 endPosition);
    public static event OnTouchEndHandler OnTouchEnd;

    public delegate void OnTouchCancelHandler(int fingerdID, Vector2 endPosition);
    public static event OnTouchCancelHandler OnTouchCancel;

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
            for (int i = 0; i < Input.touchCount; ++i)
            {
                Touch touch = Input.touches[i];
                if (touch.phase == TouchPhase.Began && OnTouchStart != null)
                {
                    OnTouchStart(touch.fingerId, touch.position);
                }
                else if (touch.phase == TouchPhase.Stationary && OnTouchHold != null)
                {
                    OnTouchHold(touch.fingerId, touch.position);
                }
                else if (touch.phase == TouchPhase.Moved && OnTouchMove != null)
                {
                    OnTouchMove(touch.fingerId, touch.position);
                }
                else if (touch.phase == TouchPhase.Ended && OnTouchEnd != null)
                {
                    OnTouchEnd(touch.fingerId, touch.position);
                }
                else if (touch.phase == TouchPhase.Canceled && OnTouchCancel != null)
                {
                    OnTouchCancel(touch.fingerId, touch.position);
                }
            }
        }
    }
}