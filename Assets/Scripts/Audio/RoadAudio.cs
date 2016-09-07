/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;

[System.Serializable]
public class TireSound
{
    public AudioClip clip;
    public bool usesSpeedCurve = false;
    public AnimationCurve speedCurve;
    public float maxSpeed;
    public bool usesTractionCurve = false;
    public AnimationCurve tractionCurve;
    public TractionType tractionType;
    public AudioMixerGroup mixerGroup;
}

public enum TractionType { ACCELL, BRAKE, MIN}

public class TireSoundInstance
{
    public AudioSource source;
    public TireSound details;
}
public class RoadAudio : MonoBehaviour {

    public List<TireSound> tireSounds;
    private List<TireSoundInstance> instances;

    public float accellTraction;
    public float brakeTraction;
    public float speed;
    public RoadSurface surface;

    public AudioMixer mixer;
    public string tarmacVolumeString;
    public string offroadVolumeString;

    public AudioSource surfaceBump;
    public List<AudioClip> surfaceBumpClips;
    public AnimationCurve surfaceBumpSpeedCurve;

    public void Awake( )
    {
        instances = new List<TireSoundInstance>();
        foreach (var sound in tireSounds)
        {
            var instance = new TireSoundInstance();
            instance.details = sound;
            instance.source = gameObject.AddComponent<AudioSource>();
            instance.source.outputAudioMixerGroup = sound.mixerGroup;
            instance.source.volume = 0f;
            instance.source.clip = sound.clip;
            instance.source.Play();
            instance.source.loop = true;
            instances.Add(instance);
        }

    }

    public void PlaySurfaceBump()
    {
        AudioClip clip = surfaceBumpClips[Mathf.RoundToInt(Random.Range(0, surfaceBumpClips.Count - 1))];
        surfaceBump.PlayOneShot(clip, surfaceBumpSpeedCurve.Evaluate(speed / 100));
    }

    public void Update()
    {
        if(surface == RoadSurface.Offroad)
        {
            mixer.SetFloat(tarmacVolumeString, -80f);
            mixer.SetFloat(offroadVolumeString, 0f);
        } else if(surface == RoadSurface.Tarmac)
        {
            mixer.SetFloat(tarmacVolumeString, 0f);
            mixer.SetFloat(offroadVolumeString, -80f);
        }
        else if(surface == RoadSurface.Airborne)
        {
            mixer.SetFloat(tarmacVolumeString, -80f);
            mixer.SetFloat(offroadVolumeString, -80f);
        }

        foreach(var instance in instances)
        {
            float volDB = 0f;
            if(instance.details.usesSpeedCurve)
            {
                float vol = instance.details.speedCurve.Evaluate(speed / instance.details.maxSpeed);
                volDB -= (1 - vol) * 80f;
            }
            if(instance.details.usesTractionCurve)
            {
                float traction = 1f;
                switch (instance.details.tractionType) {
                    case TractionType.ACCELL:
                        traction = accellTraction;
                        break;
                    case TractionType.BRAKE:
                        traction = brakeTraction;
                        break;
                    case TractionType.MIN:
                        traction = Mathf.Min(accellTraction, brakeTraction);
                        break;
                }
                float vol = instance.details.tractionCurve.Evaluate(traction);
                volDB -= (1 - vol) * 80f;
            }
            instance.source.volume = EngineAudio.DbToLinear(volDB);
        }


    }
}
