/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public abstract class BaseObstacle : MonoBehaviour 
{

    public float offRoadDistance = 6f;
    public float triggerDistance = 30f;
    public float passDistance = 1f;
    public bool spawnInLane = false;
    protected bool triggered = false;
    protected float triggerTime = 0f;
 
    protected const float triggerTimeout = 3f;

    public abstract void OnTrigger();
    public virtual void CleanUp()
    {
        Destroy(gameObject);
    }

    protected virtual void Update()
    {
        float dist = Vector3.Distance(transform.position, TrackController.Instance.car.transform.position);
        if(!triggered && dist < triggerDistance)
        {
            triggered = true;
            OnTrigger();
        }
        else if(triggered && Time.time - triggerTime > triggerTimeout && dist > triggerDistance)
        {
            CleanUp();
        }
    }

    public virtual bool SpawnAnywhere()
    {
        return true;
    }
}
