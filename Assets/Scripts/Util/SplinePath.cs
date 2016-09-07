/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplinePath : MonoBehaviour {

    public List<Vector3> pathNodes;

    public float TotalDistance
    {
        get;
        private set;
    }

    public Vector3[] PathNodesArray
    {
        get;
        private set;
    }

    public void Awake()
    {
        float distance = 0f;
        Vector3 last = iTween.PointOnPath(pathNodes.ToArray(), 0);
        int total = pathNodes.Count * 10;
        for (int p = 1; p <= total; p++)
        {
            Vector3 next = iTween.PointOnPath(pathNodes.ToArray(), (float)p/total);
            distance += Vector3.Distance(last, next);
            last = next;
        }

        TotalDistance = distance;
        PathNodesArray = pathNodes.ToArray();
    }

    public Vector3 PointOnPath(float percent)
    {
        while (percent > 1f)
            percent -= 1f;

        return iTween.PointOnPath(PathNodesArray, percent);
    }

    public Vector3 PointAtRealPercent(float percent) {
        while (percent > 1f)
            percent -= 1f; 

        float targetDist = percent * TotalDistance;
        int checks = Mathf.RoundToInt(TotalDistance);
        float currentPc = 0f;
        float distance = 0f;
        Vector3 last = PointOnPath(0f);
        float lastPc = 0f;
        for (int i = 0; i <= checks; i++)
        {
            currentPc = (float)i / checks;
            Vector3 next = PointOnPath(currentPc);
            float dist = Vector3.Distance(last, next);
            if (distance + dist > targetDist)
            {
                break;
            }
            last = next;
            lastPc = currentPc;
            distance += dist;
        }

        for (int i = 0; i <= 25; i++)
        {
            currentPc = lastPc + ((float)i / 25f) * (1f/checks);
            Vector3 next = PointOnPath(currentPc);
            float dist = Vector3.Distance(last, next);
            distance += dist;

            if (distance > targetDist)
                return next;

            last = next;

        }

        return last;
        
    }
}

