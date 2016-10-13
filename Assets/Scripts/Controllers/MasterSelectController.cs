/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MasterSelectController : MonoBehaviour {

    public List<BaseSelectController> controllers;

    public SelectScreenAdmin adminScreen;

    public int currentController;
    public float welcomeTime = 4f;

    public BaseAlphaFade welcome;
    public BaseAlphaFade loadBg;
    public OnGUITexture loadFg;

    public float startDelay = 4f;

    public Car fType;
    public Car xj;
    public Car l405;
    public Car lr4;

    public CarSelectController carSelect;
    public Animator garageDoor;
    public Animator barriers;
    public GameObject admin;

    public GameObject[] facade;

    public static Vector3 carLoadPosition;
    public static Quaternion carLoadRotation;

    public void Start()
    {
        foreach(var c in controllers)
        {
            c.enabled = false;
        }
        currentController = 0;
      //  welcome.SetStraight(1f);
       // loadBg.SetStraight(1f);
        //loadFg.color = Color.white;
        StartCoroutine(Welcome());

        AudioController.Instance.PlaySelectMusic();
    }

    private IEnumerator Welcome()
    {
        yield return null;
      /*  float startTime = Time.time;
        while(Time.time - startTime < welcomeTime)
        {
            float t = (Time.time - startTime)/welcomeTime;
            loadFg.placement.width = Mathf.Lerp(0, 202f, t);
            loadFg.uvs.width = t;
            yield return null;
        }
        welcome.SetTarget(0f);
        
        
        loadFg.FadeOut();
       * 
        loadBg.SetTarget(0f);
       * */
        foreach (var c in GetComponent<CarSelectController>().choices)
        {
            c.displayObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }

        LoadStage();        
    }

    private void LoadStage()
    {
        controllers[currentController].enabled = true;
        controllers[currentController].OnFinished = NextStage;
        controllers[currentController].OnBackCall = LastStage;
        controllers[currentController].OnSetup();
        //adminScreen.SetStage(currentController);
    }

    public void LastStage()
    {
        if(currentController > 0)
        {
            controllers[currentController--].enabled = false;
            {
                //adminScreen.ClearChoice(currentController);
                LoadStage();
            }
        }
    }

    public void NextStage()
    {
        controllers[currentController].enabled = false;
        //adminScreen.SelectChoice(currentController, controllers[currentController].currentChoice);
        if(++currentController >= controllers.Count)
        {
            //AppController.Instance.LoadDrivingScene(AppController.Instance.currentSessionSettings.selectedEnvironment);
            StartCoroutine(LoadScene());
        }
        else
        {
            LoadStage();
        }
    }


    private int quickLoadEnv = -1;
    private void Update()
    {
    	if(Input.GetKey(KeyCode.LeftShift))
    	{
            if(Input.GetKeyDown(KeyCode.Alpha1))
            {
                quickLoadEnv = 0;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                quickLoadEnv = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                quickLoadEnv = 2;
            }

            if(quickLoadEnv >= 0 && Input.GetKeyDown(KeyCode.A))
            {
                QuickLoad(quickLoadEnv, 0);
            }
            if (quickLoadEnv >= 0 && Input.GetKeyDown(KeyCode.B))
            {
                QuickLoad(quickLoadEnv, 1);
            }

        }
    }

    private void QuickLoad(int env, int car)
    {

        // AppController.Instance.currentSessionSettings.selectedCar = GetComponent(<CarSelectController>().choices[0].car);
        // AppController.Instance.currentSessionSettings.selectedCarShortname = GetComponent<CarSelectController>().choices[0].shortName;

        AppController.Instance.currentSessionSettings.selectedCar = GetComponent<CarSelectController>().choices[car].car;
        AppController.Instance.currentSessionSettings.selectedEnvironment = GetComponent<EnvironmentSelectController>().choices[env].environment;
        AudioController.Instance.FadeSelectMusic(0f);
        NetworkController.Instance.SelectCar(car);
        AppController.Instance.LoadDrivingScene(AppController.Instance.currentSessionSettings.selectedEnvironment);
    }

    IEnumerator LoadScene()
    {

        AudioController.Instance.FadeSelectMusic(0f);

        var barrierLeft = GetComponent<CarSelectController>().barrierLeft;
        var barrierRight = GetComponent<CarSelectController>().barrierRight;

        barriers.enabled = true;

        //just kill for now since anim doesn't work
        barrierLeft.SetActive(false);
        barrierRight.SetActive(false);

        GetComponent<SelectScreenForceFeedback>().enabled = false;
        var car = GetComponent<CarSelectController>().GetCurrentCar();
        carLoadPosition = car.transform.localPosition;
        carLoadRotation = car.transform.localRotation;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerCar"), LayerMask.NameToLayer("Default"), true);

        DestroyOnLoadManager.Instance.Init();

        
        foreach (var go in facade)
        {
            Destroy(go);
        }

        transform.root.gameObject.AddComponent<DontDestroyOnLoadDriving>();


        yield return AppController.Instance.LoadDrivingSceneAsync(AppController.Instance.currentSessionSettings.selectedEnvironment);

        //save last scene for infractions
        InfractionController.Instance.SetLastScene(AppController.Instance.currentSessionSettings.selectedEnvironment);

     //   yield return null;
        transform.root.position = TrackController.Instance.carSpawnTarget.transform.position;
        transform.root.rotation = TrackController.Instance.carSpawnTarget.transform.rotation;
       // GetComponent<SetMusicVolume>().enabled = false;
        

        garageDoor.enabled = true;
        //just kill for now since anim doesn't work
        garageDoor.gameObject.SetActive(false);

        var console = car.GetComponent<CarConsoleController>();
        if (console != null)
        {
            console.enabled = true;
            console.Init();
        }


    }

    IEnumerator FadeAudio()
    {
        float startVol = GetComponent<AudioSource>().volume;
        float startTime = Time.time;
        while(Time.time - startTime < 3f)
        {
            GetComponent<AudioSource>().volume = Mathf.Lerp(startVol, 0f, (Time.time - startTime) / 3f);
            yield return null;
        }
        GetComponent<AudioSource>().enabled = false;
    }
}
