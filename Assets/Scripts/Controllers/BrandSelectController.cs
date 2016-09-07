/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BrandSelectController : BaseSelectController
{

    [System.Serializable]
    public class BrandChoice : SelectChoice
    {
        public Brand brand;
    }

    public List<BrandChoice> choices;
    public float dimAlpha = 0.3f;
    public new int currentChoice = 0;

    public void Awake()
    {
        foreach(var c in choices)
        {
            c.displayObject.GetComponent<GuiTextAlphaFade>().targetAlpha = dimAlpha;
        }
    }

    public override void OnEnable()
    {
        base.OnEnable();
        choices[currentChoice].displayObject.GetComponent<GuiTextAlphaFade>().targetAlpha = 1f;
    }

    protected override void OnBack()
    {
        AppController.Instance.LoadRoadSideSelect();
    }

    protected override void OnSelectLeft()
    {
        if(currentChoice > 0)
        {
            LightChoice(currentChoice, true);
            LightChoice(--currentChoice, false);
        }
    }

    protected override void OnSelectRight()
    {
        if(currentChoice < choices.Count - 1)
        {
            LightChoice(currentChoice, true);
            LightChoice(++currentChoice, false);
        }
    }

    protected override void OnSelectConfirm()
    {
        AppController.Instance.currentSessionSettings.selectedBrand = choices[currentChoice].brand;
        AppController.Instance.LoadCarSelect();
    }

    private void LightChoice(int choice, bool dim)
    {
        choices[choice].displayObject.GetComponent<GuiTextAlphaFade>().targetAlpha = dim ? dimAlpha : 1f;
    }
}


