/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class CarHeadlights : DayNightEventListener {

    [AdminRange("headlight angle", -20, 10)]
    public float yAngle = 0f;

    public float outsideAngle = 4f;
    public Light left;
    public Light right;

    public void Update()
    {
        left.transform.localRotation = Quaternion.Euler(yAngle, -outsideAngle, 0f);
        right.transform.localRotation = Quaternion.Euler(yAngle, outsideAngle, 0f);
    }

    public void SetRange(float newRange)
    {
        left.range = newRange;
        right.range = newRange;
    }

    public bool GetState()
    {
        return left.gameObject.activeInHierarchy;
    }

    public void SetHeadlights(bool state)
    {
        left.gameObject.SetActive(state);
        right.gameObject.SetActive(state);
    }

    protected override void OnDay()
    {
        SetHeadlights(false);
    }

    protected override void OnNight()
    {
        SetHeadlights(true);
    }
}
