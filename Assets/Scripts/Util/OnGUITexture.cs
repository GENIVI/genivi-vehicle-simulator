/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class OnGUITexture : MonoBehaviour {

    public Rect placement;
    public Texture2D texture;

    public Rect uvs = new Rect(0, 0, 1, 1);
    public Color color = Color.white;

    public bool doScale = true;

    public void OnGUI()
    {
        Matrix4x4 old = GUI.matrix;
        if (doScale)
        {
            if (AppController.Instance.appSettings.projectorBlend)
            {
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 4992f, Screen.height / 1080f, 1f));
            }
            else
            {
                GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(Screen.width / 5760f, Screen.height / 1080f, 1f));
            }
        }
        Color oldColor = GUI.color;
        GUI.color = color;
        GUI.DrawTextureWithTexCoords(placement, texture, uvs);
        GUI.color = oldColor;
        GUI.matrix = old;
    }

    public void FadeOut()
    {
        StartCoroutine(_FadeOut(1f));
    }

    IEnumerator _FadeOut(float time)
    {
        float startTime = Time.time;
        while(color.a > 0f)
        {
            Color c = color;
            c.a = Mathf.Lerp(1f, 0f, (Time.time - startTime) / time);
            color = c;
            yield return null;
        }
    }
}
