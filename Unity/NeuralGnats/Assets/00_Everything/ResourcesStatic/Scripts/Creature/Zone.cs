using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zone : MonoBehaviour
{
    public delegate void TouchEvent();
    public TouchEvent eventTouched;

    public int numTouches = 0;

    private void Start()
    {
        eventTouched += () =>
        {
            ++numTouches;
        };
    }
}
