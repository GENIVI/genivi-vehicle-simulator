/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.IO;

public class ShowBuild : MonoBehaviour {

    private string buildNumber;
    public GUIStyle buildStyle;
    private string buildType;

    #if UNITY_EDITOR
    public static int GetCurrentUASBuildNumber()
    {
         System.Reflection.Assembly assembly = typeof(UnityEditor.EditorWindow).Assembly;
         System.Type assetServer = assembly.GetType("UnityEditor.AssetServer");
         System.Type changeSet = assembly.GetType("UnityEditor.Changeset");
         var getHistory = assetServer.GetMethod("GetHistory", System.Type.EmptyTypes);
         object[] changes = getHistory.Invoke(null, null) as object[];
         int buildNum = (int)changeSet.GetField("changeset").GetValue(changes[0]);

        return buildNum;
    }

    public static string GetGitBuildNumber()
    {
        string hash;
        try {
            hash = File.ReadAllText(".git/HEAD");
        } catch (IOException e) {
            hash = "Unknown";
        }

        string timestamp = System.DateTime.Now.ToString("yyyymmddHHmm");

        return timestamp + "_" + hash.Substring(0, 6);
    }

    #endif

    public static string GetBuildType()
    {
        #if UNITY_EDITOR
                return "DEV";
        #else
                    return File.ReadAllText(Application.dataPath + Path.DirectorySeparatorChar + "build_type");
        #endif
    }

    public static string GetBuildNumber()
    {
        #if UNITY_EDITOR
        //return GetCurrentUASBuildNumber().ToString();
        return GetGitBuildNumber();
        #else
        return File.ReadAllText(Application.dataPath + Path.DirectorySeparatorChar + "build");
        #endif
    }

    void Awake()
    {

        #if UNITY_EDITOR
            //pull straight from asset server if we are in the editor
            buildNumber = GetBuildNumber();

            buildType = "";
        #else
            //otherwise grab the number that was added during build postprocessing
            string build = File.ReadAllText(Application.dataPath + Path.DirectorySeparatorChar + "build");
            string type = File.ReadAllText(Application.dataPath + Path.DirectorySeparatorChar + "build_type");
            buildNumber = build;
            buildType = type;

        #endif
    }


    void Start()
    {
        Debug.Log("RUNNING BUILD: " + buildNumber + " " + buildType);
        Debug.Log("USER: " + System.Environment.UserName + "@"  + System.Environment.MachineName);
        DontDestroyOnLoad(gameObject);
    }

}
