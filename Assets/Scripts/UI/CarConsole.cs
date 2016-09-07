/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class CarConsole : MonoBehaviour {

    public Texture2D consolebg;
    public Texture2D speedNeedle;
    public Texture2D rpmNeedle;
    public Rect consoleRect;
    public Rect speedNeedleRect;
    public Rect rpmNeedleRect;
    public Vector2 speedPivot;
    public Vector2 rpmPivot;

    public float rpmAngle;
    public float speedAngle;
    public float minSpeedAngle;
    public float minRpmAngle;

    public float hundredMileAngle;
    public float fiveRpmAngle;

    public const float MPSTOMPH = 2.23694f;

    public float targetAlpha = 0f;

    private float currentAlpha = 0f;

    // Use this for initialization
    void Start () {
        rpmAngle = minRpmAngle;
        speedAngle = minSpeedAngle;
    }

    void Update()
    {
        if(currentAlpha != targetAlpha) {
            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, Time.deltaTime);
        }
    }

    void OnGUI()
    {
        if(currentAlpha > 0f)
        {
            Color oldColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, currentAlpha);
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 7680f, Screen.height / 1080f, 1f));
            GUI.DrawTexture(consoleRect, consolebg);
            Matrix4x4 nonRotated = GUI.matrix;

            GUIUtility.RotateAroundPivot(rpmAngle, rpmPivot * Screen.width / 7680f);
            GUI.DrawTexture(rpmNeedleRect, rpmNeedle);

            GUI.matrix = nonRotated;
            GUIUtility.RotateAroundPivot(speedAngle, speedPivot * Screen.width / 7680f);
            GUI.DrawTexture(speedNeedleRect, speedNeedle);

            GUI.matrix = old;
            GUI.color = oldColor;
        }
    }



    public void SetSpeed(float metersPerSecond)
    {
        speedAngle = metersPerSecond *MPSTOMPH * (hundredMileAngle - minSpeedAngle) / 100f + minSpeedAngle;
    }

}
