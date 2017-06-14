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

    private AsyncOperation sceneLoader = null;
    private static Texture2D loadScrim;
    private static Texture2D loadProgressFull;
    private static Texture2D loadProgressEmpty;

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

    public void DrawLoadProgress() {
        float xPos = (float)(Screen.width * 0.5) - 50;
        float yPos = (float)(Screen.height * 0.5) - 25;
        if (loadScrim == null) {
            loadScrim = new Texture2D(1, 1);
            loadScrim.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.5f));
            loadScrim.Apply();
        }
        if (loadProgressFull == null) {
            loadProgressFull = new Texture2D(1, 1);
            loadProgressFull.SetPixel(0, 0, Color.blue);
            loadProgressFull.Apply();
        }
        if (loadProgressEmpty == null) {
            loadProgressEmpty = new Texture2D(1, 1);
            loadProgressEmpty.SetPixel(0, 0, Color.red);
            loadProgressEmpty.Apply();
        }
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), loadScrim);
        GUI.DrawTexture(new Rect(xPos, yPos, 100, 50), loadProgressEmpty);
        GUI.DrawTexture(new Rect(xPos, yPos, 100 * sceneLoader.progress, 50), loadProgressFull);
        GUI.color = Color.black;
        GUI.Label(new Rect(xPos + 20, yPos + 10, 300, 400), "LOADING...");
    }

    public void OnGUI()
    {
        if (swappedInputAt > 0f && Time.time < swappedInputAt + 4f)
        {
            GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(1 - (Time.time - swappedInputAt - 3f)));
            GUI.Label(new Rect(200, 10, 300, 400), "INPUT: " + inputMethod);
            GUI.color = Color.white;
        }
        if (sceneLoader != null && sceneLoader.progress < 1.0) {
            DrawLoadProgress();
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
        sceneLoader = Application.LoadLevelAsync(appSettings.environmentSelectScene);
    }

    public void LoadCarSelect()
    {
        sceneLoader = Application.LoadLevelAsync(appSettings.carSelectScene);
    }

    public void LoadRoadSideSelect()
    {
        sceneLoader = Application.LoadLevelAsync(appSettings.roadSideSelectScene);
    }

    public void LoadBrandSelect()
    {
        sceneLoader = Application.LoadLevelAsync(appSettings.brandSelectScene);
    }

    public void LoadDrivingScene(string sceneName)
    {
        sceneLoader = Application.LoadLevelAsync(sceneName);
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
                sceneLoader = Application.LoadLevelAdditiveAsync(appSettings.scenicDrivingScene);
                break;
            case Environment.URBAN:
                sceneLoader = Application.LoadLevelAdditiveAsync(appSettings.urbanDrivingScene);
                break;
            case Environment.COASTAL:
                sceneLoader = Application.LoadLevelAdditiveAsync(appSettings.coastalDrivingScene);
                break;
            default:
                sceneLoader = Application.LoadLevelAdditiveAsync(appSettings.coastalDrivingScene);
                break;
        }
        return sceneLoader;
    }

    public AsyncOperation LoadDrivingSceneAsync(Environment environment)
    {
        switch (environment)
        {
            case Environment.SCENIC:
                sceneLoader = Application.LoadLevelAsync(appSettings.scenicDrivingScene);
                break;
            case Environment.URBAN:
                sceneLoader = Application.LoadLevelAsync(appSettings.urbanDrivingScene);
                break;
            case Environment.COASTAL:
                sceneLoader = Application.LoadLevelAsync(appSettings.coastalDrivingScene);
                break;
            default:
                sceneLoader = Application.LoadLevelAsync(appSettings.coastalDrivingScene);
                break;
        }
        return sceneLoader;
    }

#pragma warning restore 0618
    public bool IsProduction()
    {
        return appSettings is ProductionAppSettings;
    }

}
