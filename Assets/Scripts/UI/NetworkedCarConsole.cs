/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class NetworkedCarConsole : MonoBehaviour
{

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

    public GUIStyle clockStyle;
    public Rect clockPosition;
    public bool showClock = false;
    public GUIStyle dateStyle;
    public Rect datePosition;
    public bool showDate = false;


    public CarConsoleController controller;

    private System.Globalization.CultureInfo cultureInfo;
    private string currentTime = "";
    private string currentDate = "";

    private float speedMPH = 0f;


    private float smoothedSpeedAngle = 0f;
    private float smoothedRPMAngle = 0f;

    private const float SMOOTHING_RATE = 1f;

    public float SpeedMPH
    {
        get
        {
            return speedMPH;
        }
    }


    // Use this for initialization
    void Start()
    {
        cultureInfo = System.Globalization.CultureInfo.CreateSpecificCulture("en-us");
        rpmAngle = minRpmAngle;
        speedAngle = minSpeedAngle;

        currentColor = Color.clear;
    }

    public bool running = false;
    private float currentAlpha = 0f;
    public Color currentColor = Color.clear;

    void Update()
    {
        if (running)
        {
            if (controller != null)
            {
                SetSpeed(controller.Speed/3.6f);
                SetRpm(controller.RPM);

            }
            currentTime = System.DateTime.Now.ToString("t", cultureInfo);
            currentDate = System.DateTime.Now.ToString("d", cultureInfo);

            smoothedRPMAngle = Mathf.Lerp(smoothedRPMAngle, rpmAngle, Time.deltaTime * SMOOTHING_RATE);
            smoothedSpeedAngle = Mathf.Lerp(smoothedSpeedAngle, speedAngle, Time.deltaTime * SMOOTHING_RATE);

        }

        if (running)
            currentAlpha = Mathf.MoveTowards(currentAlpha, 1f, Time.deltaTime * 2f);
        else
            currentAlpha = Mathf.MoveTowards(currentAlpha, 0f, Time.deltaTime * 2f);

        currentColor = Color.Lerp(Color.clear, Color.white, currentAlpha);

    }

    void OnGUI()
    {
        if (currentAlpha > 0.01f)
        {
            Color old = GUI.color;
            GUI.color = currentColor;

            GUI.DrawTexture(consoleRect, consolebg);

            if(showClock)
                GUI.Label(clockPosition, currentTime, clockStyle);

            if (showDate)
                GUI.Label(datePosition, currentDate, dateStyle);

            Matrix4x4 nonRotated = GUI.matrix;

            GUIUtility.RotateAroundPivot(smoothedRPMAngle, rpmPivot);
            GUI.DrawTexture(rpmNeedleRect, rpmNeedle);

            GUI.matrix = nonRotated;
            GUIUtility.RotateAroundPivot(smoothedSpeedAngle, speedPivot);
            GUI.DrawTexture(speedNeedleRect, speedNeedle);




            GUI.color = old;
        }
        
    }

    public void FadeOut()
    {
        running = false;
    }

    public void FadeIn()
    {
        running = true;
        speedAngle = minSpeedAngle;
        smoothedSpeedAngle = minSpeedAngle;
        rpmAngle = minRpmAngle;
        smoothedRPMAngle = minRpmAngle;
    }

    public void SetSpeed(float metersPerSecond)
    {
        speedMPH = Mathf.Clamp(metersPerSecond * MPSTOMPH, 0, 160); 
        speedAngle =  speedMPH * (hundredMileAngle - minSpeedAngle) / 100f + minSpeedAngle;
    }

    public void SetRpm(float rpm)
    {
        rpmAngle = Mathf.Clamp(rpm, 0, 7000) * (fiveRpmAngle - minRpmAngle) / 5000f + minRpmAngle;
    }
}