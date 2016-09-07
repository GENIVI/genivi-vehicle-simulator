/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomPropertyDrawer(typeof(PositionedTexture))]
public class PositionedTexturePropertyDrawer : PropertyDrawer
{
    private const int _buttonWidth = 40;

    public override void OnGUI(Rect pos, SerializedProperty prop, GUIContent label)
    {
        EditorGUI.BeginProperty(pos, label, prop);
        pos = EditorGUI.PrefixLabel(pos, GUIUtility.GetControlID(FocusType.Passive), label);
        var indent = EditorGUI.indentLevel;
        EditorGUI.indentLevel = 1;
        EditorGUILayout.BeginVertical();
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("tex"), new GUIContent("Texture"));
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("texActive"), new GUIContent("Texture Active"));
        EditorGUILayout.PropertyField(prop.FindPropertyRelative("position"), new GUIContent("Position"));
        if(GUI.Button(new Rect(pos.x + pos.width - _buttonWidth, pos.y, _buttonWidth, pos.height), "Init")) {
            
            Texture2D tex = (Texture2D)(prop.FindPropertyRelative("tex").objectReferenceValue);
            var r = new Rect(0, 0, tex.width,tex.height);
            prop.FindPropertyRelative("position").rectValue = r;
        }
        EditorGUILayout.EndVertical();
        EditorGUI.EndProperty();
        EditorGUI.indentLevel = indent;
    }



}