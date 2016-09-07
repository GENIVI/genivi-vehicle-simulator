/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;



public class SteeringWheelInputController : InputController {

    private static SteeringWheelInputController inited = null;

    private enum SelectState { LEFT, REST, RIGHT }

    private SelectState currentSelectState = SelectState.REST;
    private const float selectThreshold = 0.13f;
    private const float confirmTimeout = 1f;
    private bool isConfirmDown = true;

    private float steerInput = 0f;
    private float accelInput = 0f;
    private int constant = 0;
    private int damper = 0;
    private int springSaturation = 0;
    private int springCoefficient = 0;


    private bool forceFeedbackPlaying = false;
    private bool debugInfo = false;

    private string brakeAxis;
    private string gasAxis;
    private int minBrake;
    private int maxBrake;
    private int minGas;
    private int maxGas;

    private GUIStyle debugStyle;

    private int wheelIndex = 0;
    private int pedalIndex = 1;

    public float FFBGain = 1f;

    protected override void Start()
    {
        base.Start();

        debugStyle = new GUIStyle();
        debugStyle.fontSize = 45;
        debugStyle.normal.textColor = Color.white;

        if (inited == null)
        {
            inited = this;
        }
        else
        {
            return;
        }

        DirectInputWrapper.Init();

        bool ff0 = DirectInputWrapper.HasForceFeedback(0);
        if (DirectInputWrapper.DevicesCount() > 1)
        {
            bool ff1 = DirectInputWrapper.HasForceFeedback(1);

            if (ff1 && !ff0)
            {
                wheelIndex = 1;
                pedalIndex = 0;
            }
            else if (ff0 && !ff1)
            {
                wheelIndex = 0;
                pedalIndex = 1;
            }
            else
                Debug.Log("STEERINGWHEEL: Multiple devices and couldn't find steering wheel device index");
        }

            minBrake = AppController.Instance.appSettings.minBrake;
        maxBrake = AppController.Instance.appSettings.maxBrake;
        minGas = AppController.Instance.appSettings.minGas;
        maxGas = AppController.Instance.appSettings.maxGas;
        gasAxis = AppController.Instance.appSettings.gasAxis;
        brakeAxis = AppController.Instance.appSettings.brakeAxis;
        FFBGain = AppController.Instance.appSettings.FFBMultiplier;

        
    }

    IEnumerator SpringforceFix()
    {
        yield return new WaitForSeconds(1f);
        StopSpringForce();
        yield return new WaitForSeconds(0.5f);
        InitSpringForce(0, 0);
    }

    public override void Init() {
        forceFeedbackPlaying = true;
    }

    public override void CleanUp()
    {
        forceFeedbackPlaying = false;
        constant = 0;
        damper = 0;
    }

    public void SetConstantForce(int force)
    {
        constant= force;
    }

    public void SetDamperForce(int force)
    {
        damper = force;
    }

    public void InitSpringForce(int sat, int coeff)
    {
        StartCoroutine(_InitSpringForce(sat, coeff));
    }

    public void StopSpringForce()
    {
        Debug.Log("stopping spring" + DirectInputWrapper.StopSpringForce(wheelIndex));
    }

    private IEnumerator _InitSpringForce(int sat, int coeff)
    {

        yield return new WaitForSeconds(1f);


        Debug.Log("stopping spring" + DirectInputWrapper.StopSpringForce(wheelIndex));
        yield return new WaitForSeconds(1f);
        long res = -1;
        int tries = 0;
        while (res < 0) {
            res  = DirectInputWrapper.PlaySpringForce(wheelIndex, 0, Mathf.RoundToInt(sat * FFBGain), Mathf.RoundToInt(coeff * FFBGain));
            Debug.Log("starting spring" + res);

            tries++;
            if(tries > 150)
            {
                Debug.Log("coudn't init spring force. aborting");
                break;
            }

            yield return null;
        }


    }

    public void SetSpringForce(int sat, int coeff)
    {
        springCoefficient = coeff;
        springSaturation = sat;
    }

    public void OnGUI()
    {
        if(debugInfo) {
            GUI.Label(new Rect(20, Screen.height - 180, 500, 100), "Raw Input: " + accelInput, debugStyle);
            GUI.Label(new Rect(20, Screen.height - 100, 500, 100), "Adjusted Input: " + GetAccelBrakeInput(), debugStyle);
        }
    }

    private IEnumerator InitForceFeedback()
    {
        constant = 0;
        damper = 0;
        springCoefficient = 0;
        springSaturation = 0;
        yield return new WaitForSeconds(0.5f);




        yield return new WaitForSeconds(0.5f);
        forceFeedbackPlaying = true;
    }



    public override void OnUpdate()
    {
        if (inited != this)
            return;

        //check for SelectLeft/right actions
        if(currentSelectState == SelectState.REST && GetSteerInput() < -selectThreshold)
        {
            currentSelectState = SelectState.LEFT;
            TriggerEvent(EventType.SELECT_CHOICE_LEFT);
        }
        else if(currentSelectState == SelectState.LEFT && GetSteerInput() > -selectThreshold)
        {
            currentSelectState = SelectState.REST;
        }
        else if(currentSelectState == SelectState.REST && GetSteerInput() > selectThreshold)
        {
            currentSelectState = SelectState.RIGHT;
            TriggerEvent(EventType.SELECT_CHOICE_RIGHT);
        }
        else if(currentSelectState == SelectState.RIGHT && GetSteerInput() < selectThreshold)
        {
            currentSelectState = SelectState.REST;
        }

        //Check for Throttle confirm
        if(isConfirmDown && GetAccelBrakeInput() < selectThreshold)
        {
            isConfirmDown = false;
        }       
        else if(!isConfirmDown && GetAccelBrakeInput() > selectThreshold)
        {
            isConfirmDown = true;
            if(Time.timeSinceLevelLoad > confirmTimeout)
            {
                TriggerEvent(EventType.SELECT_CHOICE_CONFIRM);
            }
        }




        DirectInputWrapper.Update();

        {
            DeviceState state =  DirectInputWrapper.GetStateManaged(wheelIndex);
            steerInput = state.lX / 32768f;
            accelInput = state.rglSlider[0] / -32768f;
           /* x = state.lX;
            y = state.lY;
            z = state.lZ;
            s0 = state.rglSlider[0];
            s1 = state.rglSlider[1];*/
            if (forceFeedbackPlaying)
            {
                DirectInputWrapper.PlayConstantForce(wheelIndex, Mathf.RoundToInt(constant * FFBGain));
                DirectInputWrapper.PlayDamperForce(wheelIndex, Mathf.RoundToInt(damper * FFBGain));
                DirectInputWrapper.PlaySpringForce(wheelIndex, 0, Mathf.RoundToInt(springSaturation * FFBGain), springCoefficient);
            }


            if(DirectInputWrapper.DevicesCount() > 1)
            {
                DeviceState state2 = DirectInputWrapper.GetStateManaged(pedalIndex);
                int gas = 0;
                int brake = 0;

               /* x2 = state2.lX;
                y2 = state2.lY;
                z2 = state2.lZ;
                s02 = state2.rglSlider[0];
                s12 = state2.rglSlider[1];*/

                switch (gasAxis) {
                    case "X": 
                        gas = state2.lX;
                        break;
                    case "Y":
                        gas = state2.lY;
                        break;
                    case "Z":
                        gas = state2.lZ;
                        break;
                }

                switch (brakeAxis)
                {
                    case "X":
                        brake = state2.lX;
                        break;
                    case "Y":
                        brake = state2.lY;
                        break;
                    case "Z":
                        brake = state2.lZ;
                        break;
                }


                float totalGas = (maxGas - minGas);
                float totalBrake = (maxBrake - minBrake);

                accelInput = (gas - minGas) / totalGas - (brake - minBrake) / totalBrake;
            }
        }


    }

    public override float GetAccelBrakeInput()
    {
        if (accelInput >= 0)
            return PedalInputController.Instance.throttleInputCurve.Evaluate(accelInput);
        else
            return -PedalInputController.Instance.brakeInputCurve.Evaluate(-accelInput);
    }

    public override float GetSteerInput()
    {
        return steerInput;
    }

    public override float GetHandBrakeInput()
    {
        return 0f;
    }
}
