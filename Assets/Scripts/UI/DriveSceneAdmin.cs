/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class DriveSceneAdmin : MonoBehaviour {

    public Rect area;
   
    public PositionedTexture global;
    public PositionedTexture selectScreen;
    public PositionedTexture confirmBox;

    public PositionedTexture map;
    public Texture2D indicator;

    public float titleSpace = 18f;
    public float subTitleSpace = 11f;
    public float leftSpace = 39f;
    public float descriptionSpace = 10f;
    public float keySpace = 7f;
    public float subtitleAfterSpace = 15f;

    public KeyItem[] globalItems;
    public KeyItem[] adminItems;
    public KeyItem[] repositionItems;
    public KeyItem[] confirmItems;

    public GUIStyle titleStyle;
    public GUIStyle subtitleStyle;
    public GUIStyle descriptionStyle;
    public GUIStyle descriptionActiveStyle;
    public GUIStyle keyImageStyle;
    public GUIStyle statsStyle;
    
    
    public bool isShowingEndScene = false;
    public Vector3 blMap;
    public Vector3 tlMap;
    public Vector3 carPosition;

    private string buildString = "build: ";
    void Start()
    {
        buildString = buildString + ShowBuild.GetBuildNumber() + " " + ShowBuild.GetBuildType();
    }

    Rect GetIndicatorPosition()
    {
        float xPc = (carPosition.x - blMap.x) / (tlMap.x - blMap.x);
        float zPc = (carPosition.z - blMap.z) / (tlMap.z - blMap.z);

        return new Rect(map.position.x + (xPc * map.position.width) - (indicator.width / 2),
                        map.position.y + (1f - zPc) * map.position.height - (indicator.height / 2),
                        indicator.width, indicator.height);

    }

    void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;
        GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 7680f, Screen.height / 1080f, 1f));

        GUI.Label(new Rect(5868, 74, 500, 200), buildString, statsStyle);
        GUI.Label(new Rect(5868, 130, 500, 200), "far clip plane: " + TrackController.Instance.driverCam.GetComponent<Camera>().farClipPlane, statsStyle);
        float oldclip = TrackController.Instance.driverCam.GetComponent<Camera>().farClipPlane;
        float newClip = GUI.HorizontalSlider(new Rect(5898, 190, 300, 70), oldclip, 10f, 8000f);
        if(oldclip != newClip)
        {
            TrackController.Instance.driverCam.GetComponent<Camera>().farClipPlane = newClip;
        }

        GUI.DrawTexture(map.position, map.tex);

        GUI.DrawTexture(GetIndicatorPosition(), indicator);

        if(isShowingEndScene)
        {
            GUI.DrawTexture(confirmBox.position, confirmBox.tex);
            GUILayout.BeginArea(confirmBox.position);
            GUILayout.BeginHorizontal();
            GUILayout.Space(leftSpace);
            GUILayout.BeginVertical();
            GUILayout.Space(titleSpace);
            GUILayout.Label("CONFIRM END SCENE?", titleStyle);
            GUILayout.FlexibleSpace();
            foreach(var item in confirmItems)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(item.isActive ? item.texActive : item.tex, keyImageStyle);
                GUILayout.Space(descriptionSpace);
                GUILayout.Label(item.label, item.isActive ? descriptionActiveStyle : descriptionStyle);
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
                GUILayout.Space(keySpace);
            }
            GUILayout.Space(subTitleSpace);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

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


        GUI.DrawTexture(selectScreen.position, selectScreen.tex);

        GUILayout.BeginArea(selectScreen.position);
        GUILayout.BeginHorizontal();
        GUILayout.Space(leftSpace);
        GUILayout.BeginVertical();
        GUILayout.Space(titleSpace);
        GUILayout.Label("ADMIN INPUT", titleStyle);
        GUILayout.FlexibleSpace();

        foreach(var item in adminItems)
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
        GUILayout.Label("REPOSITION ON TRACK", subtitleStyle);
        GUILayout.Space(subtitleAfterSpace);

        foreach(var item in repositionItems)
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
        GUI.EndGroup();
        GUI.matrix = oldMatrix;
    }
}
