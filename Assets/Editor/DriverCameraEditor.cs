/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

[CustomEditor(typeof(DriverCamera), true)]
public class DriverCameraEditor : Editor
{

    public float angle;
    public float hFov;


    void OnEnable()
    {
        var t = target as DriverCamera;

        if(t.cameraSetup == AdminScreen.DisplayType.PARABOLIC || t.cameraSetup == AdminScreen.DisplayType.FLAT)
        {
            angle = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(t.center.GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad / 2) * t.center.GetComponent<Camera>().aspect);
        }
        else
        {
            angle = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(t.center.GetComponent<Camera>().fieldOfView * Mathf.Deg2Rad / 2) * t.center.GetComponent<Camera>().aspect);
            angle = Mathf.Atan(3 * Mathf.Tan(angle/2 * Mathf.Deg2Rad)) * Mathf.Rad2Deg * 2;
        }
    }

    void OnDisable()
    {

    }

    public override void OnInspectorGUI()
    {

        var t = target as DriverCamera;

        var oldT = t.cameraSetup;

        DrawDefaultInspector();

        if(t.cameraSetup != oldT)
            RecalculateCam(t);

        EditorGUILayout.Separator();

        float oldAngle = angle;
        float oldFov = hFov;

        angle = EditorGUILayout.Slider("HFoV", angle, 0, 175f);

        if(angle != oldAngle || hFov != oldFov)
            RecalculateCam(t);
    
    }

    void RecalculateCam(DriverCamera cam)
    {
        var lCam = cam.left.GetComponent<Camera>();
        var cCam = cam.center.GetComponent<Camera>();
        var rCam = cam.right.GetComponent<Camera>();

        if(cam.cameraSetup == AdminScreen.DisplayType.PARABOLIC)
        {
            lCam.enabled = false;
            rCam.enabled = false;
            cCam.aspect = 4992f / 1080f;
            cCam.rect = new Rect(0f, 0f, 1f, 1f);
            cCam.fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(angle * Mathf.Deg2Rad / 2) / cCam.aspect);
        }
        else if (cam.cameraSetup == AdminScreen.DisplayType.FLAT)
        {
            lCam.enabled = false;
            rCam.enabled = false;
            cCam.aspect = 5760f / 1080f;
            cCam.rect = new Rect(0f, 0f, 1f, 1f);
            cCam.fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(angle * Mathf.Deg2Rad / 2) / cCam.aspect);
        }
        else
        {
            lCam.enabled = true;
            cCam.enabled = true;
            rCam.enabled = true;

            lCam.aspect = 16f / 9f;
            cCam.aspect = 16f / 9f;
            rCam.aspect = 16f / 9f;

            lCam.rect = new Rect(0f, 0f, 0.25f, 1f);
            cCam.rect = new Rect(0.25f, 0f, 0.25f, 1f);
            rCam.rect = new Rect(0.5f, 0f, 0.25f, 1f);

            var mult = Mathf.Atan(Mathf.Tan(angle * Mathf.Deg2Rad / 2) / 3f) * 2;

            var vFov = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(mult / 2) / cCam.aspect);

            lCam.fieldOfView = vFov;
            cCam.fieldOfView = vFov;
            rCam.fieldOfView = vFov;

            cCam.transform.localRotation = Quaternion.identity;
            lCam.transform.localRotation = Quaternion.Euler(0, -(mult * Mathf.Rad2Deg), 0);
            rCam.transform.localRotation = Quaternion.Euler(0, mult * Mathf.Rad2Deg, 0);
        }



    }

    void OnSceneGUI()
    {
    }


}