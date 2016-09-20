/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections;
using System.IO;
using System.Xml.Serialization;

[System.Serializable]
public class DropBoxInfo
{
    public string path;
    public int host;
    public bool is_team;
    public string subscription_type;
}

[System.Serializable]
public class DropBoxInfoParent
{
    public DropBoxInfo personal;
    public DropBoxInfo business;
}

public class BuildType : EditorWindow
{

    [MenuItem("Build/Build Custom")]
    static void OpenWindow()
    {
        EditorWindow.GetWindow<BuildType>(true);
    }

    enum BuildResolution { FULL, HALF, QUARTER }
    enum BuildControlStyle { WHEEL, KEYBOARD }
    enum BuildFullScreen { FULL_SCREEN, WINDOW }
    enum BuildDisplayType { PROJECTOR, SCREENS }
    enum BuildShowDebug { SHOW_DEBUG, DONT_SHOW_DEBUG }
    enum BuildEnableConfigurator { YES, NO }
    enum BuildWindowMode { DEFAULT, PRODUCTION, DEBUG }
    enum BuildScreens {ONE, TWO, THREE}

    BuildResolution curResolution = BuildResolution.FULL;
    BuildControlStyle curControls = BuildControlStyle.KEYBOARD;
    BuildFullScreen curFullScreen = BuildFullScreen.FULL_SCREEN;
    BuildShowDebug curShowDebug = BuildShowDebug.SHOW_DEBUG;
    BuildDisplayType curDisplayType = BuildDisplayType.SCREENS;
    BuildEnableConfigurator curEnableConfigurator = BuildEnableConfigurator.YES;
    BuildWindowMode curWindowMode = BuildWindowMode.DEFAULT;
    BuildScreens curBuildScreens = BuildScreens.THREE;

    public enum BuildName { DEV, PRODUCTION, CUSTOM, CONSOLE }

    public static BuildName buildName = BuildName.PRODUCTION;
    public static AppSettings customSettings = new DefaultAppSettings();

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        curResolution = (BuildResolution)EditorGUILayout.EnumPopup("Resolution", curResolution);
        curFullScreen = (BuildFullScreen)EditorGUILayout.EnumPopup("Full Screen?", curFullScreen);
        curControls = (BuildControlStyle)EditorGUILayout.EnumPopup("Control Type", curControls);
        curDisplayType = (BuildDisplayType)EditorGUILayout.EnumPopup("Display Type", curDisplayType);
        curBuildScreens = (BuildScreens)EditorGUILayout.EnumPopup("Number Of Screens", curBuildScreens);
        curShowDebug = (BuildShowDebug)EditorGUILayout.EnumPopup("Show Debug Info?", curShowDebug);
        curEnableConfigurator = (BuildEnableConfigurator)EditorGUILayout.EnumPopup("Enable Configurator", curEnableConfigurator);
        curWindowMode = (BuildWindowMode)EditorGUILayout.EnumPopup("Fullscreen Mode", curWindowMode);

        if (GUILayout.Button("BUILD"))
        {
            SetCustomSettings();
            Build(BuildName.CUSTOM, EditorBuildSettings.scenes, curWindowMode == BuildWindowMode.DEFAULT ? null : curWindowMode.ToString());
        }

        if (GUILayout.Button("BUILD AND DEPLOY"))
        {
            BuildAndDropBox();
        }

        EditorGUILayout.EndVertical();

    }

    private static void BuildInternalMain()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/Scenes/Loader.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/NewCarSelect_2.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Environment_Yosemite.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Environment_PCH.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Environment_SanFrancisco02.unity", true)
            };

        //clear spot for it
        var path = "TempDeploy";
        FileUtil.DeleteFileOrDirectory(path);
        Directory.CreateDirectory(path + "/");
        path = path + "/";
        //Debug.Log(path + "JLR_MAIN.exe");
        //Debug.Log(path + "JLR_CONSOLE.exe");

        Debug.Log("Start build player");

        customSettings = new ProductionAppSettings();

        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;

        Build(BuildName.CUSTOM, path + "JLR_MAIN.exe", scenes, BuildWindowMode.DEFAULT.ToString());
        while (!File.Exists(path + "JLR_MAIN_Data/BUILD_COMPLETE"))
        {
            //waiting
        }
        Debug.Log("Player Built");
    }


    private static void BuildInternalConsole()
    {
        //clear spot for it
        var path = "TempDeploy";
        FileUtil.DeleteFileOrDirectory(path);
        Directory.CreateDirectory(path + "/");
        path = path + "/";

        Debug.Log("Start build console");

        PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Disabled;

        BuildConsole(path + "JLR_CONSOLE.exe");
        while (!File.Exists(path + "JLR_CONSOLE_Data/BUILD_COMPLETE"))
        {
            //waiting
        }
        Debug.Log("Console Built");
    }


    private void BuildAndDropBox()
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[] {
                new EditorBuildSettingsScene("Assets/Scenes/Loader.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/NewCarSelect_2.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Environment_Yosemite.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Environment_PCH.unity", true),
                new EditorBuildSettingsScene("Assets/Scenes/Environment_SanFrancisco02.unity", true)
            };

        //clear spot for it
        var path = "Temp/deploy/";

        string deployId = "JLR_" + ShowBuild.GetBuildNumber();
        FileUtil.DeleteFileOrDirectory(path);
        Directory.CreateDirectory(path + deployId + "/");
        path = path + deployId + "/";
        //Debug.Log(path + "JLR_MAIN.exe");
        //Debug.Log(path + "JLR_CONSOLE.exe");

        BuildConsole(path + "JLR_CONSOLE.exe");
        while (!File.Exists(path + "JLR_CONSOLE_Data/BUILD_COMPLETE"))
        {
            //waiting
        }
        Debug.Log("Start build player");
        SetCustomSettings();
        Build(BuildName.CUSTOM, path + "JLR_MAIN.exe", scenes, curWindowMode == BuildWindowMode.DEFAULT ? null : curWindowMode.ToString());
        while (!File.Exists(path + "JLR_MAIN_Data/BUILD_COMPLETE"))
        {
            //waiting
        }


        var dbFile = Path.Combine(System.Environment.GetFolderPath(System.Environment.SpecialFolder.LocalApplicationData), "Dropbox\\info.json");
        var dbInfop = JsonUtility.FromJson<DropBoxInfoParent>(File.ReadAllText(dbFile));
        Debug.Log("dbpath: " + File.ReadAllText(dbFile));
        Debug.Log("DROPBOX: " + dbInfop.personal.path);


        var proc1 = new System.Diagnostics.ProcessStartInfo();

        proc1.UseShellExecute = true;
        proc1.WorkingDirectory = Directory.GetParent(path).ToString();

        var root = Directory.GetParent(Application.dataPath);

        string dbPath = "";
        if (dbInfop.business == null || string.IsNullOrEmpty(dbInfop.business.path))
        {
            dbPath = dbInfop.personal.path;
        }
        else
        {
            dbPath = dbInfop.business.path;
        }

        dbPath = Path.Combine(dbPath, "14015_JLR");

        Debug.Log("root:" + root);
        proc1.FileName = @"cmd.exe";
        proc1.Arguments = "/C powershell -File " + root + "/Deploy.ps1 " + dbPath + "/" + deployId + ".zip " + "../" + deployId + "/";
        Debug.Log(proc1.Arguments);
        proc1.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
        System.Diagnostics.Process.Start(proc1);
    }

    private void SetCustomSettings()
    {
        customSettings.fullscreen = curFullScreen == BuildFullScreen.FULL_SCREEN;

        int xRes = 1920;
        int yRes = 1080;
        switch (curBuildScreens)
        {
            case BuildScreens.THREE:
                xRes = curDisplayType == BuildDisplayType.SCREENS ? 5760 : 4992;
                break;
            case BuildScreens.TWO:
                xRes = curDisplayType == BuildDisplayType.SCREENS ? 3840 : 3456;
                break;
            case BuildScreens.ONE:
                xRes = 1920;
                break;
        }
        
        switch (curResolution)
        {
            case BuildResolution.FULL:
                customSettings.xResolution = xRes;
                customSettings.yResolution = yRes;
                break;
            case BuildResolution.HALF:
                customSettings.xResolution = xRes / 2;
                customSettings.yResolution = yRes / 2;
                break;
            case BuildResolution.QUARTER:
                customSettings.xResolution = xRes / 4;
                customSettings.yResolution = yRes / 4;
                break;
        }

        customSettings.showDebug = curShowDebug == BuildShowDebug.SHOW_DEBUG;

        switch (curControls)
        {
            case BuildControlStyle.KEYBOARD:
                customSettings.inputController = typeof(KeyboardInputController);
                customSettings.inputControllerBackup = typeof(SteeringWheelInputController);
                break;
            case BuildControlStyle.WHEEL:
                customSettings.inputController = typeof(SteeringWheelInputController);
                customSettings.inputControllerBackup = typeof(KeyboardInputController);
                break;
        }

        customSettings.projectorBlend = (curDisplayType == BuildDisplayType.PROJECTOR);

        customSettings.showConfigurator = (curEnableConfigurator == BuildEnableConfigurator.YES);



        if (curWindowMode == BuildWindowMode.DEBUG)
        {
            PlayerSettings.d3d9FullscreenMode = D3D9FullscreenMode.FullscreenWindow;
            PlayerSettings.d3d11FullscreenMode = D3D11FullscreenMode.FullscreenWindow;
        }
        else if (curWindowMode == BuildWindowMode.PRODUCTION)
        {
            PlayerSettings.d3d9FullscreenMode = D3D9FullscreenMode.ExclusiveMode;
            PlayerSettings.d3d11FullscreenMode = D3D11FullscreenMode.ExclusiveMode;
        }

    }


    [MenuItem("Build/Build Dev")]
    public static void BuildDev()
    {
        Build(BuildName.DEV);
    }

    [MenuItem("Build/Build Prod")]
    public static void BuildProd()
    {
        Build(BuildName.PRODUCTION);
    }

    [MenuItem("Build/Build Console")]
    private static void BuildConsole()
    {
        string savePath = EditorUtility.SaveFilePanel("Save Build", Application.dataPath, "JLR_CONSOLE_" + ShowBuild.GetBuildNumber(), "exe");
        BuildConsole(savePath);
    }

    private static void BuildConsole(string savePath)
    {
        EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene("Assets/Scenes/Loader.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Console.unity", true)
        };
        var scenesStr = scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        buildName = BuildName.CONSOLE;

       // PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;
        PlayerSettings.SetAspectRatio(AspectRatio.AspectOthers, true);
        PlayerSettings.d3d9FullscreenMode = D3D9FullscreenMode.FullscreenWindow;
        PlayerSettings.visibleInBackground = true;

        customSettings.xResolution = 1920;
        customSettings.yResolution = 720;
        customSettings.fullscreen = true;

        if (!string.IsNullOrEmpty(savePath))
        {
            BuildOptions opts = BuildOptions.ShowBuiltPlayer;
            opts = opts | BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;

            BuildPipeline.BuildPlayer(scenesStr, savePath,
                           BuildTarget.StandaloneWindows64, opts);

        }
        buildName = BuildName.CUSTOM;
    }


    private static void Build(BuildName type, EditorBuildSettingsScene[] scenes = null, string windowBuildName = null)
    {
        string fileName = "JLR_" + type.ToString() + "_" + ShowBuild.GetBuildNumber();

        if (windowBuildName != null)
            fileName += "_" + windowBuildName;

        string savePath = EditorUtility.SaveFilePanel("Save Build", Application.dataPath, fileName, "exe");
        Build(type, savePath, scenes, windowBuildName);
    }

    private static void Build(BuildName type, string savePath, EditorBuildSettingsScene[] scenes = null, string windowBuildName = null)
    {
        if (scenes == null)
            scenes = EditorBuildSettings.scenes;

        var scenesStr = scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
        buildName = type;

        //PlayerSettings.displayResolutionDialog = ResolutionDialogSetting.Enabled;

        if (!string.IsNullOrEmpty(savePath))
        {
            BuildOptions opts = BuildOptions.None;
            if(customSettings.showDebug)
                opts = opts | BuildOptions.Development | BuildOptions.AllowDebugging | BuildOptions.ConnectWithProfiler;

            BuildPipeline.BuildPlayer(scenesStr, savePath,
                           BuildTarget.StandaloneWindows64, opts);

        }
        //buildName = BuildName.PRODUCTION;
    }

    [UnityEditor.Callbacks.PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        Debug.Log("PostProcessing Build...");
        var buildname = Path.GetFileNameWithoutExtension(path);
        var targetdir = Directory.GetParent(path);
        var dataDir = targetdir.FullName + Path.DirectorySeparatorChar + buildname + "_Data" + Path.DirectorySeparatorChar;
        File.WriteAllText(dataDir + "build_type", buildName.ToString());
        Debug.Log("Build number set: " + buildName.ToString());

        //write settings file

        var ser = new XmlSerializer(typeof(DefaultAppSettings));
        TextWriter writer = new StreamWriter(dataDir + "build_settings");
        ser.Serialize(writer, customSettings);
        writer.Close();
        

        //write network settings
        NetworkSettings settings = new NetworkSettings();
        settings.SaveSettings(dataDir + "network_settings");

        File.WriteAllText(dataDir + "/BUILD_COMPLETE", "COMPLETE");
    }
}
