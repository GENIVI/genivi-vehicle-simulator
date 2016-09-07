/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class GuiTextureColorFade : MonoBehaviour {

    public float fadeSpeed = 255f;
    public Color targetColor;

    private GUITexture tex;

    public void SetTarget(Color newColor)
    {
        targetColor = newColor;
    }

    public void SetStraight(Color newColor)
    {
        targetColor = newColor;
        currentColor = newColor;
    }

    protected Color currentColor;

    protected  void Start()
    {
        currentColor = targetColor;
        tex = GetComponent<GUITexture>();
    }

    protected void Update()
    {
        if(currentColor != targetColor)
        {
            currentColor = MoveTowards(currentColor, targetColor, fadeSpeed * Time.deltaTime);
        }

        Color c = tex.color;
        c.r = currentColor.r;
        c.g = currentColor.b;
        c.g = currentColor.b;
        tex.color = c;

    }

    static Color MoveTowards(Color start, Color target, float moveSpeed) {
        float r = Mathf.MoveTowards(start.r, target.r, moveSpeed);
        float g = Mathf.MoveTowards(start.g, target.g, moveSpeed);
        float b = Mathf.MoveTowards(start.b, target.b, moveSpeed);
        return new Color(r, g, b);
    }

}
