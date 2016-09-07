/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using UnityEngine.Audio;


public class VehicleAudio : MonoBehaviour {

    public EngineAudio engineLoad;
    public EngineAudio engineNoLoad;
    public EngineAudio exhaustLoad;
    public EngineAudio exhaustNoLoad;

    public float rpm;
    public float load;

    public AnimationCurve loadFade;
    public float maxLoadAtten = 0f;
    public float maxNoLoadAtten = -3f;
    public float minAtten = -80;

    public float maxRpm = 7500;
    
	void Update () {

        rpm = Mathf.Clamp(rpm, 0, maxRpm);
        load = Mathf.Clamp(load, 0, 1);

        engineNoLoad.SetRPM(rpm);
        exhaustNoLoad.SetRPM(rpm);
        engineLoad.SetRPM(rpm);
        exhaustLoad.SetRPM(rpm);

        float loadAtten = minAtten + (loadFade.Evaluate(load) * (maxLoadAtten - minAtten));
        float noLoadAtten = minAtten + (loadFade.Evaluate(1 - load) * (maxNoLoadAtten - minAtten));

        engineNoLoad.group.audioMixer.SetFloat(engineNoLoad.volumeParamName, noLoadAtten);       
        exhaustNoLoad.group.audioMixer.SetFloat(exhaustNoLoad.volumeParamName, noLoadAtten);

        engineLoad.group.audioMixer.SetFloat(engineLoad.volumeParamName, loadAtten);
        exhaustLoad.group.audioMixer.SetFloat(exhaustLoad.volumeParamName, loadAtten);

    }
}
