/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class VehicleInputController : MonoBehaviour {

    private VehicleController controller;

    void Awake()
    {
        controller = GetComponent<VehicleController>();
    }

	void Update () {
        //grab input values
        var inputController = AppController.Instance.UserInput;
        controller.steerInput = inputController.GetSteerInput();
        controller.accellInput = inputController.GetAccelBrakeInput();
    }
}
