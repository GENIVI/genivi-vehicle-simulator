/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class CarConsoleController : MonoBehaviour {

    public CarConsole console;
    public int consoleID;
    private NetworkView view;

    private float speed;
    private float rpm;

    public NetworkedCarConsole[] consoles;
    private int currentConsole = -1;

    public float Speed
    {
        get { return speed; }
    }

    public float RPM
    {
        get
        {
            return rpm;
        }
    }

    public bool isRemoteClient = false;

    void Update () {
        if(isRemoteClient)
        {
            rpm = DataStreamListener.currentRPM;
            speed = DataStreamListener.currentSpeed;
        }
    }


    private void OnEnable()
    {
        if (isRemoteClient) {
            NetworkController.Instance.OnInitConsole += SetupView;
            NetworkController.Instance.OnSelectCar += OnSelectCar;
        }
    }


    private void OnDisable()
    {
        if (isRemoteClient && NetworkController.IsInstantiated) {
            NetworkController.Instance.OnInitConsole -= SetupView;
            NetworkController.Instance.OnSelectCar -= OnSelectCar;
        }
    }  

    public void Init()
    {
        var nc = NetworkController.Instance;
        nc.InitConsole(0);
    }

    void SetupView(int consoleNum)
    {
  //      Debug.Log("RPC!" + id);
  //      if (view == null)
  //          view = gameObject.AddComponent<NetworkView>();

    //    view.observed = this;
    //    view.stateSynchronization = NetworkStateSynchronization.Unreliable;
    //    view.viewID = id;
    }

    void OnSelectCar(int car) {
        if(car == currentConsole)
            return;
        
        if(currentConsole >= 0) 
            consoles[currentConsole].FadeOut();

        currentConsole = car;

        if (currentConsole >= 0)
            consoles[currentConsole].FadeIn();

        DataStreamListener.currentRPM = 0;
        DataStreamListener.currentSpeed = 0;
        
       
    }
}
