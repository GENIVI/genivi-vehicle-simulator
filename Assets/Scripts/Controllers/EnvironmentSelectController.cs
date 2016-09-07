/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class EnvironmentChoice
{
    public GameObject displayObject;
    public string name;
    public Environment environment;
    public RendererAlphaFade hideQuad;
}

public class EnvironmentSelectController : BaseSelectController {

    public List<EnvironmentChoice> choices;
    public float dimAlpha = 0.3f;
    public GameObject cam;
    private bool finished = false;

    public MoveToTarget camMove;


    public Color graySelectColor;

 //   public BaseAlphaFade cityIcon;
 //   public BaseAlphaFade scenicIcon;
    public BaseAlphaFade turn;
    public BaseAlphaFade gas;
 //   public BaseAlphaFade selectText;
 //   public GuiTextColorFade selectTextColor;
 //   public BaseAlphaFade city;
 //   public BaseAlphaFade scenic;
 //   public GuiTexturePositionTween scenicPosition;
 //   public GuiTexturePositionTween cityPosition;
 //   public GuiTexturePositionTween scenicIconPosition;
 //   public GuiTexturePositionTween cityIconPosition;
 //   public BaseAlphaFade cityIconText;
  //  public BaseAlphaFade scenicIconText;



    private Vector3 camInitialPosition;
    private Quaternion camInitialRotation;

    public override void OnEnable()
    {
        base.OnEnable();
        AppController.Instance.AdminInput.SelectEnvironment1 += SelectScenic;
        AppController.Instance.AdminInput.SelectEnvironment2 += SelectUrban;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
        {
            AppController.Instance.AdminInput.SelectEnvironment1 -= SelectScenic;
            AppController.Instance.AdminInput.SelectEnvironment2 -= SelectUrban;
        }
    }

    public bool CanSelect()
    {
        return !finished;
    }

    public void Trigger(int n)
    {
        if (!finished)
        {
            currentChoice = n;
            UpdateSelections();
            OnSelectConfirm();
        }
    }

    public void SelectScenic()
    {
        if(!finished)
        {
            currentChoice = 0;
            UpdateSelections();
            OnSelectConfirm();
        }
    }

    public void SelectUrban()
    {
        if(!finished)
        {
            currentChoice = 1;
            UpdateSelections();
            OnSelectConfirm();
        }
    }

    public void SelectCoastal()
    {
        if(!finished)
        {
            currentChoice = 2;
            UpdateSelections();
            OnSelectConfirm();
        }
    }

    public void Awake()
    {
 //       cityIcon.SetStraight(0f);
 //       scenicIcon.SetStraight(0f);
 //       cityIconText.SetStraight(0f);
 //       scenicIconText.SetStraight(0f);
        turn.SetStraight(0f);
        gas.SetStraight(0f);
 //       selectText.SetStraight(0f);
 //       selectTextColor.SetStraight(Color.white);
 //       city.SetStraight(0f);
 //       scenic.SetStraight(0f);
  //      scenicPosition.SetStraight(1920f);
  //      scenicIconPosition.SetStraight(2808f);
   //     cityPosition.SetStraight(3840f);
   //     cityIconPosition.SetStraight(4728f);

        camInitialPosition = camMove.transform.localPosition;
        camInitialRotation = camMove.transform.rotation;
    }

    protected override void OnBack()
    {
        finished = true;
 //       cityIcon.SetTarget(0f);
 //       scenicIcon.SetTarget(0);
 //       selectText.SetTarget(0f);
 //       cityIconText.SetTarget(0f);
 //       scenicIconText.SetTarget(0f);
        turn.SetTarget(0f);
        gas.SetTarget(0f);
 //       city.SetTarget(0f);
 //       scenic.SetTarget(0f);
 //       selectTextColor.SetTarget(Color.white);

      //  cam.GetComponentInChildren<RotateToTarget>().SetTarget(0f);
      //  cam.GetComponent<OrbitAround>().rotationSpeed = 0f;
      //  cam.GetComponent<OrbitAround>().MoveToClose(() => base.OnBack());
       
        var select = GetComponent<CarSelectController>();
        var selectedCar = select.choices[select.currentChoice].displayObject;
        
        camMove.SetTarget(camInitialPosition);
        var oldRot = camMove.transform.rotation;
        StartCoroutine(Functional.DoTimed(3f, f =>
        {
            camMove.transform.rotation = Quaternion.Slerp(oldRot, camInitialRotation, f);
        }));

        var driverCam = camMove.GetComponent<DriverCamera>();

        StartCoroutine(Functional.DoTimed(1.5f, f =>
        {
            driverCam.SetFoV(Mathf.Lerp(AdminSettings.Instance.fov, AdminSettings.Instance.selectFov, f));
        }));

        selectedCar.transform.position = select.carInitialPos;
        selectedCar.transform.rotation = select.carInitialRot;
        selectedCar.GetComponent<Rigidbody>().velocity = Vector3.zero;
        selectedCar.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        selectedCar.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

        selectedCar.GetComponent<VehicleAudioController>().StopEngine();

        var pather = selectedCar.GetComponent<VehiclePathController>();
        pather.Clear();
        pather.enabled = false;
        //selectedCar.GetComponent<CarControl>().enabled = false;

        camMove.GetComponent<DriverCamera>().SetCulingMask(~0);
        StartCoroutine(Functional.WatchFor(() => Vector3.Distance(camMove.transform.localPosition, camInitialPosition) < 3.5f, () =>
        {
            base.OnBack();
            RemoteAdminController.Instance.SendMessage(RemoteAdminController.SendMessageType.SELECT_CAR, -1);
        }));
        
    }

    public void Back()
    {
        OnBack();
    }

    public override void OnSetup() {
        finished = false;

        currentChoice = 0;
//        selectText.guiText.text = "SELECT AN ENVIRONMENT";
//        cityIcon.SetTarget(0.25f);
//        scenicIcon.SetTarget(0.25f);
//        selectText.SetTarget(1f);
//        cityIconText.SetTarget(0.5f);
//        scenicIconText.SetTarget(0.5f);
        turn.SetTarget(1f);
 //       gas.SetTarget(0f);
      //  city.SetTarget(0.2f);
       // scenic.SetTarget(0.2f);
 //       selectTextColor.SetTarget(Color.white);

//        scenicPosition.SetStraight(1920f);
//        cityPosition.SetStraight(3840f);
//        scenicIconPosition.SetStraight(2809f);
//        cityIconPosition.SetStraight(4732f);

        foreach(var c in choices)
        {
            c.hideQuad.SetTarget(0.8f);
        }

     //   var parent = choices[0].displayObject.transform.parent;
    //    parent.position = cam.transform.position + cam.transform.forward * 20f;

    }

    void UpdateSelections()
    {
        foreach(var c in choices)
        {
            c.hideQuad.SetTarget(1f);
        }
        choices[currentChoice].hideQuad.SetTarget(0f);
        if(currentChoice == 0)
        {

//            scenicPosition.SetTarget(1920f);
//            cityPosition.SetTarget(3840f);
//            scenicIconPosition.SetTarget(2808f);
//            cityIconPosition.SetTarget(4728f);


            turn.SetTarget(0f);
            gas.SetTarget(0.5f);
          //  scenicIcon.SetTarget(0.5f);
          //  scenic.SetTarget(0.5f);
          //  city.SetTarget(0.2f);
         //   cityIcon.SetTarget(0.2f);



        }
        else
        {
 //           scenicPosition.SetTarget(3840f);
 //           cityPosition.SetTarget(1920f);
 //           scenicIconPosition.SetTarget(4728f);
 //           cityIconPosition.SetTarget(2808f);

            turn.SetTarget(0f);
            gas.SetTarget(0.5f);
          //  scenicIcon.SetTarget(0.2f);
         //   cityIcon.SetTarget(0.5f);
         //   city.SetTarget(0.5f);
          //  scenic.SetTarget(0.2f);
        }
    }

    protected override void OnSelectLeft()
    {
        base.OnSelectLeft();
        if(!finished) {
            if(--currentChoice < 0)
                currentChoice = choices.Count - 1;
            UpdateSelections();
        }
    }


    protected override void OnSelectRight()
    {
        base.OnSelectRight();
        if(!finished)
        {
            if(++currentChoice >= choices.Count)
                currentChoice = 0;
            UpdateSelections();
        }
    }

    protected override void OnSelectConfirm()
    {
        if(finished)
            return;

        turn.SetTarget(0f);
        gas.SetTarget(0f);
 //       selectTextColor.SetTarget(graySelectColor);
 //       selectText.guiText.text = "SELECTED ENVIRONMENT";
       /*if(currentChoice == 0)
       {
            city.SetTarget(0f);
            cityIcon.SetTarget(0f);
            scenic.SetTarget(0.2f);
            scenicIcon.SetTarget(0.2f);
            cityIconText.SetTarget(0f);
            scenicIconText.SetTarget(0.5f);
        }
        else
        {
            scenicIcon.SetTarget(0f);
            scenic.SetTarget(0f);
            city.SetTarget(0.2f);
            cityIcon.SetTarget(0.2f);
            cityIconText.SetTarget(0.5f);
            scenicIconText.SetTarget(0f);
        }
        */

        //garage exit
 //       cityIcon.SetTarget(0f);
 //       scenicIcon.SetTarget(0);
 //       selectText.SetTarget(0f);
 //       cityIconText.SetTarget(0f);
 //       scenicIconText.SetTarget(0f);
        turn.SetTarget(0f);
        gas.SetTarget(0f);
 //       city.SetTarget(0f);
 //       scenic.SetTarget(0f);

        AudioController.Instance.PlayClip(onConfirm);
        AppController.Instance.currentSessionSettings.selectedEnvironment = choices[currentChoice].environment;

        RemoteAdminController.Instance.SendMessage(RemoteAdminController.SendMessageType.SELECT_SCENE, currentChoice);

        base.OnSelectConfirm();
    }

}
