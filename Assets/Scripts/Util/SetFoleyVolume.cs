/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class SetFoleyVolume : MonoBehaviour
{

    private float initialVolume;

    void OnEnable()
    {
        AudioController.Instance.OnFoleyVolumeChange += UpdateVolume;
    }

    void OnDisable()
    {
        if(AudioController.IsInstantiated)
            AudioController.Instance.OnFoleyVolumeChange -= UpdateVolume;
    }

    void Awake()
    {
        initialVolume = GetComponent<AudioSource>().volume;
        GetComponent<AudioSource>().volume = 0f;
        //UpdateVolume(AudioController.Instance.FoleyVolume);
    }

    IEnumerator Start()
    {
        yield return null;
        GetComponent<AudioSource>().Play();
        float startTime = Time.time;
        while (Time.time - startTime < 4f)
        {
            UpdateVolume(Mathf.Lerp(0f, AudioController.Instance.FoleyVolume, (Time.time - startTime) / 4f));
            yield return null;
        }
        UpdateVolume(AudioController.Instance.FoleyVolume);
    }

    void UpdateVolume(float newVolume)
    {
        GetComponent<AudioSource>().volume = initialVolume * newVolume;
    }

}
