/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class TreeObstacle : BaseObstacle {

    public AnimationCurve fallCurve;

    public float fallTime = 1f;

    public override void OnTrigger()
    {
        StartCoroutine(Fall());
    }

    private IEnumerator Fall()
    {
        GetComponent<AudioSource>().Play();
        float startTime = Time.time;
        Vector3 initalRot = transform.localEulerAngles;
        while(transform.localRotation.x < 90f)
        {
            transform.localRotation = Quaternion.Euler(Mathf.Lerp(0f, 90f, fallCurve.Evaluate((Time.time - startTime) / fallTime)), initalRot.y, initalRot.z);
            yield return null;
        }
        transform.localRotation = Quaternion.Euler(0f, initalRot.y, initalRot.z);
        GetComponent<Rigidbody>().isKinematic = false;
    }

    public override void CleanUp()
    {
        base.CleanUp();
    }
}
