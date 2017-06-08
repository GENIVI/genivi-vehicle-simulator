/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;

public abstract class InputController : MonoBehaviour
{

    protected enum EventType { 
        SELECT_CHOICE_LEFT, 
        SELECT_CHOICE_RIGHT,
        SELECT_CHOICE_CONFIRM, 
        NAVIGATE_BACK, 
        CHANGE_CAM_VIEW, 
        HORN,
        REPOSITION,
        END_SCENE,
        CONFIRM_POPUP,
        CANCEL_POPUP,
        VOLUME_UP,
        VOLUME_DOWN,
        TOGGLE_STATS,
        TOGGLE_CONSOLE,
        TOGGLE_STEERING_WHEEL_VISIBILITY,
        TOGGLE_DEBUG_WHEEL,
        SWAP_INPUT_METHOD,
        OPEN_CAM_SETTINGS,
        NUMERIC_1, 
        NUMERIC_2,
        NUMERIC_3,
        NUMERIC_4,
        NUMERIC_5,
        NUMERIC_6,
        NUMERIC_7,
        NUMERIC_8,
        NUMERIC_9,
        NUMERIC_0,
        SELECT_ENVIRONMENT_1,
        SELECT_ENVIRONMENT_2,
        SELECT_ROAD_LEFT,
        SELECT_ROAD_RIGHT,
        SHIFT_NUMERIC_1,
        SHIFT_NUMERIC_2,
        SHIFT_NUMERIC_3,
        SHIFT_NUMERIC_4,
        SHIFT_NUMERIC_5,
        SHIFT_NUMERIC_6,
        SHIFT_NUMERIC_7,
        SHIFT_NUMERIC_8,
        SHIFT_NUMERIC_9,
        CTRL_NUMERIC_0,
        CTRL_NUMERIC_1,
        CTRL_NUMERIC_2,
        CTRL_NUMERIC_3,
        CTRL_NUMERIC_4,
        CTRL_NUMERIC_5,
        CTRL_NUMERIC_6,
        CTRL_NUMERIC_7,
        CTRL_NUMERIC_8,
        CTRL_NUMERIC_9,
        DATA_STREAM_EVENT1,
        DATA_STREAM_EVENT2,
        DATA_STREAM_EVENT3,
    };

    private Dictionary<EventType, System.Func<System.Action>> events;

    protected virtual void Start()
    {
        events = new Dictionary<EventType, System.Func<System.Action>> { 
                { EventType.SELECT_CHOICE_LEFT, () => SelectChoiceLeft},
                { EventType.SELECT_CHOICE_RIGHT, () => SelectChoiceRight },
                { EventType.SELECT_CHOICE_CONFIRM,() => SelectChoiceConfirm },
                { EventType.NAVIGATE_BACK, () => NavigateBack },
                { EventType.HORN, () => Horn },
                { EventType.REPOSITION, () => Reposition },
                { EventType.END_SCENE, () => EndScene },
                { EventType.CONFIRM_POPUP, () => ConfirmPopup},
                { EventType.CANCEL_POPUP, () => CancelPopup},
                { EventType.VOLUME_UP, () => VolumeUp},
                { EventType.VOLUME_DOWN, () => VolumeDown},
                { EventType.TOGGLE_STATS, () => ToggleStats},
                { EventType.TOGGLE_CONSOLE, () => ToggleConsole},
                { EventType.TOGGLE_STEERING_WHEEL_VISIBILITY, () => ToggleSteeringWheelVisibility},
                { EventType.NUMERIC_1, () => Numeric1 },
                { EventType.NUMERIC_2, () => Numeric2},
                { EventType.NUMERIC_3, () => Numeric3},
                { EventType.NUMERIC_4, () => Numeric4},
                { EventType.NUMERIC_5, () => Numeric5},
                { EventType.NUMERIC_6, () => Numeric6},
                { EventType.NUMERIC_7, () => Numeric7},
                { EventType.NUMERIC_8, () => Numeric8},
                { EventType.NUMERIC_9, () => Numeric9},
                { EventType.NUMERIC_0, () => Numeric0},
                { EventType.SWAP_INPUT_METHOD, () => SwapInputMethod},
                { EventType.CHANGE_CAM_VIEW, () => ChangeCamView},
                {EventType.OPEN_CAM_SETTINGS, () => OpenCamSettings},
                {EventType.TOGGLE_DEBUG_WHEEL, () => ToggleDebugWheel},
                {EventType.SELECT_ENVIRONMENT_1, () => SelectEnvironment1},
                {EventType.SELECT_ENVIRONMENT_2, () => SelectEnvironment2},
                {EventType.SELECT_ROAD_LEFT, () => SelectRoadLeft},
                {EventType.SELECT_ROAD_RIGHT, () => SelectRoadRight},
                {EventType.SHIFT_NUMERIC_1, () => ShiftNumeric1 },
                {EventType.SHIFT_NUMERIC_2, () => ShiftNumeric2 },
                {EventType.SHIFT_NUMERIC_3, () => ShiftNumeric3 },
                {EventType.SHIFT_NUMERIC_4, () => ShiftNumeric4 },
                {EventType.SHIFT_NUMERIC_5, () => ShiftNumeric5 },
                {EventType.SHIFT_NUMERIC_6, () => ShiftNumeric6 },
                {EventType.SHIFT_NUMERIC_7, () => ShiftNumeric7 },
                {EventType.SHIFT_NUMERIC_8, () => ShiftNumeric8 },
                {EventType.SHIFT_NUMERIC_9, () => ShiftNumeric9 },
                {EventType.CTRL_NUMERIC_0, () => ControlNumeric0 },
                {EventType.CTRL_NUMERIC_1, () => ControlNumeric1 },
                {EventType.CTRL_NUMERIC_2, () => ControlNumeric2 },
                {EventType.CTRL_NUMERIC_3, () => ControlNumeric3 },
                {EventType.CTRL_NUMERIC_4, () => ControlNumeric4 },
                {EventType.CTRL_NUMERIC_5, () => ControlNumeric5 },
                {EventType.CTRL_NUMERIC_6, () => ControlNumeric6 },
                {EventType.CTRL_NUMERIC_7, () => ControlNumeric7 },
                {EventType.CTRL_NUMERIC_8, () => ControlNumeric8 },
                {EventType.CTRL_NUMERIC_9, () => ControlNumeric9 },
                {EventType.DATA_STREAM_EVENT1, () => DataStreamEvent1 },
                {EventType.DATA_STREAM_EVENT2, () => DataStreamEvent2 },
                {EventType.DATA_STREAM_EVENT3, () => DataStreamEvent3 },
        };
    }

    protected void TriggerEvent(EventType type)
    {
        if(events[type]() != null)
        {
            events[type]()();
        }
    }

    //one-shot events here
    #pragma warning disable 0067
    public event System.Action SelectChoiceLeft;
    public event System.Action SelectChoiceRight;
    public event System.Action SelectChoiceConfirm;
    public event System.Action NavigateBack;
    public event System.Action Numeric1;
    public event System.Action Numeric2;
    public event System.Action Numeric3;
    public event System.Action Numeric4;
    public event System.Action Numeric5;
    public event System.Action Numeric6;
    public event System.Action Numeric7;
    public event System.Action Numeric8;
    public event System.Action Numeric9;
    public event System.Action Numeric0;
    public event System.Action ChangeCamView;
    public event System.Action SwapInputMethod;
    public event System.Action Horn;
    public event System.Action Reposition;
    public event System.Action EndScene;
    public event System.Action ConfirmPopup;
    public event System.Action CancelPopup;
    public event System.Action VolumeUp;
    public event System.Action VolumeDown;
    public event System.Action ToggleStats;
    public event System.Action ToggleConsole;
    public event System.Action ToggleSteeringWheelVisibility;
    public event System.Action OpenCamSettings;
    public event System.Action ToggleDebugWheel;
    public event System.Action SelectEnvironment1;
    public event System.Action SelectEnvironment2;
    public event System.Action SelectRoadLeft;
    public event System.Action SelectRoadRight;
    public event System.Action ShiftNumeric1;
    public event System.Action ShiftNumeric2;
    public event System.Action ShiftNumeric3;
    public event System.Action ShiftNumeric4;
    public event System.Action ShiftNumeric5;
    public event System.Action ShiftNumeric6;
    public event System.Action ShiftNumeric7;
    public event System.Action ShiftNumeric8;
    public event System.Action ShiftNumeric9;
    public event System.Action ControlNumeric0;
    public event System.Action ControlNumeric1;
    public event System.Action ControlNumeric2;
    public event System.Action ControlNumeric3;
    public event System.Action ControlNumeric4;
    public event System.Action ControlNumeric5;
    public event System.Action ControlNumeric6;
    public event System.Action ControlNumeric7;
    public event System.Action ControlNumeric8;
    public event System.Action ControlNumeric9;
    public event System.Action DataStreamEvent1;
    public event System.Action DataStreamEvent2;
    public event System.Action DataStreamEvent3;
#pragma warning restore 0067

    //continuous input
    public abstract float GetSteerInput();

    public abstract float GetAccelBrakeInput();

    public abstract float GetHandBrakeInput();

    public void Update()
    {
        OnUpdate();
    }

    public abstract void OnUpdate();

    public virtual void Init()
    {

    }

    public virtual void CleanUp()
    {

    }

    public static InputController Create(System.Type type)
    {
        GameObject go = new GameObject();
        go.AddComponent(type);
        go.name = type.ToString();
        DontDestroyOnLoad(go);
        return go.GetComponent(type) as InputController;
    }
}
