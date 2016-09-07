/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class SetMusicVolume : MonoBehaviour {

    private float initialVolume;

    private float volumePercentage = 1f;

    void OnEnable()
    {
        AudioController.Instance.OnMusicVolumeChange += UpdateVolume;
    }

    void OnDisable()
    {
        if(AudioController.IsInstantiated)
            AudioController.Instance.OnMusicVolumeChange -= UpdateVolume;
    }

    void Awake()
    {
        initialVolume = GetComponent<AudioSource>().volume;
        UpdateVolume(AudioController.Instance.MusicVolume);
    }

    void UpdateVolume(float newVolume)
    {
        GetComponent<AudioSource>().volume = initialVolume * volumePercentage * newVolume;
    }

    public void SetVolumePercentage(float percent)
    {
        volumePercentage = percent;
        UpdateVolume(AudioController.Instance.MusicVolume);
    }

    public float GetVolumePercentage()
    {
        return volumePercentage;
    }
}
