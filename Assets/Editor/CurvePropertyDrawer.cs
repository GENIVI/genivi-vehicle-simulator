/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(AnimationCurve))]
public class CurvePropertyDrawer : PropertyDrawer
{
    private const int _buttonWidth = 12;
    private static Keyframe[] _buffer;
    private static WrapMode _preWrapMode;
    private static WrapMode _postWrapMode;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        prop.animationCurveValue = EditorGUI.CurveField(
            new Rect(pos.x, pos.y, pos.width - _buttonWidth * 2, pos.height),
            label,
            prop.animationCurveValue
        );

        // Copy
        if(
           GUI.Button(
                new Rect(pos.x + pos.width - _buttonWidth * 2, pos.y, _buttonWidth, pos.height),
                ""
            )
        )
        {
            _buffer = prop.animationCurveValue.keys;
            _preWrapMode = prop.animationCurveValue.preWrapMode;
            _postWrapMode = prop.animationCurveValue.postWrapMode;
        }
        GUI.Label(
            new Rect(pos.x + pos.width - _buttonWidth * 2, pos.y, _buttonWidth, pos.height),
            "C"
        );

        // Paste
       if(_buffer == null) return;
        if(
          GUI.Button(
                new Rect(pos.x + pos.width - _buttonWidth, pos.y, _buttonWidth, pos.height),
                ""
            )

        )
        {
            AnimationCurve newAnimationCurve = new AnimationCurve(_buffer);
            newAnimationCurve.preWrapMode = _preWrapMode;
            newAnimationCurve.postWrapMode = _postWrapMode;
            prop.animationCurveValue = newAnimationCurve;
        }
       GUI.Label(
            new Rect(pos.x + pos.width - _buttonWidth, pos.y, _buttonWidth, pos.height),
            "P"
        );

    }



}