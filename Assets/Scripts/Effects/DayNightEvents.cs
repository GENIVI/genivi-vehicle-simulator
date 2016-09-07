/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class DayNightEvents : UnitySingleton<DayNightEvents> {

    public float nightTime = 18.5f;
    public float dayTime = 6f;

 //   public TOD_Sky sky;

    public event System.Action OnNight;
    public event System.Action OnDay;

    /*
    private bool night = false;

    protected override void Awake()
    {
        base.Awake();
/       if(sky == null)
        {
            var skygo = GameObject.Find("Sky Dome");
            if(skygo != null)
               sky = skygo.GetComponent<TOD_Sky>();
        }
    }

    public void Init()
    {
        if(sky == null)
        {
            var skygo = GameObject.Find("Sky Dome");
            if(skygo != null)
                sky = skygo.GetComponent<TOD_Sky>();
        }
        Start();
    }   

    void Start()
    {
        if(sky == null)
            return;

        if(sky.Cycle.Hour > dayTime && sky.Cycle.Hour < nightTime)
            night = false;
        else
            night = true;
    }

    void Update()
    {
        if(sky == null)
            return;

        if(night && sky.Cycle.Hour > dayTime && sky.Cycle.Hour < nightTime && OnDay != null)
        {
            night = false;
            OnDay();
        }
        else if(!night && (sky.Cycle.Hour < dayTime || sky.Cycle.Hour > nightTime) && OnNight != null)
        {
            night = true;
            OnNight();
        }
    }
    */
}
