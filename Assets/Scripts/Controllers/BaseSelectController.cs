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
public class SelectChoice
{
    public GameObject displayObject;
    public string name;
}


public class BaseSelectController : MonoBehaviour
{

    public AudioClip onSelect;
    public AudioClip onConfirm;

    public int currentChoice = 0;

    public virtual void Reverse()
    {

    }

    public virtual void OnEnable()
    {
        AppController.Instance.UserInput.SelectChoiceLeft += OnSelectLeft;
        AppController.Instance.UserInput.SelectChoiceRight += OnSelectRight;
        AppController.Instance.UserInput.SelectChoiceConfirm += OnSelectConfirm;
        AppController.Instance.AdminInput.NavigateBack += OnBack;

    }

    public virtual void OnDisable()
    {
        if(AppController.IsInstantiated && AppController.Instance.UserInput != null)
        {
            AppController.Instance.UserInput.SelectChoiceLeft -= OnSelectLeft;
            AppController.Instance.UserInput.SelectChoiceRight -= OnSelectRight;
            AppController.Instance.UserInput.SelectChoiceConfirm -= OnSelectConfirm;
        }

        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
        {
            AppController.Instance.AdminInput.NavigateBack -= OnBack;
        }
    }

    protected virtual void OnSelectLeft()
    {
        AudioController.Instance.PlayClip(onSelect);
    }

    protected virtual void OnSelectRight()
    {
        AudioController.Instance.PlayClip(onSelect);
    }

    protected virtual void OnSelectConfirm()
    {
        if(OnFinished != null)
            OnFinished();
    }

    public virtual void OnSetup()
    {

    }

    protected virtual void OnBack()
    {
        if(OnBackCall != null)
            OnBackCall();
    }

    public System.Action OnFinished;
    public System.Action OnBackCall;
}
