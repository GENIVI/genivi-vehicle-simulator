/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class InfractionRecorder : MonoBehaviour {

    public float startTime = 0f;

    public bool checkLanes = true;

    private bool alive = false;

    private VehicleController car;

    void Awake()
    {
        alive = false;
        startTime = Time.time;
        car = GetComponent<VehicleController>();
    }

    IEnumerator Start()
    {
        yield return null;
        yield return new WaitForSeconds(10f);
        alive = true;
    }



    private float timer = 0f;
    private float checkTimer = 0f;
    
    
    //SF check goes in Update
    void Update()
    {
        if (!alive)
            return;

        if (!hasStopped && GetComponent<Rigidbody>().velocity.magnitude < 1f)
            hasStopped = true;

        if (TrackController.Instance.hasTraffic)
        {
            if (Time.time - timer < 10f)
            {
                return;
            }

            if (Time.time - checkTimer < 2f)
            {
                return;
            }

            checkTimer = Time.time;

            var outside = TrackController.Instance.CheckForOutsideLaneSF();
            if (outside == TrackController.OutsideLane.UNSURE)
                return;

            if (outside == TrackController.OutsideLane.OUTSIDE)
            {
                if (!isOffroad)
                {
                    var infraction = new DrivingInfraction();
                    infraction.id = TrackController.Instance.GetInfractions().Count;
                    infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
                    infraction.type = "LANE";
                    infraction.systemTime = System.DateTime.Now;
                    infraction.sessionTime = Time.time - startTime;
                    TrackController.Instance.AddInfraction(infraction);
                    timer = Time.time;
                    isOffroad = true;
                }
            }
            else
            {
                isOffroad = false;
            }

        } else
        {
            ProcessWheels();
        }

        
    }

    private bool hasStopped = false;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("StopTrigger"))
        {
            hasStopped = false;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!hasStopped && other.CompareTag("StopTrigger"))
        {
            var infraction = new DrivingInfraction();
            infraction.id = TrackController.Instance.GetInfractions().Count;
            infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
            infraction.type = "STOP";
            infraction.systemTime = System.DateTime.Now;
            infraction.sessionTime = Time.time - startTime;
            TrackController.Instance.AddInfraction(infraction);
            isOffroad = true;
        }
    }


    private bool isOffroad = true;

    //Yos/PCH checks use collision data to find RA road
    public void ProcessWheels()
    {

        if (!alive)
            return;

        if (TrackController.Instance.hasTraffic)
            return;

        if (Time.time - timer < 10f)
        {
            return;
        }

        if (Time.time - checkTimer < 2f)
        {
            return;
        }

        foreach(var wheel in car.axles)
        {
            if (ProcessWheel(wheel.hitLeft.collider))
                break;
            if (ProcessWheel(wheel.hitRight.collider))
                break;        
        }
    }

    private bool ProcessWheel(Collider other)
    { 
        Transform roadMesh = null;

        if (other.transform.parent != null && other.transform.parent.CompareTag("Road"))
            roadMesh = other.transform;
   
        if (roadMesh != null)
        {
            checkTimer = Time.time;
            if (TrackController.Instance.CheckForOutsideLane(roadMesh))
            {
                if (!isOffroad)
                {
                    var infraction = new DrivingInfraction();
                    infraction.id = TrackController.Instance.GetInfractions().Count;
                    infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
                    infraction.type = "LANE";
                    infraction.systemTime = System.DateTime.Now;
                    infraction.sessionTime = Time.time - startTime;
                    TrackController.Instance.AddInfraction(infraction);
                    timer = Time.time;
                    isOffroad = true;
                    return true;
                }
            }
            else
            {
                isOffroad = false;
            }
        }
        else if(other.GetType() == typeof(TerrainCollider))
        {
            if (!isOffroad)
            {
                var infraction = new DrivingInfraction();
                infraction.id = TrackController.Instance.GetInfractions().Count;
                infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
                infraction.type = "LANE";
                infraction.systemTime = System.DateTime.Now;
                infraction.sessionTime = Time.time - startTime;
                TrackController.Instance.AddInfraction(infraction);
                timer = Time.time;
                isOffroad = true;
            }
            return true;
        }

        return false;
        
    }

    void OnCollisionEnter(Collision other)
    {
        if (!alive)
            return;

        if (Time.time - timer < 10f)
            return;

        if (other.gameObject.CompareTag("Obstacle"))
        {
            var infraction = new DrivingInfraction();
            infraction.id = TrackController.Instance.GetInfractions().Count;
            infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
            infraction.type = "OBS";
            infraction.systemTime = System.DateTime.Now;
            infraction.sessionTime = Time.time - startTime;
            TrackController.Instance.AddInfraction(infraction);
            timer = Time.time;
        }
        else if (other.gameObject.CompareTag("TrafficCar"))
        {
            var infraction = new DrivingInfraction();
            infraction.id = TrackController.Instance.GetInfractions().Count;
            infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
            infraction.type = "TRAF";
            infraction.systemTime = System.DateTime.Now;
            infraction.sessionTime = Time.time - startTime;
            TrackController.Instance.AddInfraction(infraction);
            timer = Time.time;
        }
        else
        {
            for (int i = 0; i < other.contacts.Length; i++)
            {
                if (other.contacts[i].thisCollider.GetType() != typeof(WheelCollider) && other.contacts[i].otherCollider.GetType() != typeof(WheelCollider))
                {
                    var infraction = new DrivingInfraction();
                    infraction.id = TrackController.Instance.GetInfractions().Count;
                    infraction.speed = GetComponent<Rigidbody>().velocity.magnitude;
                    infraction.type = "ENV";
                    infraction.systemTime = System.DateTime.Now;
                    infraction.sessionTime = Time.time - startTime;
                    TrackController.Instance.AddInfraction(infraction);
                    timer = Time.time;
                    break;
                }
            }
        }


    }
}
