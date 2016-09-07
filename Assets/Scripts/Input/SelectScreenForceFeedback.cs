/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class SelectScreenForceFeedback : MonoBehaviour {

    public int saturation;
    public int coefficient;

    private SteeringWheelInputController logi;

    void Start()
    {
        if(AppController.Instance.UserInput is SteeringWheelInputController)
        {
            logi = AppController.Instance.UserInput as SteeringWheelInputController;
            logi.Init();
            //logi.InitSpringForce(saturation, coefficient);
            
        }
        else
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        logi.SetSpringForce(saturation, coefficient);
    }

    void OnDisable()
    {
        if(logi != null)
        {
            logi.StopSpringForce();
            logi.CleanUp();
        }
    }
}
