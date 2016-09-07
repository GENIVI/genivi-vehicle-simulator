/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KeyboardInputController : InputController {

    private const float SENSITIVITY = 3f;

    private  Dictionary<KeyCode, EventType> events = new Dictionary<KeyCode, EventType>
    {
        {KeyCode.UpArrow, EventType.SELECT_CHOICE_LEFT},
        {KeyCode.RightArrow, EventType.SELECT_CHOICE_RIGHT},
        {KeyCode.Return, EventType.SELECT_CHOICE_CONFIRM},
    };

    public override void OnUpdate()
    {
        foreach(var e in events)
        {           
            if(Input.GetKeyDown(e.Key))
            {
                TriggerEvent(e.Value);
            }
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            accell = Mathf.MoveTowards(accell, 1f, SENSITIVITY * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            accell = Mathf.MoveTowards(accell, -1f, SENSITIVITY * Time.deltaTime);
        } else
        {
            accell = Mathf.MoveTowards(accell, 0f, SENSITIVITY * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            steer = Mathf.MoveTowards(steer, -1f, SENSITIVITY * Time.deltaTime);
        }
        else if (Input.GetKey(KeyCode.RightArrow))
        {
            steer = Mathf.MoveTowards(steer, 1f, SENSITIVITY * Time.deltaTime);
        }
        else
        {
            steer = Mathf.MoveTowards(steer, 0f, SENSITIVITY * Time.deltaTime);
        }

    }

    private float accell = 0f;
    private float steer = 0f;

    public override float GetAccelBrakeInput()
    {
        return accell;
    }

    public override float GetSteerInput()
    {
        return steer;
    }

    public override float GetHandBrakeInput()
    {
        return Input.GetAxis("Jump");
    }
}
