/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class TextMPHDisplay : MonoBehaviour {

    public GUIStyle mphStyle;
    public GUIStyle speedStyle;
    public Rect mphRect;
    public Rect speedRect;

    public NetworkedCarConsole console;

    void OnGUI()
    {
        if (console.currentColor.a > 0.01f)
        {
            GUI.color = console.currentColor;
            GUI.depth = -1;
            GUI.Label(speedRect, Mathf.RoundToInt(console.SpeedMPH).ToString(), speedStyle);
            GUI.Label(mphRect, "MPH", mphStyle);
            
        }
    }

}
