/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class ShowFPS : MonoBehaviour {

    public GUIStyle fpsStyle;
    private bool show = true;

	public float frequency = 0.5f;

    public int fps;
 
	private void Start() {
		StartCoroutine(FPS());
	}
 
	private IEnumerator FPS() {
		while(true){
			int lastFrameCount = Time.frameCount;
			float lastTime = Time.realtimeSinceStartup;
			yield return new WaitForSeconds(frequency);
			float timeSpan = Time.realtimeSinceStartup - lastTime;
			int frameCount = Time.frameCount - lastFrameCount;
			fps = Mathf.RoundToInt(frameCount / timeSpan);
		}
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            show = !show;
        }
    }

    void OnGUI()
    {
        if(show)
            GUI.Label(new Rect(15, 245, 600, 40), "FPS: " + fps, fpsStyle);
    }
}

