/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class TrafficLightVisuals : MonoBehaviour
{

    public Material green;
    public Material yellow;
    public Material red;
    public Renderer rend;

    void Awake()
    {
        rend.material = red;
    }

    public void Set(TrafLightState state)
    {
        switch (state) {
            case TrafLightState.RED:
                rend.material = red;
                break;
            case TrafLightState.YELLOW:
                rend.material = yellow;
                break;
            case TrafLightState.GREEN:
                rend.material = green;
                break;
        }
    }
}
