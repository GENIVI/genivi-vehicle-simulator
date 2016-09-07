/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class DriverCamera : MonoBehaviour {

    private CamSmoothFollow smoothFollow;
    private CamFixTo fixTo;

    public GameObject left;
    public GameObject center;
    public GameObject right;

    public AdminScreen.DisplayType cameraSetup = AdminScreen.DisplayType.FLAT;

    public bool showControl = false;

    public string playerCarLayer;

    private float angle;

    public void Awake()
    {
        
        fixTo = GetComponent<CamFixTo>();
        smoothFollow = GetComponent<CamSmoothFollow>();
        //angle = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(center.camera.fieldOfView * Mathf.Deg2Rad / 2) * center.camera.aspect);

       // AppController.Instance.AdminInput.OpenCamSettings += ToggleCamSettings;
        if (AppController.Instance.appSettings.projectorBlend)
        {
            AdminSettings.Instance.displayType = AdminScreen.DisplayType.PARABOLIC;
        }
        else
        {
            AdminSettings.Instance.displayType = AdminScreen.DisplayType.FLAT;
        }
        cameraSetup = AdminSettings.Instance.displayType;



        SetCameraType(AdminSettings.Instance.displayType, AdminSettings.Instance.selectFov);

        SetFarClip(AdminSettings.Instance.camFarClip);

        SetNearClip(AdminSettings.Instance.camNearClip);

    }

    public void SetCameraType(AdminScreen.DisplayType newType, float fov)
    {
        
        cameraSetup = newType;
        angle = fov;
        RecalculateCam();
    }

    public void SetFarClip(float farClip)
    {
        left.GetComponent<Camera>().farClipPlane = farClip;
        center.GetComponent<Camera>().farClipPlane = farClip;
        right.GetComponent<Camera>().farClipPlane = farClip;
    }

    public void SetNearClip(float nearClip)
    {
        left.GetComponent<Camera>().nearClipPlane = nearClip;
        right.GetComponent<Camera>().nearClipPlane = nearClip;
        center.GetComponent<Camera>().nearClipPlane = nearClip;
    }

    public void SetFoV(float fov)
    {
        angle = fov;
        RecalculateCam();
    }

    public void OnDestroy() {
     //   if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
     //   {
     //       AppController.Instance.AdminInput.OpenCamSettings -= ToggleCamSettings;
     //   }
    }

    public void ToggleCamSettings() {
        showControl = !showControl;
    }

    void OnGUI() {
        if(showControl) {
            GUI.Box(new Rect(40, 90, 600, 150), "Cam Settings");
            float oldAngle = angle;
            angle = GUI.HorizontalSlider(new Rect(50, 130, 400, 80), angle, 1f, 60f);
            GUI.Label(new Rect(455, 130, 150, 100), "ANGLE: " + angle);
            if(oldAngle != angle)
                RecalculateCam();
        }
    }

    void RecalculateCam()
    {
        var lCam = left.GetComponent<Camera>();
        var cCam = center.GetComponent<Camera>();
        var rCam = right.GetComponent<Camera>();
        
        if(cameraSetup == AdminScreen.DisplayType.PARABOLIC)
        {
            lCam.enabled = false;
            rCam.enabled = false;
            //cCam.aspect = 4992f / 1080f;
            cCam.rect = new Rect(0f, 0f, 1f, 1f);
            cCam.fieldOfView = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(angle* Mathf.Deg2Rad / 2) / cCam.aspect);
        }
        else if (cameraSetup == AdminScreen.DisplayType.FLAT)
        {
            lCam.enabled = false;
            rCam.enabled = false;
            //cCam.aspect = 5760f / 1080f;
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

            var vFov = Mathf.Rad2Deg * 2 * Mathf.Atan(Mathf.Tan(angle/3 * Mathf.Deg2Rad / 2) / cCam.aspect);

            lCam.fieldOfView = vFov;
            cCam.fieldOfView = vFov;
            rCam.fieldOfView = vFov;

            cCam.transform.localRotation = Quaternion.identity;
            lCam.transform.localRotation = Quaternion.Euler(0, -(angle/3), 0);
            rCam.transform.localRotation = Quaternion.Euler(0, angle/3, 0);
        }


    }

    //init first-person only
    public void Init(Transform cameraPos)
    {
        fixTo.fixTo = cameraPos;
    }

    public void ViewInside()
    {
        fixTo.enabled = true;
        smoothFollow.enabled = false;
        LayerMask mask = center.GetComponent<Camera>().cullingMask & ~(1 << LayerMask.NameToLayer(playerCarLayer));
        center.GetComponent<Camera>().cullingMask = mask;
        left.GetComponent<Camera>().cullingMask = mask;
        right.GetComponent<Camera>().cullingMask = mask;
    }

    public void ViewThirdPerson()
    {
        fixTo.enabled = false;
        smoothFollow.enabled = true;
        LayerMask mask = center.GetComponent<Camera>().cullingMask | (1 << LayerMask.NameToLayer(playerCarLayer));
        center.GetComponent<Camera>().cullingMask = mask;
        left.GetComponent<Camera>().cullingMask = mask;
        right.GetComponent<Camera>().cullingMask = mask;
    }

    public void SetCulingMask(int mask)
    {
        center.GetComponent<Camera>().cullingMask = mask;
        left.GetComponent<Camera>().cullingMask = mask;
        right.GetComponent<Camera>().cullingMask = mask;
    }

    public void SwitchView()
    {
        if(fixTo.enabled)
        {
            ViewThirdPerson();
        }
        else
        {
            ViewInside();
        }
    }

    public static float ScaleFov(float fovAtThreeScreens)
    {
        var fov = fovAtThreeScreens;
        return fov * 4 / 5 + (fov / 5) / 2 * (((float)Screen.width / Screen.height) / (16f / 9f) - 1);
    }
}
