/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Linq;
using System.Xml.Serialization;
using System.IO;

public enum Brand { JAGUAR, LAND_ROVER, RANGE_ROVER }

public enum Environment {URBAN, SCENIC, COASTAL, NONE}

[System.Serializable]
public class Car {
    public GameObject prefab;
}

public class SessionSettings
{
    public Brand selectedBrand;
    public SideOfRoad selectedSideOfRoad;
    public Environment selectedEnvironment = Environment.NONE;
    public Car selectedCar;
    public string selectedCarShortname = "DEFAULT";
}

public class AppController : PersistentUnitySingleton<AppController>
{
    public AppSettings appSettings;

    public InputController UserInput { get; private set; }
    public AdminInputController AdminInput { get; private set; }

    private InputController backupInput;

    public SessionSettings currentSessionSettings = new SessionSettings();

    protected override void Awake()
    {
        base.Awake();
        //Set app settings from build type
        string buildType = ShowBuild.GetBuildType();
        switch(buildType)
        {
            case "DEV":
                appSettings = new DevAppSettings();
                break;
            case "PRODUCTION":
                appSettings = new ProductionAppSettings();
                break;
            case "SMALL":
                appSettings = new SmallAppSettings();
                break;
            case "CUSTOM":
            case "CONSOLE":
                try
                {
                    XmlSerializer ser = new XmlSerializer(typeof(DefaultAppSettings));
                    TextReader reader = new StreamReader(Application.dataPath + Path.DirectorySeparatorChar + "build_settings");
                    appSettings = (DefaultAppSettings)ser.Deserialize(reader);
                }
                catch(System.Exception e)
                {
                    appSettings = new DefaultAppSettings();
                    Debug.Log("error reading custom settings, reverting to default " + e.Message);
                }
                break;
            default:
                appSettings = new DefaultAppSettings();
                break;
        }

        //only make keyboard input for console
        if (buildType != "CONSOLE")
        {
            UserInput = InputController.Create(appSettings.inputController);
            backupInput = InputController.Create(appSettings.inputControllerBackup);
        }
        else
        {
           UserInput = InputController.Create(typeof(KeyboardInputController));
        }

        AdminInput = InputController.Create(typeof(AdminInputController)) as AdminInputController;

        //if(buildType != "CONSOLE")
            Screen.SetResolution(appSettings.xResolution, appSettings.yResolution, appSettings.fullscreen);

        AdminInput.SwapInputMethod += SwapInputMethod;
    }

    public void OnGUI()
    {
        if (swappedInputAt > 0f && Time.time < swappedInputAt + 4f)
        {
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1 - (Time.time - swappedInputAt - 3f)));
            GUI.Label(new Rect(200, 10, 300, 400), "INPUT: " + inputMethod);
            GUI.color = Color.white;
        }

    }

    private float swappedInputAt = 0f;
    private string inputMethod;

    public void SwapInputMethod()
    {
        var t = UserInput;
        UserInput = backupInput;
        backupInput = t;

        swappedInputAt = Time.time;
        inputMethod = UserInput.gameObject.name;
        
    }

#pragma warning disable 0618
    public void LoadEnvironmentSelect()
    {
        Application.LoadLevel(appSettings.environmentSelectScene);
    }

    public void LoadCarSelect()
    {
        Application.LoadLevel(appSettings.carSelectScene);
    }

    public void LoadRoadSideSelect()
    {
        Application.LoadLevel(appSettings.roadSideSelectScene);
    }

    public void LoadBrandSelect()
    {
        Application.LoadLevel(appSettings.brandSelectScene);
    }

    public void LoadDrivingScene(string sceneName)
    {
        Application.LoadLevel(sceneName);
    }

    public void LoadDrivingScene(Environment environment)
    {
        switch(environment)
        {
            case Environment.SCENIC:
                LoadDrivingScene(appSettings.scenicDrivingScene);
                break;
            case Environment.URBAN:
                LoadDrivingScene(appSettings.urbanDrivingScene);
                break;
            case Environment.COASTAL:
                LoadDrivingScene(appSettings.coastalDrivingScene);
                break;
        }
        
    }

    public AsyncOperation LoadDrivingSceneAdditive(Environment environment)
    {
        switch(environment)
        {
            case Environment.SCENIC:
                return Application.LoadLevelAdditiveAsync(appSettings.scenicDrivingScene);
            case Environment.URBAN:
                return Application.LoadLevelAdditiveAsync(appSettings.urbanDrivingScene);
            case Environment.COASTAL:
                return Application.LoadLevelAdditiveAsync(appSettings.coastalDrivingScene);
            default:
                return Application.LoadLevelAdditiveAsync(appSettings.coastalDrivingScene);
        }

    }

    public AsyncOperation LoadDrivingSceneAsync(Environment environment)
    {
        switch (environment)
        {
            case Environment.SCENIC:
                return Application.LoadLevelAsync(appSettings.scenicDrivingScene);
            case Environment.URBAN:
                return Application.LoadLevelAsync(appSettings.urbanDrivingScene);
            case Environment.COASTAL:
                return Application.LoadLevelAsync(appSettings.coastalDrivingScene);
            default:
                return Application.LoadLevelAsync(appSettings.coastalDrivingScene);
        }
    }
#pragma warning restore 0618
    public bool IsProduction()
    {
        return appSettings is ProductionAppSettings;
    }

}
