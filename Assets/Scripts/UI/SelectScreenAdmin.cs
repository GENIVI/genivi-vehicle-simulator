/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

[System.Serializable]
public class PositionedTexture
{
    public Texture2D tex;
    public Texture2D texActive;
    public Rect position;

    public void InitPos() {
        position = new Rect(0, 0, tex.width, tex.height);
    }
}


[System.Serializable]
public class KeyItem
{
    public string label;
    public Texture2D tex;
    public Texture2D texActive;
    public bool isActive = false;
}

public class SelectScreenAdmin : MonoBehaviour
{

    public Rect area;

    public PositionedTexture global;
    public PositionedTexture selectScreen;
    public PositionedTexture vehicle;
    public PositionedTexture environment;
    public PositionedTexture roadSide;

    public float titleSpace = 18f;
    public float subTitleSpace = 11f;
    public float leftSpace = 39f;
    public float descriptionSpace = 10f;
    public float keySpace = 7f;
    public float subtitleAfterSpace = 15f;

    public KeyItem[] globalItems;
    public KeyItem[] vehicleItems;
    public KeyItem[] sceneItems;
    public KeyItem[] roadItems;
    public KeyItem backItem;

    public GUIStyle titleStyle;
    public GUIStyle subtitleStyle;
    public GUIStyle descriptionStyle;
    public GUIStyle descriptionActiveStyle;
    public GUIStyle keyImageStyle;

    private float vehicleBgAlpha = 0f;
    private float environmentAlpha = 0f;
    private float roadAlpha = 0f;

    public float alphaFadeSpeed = 1f;

    public float vehicleTargetAlpha = 1f;
    public float environmentTargetAlpha = 0f;
    public float roadTargetAlpha = 0f;


    public void Update()
    {
        vehicleBgAlpha = Mathf.MoveTowards(vehicleBgAlpha, vehicleTargetAlpha, alphaFadeSpeed * Time.deltaTime);
        roadAlpha = Mathf.MoveTowards(roadAlpha, roadTargetAlpha, alphaFadeSpeed * Time.deltaTime);
        environmentAlpha = Mathf.MoveTowards(environmentAlpha, environmentTargetAlpha, alphaFadeSpeed * Time.deltaTime);
    }

    public void SetStage(int stage)
    {
        vehicleTargetAlpha = 0f;
        roadTargetAlpha = 0f;
        environmentTargetAlpha = 0f;

        if(stage <= 0)
        {
            vehicleTargetAlpha = 1f;
        }
        else if(stage == 1)
        {
            environmentTargetAlpha = 1f;
        }
        else if(stage >= 2)
        {
            roadTargetAlpha = 1f;
        }
    }

    private int ConvertChoice(int stage, int choice)
    {
        if(stage != 0)
            return choice;

        if(choice == 0)
            return 2;
        else if(choice == 1)
            return 3;
        else if(choice == 2)
            return 1;
        else if(choice == 3)
            return 0;

        return choice;
    }

    public void SelectChoice(int stage, int choice)
    {
        choice = ConvertChoice(stage, choice);
        KeyItem[] items = new KeyItem[0];
        if(stage <= 0)
        {
            items = vehicleItems;
        }
        else if(stage == 1)
        {
            items = sceneItems;
        }
        else if(stage >= 2)
        {
            items = roadItems;
        }

        for(int i = 0; i < items.Length; i++)
        {
            if(i == choice)
            {
                items[i].isActive = true;
            }
            else
            {
                items[i].isActive = false;
            }
        }
    }

    public void ClearChoice(int stage)
    {
        KeyItem[] items = new KeyItem[0];
        if(stage <= 0)
        {
            items = vehicleItems;
        }
        else if(stage == 1)
        {
            items = sceneItems;
        }
        else if(stage >= 2)
        {
            items = roadItems;
        }

        for(int i = 0; i < items.Length; i++)
        {
            items[i].isActive = false;
        }
    }

    private string buildString = "build: ";
    void Start()
    {
        buildString = buildString + ShowBuild.GetBuildNumber() + " " + ShowBuild.GetBuildType();
    }

    void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 7680f, Screen.height / 1080f, 1f));

        GUI.Label(new Rect(5868, 74, 500, 200), buildString, titleStyle);

        GUI.BeginGroup(area);
        GUI.DrawTexture(global.position, global.tex);

        //global
        GUILayout.BeginArea(global.position);
        GUILayout.BeginHorizontal();
        GUILayout.Space(leftSpace);
        GUILayout.BeginVertical();
        GUILayout.Space(titleSpace);
        GUILayout.Label("GLOBAL CONTROLS", titleStyle);
        GUILayout.FlexibleSpace();
        foreach(var item in globalItems)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.isActive ? item.texActive : item.tex, keyImageStyle);
            GUILayout.Space(descriptionSpace);
            GUILayout.Label(item.label, item.isActive ? descriptionActiveStyle : descriptionStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(keySpace);
        }
        GUILayout.Space(33f - keySpace);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        //"active" backgrounds
        GUI.DrawTexture(selectScreen.position, selectScreen.tex);
        Color c = GUI.color;
        c.a = vehicleBgAlpha;
        GUI.color = c;
        GUI.DrawTexture(vehicle.position, vehicle.tex, ScaleMode.StretchToFill, true);

        c.a = environmentAlpha;
        GUI.color = c;
        GUI.DrawTexture(environment.position, environment.tex, ScaleMode.StretchToFill, true);

        c.a = roadAlpha;
        GUI.color = c;
        GUI.DrawTexture(roadSide.position, roadSide.tex, ScaleMode.StretchToFill, true);
   
        c.a = 1f;
        GUI.color = c;


        GUILayout.BeginArea(selectScreen.position);
        GUILayout.BeginHorizontal();
        GUILayout.Space(leftSpace);
        GUILayout.BeginVertical();

        //vehicle
        GUILayout.Space(titleSpace);
        GUILayout.Label("SELECTION SCREEN CONTROLS", titleStyle);
        GUILayout.Space(subTitleSpace * 2);
        GUILayout.Label("VEHICLE SELECTION", subtitleStyle);
        GUILayout.Space(subtitleAfterSpace);
        foreach(var item in vehicleItems)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.isActive ? item.texActive : item.tex, keyImageStyle);
            GUILayout.Space(descriptionSpace);
            GUILayout.Label(item.label, item.isActive ? descriptionActiveStyle : descriptionStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(keySpace);
        }
        GUILayout.Space(subtitleAfterSpace - keySpace);

        //scene
        GUILayout.Space(subTitleSpace);
        GUILayout.Label("SCENE SELECTION", subtitleStyle);
        GUILayout.Space(subtitleAfterSpace);
        foreach(var item in sceneItems)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.isActive ? item.texActive : item.tex, keyImageStyle);
            GUILayout.Space(descriptionSpace);
            GUILayout.Label(item.label, item.isActive ? descriptionActiveStyle : descriptionStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(keySpace);
        }
        GUILayout.Space(subtitleAfterSpace - keySpace);
        
        //road side
        GUILayout.Space(subTitleSpace);
        GUILayout.Label("ROAD SIDE SELECTION", subtitleStyle);
        GUILayout.Space(subtitleAfterSpace);
        foreach(var item in roadItems)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.isActive ? item.texActive : item.tex, keyImageStyle);
            GUILayout.Space(descriptionSpace);
            GUILayout.Label(item.label, item.isActive ? descriptionActiveStyle : descriptionStyle);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.Space(keySpace);
        }
        GUILayout.Space(subtitleAfterSpace - keySpace);
        GUILayout.Space(subTitleSpace);

        //back
        GUILayout.BeginHorizontal();
        GUILayout.Label(backItem.isActive ? backItem.texActive : backItem.tex, keyImageStyle);
        GUILayout.Space(descriptionSpace);
        GUILayout.Label(backItem.label, backItem.isActive ? descriptionActiveStyle : descriptionStyle);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        GUILayout.Space(33f);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        GUI.EndGroup();

        GUI.matrix = oldMatrix;
    }
}
