/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class GarageExitController : BaseSelectController
{

    public OrbitAround cam;
    public GameObject exitTarget;


    private GameObject selectedCar;

    public override void OnSetup()
    {
        //selectedCar = cam.target;
        StartCoroutine(MoveCar());
        //cam.GetComponentInChildren<RotateToTarget>().SetTarget(0f);
        //cam.rotationSpeed = 0f;
        //cam.LerpYaw(90f, () => cam.Move(exitTarget));
        
    }

    IEnumerator MoveCar() {
        yield return new WaitForSeconds(2f);
        //selectedCar.GetComponent<CarControl>().enabled = true;
        //selectedCar.GetComponent<CarExternalInputPath>().enabled = true;
        //yield return new WaitForSeconds(6f);
        base.OnSelectConfirm();
    }

    protected override void OnSelectConfirm()
    {
        return;
    }
}
