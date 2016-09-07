/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TrafAIMotor : MonoBehaviour
{

    public TrafSystem system;
    public TrafEntry currentEntry;
    private TrafEntry nextEntry = null;
    private bool hasNextEntry = false;
    public int currentIndex = 0;

    public float currentSpeed;
    public float currentTurn;
    public GameObject nose;
    public Transform raycastOrigin;
    public float waypointThreshold = 0.3f;
    public float maxSpeed = 10f;
    public float maxTurn = 45f;
    public float maxAccell = 3f;
    public float maxBrake = 4f;
    public float targetHeight = 0f;

    public bool hasStopTarget = false;
    public bool hasGiveWayTarget = false;
    public bool isInIntersection = false;
    public Vector3 stopTarget;
    public Vector3 targetTangent = Vector3.zero;
    private Vector3 target = Vector3.zero;
    private Vector3 nextTarget = Vector3.zero;

    public const float giveWayRegisterDistance = 12f;
    public const float brakeDistance = 10f;
    public const float yellowLightGoDistance = 4f;
    public const float stopLength = 6f;
    
    private bool inited = false;
    private float intersectionCornerSpeed = 1f;

    private float nextRaycast = 0f;
    private RaycastHit hitInfo;
    private bool somethingInFront = false;


    private float stopEnd = 0f;

    //117 118 119 120 121 122  100 101 102 165 129 52 53 34 35             

    public bool fixedRoute = false;
    public List<RoadGraphEdge> fixedPath;
    public int currentFixedNode = 0;

    private RaycastHit heightHit;

    public void Init()
    {
        target = currentEntry.waypoints[currentIndex];
        CheckHeight();
        inited = true;
        nextRaycast = 0f;
        CheckHeight();


        InvokeRepeating("CheckHeight", 0.2f, 0.2f);
    }


    public float NextRaycastTime()
    {
        return Time.time + Random.value / 4 + 0.1f;
    }

    //check for something in front of us, populate blockedInfo if something was found
    private bool CheckFrontBlocked(out RaycastHit blockedInfo)
    {
        Collider[] colls = Physics.OverlapSphere(raycastOrigin.position, 0.2f, 1 << LayerMask.NameToLayer("Traffic"));
        foreach (var c in colls)
        {
            if(c.transform.root != transform.root)
            {
                blockedInfo = new RaycastHit();
                blockedInfo.distance = 0f;
                return true;
            }
        }

        if(Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out blockedInfo, brakeDistance, ~(1 << LayerMask.NameToLayer("BridgeRoad"))))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    void GetNextPath()
    {

    }

    //TODO: tend to target height over time
    void CheckHeight()
    {
        Physics.Raycast(transform.position + Vector3.up * 2, -transform.up, out heightHit, 100f, ~(1 << LayerMask.NameToLayer("Traffic")));
        targetHeight =  heightHit.point.y;

        transform.position = new Vector3(transform.position.x, targetHeight, transform.position.z);    
    }

    void FixedUpdate()
    {
        if(!inited)
            return;
        MoveCar();
    }

    void Update()
    {

        if(!inited)
            return;

        if(!currentEntry.isIntersection() && currentIndex > 0 && !hasNextEntry)
        {
            //last waypoint in this entry, grab the next path when we are in range
            if(Vector3.Distance(nose.transform.position, currentEntry.waypoints[currentEntry.waypoints.Count - 1]) <= giveWayRegisterDistance)
            {
                var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);
 
                RoadGraphEdge newNode;

                if (fixedRoute)
                {
                    if(++currentFixedNode >= fixedPath.Count)
                        currentFixedNode = 0;
                    newNode = system.FindJoiningIntersection(node, fixedPath[currentFixedNode]);
                } else {
                    newNode = node.SelectRandom();
                }

                if(newNode == null)
                {
                    Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                    Destroy(gameObject);
                    inited = false;
                    return;
                }

                nextEntry = system.GetEntry(newNode.id, newNode.subId);
                nextTarget = nextEntry.waypoints[0];
                hasNextEntry = true;

                nextEntry.RegisterInterest(this);

                //see if we need to slow down for this intersection
                intersectionCornerSpeed = Mathf.Clamp(1 - Vector3.Angle(nextEntry.path.start.transform.forward, nextEntry.path.end.transform.forward) / 90f, 0.4f, 1f);

            }
        }

        //check if we have reached the target waypoint
        if(Vector3.Distance(nose.transform.position, target) <= waypointThreshold )// && !hasStopTarget && !hasGiveWayTarget)
        {
            if(++currentIndex >= currentEntry.waypoints.Count)
            {
                if(currentEntry.isIntersection())
                {
                    currentEntry.DeregisterInterest();
                    var node = system.roadGraph.GetNode(currentEntry.identifier, currentEntry.subIdentifier);
                    var newNode = node.SelectRandom();

                    if(newNode == null)
                    {
                        Debug.Log("no edges on " + currentEntry.identifier + "_" + currentEntry.subIdentifier);
                        Destroy(gameObject);
                        inited = false;
                        return;
                    }

                    currentEntry = system.GetEntry(newNode.id, newNode.subId);
                    nextEntry = null;
                    hasNextEntry = false;

                    targetTangent = (currentEntry.waypoints[1] - currentEntry.waypoints[0]).normalized;

                }
                else
                {
                    if(hasStopTarget || hasGiveWayTarget)
                    {
                        target = nextEntry.waypoints[0];
                    }
                    else
                    {
                        currentEntry = nextEntry;
                        nextEntry = null;
                        hasNextEntry = false;
                        targetTangent = Vector3.zero;
                    }
                }

                if(!hasStopTarget && !hasGiveWayTarget)
                    currentIndex = 0;


            }
            if(currentIndex > 1)
            {
                targetTangent = Vector3.zero;
            }

            if(!hasStopTarget && !hasGiveWayTarget)
                target = currentEntry.waypoints[currentIndex];
        }



        SteerCar();

        if(hasNextEntry && nextEntry.isIntersection() && nextEntry.intersection.stopSign) 
        {
            if(stopEnd == 0f) {
                hasStopTarget = true;
                stopTarget = nextTarget;
                stopEnd = Time.time + stopLength;
            } else if(Time.time > stopEnd) {
                if(nextEntry.intersection.stopQueue.Peek() == this) {
                    hasGiveWayTarget = false;
                    hasStopTarget = false;
                    stopEnd = 0f;
                } 
            }
        }


        if(hasNextEntry && nextEntry.isIntersection() && !nextEntry.intersection.stopSign)
        {
            //check next entry for stop needed
            if(nextEntry.MustGiveWay())
            {
                hasGiveWayTarget = true;
                stopTarget = target;
            }
            else
            {
                hasGiveWayTarget = false;
            }
        }

        if(!hasGiveWayTarget && hasNextEntry && nextEntry.light != null)
        {
            if(!hasStopTarget && nextEntry.light.State == TrafLightState.RED)
            {
                //light is red, stop here
                hasStopTarget = true;
                stopTarget = nextTarget;

            }
            else if(hasStopTarget && nextEntry.light.State == TrafLightState.GREEN)
            {

                //green light, go!                   
                hasStopTarget = false;
                return;
            }
            else if(!hasStopTarget && nextEntry.light.State == TrafLightState.YELLOW)
            {
                //yellow, stop if we aren't zooming on through
                //TODO: carry on if we are too fast/close

                if(Vector3.Distance(nextTarget, nose.transform.position) > yellowLightGoDistance)
                {
                    hasStopTarget = true;
                    stopTarget = nextTarget;
                }
            }


        }

        float targetSpeed = maxSpeed;

        //check in front of us
        if(Time.time > nextRaycast)
        {
            hitInfo = new RaycastHit();
            somethingInFront = CheckFrontBlocked(out hitInfo);
            nextRaycast = NextRaycastTime();
        }

        if(somethingInFront)
        {
            float frontSpeed = Mathf.Clamp((hitInfo.distance - 1f) / 3f, 0f, maxSpeed);
            if(frontSpeed < 0.2f)
                frontSpeed = 0f;

            targetSpeed = Mathf.Min(targetSpeed, frontSpeed);
        }

        if(hasStopTarget || hasGiveWayTarget)
        {
            Vector3 targetVec = (stopTarget - nose.transform.position);

            float stopSpeed = Mathf.Clamp(targetVec.magnitude * (Vector3.Dot(targetVec, nose.transform.forward) > 0 ? 1f : 0f) / 3f, 0f, maxSpeed);
            if(stopSpeed < 0.24f)
                stopSpeed = 0f;

            targetSpeed = Mathf.Min(targetSpeed, stopSpeed);
        }

        //slow down if we need to turn
        if(currentEntry.isIntersection() || hasNextEntry)
        {
            targetSpeed = targetSpeed * intersectionCornerSpeed;
        }
        else
        {
            targetSpeed = targetSpeed * Mathf.Clamp(1 - (currentTurn / maxTurn), 0.1f, 1f);
        }

        if(targetSpeed > currentSpeed)
            currentSpeed += Mathf.Min(maxAccell * Time.deltaTime, targetSpeed - currentSpeed);
        else 
        {
            currentSpeed -= Mathf.Min(maxBrake * Time.deltaTime, currentSpeed - targetSpeed);
            if(currentSpeed < 0)
                currentSpeed = 0;
        }

     
    }

    void SteerCar()
    {
        
        float targetDist = Vector3.Distance(target, transform.position);
        //head towards target
        Vector3 newTarget = target;
        if(targetTangent != Vector3.zero && targetDist > 6f)
        {
            newTarget = target - (targetTangent * (targetDist - 6f));
        } 
        Vector3 steerVector = new Vector3(newTarget.x, transform.position.y, newTarget.z) - transform.position;
        float steer = Vector3.Angle(transform.forward, steerVector);
        if(steer > 140f)
        {
            steer = currentTurn;
        }
        currentTurn = Mathf.Clamp((Vector3.Cross(transform.forward, steerVector).y < 0 ? -steer : steer), -maxTurn, maxTurn);

    }

    void MoveCar()
    {
        //transform.Rotate(0f, currentTurn * Time.deltaTime, 0f);
        GetComponent<Rigidbody>().MoveRotation(Quaternion.FromToRotation(Vector3.up, heightHit.normal) * Quaternion.Euler(0f, transform.eulerAngles.y + currentTurn * Time.fixedDeltaTime, 0f));
        GetComponent<Rigidbody>().MovePosition(transform.position + transform.forward * currentSpeed * Time.fixedDeltaTime);
        //transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime);
    }

}