/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

[System.Serializable]
public class CarChoice
{
    public GameObject displayObject;
    public CarSelectIndicator indicator;
    public Car car;
    public string name;
    public Brand brand;
    public string shortName;
}

public class CarSelectController : BaseSelectController
{

    public List<CarChoice> choices;
    public float dimAlpha = 0.3f;
    public MoveToTarget indicatorObj;
    
    public OrbitAround cam;
    public MoveToTarget camMove;
    //public RotateToTarget carousel;
    public Color graySelectColor;
    private bool finished = false;

    //public BaseAlphaFade pleaseSelectText;
   // public GuiTextColorFade pleaseSelectTextColor;
    //public BaseAlphaFade selectedCarText;   
    //public BaseAlphaFade currentCarText;
    public BaseAlphaFade turnTex;
    public BaseAlphaFade gasTex;
    public BaseAlphaFade bghideTex;

    public float startDelay = 1f;

    public GameObject startTarget;
    bool hasChosen = false;

    private Vector3 camInitialPosition;

    public Vector3 indicatorOffset;
    public Vector3 carInitialPos;
    public Quaternion carInitialRot;

    public GameObject barrierLeft;
    public GameObject barrierRight;

    public bool CanSelect() {
        return !finished;
    }

    public bool hasSelected()
    {
        return hasChosen;
    }

    private bool ending = false;

    public override void OnEnable()
    {
        base.OnEnable();
        AppController.Instance.AdminInput.Numeric1 += Trigger1;
        AppController.Instance.AdminInput.Numeric2 += Trigger2;
        AppController.Instance.AdminInput.Numeric3 += Trigger3;
        AppController.Instance.AdminInput.Numeric4 += Trigger4;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
        {
            AppController.Instance.AdminInput.Numeric1 -= Trigger1;
            AppController.Instance.AdminInput.Numeric2 -= Trigger2;
            AppController.Instance.AdminInput.Numeric3 -= Trigger3;
            AppController.Instance.AdminInput.Numeric4 -= Trigger4;
        }
    }

    public void Trigger(int n)
    {
        if (!finished)
        {
            currentChoice = n;
           // UpdateCarName();
            NetworkController.Instance.SelectCar(currentChoice);
            ending = true;
            OnSelectConfirm();
        }

    }

    public void Trigger1()
    {
        if(!finished)
        {
            currentChoice = 3;         
          //  carousel.SetTarget(currentChoice * 90f);
            //cam.Move(choices[currentChoice].displayObject);
           // UpdateCarName();
            NetworkController.Instance.SelectCar(currentChoice);
            finished = false;
            OnSelectConfirm();
        }    
    }

    public void Trigger2()
    {
        if(!finished)
        {
            currentChoice = 2;
          //  carousel.SetTarget(currentChoice * 90f);
           // cam.Move(choices[currentChoice].displayObject);
          //  UpdateCarName();
            NetworkController.Instance.SelectCar(currentChoice);
            finished = false;
            OnSelectConfirm();
        }  
    }

    public void Trigger3()
    {
        if(!finished)
        {
            currentChoice = 0;
          //  carousel.SetTarget(currentChoice * 90f);
          //  cam.Move(choices[currentChoice].displayObject);
         //   UpdateCarName();
            NetworkController.Instance.SelectCar(currentChoice);
            finished = false;
            OnSelectConfirm();
        }  
    }

    public void Trigger4()
    {
        if(!finished)
        {
            currentChoice = 1;
         //   carousel.SetTarget(currentChoice * 90f);
         //   cam.Move(choices[currentChoice].displayObject);
         //   UpdateCarName();
            NetworkController.Instance.SelectCar(currentChoice);
            finished = false;
            OnSelectConfirm();
        }  
    }

    public void Awake()
    {
  //      cam.target = startTarget;
  //      cam.yaw = 18f;
  //      cam.distance = 27f;
    //    carousel.ForceTarget(0f);
     //   pleaseSelectText.SetStraight(0f);
     //   pleaseSelectTextColor.SetStraight(Color.white);
       // selectedCarText.SetStraight(0f);
       // currentCarText.SetStraight(0f);
        turnTex.SetStraight(0f);
        gasTex.SetStraight(0f);

        bghideTex.SetStraight(1f);

        camInitialPosition = camMove.transform.localPosition;

        foreach(var c in choices)
        {
            c.displayObject.SetActive(true);
        }

        LoadMisc();
    }

    public override void OnSetup()
    {        
        base.OnSetup();
        finished = true;
        hasChosen = false;
        ending = false;
 //       cam.target = startTarget;
 //       cam.yaw = 180f;
 //       cam.distance = 35f;

  //      selectedCarText.SetTarget(0f);
  //      currentCarText.SetTarget(0f);

        gasTex.SetTarget(0f);

        turnTex.SetTarget(1f);
        StartCoroutine(StartDelay());
        bghideTex.SetTarget(0f);

        NetworkController.Instance.SelectCar(-1);
        
    //    carousel.SetTarget(currentChoice * 90f);
     //   cam.GetComponent<MoveToTarget>().SetTarget(new Vector3(0f, 2.86f, -24.551f));
        //SetBrandLogo();


    }
    private IEnumerator StartDelay()
    {
        camMove.SetTarget(camInitialPosition);
        yield return new WaitForSeconds(startDelay / 2);
  //      pleaseSelectText.SetTarget(1f);
        turnTex.SetTarget(1f);
//        pleaseSelectText.guiText.text = "SELECT A VEHICLE";
//        pleaseSelectTextColor.SetTarget(Color.white);
        yield return new WaitForSeconds(startDelay / 2);
        foreach(var c in choices)
        {
            c.displayObject.SetActive(true);
        }
        finished = false;
    }

    private IEnumerator UpdateIndicators()
    {
        finished = true;
        foreach (var c in choices)
        {
            c.indicator.Fade(false);
        }
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(choices[currentChoice].indicator._Fade(true));
        if(!ending)
            finished = false;
    }

    private void UpdateCarName()
    {
        StartCoroutine(UpdateIndicators());

        NetworkController.Instance.SelectCar(currentChoice);

  //      currentCarText.guiText.text = choices[currentChoice].name;
 //       currentCarText.SetTarget(1f);
        turnTex.SetTarget(0f);
        gasTex.SetTarget(1f);
        hasChosen = true;

        indicatorObj.gameObject.SetActive(true);
        indicatorObj.SetTarget(choices[currentChoice].displayObject.transform.position + indicatorOffset);

    }

    private IEnumerator InitialZoom()
    {
        currentChoice = 0;
        finished = true;
        yield return null;
       // cam.Move(choices[currentChoice].displayObject);
      //  yield return new WaitForSeconds(OrbitAround.moveTime);
  //      cam.MoveToClose(() => {  });
       // finished = false;
        //carousel.SetTarget(currentChoice * 90f);
        UpdateCarName();
    }

    protected override void OnSelectLeft()
    {
        base.OnSelectLeft();
        if(!hasChosen && !finished)
        {
            if(--currentChoice < 0)
                currentChoice += choices.Count;

            StartCoroutine(InitialZoom());


        }
        else if(!finished)
        {
            
            if(--currentChoice < 0)
                currentChoice += choices.Count;

         //   cam.Move(choices[currentChoice].displayObject);
         //   carousel.SetTarget(currentChoice * 90f);
            UpdateCarName();
        }        
    }

    protected override void OnSelectRight()
    {
        base.OnSelectRight();
        if(!hasChosen && !finished)
        {
            if(++currentChoice > choices.Count - 1)
            {
                currentChoice -= (choices.Count);
            }
            StartCoroutine(InitialZoom());
        }
        else if(!finished)
        {
            
            if(++currentChoice > choices.Count - 1)
            {
                currentChoice -= (choices.Count);
            }
          //  cam.Move(choices[currentChoice].displayObject);
            //carousel.SetTarget(currentChoice * 90f);
            UpdateCarName();
        }       
    }

    public GameObject GetCurrentCar()
    {
        return choices[currentChoice].displayObject;
    }

    void LoadMisc()
    {

        var serializer = new System.Xml.Serialization.XmlSerializer(typeof(MiscSettings));

        using (var filestream = new FileStream(Application.streamingAssetsPath + "/SavedPresets/MiscSettings.xml", FileMode.Open))
        {
            var reader = new System.Xml.XmlTextReader(filestream);
            var savedSettings = serializer.Deserialize(reader) as MiscSettings;

            AdminSettings.Instance.fov = DriverCamera.ScaleFov(savedSettings.fov);
        }
    }

    protected override void OnSelectConfirm()
    {
        if(!finished || ending)
        {
            foreach (var c in choices)
            {
                c.indicator.StopAllCoroutines();
                c.indicator.Fade(false);
            }

            NetworkController.Instance.SelectCar(currentChoice);

           

            AudioController.Instance.PlayClip(onConfirm);
            finished = true;
            AppController.Instance.currentSessionSettings.selectedCar = choices[currentChoice].car;
            AppController.Instance.currentSessionSettings.selectedCarShortname = choices[currentChoice].shortName;

            gasTex.SetTarget(0f);
            turnTex.SetTarget(0f);

            var selectedCar = choices[currentChoice].displayObject;
            selectedCar.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;

            selectedCar.GetComponent<VehicleAudioController>().PlayIgnition();

            //OLD SOUND CODE
            /*var sound = selectedCar.GetComponent<CarSound>();
            sound.SelectSceneInit();
            sound.selectAudio.PlayOneShot(sound.startup, 0.7f * AudioController.Instance.VehicleVolume);

            StartCoroutine(Functional.DoAfter(sound.startup.length, () =>
            {
                sound.selectAudio.clip = sound.selectIdle;
                sound.selectAudio.Play();
            }));
            */

            var vehicleController = selectedCar.GetComponent<VehicleController>();

            //Camera follow
            StartCoroutine(Functional.DoAfter(4f, () => {

                var camTarget = vehicleController.cameraPosition;
                camMove.SetFollowTarget(camTarget);

                var driverCam = camMove.GetComponent<DriverCamera>();

                StartCoroutine(Functional.DoTimed(2f, f =>
                {
                    driverCam.SetFoV(Mathf.Lerp(AdminSettings.Instance.selectFov, AdminSettings.Instance.fov, f));
                }));

                StartCoroutine(Functional.DoAfter(1f, () => {
                    camMove.updateRotation = true;
                }));

                StartCoroutine(Functional.WatchFor(() => Vector3.Distance(camMove.transform.position, camTarget.position) < 1.5f, () =>
                {
                    camMove.GetComponent<DriverCamera>().SetCulingMask(~(1 << LayerMask.NameToLayer("PlayerCar")));
                }));
            }));


            //Car Pathing
            StartCoroutine(Functional.DoAfter(0.5f, () =>
            {
                carInitialPos = selectedCar.transform.position;
                carInitialRot = selectedCar.transform.rotation;

                selectedCar.GetComponent<VehicleController>().enabled = true;


                var careip = selectedCar.GetComponent<VehiclePathController>();
                careip.enabled = true;

                careip.onStop = () =>
                {

                    StartCoroutine(Functional.DoAfter(1.5f, () => base.OnSelectConfirm()));

                    RemoteAdminController.Instance.SendMessage(RemoteAdminController.SendMessageType.SELECT_CAR, currentChoice);

                    Quaternion initalRot = selectedCar.transform.localRotation;
                    Quaternion targetRot = Quaternion.Euler(initalRot.eulerAngles.x, 90f, initalRot.eulerAngles.z);
                    StartCoroutine(Functional.DoTimed(1.5f, (f) => {
                        selectedCar.transform.localRotation = Quaternion.Lerp(initalRot, targetRot, f);
                        selectedCar.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;



                    }));

                };
            }));
            //cam.MoveToFar(() =>
           // {
           //     base.OnSelectConfirm();
           // });
        }
     
    }



}


