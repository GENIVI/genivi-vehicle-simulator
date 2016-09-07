/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class InfractionReviewUI : MonoBehaviour {

    public Texture2D bg;
    public Rect areaRect;
    public Rect bgRect;
    public Rect screenshotRect;
    public Vector2 detailTitles;
    public Vector2 detailDetails;


    public UILabel title;
    public Texture2D screenshot;

    public GUIStyle titleStyle;
    public GUIStyle detailStyle;
    public GUIStyle listStyle;
    public GUIStyle listSelectedStyle;

    private DrivingInfraction infractionDetails;
    private List<DrivingInfraction> infractions;
    public float[] listColumns;
    public string[] listTitles;

    private int selectedId = 1;

    void Awake()
    {
        selectedId = -1;

    }

    public void Enable(int selected)
    {
        infractions = InfractionController.Instance.GetInfractions();
        selectedId = selected;
        infractionDetails = infractions.Where(i => i.id == selected).FirstOrDefault();
    }

    public void Disable()
    {
        selectedId = -1;
    }

    void OnGUI()
    {

        if (selectedId < 0)
            return;

        Matrix4x4 oldMatrix = GUI.matrix;

        
        if (AdminSettings.Instance.displayType == AdminScreen.DisplayType.PARABOLIC)
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.height / 1080f, Screen.height / 1080f, 1f));
            GUI.BeginGroup(new Rect(Screen.width/2 - 525, 255, 1200, 800));
        }
        else
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.height / 1080f, Screen.height / 1080f, 1f));
            GUI.BeginGroup(new Rect(Screen.width / 2 - 525, 255, 1200, 700));
        }
        

      
        GUI.DrawTexture(bgRect, bg);

        GUI.Label(title.location, title.text, titleStyle);

        GUI.DrawTexture(screenshotRect, screenshot);

        string[] infractionTitles = new string[] { "INFRACTION " + infractionDetails.id, "Type", "Current Speed", "Scene", "System Time", "Session Time" };
        string[] infractionDetail = new string[] {
            InfractionController.GetFullInfractionType(infractionDetails), 
            (infractionDetails.speed * NetworkedCarConsole.MPSTOMPH).ToString("F2") + " MPH", 
            InfractionController.Instance.lastScene,
            infractionDetails.systemTime.ToShortTimeString(),
            System.TimeSpan.FromSeconds(infractionDetails.sessionTime).ToString()
        };

        for (int t = 0; t < infractionTitles.Length; t++)
        {
            GUI.Label(new Rect(detailTitles.x, detailTitles.y + t * 24, 150, 150), infractionTitles[t], detailStyle);
        }

        for (int d = 0; d < infractionDetail.Length; d++)
        {
            GUI.Label(new Rect(detailDetails.x, detailDetails.y + d * 24, 150, 150), infractionDetail[d], detailStyle);
        }

        for (int i = 0; i < listTitles.Length; i++)
        {
            GUI.Label(new Rect(listColumns[i], 358, 100, 50), listTitles[i], listSelectedStyle);
        }

        for (int inf = 0; inf < infractions.Count; inf++)
        {
            var fraction = infractions[inf];
            GUIStyle style = (fraction.id == selectedId) ? listSelectedStyle : listStyle;

            GUI.Label(new Rect(listColumns[0], 376 + 18 * inf, 100, 50), fraction.id.ToString(), style);
            GUI.Label(new Rect(listColumns[1], 376 + 18 * inf, 100, 50), fraction.type, style);
            GUI.Label(new Rect(listColumns[2], 376 + 18 * inf, 100, 50), fraction.systemTime.ToShortTimeString(), style);
            GUI.Label(new Rect(listColumns[3], 376 + 18 * inf, 100, 50), System.TimeSpan.FromSeconds(fraction.sessionTime).ToString(), style);
            GUI.Label(new Rect(listColumns[4], 376 + 18 * inf, 100, 50), (fraction.speed * NetworkedCarConsole.MPSTOMPH).ToString("F2") + " MPH", style);
        }



        GUI.EndGroup();

        GUI.matrix = oldMatrix;

    }
}
