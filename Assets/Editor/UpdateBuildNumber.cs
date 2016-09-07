/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

public class UpdateBuildNumber {
    [UnityEditor.Callbacks.PostProcessBuild]
    public static void OnPostProcessBuild(BuildTarget target, string path)
    {
        Debug.Log("PostProcessing Build..");
        string buildNum = ShowBuild.GetBuildNumber();
        var buildname = Path.GetFileNameWithoutExtension(path);
        var targetdir = Directory.GetParent(path);
        var dataDir = targetdir.FullName + Path.DirectorySeparatorChar + buildname + "_Data" + Path.DirectorySeparatorChar;
        File.WriteAllText(dataDir + "build", buildNum.ToString());
        Debug.Log("Build number set: " + buildNum);
    }

}
