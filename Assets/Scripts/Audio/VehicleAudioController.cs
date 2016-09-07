/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class VehicleAudioController : MonoBehaviour {

    public VehicleAudio vehicleAudio;
    public RoadAudio roadAudio;
    public CollisionAudio collisionAudio;
    public IgnitionAudio ignitionAudio;
    public WindAudio windAudio;

    private VehicleController vehicleController;

    public AudioMixerSnapshot driveSnapshot;
    public AudioMixerSnapshot selectSnapshot;

    private float load = 0f;

    private RoadSurface lastSurface = RoadSurface.Airborne;

    void Awake()
    {
        vehicleController = GetComponent<VehicleController>();
    }

    public void PlayIgnition()
    {
        selectSnapshot.TransitionTo(0f);
        ignitionAudio.Play(PlayEngine);
    }

    public void PlayEngine()
    {
        this.enabled = true;
        driveSnapshot.TransitionTo(3f);
        
    }

    public void StopEngine()
    {
        this.enabled = false;
        selectSnapshot.TransitionTo(0f);
    }

    private void PlaySurfaceBump()
    {
        roadAudio.PlaySurfaceBump();
    }
	
	void Update () {
        load = Mathf.Lerp(load, vehicleController.IsShifting ? 0f : vehicleController.accellInput, Time.deltaTime * 2f);
        vehicleAudio.rpm = vehicleController.RPM;
        vehicleAudio.load = load;

        var traction = vehicleController.traction + vehicleController.tractionR + vehicleController.rtraction + vehicleController.rtractionR;
        traction = traction / 4f;

        var accellTraction = 1f - Mathf.Clamp01(vehicleController.MotorWheelsSlip);
        var brakeTraction = 1f - Mathf.Clamp01(-traction);

        roadAudio.accellTraction = accellTraction;
        roadAudio.brakeTraction = brakeTraction;
        roadAudio.speed = vehicleController.CurrentSpeed;
        roadAudio.surface = vehicleController.CurrentSurface;

        if(vehicleController.CurrentSurface != RoadSurface.Airborne && lastSurface != vehicleController.CurrentSurface)
        {
            lastSurface = vehicleController.CurrentSurface;
            PlaySurfaceBump();
        }

        windAudio.speed = vehicleController.CurrentSpeed;

    }

    void OnCollisionEnter(Collision collision)
    {
        collisionAudio.PlayCollision(collision.contacts[0].point, collision.relativeVelocity.magnitude);       
    }

}
