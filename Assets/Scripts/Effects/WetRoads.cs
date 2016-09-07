/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class WetRoads : MonoBehaviour {

    public float wetness = 0f;
    private float wetnessTarget = 0f;
    public float fadeTime = 4f;

    public float rainyness = 0f;

    public static WetRoads instance
    {
        get;
        private set;
    }

    public event System.Action<float> OnWetness;

    public void SetWetness(float wet)
    {
        wetness = wet;
        wetnessTarget = wet;
        Shader.SetGlobalFloat("_WetRoads", wet);
        if (OnWetness != null)
            OnWetness(wet);
    }

    public void SetRainLevel(float rain)
    {
        rainyness = rain;
    }

    public bool IsWet()
    {
        return wetnessTarget > 0.5f;
    }

    public void ToggleWet(bool val)
    {
        if (val)
            wetnessTarget = 1f;
        else
            wetnessTarget = 0f;
    }

    void Awake()
    {
        instance = this;
        wetness = 0f;
        wetnessTarget = 0f;
        SetWetness(0f);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (wetnessTarget > 0.5f)
                wetnessTarget = 0f;
            else
                wetnessTarget = 1f;
        }

        if (wetness != wetnessTarget)
        {
            wetness = Mathf.MoveTowards(wetness, wetnessTarget, Time.deltaTime / fadeTime);
            Shader.SetGlobalFloat("_WetRoads", wetness);
            if (OnWetness != null)
                OnWetness(wetness);
        }

    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }


}
