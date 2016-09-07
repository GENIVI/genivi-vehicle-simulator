/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class GuiTexturePositionTween : MonoBehaviour {

    public float fadeSpeed = 1f;
    public float targetX;

    private GUITexture tex;

    public void SetTarget(float newX)
    {
        if(AppController.Instance.appSettings.scaleUi)
        {
            float scale = Screen.width / 5760f;
            newX *= scale;
        }
        targetX = newX;
    }

    public void SetStraight(float newX)
    {
        if(AppController.Instance.appSettings.scaleUi)
        {
            float scale = Screen.width / 5760f;
            newX *= scale;
        }
        targetX = newX;
        currentX = newX;
    }

    protected float currentX;

    protected void Start()
    {
        currentX = targetX;
        tex = GetComponent<GUITexture>();
    }

    protected void Update()
    {
        if(currentX != targetX)
        {
            currentX = Mathf.MoveTowards(currentX, targetX, fadeSpeed * Time.deltaTime);
        }

        Rect pi = tex.pixelInset;
        pi.x = currentX;
        tex.pixelInset = pi;

    }
}
