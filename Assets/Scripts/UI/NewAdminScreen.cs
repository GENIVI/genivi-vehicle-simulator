/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class NewAdminScreen : MonoBehaviour {

    public GameObject admin;
    public GameObject vehicleStettings;
    public GameObject map;
    public GameObject panel;

    public DriveAdminUISettings settings;
    public VehicleController vehicle;

    public void ResetDefaults()
    {
        gameObject.BroadcastMessage("OnResetDefaults", SendMessageOptions.DontRequireReceiver);
    }

    public void Init()
    {
        vehicle = TrackController.Instance.car.GetComponent<VehicleController>();
        ResetDefaults();    
    }

    public void Update()
    {
        if(Input.GetKeyDown(KeyCode.F1))
        {
            if(panel.activeSelf)
            {
                panel.SetActive(false);
            } else
            {
                panel.SetActive(true);
                ResetDefaults();
            }
        }
    }

    public void SelectAdmin()
    {
        admin.SetActive(true);
        vehicleStettings.SetActive(false);
        map.SetActive(false);
        ResetDefaults();
    }

    public void SelectVehicleSettings()
    {
        admin.SetActive(false);
        vehicleStettings.SetActive(true);
        map.SetActive(false);
        ResetDefaults();
    }

    public void SelectMap()
    {
        admin.SetActive(false);
        vehicleStettings.SetActive(false);
        map.SetActive(true);
        ResetDefaults();
    }

    public void TriggerObstacle1()
    {
        TrackController.Instance.TriggerObstacle1();
    }
    public void TriggerObstacle2()
    {
        TrackController.Instance.TriggerObstacle2();
    }
    public void TriggerObstacle3()
    {
        TrackController.Instance.TriggerObstacle3();
    }
    public void TriggerObstacle4()
    {
        TrackController.Instance.TriggerObstacle4();
    }
    public void TriggerObstacle5()
    {
        TrackController.Instance.TriggerObstacle5();
    }

    public void TriggerWaypoint1()
    {
        TrackController.Instance.RepositionWaypoint1();
    }

    public void TriggerWaypoint2()
    {
        TrackController.Instance.RepositionWaypoint2();
    }

    public void TriggerWaypoint3()
    {
        TrackController.Instance.RepositionWaypoint3();
    }

    public void TriggerWaypoint4()
    {
        TrackController.Instance.RepositionWaypoint4();
    }

    public void TriggerWaypoint5()
    {
        TrackController.Instance.RepositionWaypoint5();
    }

    public void SetDisplayParabolic()
    {
        AdminSettings.Instance.displayType = AdminScreen.DisplayType.PARABOLIC;
        settings.SetDisplay();
    }

    public void SetDisplayFlat()
    {
        AdminSettings.Instance.displayType = AdminScreen.DisplayType.FLAT;
        settings.SetDisplay();
    }

    public void SetFoV(string fov)
    {
        float f = 0.0f;
        if (float.TryParse(fov, out f))
        {
            settings.driverCam.SetFoV(f);
            AdminSettings.Instance.fov = f;
        }
    }

    public string GetFoV()
    {
        return AdminSettings.Instance.fov.ToString();
    }

    public void SetCamFarClip(string clip)
    {
        float newFarClip = 0.0f;
        if (float.TryParse(clip, out newFarClip))
        {
            settings.SetFarClip(newFarClip);
        }
    }

    public string GetCamFarClip()
    {
        return AdminSettings.Instance.camFarClip.ToString();
    }

    public void SetMusicVol(string vol)
    {
        float f = 0.0f;
        if (float.TryParse(vol, out f))
        {
            AudioController.Instance.MusicVolume = f;
        }
    }

    public string GetMusicVol()
    {
        return AudioController.Instance.MusicVolume.ToString();
    }

    public void SetFoleyVol(string vol)
    {
        float f = 0.0f;
        if (float.TryParse(vol, out f))
        {
            AudioController.Instance.FoleyVolume = f;
        }
    }

    public string GetFoleyVol()
    {
        return AudioController.Instance.FoleyVolume.ToString();
    }

    public void SetVehicleVol(string vol)
    {
        float f = 0.0f;
        if (float.TryParse(vol, out f))
        {
            AudioController.Instance.VehicleVolume = f;
        }
    }

    public string GetVehicleVol()
    {
        return AudioController.Instance.VehicleVolume.ToString();
    }

    public void ToggleAutoPlay(bool newVal)
    {
        TrackController.Instance.SetAutoPath(newVal);
    }

    public bool GetAutoPlay()
    {
        return TrackController.Instance.IsInAutoPath();
    }

    public void ToggleTraffic(bool newVal)
    {
        if (settings.traffic != null)
            settings.traffic.SetTraffic(newVal);
    }

    public bool GetTraffic()
    {
        if (settings.traffic == null)
            return false;
        else
            return settings.traffic.GetState();
    }

    public void SetMass(string mass)
    {
        float f = 0.0f;
        if (float.TryParse(mass, out f))
        {
            vehicle.GetComponent<Rigidbody>().mass = f;
        }
    }

    public string GetMass()
    {
        return vehicle.GetComponent<Rigidbody>().mass.ToString();
    }

    public void SetFWD(bool newVal)
    {
        vehicle.axles[0].motor = newVal;
        vehicle.RecalcDrivingWheels();
    }

    public bool GetFWD()
    {
        return vehicle.axles[0].motor;
    }

    public void SetRWD(bool newVal)
    {
        vehicle.axles[1].motor = newVal;
        vehicle.RecalcDrivingWheels();
    }

    public bool GetRWD()
    {
        return vehicle.axles[1].motor;
    }


    public void SetAirDrag(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.airDragCoeff = f;
        }
    }

    public string GetAirDrag()
    {
        return vehicle.airDragCoeff.ToString();
    }

    public void SetAirDownForce(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.airDownForceCoeff = f;
        }
    }

    public string GetAirDownForce()
    {
        return vehicle.airDownForceCoeff.ToString();
    }

    public void SetTireDrag(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.tireDragCoeff = f;
        }
    }

    public string GetTireDrag()
    {
        return vehicle.tireDragCoeff.ToString();
    }


    public void SetMaxTorque(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.maxMotorTorque = f;
        }

    }

    public string GetMaxTorque()
    {
        return vehicle.maxMotorTorque.ToString();
    }

    public void SetMinRpm(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.minRPM = f;
        }
    }

    public string GetMinRpm()
    {
        return vehicle.minRPM.ToString();
    }

    public void SetMaxRpm(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.maxRPM = f;
        }
    }

    public string GetMaxRpm()
    {
        return vehicle.maxRPM.ToString();
    }

    public void SetShiftDelay(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.shiftDelay = f;
        }
    }

    public string GetShiftDelay()
    {
        return vehicle.shiftDelay.ToString();
    }

    public void SetShiftTime(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.shiftTime = f;
        }
    }

    public string GetShiftTime()
    {
        return vehicle.shiftTime.ToString();
    }

    public void SetFrontBrakeBias(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.axles[0].brakeBias = f;
        }
    }

    public string GetFrontBrakeBias()
    {
        return vehicle.axles[0].brakeBias.ToString();
    }

    public void SetRearBrakeBias(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.axles[1].brakeBias = f;
        }
    }

    public string GetRearBrakeBias()
    {
        return vehicle.axles[1].brakeBias.ToString();
    }

    public void SetMaxBrakeTorque(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.maxBrakeTorque = f;
        }
    }

    public string GetMaxBrakeTorque()
    {
        return vehicle.maxBrakeTorque.ToString();
    }

    public void SetForwardFrictionStiffness(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            var friction = vehicle.axles[0].left.forwardFriction;
            friction.stiffness = f;
            vehicle.axles[0].left.forwardFriction = friction;
            vehicle.axles[0].right.forwardFriction = friction;
            vehicle.axles[1].left.forwardFriction = friction;
            vehicle.axles[1].right.forwardFriction = friction;
        }
    }

    public string GetForwardFrictionStiffness()
    {
        return vehicle.axles[0].left.forwardFriction.stiffness.ToString();
    }

    public void SetSidewaysFrictionStiffness(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            var friction = vehicle.axles[0].left.sidewaysFriction;
            friction.stiffness = f;
            vehicle.axles[0].left.sidewaysFriction = friction;
            vehicle.axles[0].right.sidewaysFriction = friction;
            vehicle.axles[1].left.sidewaysFriction = friction;
            vehicle.axles[1].right.sidewaysFriction = friction;
        }
    }

    public string GetSidewaysFrictionStiffness()
    {
        return vehicle.axles[0].left.sidewaysFriction.stiffness.ToString();
    }

    public void SetMaxSteeringAngle(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.maxSteeringAngle = f;
        }
    }

    public string GetMaxSteeringAngle()
    {
        return vehicle.maxSteeringAngle.ToString();
    }

    public void SetAutoSteerAmount(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.autoSteerAmount= f;
        }

    }

    public string GetAutoSteerAmount()
    {
        return vehicle.autoSteerAmount.ToString();
    }

    public void SetTractionControlAmount(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.tractionControlAmount = f;
        }

    }

    public string GetTractionControlAmount()
    {
        return vehicle.tractionControlAmount.ToString();
    }

    public void SetTractionControlSlipLimit(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            vehicle.tractionControlSlipLimit = f;
        }
    }

    public string GetTractionControlSlipLimit()
    {
        return vehicle.tractionControlSlipLimit.ToString();
    }

    public void SetFrontSpring(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            var suspension = vehicle.axles[0].left.suspensionSpring;
            suspension.spring = f;
            vehicle.axles[0].left.suspensionSpring = suspension;
            vehicle.axles[0].right.suspensionSpring = suspension;
        }
    }

    public string GetFrontSpring()
    {
        return vehicle.axles[0].left.suspensionSpring.spring.ToString();
    }

    public void SetRearSpring(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            var suspension = vehicle.axles[1].left.suspensionSpring;
            suspension.spring = f;
            vehicle.axles[1].left.suspensionSpring = suspension;
            vehicle.axles[1].right.suspensionSpring = suspension;
        }

    }

    public string GetRearSpring()
    {
        return vehicle.axles[1].left.suspensionSpring.spring.ToString();
    }


    public void SetFrontDamper(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            var suspension = vehicle.axles[0].left.suspensionSpring;
            suspension.damper = f;
            vehicle.axles[0].left.suspensionSpring = suspension;
            vehicle.axles[0].right.suspensionSpring = suspension;
        }
    }

    public string GetFrontDamper()
    {
        return vehicle.axles[0].left.suspensionSpring.damper.ToString();
    }

    public void SetRearDamper(string val)
    {
        float f = 0.0f;
        if (float.TryParse(val, out f))
        {
            var suspension = vehicle.axles[1].left.suspensionSpring;
            suspension.damper = f;
            vehicle.axles[1].left.suspensionSpring = suspension;
            vehicle.axles[1].right.suspensionSpring = suspension;
        }

    }

    public string GetRearDamper()
    {
        return vehicle.axles[1].left.suspensionSpring.damper.ToString();
    }

    public void SaveSettings()
    {
        vehicle.GetComponent<VehicleConfigurator>().SaveSettings();
    }

    public void LoadSettings()
    {
        vehicle.GetComponent<VehicleConfigurator>().LoadSettings();
        ResetDefaults();
    }

    public string GetInputMethod()
    {
        return AppController.Instance.UserInput.gameObject.name;
    }
}
