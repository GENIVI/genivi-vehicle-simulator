/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public class AdminSettings : Singleton<AdminSettings>
{
    public float musicVol = 0.8f;
    public float foleyVol = 0.8f;
    public float vehicleVol = 0.8f;
  
    public AdminScreen.DisplayType displayType = AdminScreen.DisplayType.FLAT;
    public float fov = 65f;
    public float selectFov = 65f;
    public float motionBlur = 0f;
    public float timeOfDay = 12f;
    public float moonPhase = 0.5f;
    public float skyContrast = 1f;

    public bool progressTime = false;
    public bool sunshafts = true;
    public bool fog = false;

    public float sunIntensity = 0.8f;
    public float shadowIntensity = 0.8f;

    public float moonIntensity = 0.1f;
    public float moonShadows = 1f;

    public float headlights = 80f;

    public int weather = 0;

    public float camFarClip = 10000f;

    public float camNearClip = 0.5f;

    public float moonLightIntensity = 0.15f;



}

[System.AttributeUsage(System.AttributeTargets.Field)]
public class AdminRangeAttribute : System.Attribute
{
    public readonly float min;
    public readonly float max;
    public readonly string label;

    public AdminRangeAttribute(string label, float min, float max)
    {
        this.min = min;
        this.max = max;
        this.label = label;
    }

}

public class GetterSetter
{
    public System.Action<float> setter;
    public System.Func<float> getter;
    public float min;
    public float max;
    public string label;
}

public class AdminScreen : MonoBehaviour {

    public enum DisplayType {PARABOLIC, TENTYFOURINCH, FIFTYFIVEINCH, FLAT}

    public GUIStyle topTitle;
    public GUIStyle title;
    public GUIStyle button;
    public GUIStyle smallButton;
    public GUIStyle slider;
    public GUIStyle toolLabel;
    public GUIStyle toggle;

    public float colWidth;

    private bool traffic = false;
    private bool headlights = false;

    public TrafSpawner trafSpawner;

    public bool driveScene = true;

    private float targetTimeOfDay;

    public CarSelectController carSelect;
    public EnvironmentSelectController environmentSelect;
    public DriverCamera driverCam;

    private List<System.Type> dynamicAdminTypes;
    private List<GetterSetter> cachedDynamicObjects;

    public bool hidden = false;
    void Awake()
    {

        dynamicAdminTypes = new List<System.Type>() {
            typeof(CarHeadlights)
        };

        if(driverCam == null)
            driverCam = TrackController.Instance.driverCam;

        if(AppController.Instance.appSettings.projectorBlend)
        {
            AdminSettings.Instance.displayType = DisplayType.PARABOLIC;
        }


        //SetDisplay();

        SetFarClip(AdminSettings.Instance.camFarClip);


        StartCoroutine(LoadDynamicTypes());


        

    }

    IEnumerator LoadDynamicTypes()
    {

        cachedDynamicObjects = new List<GetterSetter>();

        yield return null;
        yield return new WaitForSeconds(4f);
        yield return null;
        
        if(driverCam == null)
            driverCam = TrackController.Instance.driverCam;
        foreach(var o in dynamicAdminTypes)
        {
            var sceneObjs = FindObjectsOfType(o);
            foreach(var sceneObj in sceneObjs)
            {
                foreach(var f in o.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    foreach(var a in f.GetCustomAttributes(typeof(AdminRangeAttribute), false)) {
                        var rangeAttr = (AdminRangeAttribute)a;
                        var so = sceneObj;
                        var finfo = f;
                        cachedDynamicObjects.Add(new GetterSetter {
                            label = rangeAttr.label, 
                            min = rangeAttr.min, 
                            max = rangeAttr.max, 
                            setter = (newVal) => finfo.SetValue(so, newVal),
                            getter = () => { return (float)finfo.GetValue(so); } 
                        });
                    }
                }
            
        
                
            }
        }
    }

    // Update is called once per frame
    void Update () {
        //sync values that may move from other sources
        if(!Input.GetMouseButton(0))
        {
 

        }

        //values that need to lerp over time

    }

    void OnGUI()
    {
        if (hidden)
            return;

        //Matrix4x4 oldMatrix = GUI.matrix;
        //GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 1920f, Screen.height / 1080f, 1f));
        //GUILayout.BeginArea(new Rect(0, 0, 1920, 1080));

        GUI.depth = -2;

        Matrix4x4 oldMatrix = GUI.matrix;

        /*
        if(AdminSettings.Instance.displayType == DisplayType.PARABOLIC)
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 6912f, Screen.height / 1080f, 1f));
            GUILayout.BeginArea(new Rect(4992, 120, 1920, 1080));
        }
        else
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 7680f, Screen.height / 1080f, 1f));
            GUILayout.BeginArea(new Rect(5760, 120, 1920, 1080));
        }
        */
        OnGUIScaler.ScaleUI();
        
        
        GUILayout.BeginVertical();
        GUILayout.Label("Global Controls", topTitle, GUILayout.ExpandWidth(true), GUILayout.Height(80f));
        GUILayout.BeginHorizontal(GUILayout.Height(460));
        
        //Audio
        GUILayout.BeginVertical(GUILayout.Width(colWidth));
        GUILayout.Label("Audio", title);
        DrawSlider("Music", 0, 1, SetMusicVol, ref AdminSettings.Instance.musicVol);
        DrawSlider("Foley", 0, 1, SetFoleyVol, ref AdminSettings.Instance.foleyVol);
        DrawSlider("Vehicle", 0, 1, SetVehicleVol, ref AdminSettings.Instance.vehicleVol);
        GUILayout.EndVertical();

        //Display
        GUILayout.BeginVertical(GUILayout.Width(colWidth));
        GUILayout.Label("Display Configurations", title);
        DisplayType oldDisplay = AdminSettings.Instance.displayType;
        AdminSettings.Instance.displayType = (DisplayType)GUILayout.SelectionGrid((int)AdminSettings.Instance.displayType, new string[] { "Parabolic projection", "24\" configuration" }, 1, button);
        if(oldDisplay != AdminSettings.Instance.displayType)
            SetDisplay();
        GUILayout.EndVertical();

        //Calibrate Camera
        GUILayout.BeginVertical(GUILayout.Width(colWidth));
        GUILayout.Label("Calibrate Camera", title);
        DrawSlider("Motion Blur", 0f, 100f, null, ref AdminSettings.Instance.motionBlur);
        DrawSlider("Field Of View", 1f, 175f, SetFoV, ref AdminSettings.Instance.fov);
        DrawSlider("Far Clip Plane", 50f, 12000f, SetFarClip, ref AdminSettings.Instance.camFarClip);
        GUILayout.EndVertical();

        //Dynamic
        GUILayout.BeginVertical(GUILayout.Width(colWidth));
        GUILayout.Label("Extra Settings", title);
        if(cachedDynamicObjects != null)
        {
            foreach(var r in cachedDynamicObjects)
            {
                DrawSliderDynamic(r.label, r.min, r.max, r.setter, r.getter());
            }
        }
        GUILayout.EndVertical();


        GUILayout.EndHorizontal();
        GUILayout.Label("Scene Controls", topTitle, GUILayout.ExpandWidth(true), GUILayout.Height(80f));
        GUILayout.BeginHorizontal(GUILayout.Height(460));

        if(driveScene)
        {
            //weather
            GUILayout.BeginVertical(GUILayout.Width(colWidth));
            GUILayout.Label("Adjust Lighting and Weather", title);


            bool oldVal = AdminSettings.Instance.fog;
            AdminSettings.Instance.fog = DrawToggle("Enable Fog", AdminSettings.Instance.fog);
            if(oldVal != AdminSettings.Instance.fog)
            {
                RenderSettings.fog = AdminSettings.Instance.fog;
            }

/*            int old = AdminSettings.Instance.weather;
            AdminSettings.Instance.weather = GUILayout.Toolbar(AdminSettings.Instance.weather,
                new string[] { 
                    "Clear",
                    "Storm",
                    "Overcast",
                    "Fog"
                }, smallButton);

            if(old != AdminSettings.Instance.weather)
            {
                var newWeather = AdminSettings.Instance.GetWeather();
                weather.currentWeather = newWeather;
                weather.ChangeWeather(newWeather);
            }*/

            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(colWidth));
            GUILayout.Label("Lighting", title);

            bool oldLights = headlights;
            headlights = DrawToggle("Enable Headlights", headlights);
            if(oldLights != headlights)
            {
                TrackController.Instance.car.GetComponent<CarHeadlights>().SetHeadlights(headlights);
            }
            DrawSlider("Headlight Range", 0f, 200f, SetHeadlights, ref AdminSettings.Instance.headlights);
            GUILayout.EndHorizontal();

            //Obstacles
            GUILayout.BeginVertical(GUILayout.Width(colWidth / 2));
            GUILayout.Label("Obstacles", title);
            DrawButton("Obstacle 1", TrackController.Instance.TriggerObstacle1);
            DrawButton("Obstacle 2", TrackController.Instance.TriggerObstacle2);
            DrawButton("Obstacle 3", TrackController.Instance.TriggerObstacle3);
            DrawButton("Obstacle 4", TrackController.Instance.TriggerObstacle4);
            GUILayout.EndVertical();

            //Waypoints
            GUILayout.BeginVertical(GUILayout.Width(colWidth / 2));
            GUILayout.Label("Waypoints", title);
            DrawButton("Waypoint 1", TrackController.Instance.RepositionWaypoint1);
            DrawButton("Waypoint 2", TrackController.Instance.RepositionWaypoint2);
            DrawButton("Waypoint 3", TrackController.Instance.RepositionWaypoint3);
            DrawButton("Waypoint 4", TrackController.Instance.RepositionWaypoint4);
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(colWidth / 2));
            GUILayout.Label("Traffic", title);
            bool oldTraf = traffic;
            traffic = DrawToggle("Enable Traffic", traffic);
            if(oldTraf != traffic && trafSpawner != null)
            {
                trafSpawner.SetTraffic(traffic);
            }
            GUILayout.EndVertical();

            //end scene
            GUILayout.BeginVertical(GUILayout.Width(colWidth / 2));
            GUILayout.Label("End Scene", title);
            DrawButton("End Scene", () => { showingConfirmEnd = true; });
            GUILayout.EndVertical();

            //confirm
            if(showingConfirmEnd)
            {
                GUILayout.BeginVertical(GUILayout.Width(colWidth / 2));
                GUILayout.Label("Are You Sure?", title);
                DrawButton("YES", () =>
                {
                    showingConfirmEnd = false;
                    TrackController.Instance.ConfirmEndScene();
                });
                DrawButton("NO", () => { showingConfirmEnd = false; });
                GUILayout.EndVertical();
            }
        }
        else
        {
            /*GUILayout.BeginVertical(GUILayout.Width(colWidth));
            GUILayout.Label("Vechicle Select", title);
            DrawButton("Vehicle 1", () => SelectCar(1));
            DrawButton("Vehicle 2", () => SelectCar(1));
            DrawButton("Vehicle 3", () => SelectCar(1));
            DrawButton("Vehicle 4", () => SelectCar(1));
            GUILayout.EndVertical();

            GUILayout.BeginVertical(GUILayout.Width(colWidth));
            GUILayout.Label("Environment Select", title);
            DrawButton("Scenic", () => SelectEnv(1));
            DrawButton("Urban", () => SelectEnv(2));
            DrawButton("Coastal", () => SelectEnv(3));
            DrawButton("Change Vehicle", () => SelectEnv(0));
            GUILayout.EndVertical();*/

        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
        GUILayout.EndArea();
        GUI.matrix = oldMatrix;

    }

    private bool showingConfirmEnd = false;


    void SelectEnv(int env)
    {
        if(environmentSelect != null && environmentSelect.enabled && environmentSelect.gameObject.activeInHierarchy)
        {
            if(env == 1)
                environmentSelect.SelectScenic();
            else if(env == 2)
                environmentSelect.SelectUrban();
            else if(env == 3)
                environmentSelect.SelectCoastal();
            else if(env == 0)
                environmentSelect.Back();

        }
    }

    void SelectCar(int car)
    {
        if(carSelect != null && carSelect.enabled && carSelect.gameObject.activeInHierarchy)
        {
            if(car == 1)
                carSelect.Trigger1();
            else if(car == 2)
                carSelect.Trigger2();
            else if(car == 3)
                carSelect.Trigger3();
            else if(car == 4)
                carSelect.Trigger4();
        }
    }


    //TODO: IMplement
    void SetHeadlights(float val)
    {
        TrackController.Instance.car.GetComponent<CarHeadlights>().SetRange(val);
    }


    void SetVehicleVol(float newVol)
    {
        AudioController.Instance.VehicleVolume = newVol;
    }

    void SetMusicVol(float newVol)
    {
        AudioController.Instance.MusicVolume = newVol;
    }

    void SetFoleyVol(float newVol)
    {
        AudioController.Instance.FoleyVolume = newVol;
    }

    void SetFoV(float newFov)
    {   
        driverCam.SetFoV(newFov);
    }

    void SetFarClip(float newFarClip)
    {
        driverCam.SetFarClip(newFarClip);
        RenderSettings.fogStartDistance = newFarClip - 200;
        RenderSettings.fogEndDistance = newFarClip;
    }

    void SetDisplay()
    {
        driverCam.SetCameraType(AdminSettings.Instance.displayType, AdminSettings.Instance.fov);
        if(AdminSettings.Instance.displayType == DisplayType.PARABOLIC)
        {
            GetComponent<Camera>().rect = new Rect(0.722222222f, 0, 0.277777777f, 1f);
        }
        else
        {
            GetComponent<Camera>().rect = new Rect(0.75f, 0f, 0.25f, 1f);
        }
    }

    void DrawButton(string label, System.Action onPressed)
    {
        if(GUILayout.Button(label, button) && onPressed != null)
            onPressed();
    }

    bool DrawToggle(string label, bool val)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, toolLabel);
        GUILayout.FlexibleSpace();
        bool r;
        r = GUILayout.Toggle(val, "");
        GUILayout.Space(toggle.fixedWidth);
        GUILayout.EndHorizontal();
        return r;
    }

    void DrawSlider(string label, float min, float max, System.Action<float> onChanged, ref float val)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, toolLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label(val.ToString("N2"), toolLabel);
        float oldVal = val;
        val = GUILayout.HorizontalSlider(val, min, max, GUILayout.Width(300f));
        GUILayout.EndHorizontal();
        if(oldVal != val && onChanged != null)
        {
            onChanged(val);
        }
        
    }

    void DrawSliderDynamic(string label, float min, float max, System.Action<float> onChanged, float val)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(label, toolLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label(val.ToString("N2"), toolLabel);
        
        float newVal = GUILayout.HorizontalSlider(val, min, max, GUILayout.Width(300f));
        GUILayout.EndHorizontal();
        if(newVal != val && onChanged != null)
        {
            onChanged(newVal);
        }
    
    }

}
