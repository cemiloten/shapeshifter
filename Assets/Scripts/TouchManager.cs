using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchInfo : IEquatable<TouchInfo>
{
    private Vector2 startPosition;
    private Touch touch;

    public int FingerId => touch.fingerId;
    public TouchPhase Phase => touch.phase;
    public Vector2 Position => touch.position;
    public Vector2 StartPosition => startPosition;

    public TouchInfo(Touch touch)
    {
        this.touch = touch;
        startPosition = touch.position;
    }

    public void UpdateInfo(Touch touch)
    {
        this.touch = touch;
    }

    public bool Equals(TouchInfo other)
    {
        return other == null ? false : other.touch.fingerId == touch.fingerId;
    }

    public override bool Equals(object o)
    {
        if (o == null)
            return false;
        return o as TouchInfo == null ? false : Equals(o);
    }

    public override int GetHashCode() { return base.GetHashCode(); }

    public static bool operator ==(TouchInfo t1, TouchInfo t2)
    {
        if ((object)t1 == null || (object)t2 == null)
            return object.Equals(t1, t2);
        return t1.Equals(t2);
    }

    public static bool operator !=(TouchInfo t1, TouchInfo t2)
    {
        if ((object)t1 == null || (object)t2 == null)
            return !object.Equals(t1, t2);
        return !t1.Equals(t2);
    }
}

public static class TouchManager
{
    public delegate void OnTouchCountChangedHandler();
    public delegate void OnTouchStartHandler(TouchInfo touchInfo);
    public delegate void OnTouchHoldHandler(TouchInfo touchInfo);
    public delegate void OnTouchMoveHandler(TouchInfo touchInfo);
    public delegate void OnTouchEndHandler(TouchInfo touchInfo);
    public delegate void OnTouchCancelHandler(TouchInfo touchInfo);
    public static event OnTouchCountChangedHandler OnTouchCountChanged;
    public static event OnTouchStartHandler OnTouchStart;
    public static event OnTouchHoldHandler OnTouchHold;
    public static event OnTouchMoveHandler OnTouchMove;
    public static event OnTouchEndHandler OnTouchEnd;
    public static event OnTouchCancelHandler OnTouchCancel;

    private static List<TouchInfo> touchInfos = new List<TouchInfo>();

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
                Touch currentTouch = Input.touches[i];
                TouchInfo touchInfo = GetTouchInfo(currentTouch);

                if (touchInfo == null)
                    touchInfos.Add(new TouchInfo(currentTouch));
                else
                    touchInfo.UpdateInfo(currentTouch);

                switch (currentTouch.phase)
                {
                    case TouchPhase.Began:
                        OnTouchStart?.Invoke(touchInfo);
                        break;

                    case TouchPhase.Stationary:
                        OnTouchHold?.Invoke(touchInfo);
                        break;

                    case TouchPhase.Moved:
                        OnTouchMove?.Invoke(touchInfo);
                        break;

                    case TouchPhase.Ended:
                        touchInfos.Remove(touchInfo);
                        OnTouchEnd?.Invoke(touchInfo);
                        break;

                    case TouchPhase.Canceled:
                        touchInfos.Remove(touchInfo);
                        OnTouchCancel?.Invoke(touchInfo);
                        break;

                    default:
                        continue;
                }
            }
        }
    }

    private static TouchInfo GetTouchInfo(Touch touch)
    {
        for (int i = 0; i < touchInfos.Count; ++i)
        {
            if (touchInfos[i].FingerId == touch.fingerId)
                return touchInfos[i];
        }
        return null;
    }

}