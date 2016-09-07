/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class AxleInfo
{
    public WheelCollider left;
    public WheelCollider right;
    public GameObject leftVisual;
    public GameObject rightVisual;
    public bool motor;
    public bool steering;
    public float brakeBias = 0.5f;

    [System.NonSerialized]
    public WheelHit hitLeft;
    [System.NonSerialized]
    public WheelHit hitRight;
    [System.NonSerialized]
    public bool isGroundedLeft = false;
    [System.NonSerialized]
    public bool isGroundedRight = false;
}

public enum RoadSurface { Tarmac, Offroad, Airborne}

public class VehicleController : MonoBehaviour {

    //all car wheel info
    public List<AxleInfo> axles;

    //position where first person driver camera should stay
    public Transform cameraPosition;

    //torque at peak of torque curve
    public float maxMotorTorque;

    //torque at max brake
    public float maxBrakeTorque;

    //steering range is [-maxSteeringAngle, maxSteeringAngle]
    public float maxSteeringAngle;

    //idle rpm
    public float minRPM = 1500;

    //maxiumum RPM
    public float maxRPM = 8000;

    //gearbox ratios
    public float finalDriveRatio = 2.56f;
    public float[] gearRatios;

    //minimum time between any 2 gear changes
    public float shiftDelay = 1.0f;

    //time it takes for a gear shift to complete (interpolated)
    public float shiftTime = 0.4f;

    //torque curve that gives torque at specific percentage of max RPM;
    public AnimationCurve rpmCurve;

    //curves controlling whether to shift up/down at specific rpm, based on throttle position
    public AnimationCurve shiftUpCurve;
    public AnimationCurve shiftDownCurve;

    //drag coefficients
    public float airDragCoeff = 2.0f;
    public float airDownForceCoeff = 4.0f;
    public float tireDragCoeff = 500.0f;

    //wheel collider damping rate
    public float wheelDamping = 2f;

    //autosteer helps the car maintain its heading
    [Range(0,1)]
    public float autoSteerAmount;

    //traction control limits torque based on wheel slip - traction reduced by amount when slip exceeds the tractionControlSlipLimit
    [Range(0, 1)]
    public float tractionControlAmount;
    public float tractionControlSlipLimit;

    //rigidbody center of mass
    public Vector3 centerOfMass;

    //how much to smooth out real RPM
    public float RPMSmoothness = 2f;

    //combined throttle and brake input
    public float accellInput = 0f;

    //steering input
    public float steerInput = 0f;

    public float RPM
    {
        get
        {
            return currentRPM;
        }
    }

    public int Gear
    {
        get
        {
            return targetGear;
        }
    }

    public bool IsShifting
    {
        get
        {
            return shifting;
        }
    }

    public WheelCollider WheelFL
    {
        get
        {
            return axles[0].left;
        }
    }

    public WheelCollider WheelFR
    {
        get
        {
            return axles[0].right;
        }
    }

    public WheelCollider WheelRL
    {
        get
        {
            return axles[1].left;
        }
    }

    public WheelCollider WheelRR
    {
        get
        {
            return axles[1].right;
        }
    }

    public float MotorWheelsSlip
    {
        get
        {
            float slip = 0f;
            int i = 0;
            foreach (var axle in axles)
            {
                if (axle.motor)
                {
                    i += 2;
                    if (axle.isGroundedLeft)
                        slip += axle.hitLeft.forwardSlip;
                    if (axle.isGroundedRight)
                        slip += axle.hitRight.forwardSlip;
                }
            }
            return slip / i;
        }
    }

    public RoadSurface CurrentSurface
    {
        get;
        private set;
    }

    public float CurrentSpeed
    {
        get
        {
            return currentSpeed;
        }
    }

    private float lastShift = 0.0f;

    private float currentRPM = 0.0f;
    private float currentSpeed = 0.0f;

    private Rigidbody rb;
    private int numberOfDrivingWheels;

    private float oldRotation;
    private float tractionControlAdjustedMaxTorque;
    private float currentTorque = 0;

    private const float LOW_SPEED = 5f;
    private const float LARGE_FACING_ANGLE = 50f;

    private float currentGear = 1;
    private int targetGear = 1;
    private int lastGear = 1;
    private bool shifting = false;

    public float traction = 0f;
    public float tractionR = 0f;
    public float rtraction = 0f;
    public float rtractionR = 0f;
    public string roadType;

    void OnEnable()
    {
        //cache rigidbody
        rb = gameObject.GetComponent<Rigidbody>();

        RecalcDrivingWheels();

        tractionControlAdjustedMaxTorque = maxMotorTorque - (tractionControlAmount * maxMotorTorque);

        axles[0].left.ConfigureVehicleSubsteps(5.0f, 30, 10);
        axles[0].right.ConfigureVehicleSubsteps(5.0f, 30, 10);
        axles[1].left.ConfigureVehicleSubsteps(5.0f, 30, 10);
        axles[1].right.ConfigureVehicleSubsteps(5.0f, 30, 10);

        foreach(var axle in axles)
        {
            axle.left.wheelDampingRate = wheelDamping;
            axle.right.wheelDampingRate = wheelDamping;
        }
    }

    public void RecalcDrivingWheels()
    {
        //calculate how many wheels are driving
        numberOfDrivingWheels = axles.Where(a => a.motor).Count() * 2;
    }

    private void AutoGearBox()
    {
        //check delay so we cant shift up/down too quick
        if (Time.time - lastShift > shiftDelay)
        {
            
            //shift up
            if (currentRPM / maxRPM > shiftUpCurve.Evaluate(accellInput) && Mathf.RoundToInt(currentGear) < gearRatios.Length)
            {
                //don't shift up if we are just spinning in 1st
                if (Mathf.RoundToInt(currentGear) > 1 || currentSpeed > 15f)
                {
                    lastGear = Mathf.RoundToInt(currentGear);
                    targetGear = lastGear + 1;
                    lastShift = Time.time;
                    shifting = true;
                }
            }
            //else down
            if (currentRPM / maxRPM < shiftDownCurve.Evaluate(accellInput) && Mathf.RoundToInt(currentGear) > 1)
            {
                lastGear = Mathf.RoundToInt(currentGear);
                targetGear = lastGear - 1;
                lastShift = Time.time;
                shifting = true;
            }
        }

        if(shifting)
        {
            float lerpVal = (Time.time - lastShift) / shiftTime;
            currentGear = Mathf.Lerp(lastGear, targetGear, lerpVal);
            if (lerpVal >= 1f)
                shifting = false;
        }

        //clamp to gear range
        if (currentGear >= gearRatios.Length)
        {
            currentGear = gearRatios.Length - 1;
        }
        else if (currentGear < 1)
        {
            currentGear = 1;
        }
    }

  

    public void FixedUpdate()
    {

        //air drag (quadratic)
        rb.AddForce(-airDragCoeff * rb.velocity * rb.velocity.magnitude);
        
        //downforce (quadratic)
        rb.AddForce(-airDownForceCoeff * rb.velocity.sqrMagnitude * transform.up);

        //tire drag (Linear)
        rb.AddForceAtPosition(-tireDragCoeff * rb.velocity, transform.position);

        //calc current gear ratio
        float gearRatio = Mathf.Lerp(gearRatios[Mathf.FloorToInt(currentGear) - 1], gearRatios[Mathf.CeilToInt(currentGear) - 1], currentGear - Mathf.Floor(currentGear));
        
        //calc engine RPM from wheel rpm
        float wheelsRPM = (axles[1].right.rpm + axles[1].left.rpm) / 2f;
        if (wheelsRPM < 0)
            wheelsRPM = 0;

        currentRPM = Mathf.Lerp(currentRPM, minRPM + (wheelsRPM * finalDriveRatio * gearRatio), Time.fixedDeltaTime * RPMSmoothness);

        //find out which wheels are on the ground
        foreach(var axle in axles)
        {
            axle.isGroundedLeft = axle.left.GetGroundHit(out axle.hitLeft);
            axle.isGroundedRight = axle.right.GetGroundHit(out axle.hitRight);
        }

        //convert inputs to torques
        float steer = maxSteeringAngle * steerInput;
        currentTorque = rpmCurve.Evaluate(currentRPM / maxRPM) * gearRatio * finalDriveRatio * tractionControlAdjustedMaxTorque;

        foreach (var axle in axles)
        {
            if (axle.steering)
            {
                axle.left.steerAngle = steer;
                axle.right.steerAngle = steer;
            }
        }

        AutoSteer();
        ApplyTorque();
        TractionControl();

        //shift if need be
        AutoGearBox();

        //record current speed in MPH
        currentSpeed = rb.velocity.magnitude * 2.23693629f;

        //find current road surface type
        WheelHit hit;
        if(axles[0].left.GetGroundHit(out hit))
        {
            traction = hit.forwardSlip;
            var roadObject = hit.collider.transform.parent == null ? hit.collider.transform : hit.collider.transform.parent;
            CurrentSurface = roadType == "roads" || roadObject.CompareTag("Road") ? RoadSurface.Tarmac : RoadSurface.Offroad;
        }
        else
        {
            traction = 0f;
            CurrentSurface = RoadSurface.Airborne;
        }

        //find traction values for audio
        if (axles[0].right.GetGroundHit(out hit))
        {
            tractionR = hit.forwardSlip;
        }
        else
        {
            tractionR = 0f;
        }
        if (axles[1].right.GetGroundHit(out hit))
        {
            rtractionR = hit.forwardSlip;
        }
        else
        {
            rtractionR = 0f;
        }
        if (axles[1].left.GetGroundHit(out hit))
        {
            rtraction = hit.forwardSlip;
        }
        else
        {
            rtraction = 0f;
        }

    }

    //
    private void AutoSteer()
    {
        //bail if a wheel isn't on the ground
        foreach (var axle in axles)
        {
            if(axle.isGroundedLeft == false || axle.isGroundedRight == false)
            return; 
        }

        var yawRate = oldRotation - transform.eulerAngles.y;

        //don't adjust if the yaw rate is super high
        if (Mathf.Abs(yawRate) < 10f)      
            rb.velocity = Quaternion.AngleAxis(yawRate * autoSteerAmount, Vector3.up) * rb.velocity;
        
        oldRotation = transform.eulerAngles.y;
    }

    private void ApplyTorque()
    {
        
        if (accellInput >= 0) {
            //motor
            float torquePerWheel = accellInput * (currentTorque / numberOfDrivingWheels);
            foreach (var axle in axles)
            {
                if (axle.motor)
                {
                    if (axle.isGroundedLeft)
                        axle.left.motorTorque = torquePerWheel;
                    else
                        axle.left.motorTorque = 0f;

                    if (axle.isGroundedRight)
                        axle.right.motorTorque = torquePerWheel;
                    else
                        axle.left.motorTorque = 0f;
                }

                axle.left.brakeTorque = 0f;
                axle.right.brakeTorque = 0f;
            }

        }
        else
        {
            //brakes 
            foreach (var axle in axles)
            {
                var brakeTorque = maxBrakeTorque * accellInput * -1 * axle.brakeBias;
                axle.left.brakeTorque = brakeTorque;
                axle.right.brakeTorque = brakeTorque;
                axle.left.motorTorque = 0f;
                axle.right.motorTorque = 0f;
            }
        }
        
    }

    private void TractionControl()
    {
        foreach (var axle in axles)
        {
            if (axle.motor)
            {
               if(axle.left.isGrounded)
                    AdjustTractionControlTorque(axle.hitLeft.forwardSlip);

                if(axle.right.isGrounded)
                    AdjustTractionControlTorque(axle.hitRight.forwardSlip);
            }
        }

    }

    private void AdjustTractionControlTorque(float forwardSlip)
    {
        if (forwardSlip >= tractionControlSlipLimit && tractionControlAdjustedMaxTorque >= 0)
        {
            tractionControlAdjustedMaxTorque -= 10 * tractionControlAmount;
            if (tractionControlAdjustedMaxTorque < 0)
                tractionControlAdjustedMaxTorque = 0f;
        }
        else
        {
            tractionControlAdjustedMaxTorque += 10 * tractionControlAmount;
            if (tractionControlAdjustedMaxTorque > maxMotorTorque)
                tractionControlAdjustedMaxTorque = maxMotorTorque;
        }
    }

    public void Update()
    {
        
        if (rb.centerOfMass != centerOfMass)
            rb.centerOfMass = centerOfMass;

        if(axles[0].left.wheelDampingRate != wheelDamping)
        {
            foreach(var axle in axles)
            {
                axle.left.wheelDampingRate = wheelDamping;
                axle.right.wheelDampingRate = wheelDamping;
            }
        }

        //update wheel visual component rotations
        foreach (var axle in axles)
        {
            ApplyLocalPositionToVisuals(axle.left, axle.leftVisual);
            ApplyLocalPositionToVisuals(axle.right, axle.rightVisual);
        }


    }

    private void ApplyLocalPositionToVisuals(WheelCollider collider, GameObject visual)
    {
        Transform visualWheel = visual.transform;

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 200), "GEAR " + (Mathf.RoundToInt(currentGear)));
        GUI.Label(new Rect(10, 30, 500, 200), "RPM " + Mathf.RoundToInt(currentRPM));
        GUI.Label(new Rect(10, 50, 500, 200), "MPH " + Mathf.RoundToInt(currentSpeed));
        GUI.Label(new Rect(10, 70, 500, 200), "THROTTLE " +  accellInput.ToString("F2"));
        GUI.Label(new Rect(10, 90, 500, 200), "WHEEL POSITION " + steerInput.ToString("F3"));
        GUI.Label(new Rect(10, 110, 500, 200), "TRACTION " + traction.ToString("F3"));
        GUI.Label(new Rect(10, 130, 500, 200), "TRACTIONR " + tractionR.ToString("F3"));
        GUI.Label(new Rect(10, 160, 500, 200), "TRACTION " + rtraction.ToString("F3"));
        GUI.Label(new Rect(10, 180, 500, 200), "TRACTIONR " + rtractionR.ToString("F3"));

    }
}
