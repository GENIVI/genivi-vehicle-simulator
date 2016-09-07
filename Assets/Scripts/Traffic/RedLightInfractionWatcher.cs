/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Linq;

public class RedLightInfractionWatcher : MonoBehaviour {

    bool redLight = false;
    public float checkRadius = 4f;
    public Vector3 entryPoint = Vector3.zero;

    private float startTime;

    void Awake()
    {
        startTime = Time.time;
    }


    void OnTriggerEnter(Collider other) 
    {

        if (!other.CompareTag("Player"))
            return;

        redLight = false;

        var traf = transform.root.GetComponent<TrafSystem>();
        var identifier = int.Parse(transform.parent.name);

        var entries = traf.intersections.Where(t => t.identifier == identifier + 1000);

        float minDist = 100000f;
        TrafEntry minEntry = null;
        foreach (var e in entries)
        {
            float d = Vector3.Distance(e.waypoints[0], other.transform.position);
            if (d < minDist)
            {
                minDist = d;
                minEntry = e;
            }
        }

        if (minEntry.light != null && minEntry.light.currentState == TrafLightState.RED)
        {
            redLight = true;
            entryPoint = minEntry.waypoints[0];
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (redLight && Vector3.Distance(entryPoint, other.transform.position) > checkRadius)
        {
            var infraction = new DrivingInfraction();
            infraction.id = TrackController.Instance.GetInfractions().Count;
            infraction.speed = TrackController.Instance.car.GetComponent<Rigidbody>().velocity.magnitude;
            infraction.type = "LIGHT";
            infraction.systemTime = System.DateTime.Now;
            infraction.sessionTime = Time.time - startTime;
            TrackController.Instance.AddInfraction(infraction);
        }

        redLight = false;
    }
}
