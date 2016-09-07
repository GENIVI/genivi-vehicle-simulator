/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

public class RoadSaver : Singleton<RoadSaver>
{
    public TrafRoad currentRoad;
}


[CustomEditor(typeof(TrafSystemData))]
public class TrafSystemDataEditor : Editor
{

    private const float terrainOffsetUp = 2f;
    private const float startDisplayOffsetRight = 10f;
    private const float maxEdgesConnectedDistance = 15f;

    private TrafEditState currentState = TrafEditState.IDLE;
    private TrafRoad currentRoad;

#pragma warning disable 0649
    private List<Vector3>[] currentRoadMulti;
#pragma warning restore 0649
    private float laneWidth = 4f;

    public int currentId;
    public int currentSubId;

    public List<TrafEntry> entries;

    public int collisionLayer = 0;

    private bool showIds = false;

    void OnEnable()
    {
        entries = (target as TrafSystemData).system.entries;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        EditorGUILayout.BeginVertical();
        EditorGUILayout.BeginHorizontal();
        currentId = EditorGUILayout.IntField("ID", currentId);
        if(GUILayout.Button("Next Free ID"))
        {
            currentId = entries.Max(e => e.identifier) + 1;
        }
        currentSubId = EditorGUILayout.IntField("SubID", currentSubId);
        EditorGUILayout.EndHorizontal();

        switch(currentState)
        {
            case TrafEditState.IDLE:
                if(GUILayout.Button("UpdateSystem"))
                {
                    var sys = (target as TrafSystemData).system;
                    sys.entries = entries;
                    EditorUtility.SetDirty(sys);
                }
                
                if(GUILayout.Button("DELETE - CAREFUL!"))
                {
                    entries.RemoveAll(e => e.identifier == currentId);
                }

                if(GUILayout.Button("new/edit"))
                {
                    if(entries.Any(entry => entry.identifier == currentId && entry.subIdentifier == currentSubId))
                    {
                        var road = entries.Find(entry => entry.identifier == currentId && entry.subIdentifier == currentSubId);
                        currentRoad = road.road;
                        entries.Remove(road);
                        currentState = TrafEditState.EDIT;
                    }
                    else
                    {
                        currentRoad = ScriptableObject.CreateInstance(typeof(TrafRoad)) as TrafRoad;
                        InitRoad(currentRoad);
                        currentState = TrafEditState.EDIT;
                    }
                }

                if(GUILayout.Button("new multi one way 4 lanes"))
                {
                     if(entries.Any(entry => entry.identifier == currentId))
                    {
                        Debug.Log("TrafSystem: A road with that ID already exists. Multi editing not supported");
                    }
                    else
                    {
                        currentRoad = ScriptableObject.CreateInstance(typeof(TrafRoad)) as TrafRoad;
                        InitRoad(currentRoad);
                        currentState = TrafEditState.EDIT_MULTI;
                    }
                }

                if (GUILayout.Button("new multi one way 3 lanes"))
                {
                    if (entries.Any(entry => entry.identifier == currentId))
                    {
                        Debug.Log("TrafSystem: A road with that ID already exists. Multi editing not supported");
                    }
                    else
                    {
                        currentRoad = ScriptableObject.CreateInstance(typeof(TrafRoad)) as TrafRoad;
                        InitRoad(currentRoad);
                        currentState = TrafEditState.EDIT_MULTI_3;
                    }
                }

                if(GUILayout.Button("new multi two way"))
                {
                    if(entries.Any(entry => entry.identifier == currentId))
                    {
                        Debug.Log("TrafSystem: A road with that ID already exists. Multi editing not supported");
                    }
                    else
                    {
                        currentRoad = ScriptableObject.CreateInstance(typeof(TrafRoad)) as TrafRoad;
                        InitRoad(currentRoad);
                        currentState = TrafEditState.EDIT_MULTI_TWOWAY;
                        SceneView.RepaintAll();
                    }
                }

                if(GUILayout.Button("Visualize"))
                {
                    currentState = TrafEditState.DISPLAY;
                    SceneView.RepaintAll();
                }


                if (GUILayout.Button("Visualize Splines"))
                {
                    currentState = TrafEditState.DISPLAY_SPLINES;
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Visualize Intersection Splines"))
                {
                    currentState = TrafEditState.DISPLAY_INTERSECTION_SPLINES;
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("Edit Ends"))
                {
                    currentState = TrafEditState.EDIT_ENDS;
                    SceneView.RepaintAll();
                }

                if(GUILayout.Button("TEMP"))
                {
                    foreach(var e in entries)
                    {
                        if(e.road != null)
                        {
                            e.waypoints = new List<Vector3>();
                            foreach(var v in e.road.waypoints)
                            {
                                e.waypoints.Add(v.position);
                            }
                        }
                    }
                }
                break;
            case TrafEditState.EDIT:
                if(GUILayout.Button("save"))
                {
                    currentState = TrafEditState.IDLE;
                    entries.Add(new TrafEntry() { road = currentRoad, identifier = currentId, subIdentifier = currentSubId });
                }
                break;
            case TrafEditState.EDIT_MULTI:
                laneWidth = EditorGUILayout.Slider("lane width", laneWidth, 0f, 50f);

                if(GUILayout.Button("save"))
                {
                    currentState = TrafEditState.IDLE;
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * -1.5f, false), identifier = currentId, subIdentifier = 0 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * -.5f, false), identifier = currentId, subIdentifier = 1 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * .5f, false), identifier = currentId, subIdentifier = 2 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * 1.5f, false), identifier = currentId, subIdentifier = 3 });
                }
                if(GUILayout.Button("cancel"))
                {
                    currentState = TrafEditState.IDLE;
                }
                break;
            case TrafEditState.EDIT_MULTI_3:
                laneWidth = EditorGUILayout.Slider("lane width", laneWidth, 0f, 50f);
                if (GUILayout.Button("add"))
                {
                    currentRoad.waypoints.Add(
                        new TrafRoadPoint
                        {
                            position = currentRoad.waypoints[currentRoad.waypoints.Count - 1].position
                                + (currentRoad.waypoints[currentRoad.waypoints.Count - 1].position - currentRoad.waypoints[currentRoad.waypoints.Count - 2].position).normalized * 5f
                        });
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("align height"))
                {
                    foreach (var wp in currentRoad.waypoints)
                    {

                        RaycastHit[] hits = Physics.RaycastAll(wp.position + Vector3.up * 20f, Vector3.down, 1000f);
                        if (hits.Length == 0 || collisionLayer >= hits.Length)
                        {
                            Debug.Log("Nothing hit on wp " + wp.position);
                        }
                        else
                        {
                            wp.position = hits[collisionLayer].point + Vector3.up * 0.5f;
                        }
                    }
                    SceneView.RepaintAll();
                    
                }

                if (GUILayout.Button("Save temp"))
                {
                    RoadSaver.Instance.currentRoad = currentRoad;
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Restore temp"))
                {
                    currentRoad = RoadSaver.Instance.currentRoad;
                    SceneView.RepaintAll();
                }

                if (GUILayout.Button("save"))
                {
                    currentState = TrafEditState.IDLE;
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * -1f, false), identifier = currentId, subIdentifier = 0 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * 0f, false), identifier = currentId, subIdentifier = 1 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * 1f, false), identifier = currentId, subIdentifier = 2 });
                }
                if (GUILayout.Button("cancel"))
                {
                    currentState = TrafEditState.IDLE;
                }
                break;

            case TrafEditState.EDIT_MULTI_TWOWAY:
                laneWidth = EditorGUILayout.Slider("lane width", laneWidth, 0f, 50f);

                if(GUILayout.Button("save"))
                {
                    currentState = TrafEditState.IDLE;
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * -1.5f, true), identifier = currentId, subIdentifier = 0 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * -.5f, true), identifier = currentId, subIdentifier = 1 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * .5f, false), identifier = currentId, subIdentifier = 2 });
                    entries.Add(new TrafEntry() { road = InitRoadMulti(currentRoad, laneWidth * 1.5f, false), identifier = currentId, subIdentifier = 3 });
                }
                break;

            case TrafEditState.EDIT_MULTI_RA:
                laneWidth = EditorGUILayout.Slider("lane width", laneWidth, 0f, 50f);
                if(GUILayout.Button("save"))
                {
                    currentState = TrafEditState.IDLE;
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 0, waypoints = currentRoadMulti[0] });
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 1, waypoints = currentRoadMulti[1] });
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 2, waypoints = currentRoadMulti[2] });
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 3, waypoints = currentRoadMulti[3] });
                }
                break;

            case TrafEditState.EDIT_MULTI_RA_TWOWAY:
                laneWidth = EditorGUILayout.Slider("lane width", laneWidth, 0f, 50f);
                if(GUILayout.Button("save"))
                {
                    currentState = TrafEditState.IDLE;
                    currentRoadMulti[0].Reverse();
                    currentRoadMulti[1].Reverse();
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 0, waypoints = currentRoadMulti[0] });
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 1, waypoints = currentRoadMulti[1] });
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 2, waypoints = currentRoadMulti[2] });
                    entries.Add(new TrafEntry() { identifier = currentId, subIdentifier = 3, waypoints = currentRoadMulti[3] });
                }
                break;

            case TrafEditState.DISPLAY:
            case TrafEditState.DISPLAY_SPLINES:

                if(GUILayout.Button("Back"))
                {
                    currentState = TrafEditState.IDLE;
                    SceneView.RepaintAll();
                }
                showIds = GUILayout.Toggle(showIds, "Show IDs?");
                break;

            case TrafEditState.EDIT_ENDS:
                if(GUILayout.Button("Back"))
                {
                    currentState = TrafEditState.IDLE;
                    SceneView.RepaintAll();
                }
                break;
        }


        EditorGUILayout.EndVertical();
    }

    void OnSceneGUI()
    {
        switch(currentState)
        {
            case TrafEditState.EDIT:
                DrawPath(currentRoad);
                break;
            case TrafEditState.EDIT_MULTI:
                DrawPathMulti(currentRoad, laneWidth);
                break;
            case TrafEditState.EDIT_MULTI_3:
                DrawPathMultiTHREE(currentRoad, laneWidth);
                break;
            case TrafEditState.EDIT_MULTI_TWOWAY:
                DrawPathMulti(currentRoad, laneWidth);
                break;
            case TrafEditState.DISPLAY:
                Visualize();
                break;
            case TrafEditState.DISPLAY_SPLINES:
                VisualizeSplines();
                break;
            case TrafEditState.DISPLAY_INTERSECTION_SPLINES:
                VisualizeIntersectionSplines();
                break;
            case TrafEditState.EDIT_ENDS:
                EditEnds();
                break;
        }

    }

    private TrafRoad InitRoadMulti(TrafRoad cursor, float offset, bool reverse)
    {
        var r = ScriptableObject.CreateInstance(typeof(TrafRoad)) as TrafRoad;
        r.waypoints = new List<TrafRoadPoint>();
        for(int wp = 0; wp < cursor.waypoints.Count; wp++)
        {
            if(wp < cursor.waypoints.Count - 1)
            {
                Vector3 tangent = cursor.waypoints[wp + 1].position - cursor.waypoints[wp].position;
                var off = (Quaternion.Euler(0, 90f, 0) * tangent);
                off.y = 0;
                off.Normalize();
                r.waypoints.Add(new TrafRoadPoint() { position = cursor.waypoints[wp].position + off * offset });
            }
            else
            {
                Vector3 tangent = cursor.waypoints[wp].position - cursor.waypoints[wp - 1].position;
                var off = (Quaternion.Euler(0, 90f, 0) * tangent);
                off.y = 0;
                off.Normalize();
                r.waypoints.Add(new TrafRoadPoint() { position = cursor.waypoints[wp].position + off * offset });
            }
        }

        if(reverse)
        {
            r.waypoints.Reverse();
        }

        return r;
    }

    public void InitRoad(TrafRoad road)
    {
        var t = road;
        t.waypoints = new List<TrafRoadPoint>();
        var scenecam = SceneView.currentDrawingSceneView.camera;
        RaycastHit[] hits = Physics.RaycastAll(scenecam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 2f)), 1000f);
        for(int i = 0; i < hits.Length; i++)
        {

            if(t != null)
            {
                Vector3 pos = hits[i].point + Vector3.up * terrainOffsetUp;
                t.waypoints.Add(new TrafRoadPoint { position = pos });
                t.waypoints.Add(new TrafRoadPoint { position = pos + Vector3.right * startDisplayOffsetRight });
                break;
            }
        }
    }

    public static void DrawPath(TrafRoad t)
    {
        if(t.waypoints != null && t.waypoints.Count > 0)
        {
            var wps = t.waypoints;
            for(int wp = 0; wp < wps.Count; wp++)
            {

                wps[wp].position = Handles.PositionHandle(wps[wp].position, Quaternion.identity);

                if(wp == 0)
                    Handles.color = Color.red;
                else if(wp == wps.Count - 1)
                    Handles.color = Color.green;
                else
                    Handles.color = Color.yellow;

                Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position, Quaternion.identity, 1f);
            }
            Handles.DrawPolyLine(wps.Select(w => w.position).ToArray());
        }
    }

    public static void DrawPathMulti(TrafRoad r, float laneWidth)
    {
        if(r.waypoints != null && r.waypoints.Count > 0)
        {
            var wps = r.waypoints;
            for(int wp = 0; wp < wps.Count; wp++)
            {

                wps[wp].position = Handles.PositionHandle(wps[wp].position, Quaternion.identity);

                if(wp == 0)
                    Handles.color = Color.red;
                else if(wp == wps.Count - 1)
                    Handles.color = Color.green;
                else
                    Handles.color = Color.yellow;

                if(wp < r.waypoints.Count - 1)
                {
                    Vector3 tangent = wps[wp + 1].position - wps[wp].position;
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * -1.5f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * -.5f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * .5f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * 1.5f, Quaternion.identity, 1f);
                }
                else
                {
                    Vector3 tangent = wps[wp].position - wps[wp - 1].position;
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * -1.5f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * -.5f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * .5f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * laneWidth * 1.5f, Quaternion.identity, 1f);
                }


            }
            Handles.DrawPolyLine(wps.Select(w => w.position).ToArray());
        }
    }

    public static void DrawPathMultiTHREE(TrafRoad r, float laneWidth)
    {
        if (r.waypoints != null && r.waypoints.Count > 0)
        {
            var wps = r.waypoints;
            for (int wp = 0; wp < wps.Count; wp++)
            {

                wps[wp].position = Handles.PositionHandle(wps[wp].position, Quaternion.identity);

                if (wp == 0)
                    Handles.color = Color.red;
                else if (wp == wps.Count - 1)
                    Handles.color = Color.green;
                else
                    Handles.color = Color.yellow;

                if (wp < r.waypoints.Count - 1)
                {
                    Vector3 tangent = wps[wp + 1].position - wps[wp].position;
                    var offset = (Quaternion.Euler(0, 90f, 0) * tangent);
                    offset.y = 0;
                    offset.Normalize();
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + offset * laneWidth * -1f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + offset * laneWidth * 0f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + offset * laneWidth * 1f, Quaternion.identity, 1f);
                }
                else
                {
                    Vector3 tangent = wps[wp].position - wps[wp - 1].position;
                    var offset = (Quaternion.Euler(0, 90f, 0) * tangent);
                    offset.y = 0;
                    offset.Normalize();
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + offset * laneWidth * -1f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + offset * laneWidth * 0f, Quaternion.identity, 1f);
                    Handles.SphereCap(HandleUtility.nearestControl, wps[wp].position + offset * laneWidth * 1f, Quaternion.identity, 1f);
                }


            }
            Handles.DrawPolyLine(wps.Select(w => w.position).ToArray());
        }
    }

    private void Visualize()
    {

        foreach(var r in entries)
        {
            var points = r.GetPoints();
            Handles.color = Color.green;
            Handles.DrawPolyLine(points);
            Handles.color = Color.red;
            Vector3 position = Vector3.Lerp(points[points.Length / 2 - 1], points[points.Length / 2], 0.5f);
            Handles.ArrowCap(0, position, Quaternion.LookRotation(points[points.Length / 2] - points[points.Length / 2 - 1]), 15f);

            if(showIds)
            {

                Handles.color = Color.white;
                Handles.Label(position, r.identifier + "_" + r.subIdentifier);
            }

        }
    }

    private void VisualizeSplines()
    {

        var sys = (target as TrafSystemData).system;

        foreach (var r in sys.entries)
        {
            var spline = r.GetSpline();

            float pc = 0f;
            var line = new List<Vector3>();
            int i = 0;
            while (true)
            {


                if (pc > 1.08f)
                {
                    i++;
                    pc -= 1f;
                }

                if (i >= spline.Count -1)
                    break;

                line.Add(HermiteMath.HermiteVal(spline[i].position, spline[i + 1].position, spline[i].tangent, spline[i+1].tangent, pc));


                pc += 0.1f;
            }

            Handles.color = Color.green;
            Handles.DrawPolyLine(line.ToArray());
        }
    }

    private void VisualizeIntersectionSplines()
    {
        var sys = (target as TrafSystemData).system;
        foreach (var intersection in sys.intersections)
        {
            var spline = intersection.GetSpline();

            float pc = 0f;
            var line = new List<Vector3>();
            while (true)
            {
                if (pc > 1.08f)
                {
                    break;
                }
                line.Add(HermiteMath.HermiteVal(spline[0].position, spline[1].position, spline[0].tangent, spline[1].tangent, pc));
                pc += 0.1f;
            }

            Handles.color = Color.green;
            Handles.DrawPolyLine(line.ToArray());
        }
    }

    private void LoadSplines()
    {
        foreach (var r in entries)
        {
            r.spline = new List<SplineNode>();

        }

    }


    private void EditEnds()
    {
        var cur = entries.Where(e => e.identifier == currentId);

        foreach(var r in cur){
            var points = r.GetPoints();
            Handles.color = Color.green;
            Handles.DrawPolyLine(points);
            Handles.color = Color.red;
            Vector3 position = Vector3.Lerp(points[points.Length / 2 - 1], points[points.Length / 2], 0.5f);
            Handles.ArrowCap(0, position, Quaternion.LookRotation(points[points.Length / 2] - points[points.Length / 2 - 1]), 15f);

            r.waypoints[0] = Handles.PositionHandle(r.waypoints[0], Quaternion.identity);
            r.waypoints[r.waypoints.Count - 1] = Handles.PositionHandle(r.waypoints[r.waypoints.Count - 1], Quaternion.identity);
        }
    }
}
