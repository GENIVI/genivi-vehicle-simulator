/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class CamSmoothFollow : MonoBehaviour
{

    public Transform targetObject;
    public float targetDistance = 5f;
    public float targetHeight = 1.7f;
    public float lookAbove = 0.85f;
    public float heightDamping = 2f;
    public float rotationDamping = 4.5f;
    public float positionDamping = 5f;

    private Vector3 currentPosition;
    private float currentRotation;
    private float currentHeight;

    public void LateUpdate()
    {
        if (targetObject == null)
            return;

        
        currentHeight = Mathf.Lerp(currentHeight, targetObject.position.y + targetHeight, Time.deltaTime * heightDamping);
        currentRotation = Mathf.LerpAngle(currentRotation, targetObject.rotation.eulerAngles.y, Time.deltaTime * rotationDamping);

        var newPosition = targetObject.position - (Quaternion.Euler(0f, currentRotation, 0f) * Vector3.forward * targetDistance);
        newPosition.y = currentHeight;

        currentPosition = Vector3.Lerp(currentPosition, newPosition, Time.deltaTime * positionDamping);

        transform.position = currentPosition;
        transform.LookAt(targetObject.position + Vector3.up * lookAbove);

    }




}