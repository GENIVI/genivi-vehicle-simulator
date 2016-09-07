/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class ShowControls : MonoBehaviour {

    private bool opened = false;

    private Rect rect = new Rect(7380, 30, 270, 400);
    public Texture2D white;

    void OnGUI()
    {
        
        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width/5760f, Screen.height/1080f,1));
        //clear screen
        GUI.depth = -9999;
        GUI.color = new Color(0.4f, 0.4f, 0.4f);
        GUI.DrawTexture(new Rect(5760, 0, 1920, 1080), white);
        GUI.color = Color.white;
        if(!opened)
        {
            GUI.Label(new Rect(rect.x, rect.y, rect.width, rect.height), "Press Esc to show controls");
        }
        else
        {
            GUI.Box(rect, "Controls");
            GUILayout.BeginArea(rect);
                GUILayout.BeginVertical();
                    GUILayout.Space(30);
                    GUILayout.Label("Admin Controls");
                    GUILayout.Label("Up Arrow - Go back");
                    GUILayout.Label("F - Open Camera Settings");
            //        GUILayout.Label("Z - Swap input mode");
            //        GUILayout.Label("+ - Volume up");
            //        GUILayout.Label("- - Volume down");
                    GUILayout.Label("I - Toggle stats");
            //        GUILayout.Label("C - Toggle console visibility");
            //        GUILayout.Label("W - Toggle steering wheel visibility");
                    GUILayout.Label("K - End driving scene");
                    GUILayout.Label("V - Change view");
             //       GUILayout.Label("1 - Trigger obstacle 1");
                    GUILayout.FlexibleSpace();
                    GUILayout.Label("Press Esc to close this menu");
                GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            opened = !opened;
        }
    }
}
