/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

#pragma warning disable 0618
public class LoadApp : MonoBehaviour {

    public float delay;

    public bool loadConsole = false;

    public LaunchScreen launchScreen;

    public IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);

        string buildType = ShowBuild.GetBuildType();
        if (loadConsole || buildType == "CONSOLE")
            Application.LoadLevel("Console");
        else
        {
            AudioController.Instance.PlaySelectMusic();
            StartCoroutine(launchScreen.StartSequence(() => AppController.Instance.LoadCarSelect()));
        }
    }
}
#pragma warning restore 0618
