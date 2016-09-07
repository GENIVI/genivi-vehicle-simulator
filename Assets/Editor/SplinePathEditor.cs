/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[CustomEditor(typeof(SplinePath), true)]
public class SplinePathEditor : Editor
{


    void OnEnable()
    {
    }

    void OnDisable()
    {
    }

    public override void OnInspectorGUI()
    {

        

        var t = target as SplinePath;

        if (t.pathNodes == null)
            t.pathNodes = new List<Vector3>();

        if (t.pathNodes.Count == 0)
        {
            if (GUILayout.Button("Init"))
            {
                t.pathNodes.Add(t.transform.position);
                t.pathNodes.Add(t.transform.position + t.transform.forward * 4f);
                SceneView.lastActiveSceneView.Repaint();
            }
        }
        else
        {
            if(GUILayout.Button("Add WP")) {
                var last = t.pathNodes[t.pathNodes.Count - 1];
                var lasttan = last - t.pathNodes[t.pathNodes.Count - 2];
                t.pathNodes.Add(last + lasttan.normalized * 4f);
                SceneView.lastActiveSceneView.Repaint();
            }

            if (GUILayout.Button("Delete WP"))
            {
                t.pathNodes.RemoveAt(t.pathNodes.Count - 1);
                SceneView.lastActiveSceneView.Repaint();
            }

            if (GUILayout.Button("Clear all WPs"))
            {
                t.pathNodes.Clear();
                SceneView.lastActiveSceneView.Repaint();
            }
             
            
        }

        DrawDefaultInspector();


    }

    public void OnSceneGUI()
    {
        var t = target as SplinePath;
        if (t.pathNodes != null && t.pathNodes.Count > 0)
        {

            for (int i = 1; i < t.pathNodes.Count; i++)
            {
                t.pathNodes[i] = Handles.PositionHandle(t.pathNodes[i], Quaternion.LookRotation(t.pathNodes[i] - t.pathNodes[i - 1]));
            }

            int points = t.pathNodes.Count * 5;
            Handles.DrawPolyLine(
                Enumerable.Range(0, points).Select(
                e => iTween.PointOnPath(t.pathNodes.ToArray(), (float)e / points)
            ).ToArray());
        }

        //
         //   iTween.DrawPath(t.pathNodes.ToArray(), Color.green);
    }

}
