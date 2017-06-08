/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AdminInputController : InputController
{

    private Dictionary<KeyCode, EventType> events = new Dictionary<KeyCode, EventType>
    {
        {KeyCode.LeftArrow, EventType.NAVIGATE_BACK},
        {KeyCode.Alpha1, EventType.NUMERIC_1},
        {KeyCode.Alpha2, EventType.NUMERIC_2},
        {KeyCode.Alpha3, EventType.NUMERIC_3},
        {KeyCode.Alpha4, EventType.NUMERIC_4},
        {KeyCode.Alpha5, EventType.NUMERIC_5},
        {KeyCode.Alpha6, EventType.NUMERIC_6},
        {KeyCode.Alpha7, EventType.NUMERIC_7},
        {KeyCode.Alpha8, EventType.NUMERIC_8},
        {KeyCode.Alpha9, EventType.NUMERIC_9},
        {KeyCode.Alpha0, EventType.NUMERIC_0},
        {KeyCode.V, EventType.CHANGE_CAM_VIEW},
        {KeyCode.F, EventType.OPEN_CAM_SETTINGS},
        {KeyCode.Z, EventType.SWAP_INPUT_METHOD},
        {KeyCode.Plus, EventType.VOLUME_UP},
        {KeyCode.KeypadPlus, EventType.VOLUME_UP},
        {KeyCode.KeypadMinus, EventType.VOLUME_DOWN},
        {KeyCode.Minus, EventType.VOLUME_DOWN},
        {KeyCode.I, EventType.TOGGLE_STATS},
        {KeyCode.C, EventType.TOGGLE_CONSOLE},
        {KeyCode.None, EventType.TOGGLE_STEERING_WHEEL_VISIBILITY},
        {KeyCode.W, EventType.REPOSITION},
        {KeyCode.K, EventType.END_SCENE},
        {KeyCode.Y, EventType.CONFIRM_POPUP},
        {KeyCode.N, EventType.CANCEL_POPUP},
        {KeyCode.G, EventType.TOGGLE_DEBUG_WHEEL},
        {KeyCode.A, EventType.SELECT_ENVIRONMENT_1},
        {KeyCode.B, EventType.SELECT_ENVIRONMENT_2},
        {KeyCode.R, EventType.SELECT_ROAD_RIGHT},
        {KeyCode.L, EventType.SELECT_ROAD_LEFT},
        {KeyCode.F10, EventType.DATA_STREAM_EVENT1 },
        { KeyCode.F11, EventType.DATA_STREAM_EVENT2  },
        {KeyCode.F12, EventType.DATA_STREAM_EVENT3 }

    };

    private Dictionary<KeyCode, EventType> shiftModifiedEvents = new Dictionary<KeyCode, EventType>
    {
        {KeyCode.Alpha1, EventType.SHIFT_NUMERIC_1},
        {KeyCode.Alpha2, EventType.SHIFT_NUMERIC_2},
        {KeyCode.Alpha3, EventType.SHIFT_NUMERIC_3},
        {KeyCode.Alpha4, EventType.SHIFT_NUMERIC_4},
        {KeyCode.Alpha5, EventType.SHIFT_NUMERIC_5},
        {KeyCode.Alpha6, EventType.SHIFT_NUMERIC_6},
        {KeyCode.Alpha7, EventType.SHIFT_NUMERIC_7},
        {KeyCode.Alpha8, EventType.SHIFT_NUMERIC_8},
        {KeyCode.Alpha9, EventType.SHIFT_NUMERIC_9},

    };

    private Dictionary<KeyCode, EventType> controlModifiedEvents = new Dictionary<KeyCode, EventType>
    {
        {KeyCode.Alpha0, EventType.CTRL_NUMERIC_0},
        {KeyCode.Alpha1, EventType.CTRL_NUMERIC_1},
        {KeyCode.Alpha2, EventType.CTRL_NUMERIC_2},
        {KeyCode.Alpha3, EventType.CTRL_NUMERIC_3},
        {KeyCode.Alpha4, EventType.CTRL_NUMERIC_4},
        {KeyCode.Alpha5, EventType.CTRL_NUMERIC_5},
        {KeyCode.Alpha6, EventType.CTRL_NUMERIC_6},
        {KeyCode.Alpha7, EventType.CTRL_NUMERIC_7},
        {KeyCode.Alpha8, EventType.CTRL_NUMERIC_8},
        {KeyCode.Alpha9, EventType.CTRL_NUMERIC_9},

    };

    public override void OnUpdate()
    {


        if(Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            foreach(var e in shiftModifiedEvents)
            {
                if(Input.GetKeyDown(e.Key))
                {
                    TriggerEvent(e.Value);
                }
            }
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            foreach (var e in controlModifiedEvents)
            {
                if (Input.GetKeyDown(e.Key))
                {
                    TriggerEvent(e.Value);
                }
            }
        }
        else
        {
            foreach(var e in events)
            {
                if(Input.GetKeyDown(e.Key))
                {
                    TriggerEvent(e.Value);
                }
            }
        }

    }

    public override float GetAccelBrakeInput()
    {
        throw new System.NotImplementedException();
    }

    public override float GetHandBrakeInput()
    {
        throw new System.NotImplementedException();
    }

    public override float GetSteerInput()
    {
        throw new System.NotImplementedException();
    }
}
