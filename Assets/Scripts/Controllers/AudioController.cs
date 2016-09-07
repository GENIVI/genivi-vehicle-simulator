/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class AudioController : PersistentUnitySingleton<AudioController> {

    public float Volume {
        get
        {
            return volumeMultiplier;
        }
    }

    public float VehicleVolume
    {
        get { return vehicleVolumeMultiplier; }
        set 
        { 
            vehicleVolumeMultiplier = value;
            if(OnVehicleVolumeChange != null)
                OnVehicleVolumeChange(vehicleVolumeMultiplier);
        }
    }

    public float MusicVolume
    {
        get { return musicVolumeMultiplier; }
        set 
        {
            musicVolumeMultiplier = value;
            if(OnMusicVolumeChange != null)
                OnMusicVolumeChange(musicVolumeMultiplier);
        }
    }

    public float FoleyVolume
    {
        get { return foleyVolumeMultiplier; }
        set
        {
            foleyVolumeMultiplier = value;
            if(OnFoleyVolumeChange != null)
                OnFoleyVolumeChange(foleyVolumeMultiplier);
        }
    }

    private AudioClip volumeUp;
    private AudioClip volumeDown;

    public event System.Action<float> OnVolumeChange;
    public event System.Action<float> OnVehicleVolumeChange;
    public event System.Action<float> OnMusicVolumeChange;
    public event System.Action<float> OnFoleyVolumeChange;

    public float volumeStep = 0.05f;

    private float volumeMultiplier = 1f;
    private float musicVolumeMultiplier = 0.8f;
    private float vehicleVolumeMultiplier = 0.8f;
    private float foleyVolumeMultiplier = 0.8f;
 
    private GameObject selectMusic;

    public void PlaySelectMusic()
    {
        if (selectMusic == null)
        {
            selectMusic = GameObject.Instantiate(Resources.Load<GameObject>("SelectMusic")) as GameObject;
            DontDestroyOnLoad(selectMusic);
        }
        if (!selectMusic.GetComponent<AudioSource>().isPlaying)
            selectMusic.GetComponent<AudioSource>().Play();

        if (selectMusic.GetComponent<SetMusicVolume>().GetVolumePercentage() < 0.1f)
        {
            FadeSelectMusic(1f);
        }
    }

    public void FadeSelectMusic(float target)
    {
        StartCoroutine(_FadeSelectMusic(target));
    }

    private IEnumerator _FadeSelectMusic(float target)
    {
        var vol = selectMusic.GetComponent<SetMusicVolume>();
        float start = 1f - target;
        float startTime = Time.time;

        while (Time.time - startTime < 3f)
        {
            vol.SetVolumePercentage(Mathf.Lerp(start, target, (Time.time - startTime) / 3f));
            yield return null;
        }
        vol.SetVolumePercentage(target);

    }

    protected override void Awake()
    {
        base.Awake();
        if (GetComponent<AudioSource>() == null)
        {
            gameObject.AddComponent<AudioSource>();
            GetComponent<AudioSource>().spatialBlend = 0f;
        }

        GetComponent<AudioSource>().volume = volumeMultiplier;

        volumeUp = Resources.Load<AudioClip>("VolumeUp");
        volumeDown = Resources.Load<AudioClip>("VolumeDown");
    }

    void OnEnable()
    {
        AppController.Instance.AdminInput.VolumeUp += IncreaseVolume;
        AppController.Instance.AdminInput.VolumeDown += DecreaseVolume;
    }

    void OnDisable()
    {
        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
        {
            AppController.Instance.AdminInput.VolumeUp -= IncreaseVolume;
            AppController.Instance.AdminInput.VolumeDown -= DecreaseVolume;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        Resources.UnloadAsset(volumeUp);
        Resources.UnloadAsset(volumeDown);
    }

    public void PlayClip(AudioClip clip) {
        GetComponent<AudioSource>().PlayOneShot(clip, volumeMultiplier);
    }


    public void IncreaseVolume() {
        volumeMultiplier += volumeStep;

        if(OnVolumeChange != null)
            OnVolumeChange(Volume);

        PlayClip(volumeUp);
    }

    public void DecreaseVolume()
    {
        volumeMultiplier -= volumeStep;
        if(volumeMultiplier < 0f)
            volumeMultiplier = 0f;

        if(OnVolumeChange != null)
            OnVolumeChange(Volume);

        PlayClip(volumeDown);
    }
}
