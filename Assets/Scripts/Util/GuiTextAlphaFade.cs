/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;


public class GuiTextAlphaFade : BaseAlphaFade {
    
    private GUIText tex;

    protected override void Start()
    {
        base.Start();
        tex = GetComponent<GUIText>();
    }

    protected override void Update()
    {
        base.Update();
        Color c = tex.color;
        c.a = currentAlpha;
        tex.color = c;

    }
}
