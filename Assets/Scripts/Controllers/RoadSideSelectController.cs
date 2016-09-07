/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoadSideSelectController : BaseSelectController
{

    public List<SelectChoice> choices;
    public float dimAlpha = 0.3f;

    public BaseAlphaFade left;
    public BaseAlphaFade leftText;
    public BaseAlphaFade right;
    public BaseAlphaFade rightText;
    public BaseAlphaFade chooseText;
    public BaseAlphaFade turn;
    public BaseAlphaFade gas;
    private bool finished = false;

    public override void OnEnable()
    {
        base.OnEnable();
        AppController.Instance.AdminInput.SelectRoadLeft += SelectRoadLeft;
        AppController.Instance.AdminInput.SelectRoadRight += SelectRoadRight;
    }

    public override void OnDisable()
    {
        base.OnDisable();
        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
        {
            AppController.Instance.AdminInput.SelectRoadLeft -= SelectRoadLeft;
            AppController.Instance.AdminInput.SelectRoadRight -= SelectRoadRight;
        }
    }

    private void SelectRoadLeft()
    {
        if(!finished)
        {
            currentChoice = 0;
            UpdateSelections();
            OnSelectConfirm();
        }
    }

    private void SelectRoadRight()
    {
        if(!finished)
        {
            currentChoice = 1;
            UpdateSelections();
            OnSelectConfirm();
        }
    }

    public void Awake()
    {
        left.SetStraight(0f);
        leftText.SetStraight(0f);
        right.SetStraight(0f);
        rightText.SetStraight(0f);
        chooseText.SetStraight(0f);
        turn.SetStraight(0f);
        gas.SetStraight(0f);
    }

    protected override void OnBack()
    {
        finished = true;
        left.SetTarget(0f);
        leftText.SetTarget(0f);
        right.SetTarget(0f);
        rightText.SetTarget(0);
        chooseText.SetTarget(0);
        turn.SetTarget(0f);
        gas.SetTarget(0f);
        base.OnBack();
    }

    public override void OnSetup()
    {
        finished = false;
        left.SetTarget(0.5f);
        leftText.SetTarget(0.5f);
        right.SetTarget(dimAlpha);
        rightText.SetTarget(0.5f);
        chooseText.SetTarget(1f);
        turn.SetTarget(1f);
        gas.SetTarget(0f);
    }

    void UpdateSelections()
    {
        if(currentChoice == 0)
        {
            turn.SetTarget(0f);
            gas.SetTarget(0.5f);
            left.SetTarget(0.5f);
            right.SetTarget(dimAlpha);
        }
        else
        {
            turn.SetTarget(0f);
            gas.SetTarget(0.5f);
            left.SetTarget(dimAlpha);
            right.SetTarget(0.5f);
        }
    }


    protected override void OnSelectLeft()
    {
        base.OnSelectLeft();
        if(!finished)
        {
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
        GetComponent<AudioSource>().PlayOneShot(onConfirm);
        AppController.Instance.currentSessionSettings.selectedSideOfRoad = currentChoice == 0 ? SideOfRoad.LEFT : SideOfRoad.RIGHT;
        base.OnSelectConfirm();
    }
}




