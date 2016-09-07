/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

public class CarSelectIndicator : MonoBehaviour {

    public bool reverse = false;

    private Color onColorBg;
    private Color offColorBg;
    private Color onColorText;
    private Color offColorText;
    private Color onColorShadow;
    private Color offColorShadow;

    public GUITexture texture;
    public GUIText text;

    public Transform target;
    public Renderer shadow;

    void Awake()
    {
        onColorBg = texture.color;
        offColorBg = new Color(onColorBg.r, onColorBg.g, onColorBg.b, 0f);

        onColorText = text.color;
        offColorText = new Color(onColorText.r, onColorText.g, onColorText.b, 0f);

        onColorShadow = shadow.material.color;
        offColorShadow = new Color(onColorShadow.r, onColorShadow.g, onColorShadow.b, 0f);

        texture.color = offColorBg;
        text.color = offColorText;
        shadow.material.color = offColorShadow;
    }


    void Start () {
        var UICam = Camera.main;
        var screenPos = UICam.WorldToScreenPoint(target.position);

        float w = texture.pixelInset.width;
        float h = texture.pixelInset.height;

        if (!reverse)
        {
            texture.pixelInset = new Rect(screenPos.x, screenPos.y, w, h);
            text.pixelOffset = new Vector2(screenPos.x + w / 2, screenPos.y + h / 2);
        }
        else
        {
            texture.pixelInset = new Rect(screenPos.x - w, screenPos.y, w, h);
            text.pixelOffset = new Vector2(screenPos.x - w + w / 2, screenPos.y + h / 2);
        }
    }

    public void Fade(bool fadeIn)
    {
        StartCoroutine(_Fade(fadeIn));
    }

    public IEnumerator _Fade(bool fadeIn)
    {
        float startTime = Time.time;
        Color startBg = texture.color;
        Color startText = text.color;
        Color startShadow = shadow.material.color;

        while (Time.time - startTime < 0.2f)
        {
            float lerp = (Time.time - startTime)/0.2f;
            texture.color = Color.Lerp(startBg, !fadeIn ? offColorBg : onColorBg, lerp);
            text.color = Color.Lerp(startText, !fadeIn ? offColorText : onColorText, lerp);
            shadow.material.color = Color.Lerp(startShadow, !fadeIn ? offColorShadow : onColorShadow, lerp);
            yield return null;
        }
        text.color = fadeIn ? onColorText : offColorText;
        texture.color = fadeIn ? onColorBg : offColorBg;
        shadow.material.color = fadeIn ? onColorShadow : offColorShadow;
    }

}
