/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Xml.Serialization;
using System.IO;

[System.Serializable]
public class NetworkSettings
{
    public string hostIp;
    public string clientIp;
    public string altClientIP;

    public NetworkSettings()
    {
        //hostIp = "127.0.0.1";
        //clientIp = "192.168.178.29";
        //altClientIP = "127.0.0.1";
        hostIp = "192.168.16.129";
        clientIp = "192.168.16.128";
        altClientIP = "192.168.16.85";

    }

    public NetworkSettings(string host, string client)
    {
        hostIp = host;
        clientIp = client;
    }

    public void SaveSettings(string path)
    {
        var serializer = new XmlSerializer(typeof(NetworkSettings));

        using (var filestream = new FileStream(path, FileMode.Create))
        {
            var writer = new System.Xml.XmlTextWriter(filestream, System.Text.Encoding.Unicode);
            serializer.Serialize(writer, this);
        }
    }

    public static NetworkSettings LoadSettings(string path)
    {
        try
        {
            XmlSerializer ser = new XmlSerializer(typeof(NetworkSettings));
            TextReader reader = new StreamReader(path);
            return (NetworkSettings)ser.Deserialize(reader);
        }
        catch (System.Exception e)
        {      
            Debug.LogWarning("error reading network settings, reverting to default: " + e.Message);
            return new NetworkSettings();
        }

    }
}

public class NetworkController : PersistentUnitySingleton<NetworkController> {

    public bool isMaster = true;

    public event System.Action<int> OnInitConsole;
    public event System.Action<int> OnSelectCar;

    public static NetworkSettings settings;

    protected override void Awake()
    {
        base.Awake();
        if (_instance != this)
            return;

        settings = NetworkSettings.LoadSettings(Application.dataPath + Path.DirectorySeparatorChar + "network_settings");

        if(ShowBuild.GetBuildType() == "CONSOLE")
            isMaster = false;

        if (isMaster)
        {
            Network.InitializeServer(1, 25552, false);
        }
        else
        {
            Debug.Log("connecting: " + Network.Connect(NetworkController.settings.hostIp, 25552));

        }





    }


    public void Update()
    {
        if (!isMaster && Input.GetKeyDown(KeyCode.Escape)) {
            Application.Quit();
        }
    }
#pragma warning disable 0618
    public void SelectCar(int car)
    {
        if(GetComponent<NetworkView>())
            GetComponent<NetworkView>().RPC("SelectCarRPC", RPCMode.Others, car);
    }

    [RPC]
    private void SelectCarRPC(int car)
    {
        if (OnSelectCar != null)
            OnSelectCar(car);
    }

    
    public void InitConsole(int consoleNum)
    {
        if(GetComponent<NetworkView>())
            GetComponent<NetworkView>().RPC("InitConsoleRPC", RPCMode.Others, consoleNum);
    }

    [RPC]
    private void InitConsoleRPC(int consoleNum)
    {
        Debug.Log("Init Console");
        if (OnInitConsole != null)
            OnInitConsole(consoleNum);
    }

#pragma warning restore 0618
}
