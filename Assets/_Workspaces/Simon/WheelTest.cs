using UnityEngine;
using System.Collections;

public class WheelTest : MonoBehaviour {
    int wheelIndex = 0;
    int pedalIndex = 1;

	// Use this for initialization
	void Start () {
        DirectInputWrapper.Init();

        /*LogitechGSDK.LogiControllerPropertiesData properties = new LogitechGSDK.LogiControllerPropertiesData();
        properties.wheelRange = 900;
        properties.forceEnable = true;
        properties.overallGain = 80;
        properties.springGain = 80;
        properties.damperGain = 80;
        properties.allowGameSettings = true;
        properties.combinePedals = true;
        properties.defaultSpringEnabled = false;
        properties.defaultSpringGain = 0;
        LogitechGSDK.LogiSetPreferredControllerProperties(properties);
        */

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

    }

    private int constant = 0;

    private bool forceFeedbackPlaying = false;

    private float x;
    private float y;
    private float z;
    private float s0;
    private float s1;
    private float x2;
    private float y2;
    private float z2;
    private float s02;
    private float s12;


    public void OnGUI()
    {
        GUILayout.Label("X: " + x);
        GUILayout.Label("Y: " + y);
        GUILayout.Label("Z: " + z);
        GUILayout.Label("S0: " + s0);
        GUILayout.Label("S1: " + s1);
        GUILayout.Label("X2: " + x2);
        GUILayout.Label("Y2: " + y2);
        GUILayout.Label("Z2: " + z2);
        GUILayout.Label("S02: " + s02);
        GUILayout.Label("S12: " + s12);
        
    }

	// Update is called once per frame
	void Update () {
        DirectInputWrapper.Update();

        DeviceState rec = DirectInputWrapper.GetStateManaged(wheelIndex);
    //    steerInput = rec.lX / 32768f;
  //      accelInput = rec.rglSlider[0] / -32768f;
        x = rec.lX;
        y = rec.lY;
        z = rec.lZ;
        s0 = rec.rglSlider[0];
        s1 = rec.rglSlider[1];

        Debug.Log("d0 = " + DirectInputWrapper.GetProductNameManaged(wheelIndex));
        Debug.Log("ff0 = " + DirectInputWrapper.HasForceFeedback(wheelIndex));

        if (forceFeedbackPlaying)
            DirectInputWrapper.PlayConstantForce(wheelIndex, constant);


        if (DirectInputWrapper.DevicesCount() > 1)
        {
            Debug.Log("d1 = " + DirectInputWrapper.GetProductNameManaged(pedalIndex));
            Debug.Log("ff1 = " + DirectInputWrapper.HasForceFeedback(pedalIndex));
            DeviceState rec2 = DirectInputWrapper.GetStateManaged(pedalIndex);
       //     accelInput = ((32768 + rec2.lZ) - (rec2.lY + 32768f)) / 65535f;
            x2 = rec2.lX;
            y2 = rec2.lY;
            z2 = rec2.lZ;
            s02 = rec2.rglSlider[0];
            s12 = rec2.rglSlider[1];
        }
        
	}
}
