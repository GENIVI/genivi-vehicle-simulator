/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class DriveAdminUISettings : MonoBehaviour
{
    public VehicleConfigurator configurator;

    public DriverCamera driverCam;

    public ITrafficSpawner traffic;

    public Environment mapEnvironment;

    public void SetTimeOfDay(float val)
    {
        StopCoroutine("_SetTimeOfDay");
        StartCoroutine("_SetTimeOfDay", val);
    }

    public List<AdminItem> updateEachFrame;

    bool settingTimeOfDay = false;

    public bool disableupdate = false;


    public void SetFarClip(float newFarClip)
    {
        AdminSettings.Instance.camFarClip = newFarClip;
        driverCam.SetFarClip(newFarClip);
        RenderSettings.fogStartDistance = newFarClip - 200;
        RenderSettings.fogEndDistance = newFarClip;
    }

    public void SetDisplay()
    {
        driverCam.SetCameraType(AdminSettings.Instance.displayType, AdminSettings.Instance.fov);
        if (AdminSettings.Instance.displayType == AdminScreen.DisplayType.PARABOLIC)
        {
//            GetComponent<Camera>().rect = new Rect(0.722222222f, 0, 0.277777777f, 1f);
//removed since no camera needed in popup aadmin
        }
        else
        {
//            GetComponent<Camera>().rect = new Rect(0.75f, 0f, 0.25f, 1f);
        }
    }

    void OnEnable()
    {
        RemoteAdminController.OnSetTimeOfDay += SetTimeOfDay;
        RemoteAdminController.OnSetProgressTime += OnSetProgressTime;
    }

    void OnDisable()
    {
        RemoteAdminController.OnSetTimeOfDay -= SetTimeOfDay;
        RemoteAdminController.OnSetProgressTime -= OnSetProgressTime;
    }

    void OnSetProgressTime(bool prog)
    {
        AdminSettings.Instance.progressTime = prog;
    }

    void Awake()
    {
        updateEachFrame = new List<AdminItem>();
    }

    public void Init()
    {

    }


    private const float remoteUpdateTime = 1f;
    private float lastRemoteUpdate = 0f;

    public void Update()
    {
        if (didInit && Time.time - lastRemoteUpdate > remoteUpdateTime)
        {
            RemoteAdminController.Instance.SendMessageRaw(RemoteAdminController.SendMessageType.REFRESH_STATE, BuildRemoteState());
            lastRemoteUpdate = Time.time;
        }


        //sync values that may move from other sources
        if (!Input.GetMouseButton(0) && !disableupdate)
        {
            foreach (var i in updateEachFrame)
            {
                i.UpdateValue();
            }
        }

    }

    public string BuildRemoteState()
    {
        string trafState = "false";
        if (traffic != null)
            trafState = traffic.GetState().ToString();

        string ret = @"{
            ""enableTraffic"" : """ + trafState + @""",
            ""enableAutoPlay"" : """ + TrackController.Instance.IsInAutoPath() + @""",
            ""musicVol"" : """ + AudioController.Instance.MusicVolume + @""",
            ""foleyVol"" : """ + AudioController.Instance.FoleyVolume + @""",
            ""VehicleVol"" : """ + AudioController.Instance.VehicleVolume + @""",
            ""timeOfDay"" : """ + 0 + @""",
            ";
        ret += @"progressTime"" : """ + true + @""",
            ""currentWeather"" : """ + "clear" + @""",
            ""wetRoads"" : """ + false + @""",
            ""sunShafts"" : """ + AdminSettings.Instance.sunshafts + @""",
            ";
        ret += @"camFoV"" : """ + AdminSettings.Instance.fov + @""",
            ""camFarClip"" : """ + AdminSettings.Instance.camFarClip + @""",
            ""mapXPos"" : """ + TrackController.Instance.mapXPos + @""",
            ""mapZPos"" : """ + TrackController.Instance.mapZPos + @""",
            ""settingTimeOfDay"" : """ + settingTimeOfDay + @"""      
        }";

        return ret;
    }

    [HideInInspector]
    public bool didInit = false;
}

[System.Serializable]
public class MiscSettings
{
    public bool progressTime;
    public bool sunShafts;
    public bool fog;
    public float fov;
    public float cameraClip;
    public float musicVol;
    public float foleyVol;
    public float vehicleVol;
}

public partial class DriveAdminUI : MonoBehaviour {

    //TODO: implement
    public string BuildRemoteState()
    {
        return @"{
            ""enableTraffic"" : """ + settings.traffic == null ? "false" : settings.traffic.GetState() + @""",
            ""enableAutoPlay"" : """ + TrackController.Instance.IsInAutoPath() + @""",
            ""musicVol"" : """ + AudioController.Instance.MusicVolume + @""",
            ""foleyVol"" : """ + AudioController.Instance.FoleyVolume + @""",
            ""VehicleVol"" : """ + AudioController.Instance.VehicleVolume + @""",
            ""timeOfDay"" : """ + 0 + @""",
            ""progressTime"" : """ + false + @""",
            ""currentWeather"" : """ + GetRemoteWeatherString() + @""",
            ""wetRoads"" : """ + WetRoads.instance.IsWet() + @""",
            ""sunShafts"" : """ + AdminSettings.Instance.sunshafts + @""",
            ""camFoV"" : """ + AdminSettings.Instance.fov + @""",
            ""camFarClip"" : """ + AdminSettings.Instance.camFarClip + @""",
            ""mapXPos"" : """ + TrackController.Instance.mapXPos + @""",
            ""mapZPos"" : """ + TrackController.Instance.mapZPos + @"""      
            
        }";
    }

    public static string GetRemoteWeatherString()
    {
        int w = AdminSettings.Instance.weather;
        if (w == 0)
            return "clear";
        else if (w == 1)
            return "stormy";
        else if (w == 2)
            return "overcast";

        return "clear";
    }

    private DriveAdminUISettings settings;

    public void Init()
    {
        settings = GetComponent<DriveAdminUISettings>();
        if (AppController.Instance.appSettings.projectorBlend)
        {
            AdminSettings.Instance.displayType = AdminScreen.DisplayType.PARABOLIC;
        }
        settings.SetDisplay();

        settings.SetFarClip(AdminSettings.Instance.camFarClip);


        GenerateUI();

        foreach (var cat in adminCategories)
        {
            cat.Calculate();
        }

        settings.didInit = true;

    }

    void SaveMisc()
    {
        var savedSettings = new MiscSettings()
        {
            sunShafts = AdminSettings.Instance.sunshafts,
            fog = AdminSettings.Instance.fog,
            fov = AdminSettings.Instance.fov,
            cameraClip = AdminSettings.Instance.camFarClip,
            musicVol = AudioController.Instance.MusicVolume,
            foleyVol = AudioController.Instance.FoleyVolume,
            vehicleVol = AudioController.Instance.VehicleVolume
        };

        var serializer = new XmlSerializer(typeof(MiscSettings));

        using (var filestream = new FileStream(Application.streamingAssetsPath + "/SavedPresets/MiscSettings.xml", FileMode.Create))
        {
             var writer = new System.Xml.XmlTextWriter(filestream, System.Text.Encoding.Unicode);
            serializer.Serialize(writer, savedSettings);
        }

    }

    void SaveCar(bool wet)
    {
        var car = AppController.Instance.currentSessionSettings.selectedCarShortname;
        var settings = GetComponent<DriveAdminUISettings>();
        var configurator = settings.configurator;

        var savedSettings = new VehicleSettings(configurator);

        var serializer = new XmlSerializer(typeof(VehicleSettings));

        using (var filestream = new FileStream(Application.streamingAssetsPath + "/SavedPresets/" + car + ( wet ? "_WET.xml" : ".xml"), FileMode.Create))
        {
            var writer = new System.Xml.XmlTextWriter(filestream, System.Text.Encoding.Unicode);
            serializer.Serialize(writer, savedSettings);
        }
    }

    public void LoadCar(bool wet)
    {
        var settings = GetComponent<DriveAdminUISettings>();
        var configurator = settings.configurator;

        var car = AppController.Instance.currentSessionSettings.selectedCarShortname;

        var serializer = new XmlSerializer(typeof(VehicleSettings));

        using (var filestream = new FileStream(Application.streamingAssetsPath + "/SavedPresets/" + car + (wet ? "_WET.xml" : ".xml"), FileMode.Open))
        {
            var reader = new System.Xml.XmlTextReader(filestream);
            var savedSettings = serializer.Deserialize(reader) as VehicleSettings;
            savedSettings.Apply(configurator);
        }

        //refresh vehicle panel info
        adminCategories[4].UpdateValues();
    }

    void LoadMisc()
    {
        var settings = GetComponent<DriveAdminUISettings>();
        var driverCam = settings.driverCam;

        var serializer = new XmlSerializer(typeof(MiscSettings));

        using (var filestream = new FileStream(Application.streamingAssetsPath + "/SavedPresets/MiscSettings.xml", FileMode.Open))
        {
            var reader = new System.Xml.XmlTextReader(filestream);
            var savedSettings = serializer.Deserialize(reader) as MiscSettings;
            
//            timeOfDay.Components.Time.enabled = savedSettings.progressTime;
            AdminSettings.Instance.sunshafts = savedSettings.sunShafts;
            AdminSettings.Instance.fog = savedSettings.fog;
            AdminSettings.Instance.fov = DriverCamera.ScaleFov(savedSettings.fov);
            driverCam.SetFoV(DriverCamera.ScaleFov(savedSettings.fov));
            AdminSettings.Instance.camFarClip = savedSettings.cameraClip;
            settings.SetFarClip(savedSettings.cameraClip);
            AudioController.Instance.MusicVolume = savedSettings.musicVol;
            AudioController.Instance.FoleyVolume = savedSettings.foleyVol;
            AudioController.Instance.VehicleVolume = savedSettings.foleyVol;
            
        }

        //refresh all misc + audio panel info
        adminCategories[1].UpdateValues();
        adminCategories[3].UpdateValues();

    }

    void GenerateUI()
    {

        var settings = GetComponent<DriveAdminUISettings>();
        var driverCam = settings.driverCam;

        adminCategories = new AdminCategory[6];

        //Admin
        var admin = new AdminCategory("OBSTACLES", "WAYPOINTS", "TRAFFIC", "AUTOPLAY");
        admin.col0.Add(new ButtonItem()
        {
            label = "OBSTACLE ONE",
            callback = TrackController.Instance.TriggerObstacle1
        });
        admin.col0.Add(new ButtonItem()
        {
            label = "OBSTACLE TWO",
            callback = TrackController.Instance.TriggerObstacle2
        });
        if (TrackController.Instance.obstacles.Count > 2)
        {
            admin.col0.Add(new ButtonItem()
            {
                label = "OBSTACLE THREE",
                callback = TrackController.Instance.TriggerObstacle3
            });
        }
        if (TrackController.Instance.obstacles.Count > 3)
        {
            admin.col0.Add(new ButtonItem()
            {
                label = "OBSTACLE FOUR",
                callback = TrackController.Instance.TriggerObstacle4
            });

        }
        if (TrackController.Instance.obstacles.Count > 4)
        {
            admin.col0.Add(new ButtonItem()
            {
                label = "OBSTACLE FIVE",
                callback = TrackController.Instance.TriggerObstacle5
            });
        }

        admin.col0.Add(new ButtonItem()
        {
            label = "DESTROY OBSTACLES",
            callback = TrackController.Instance.KillObstacles
        });

        admin.col1.Add(new ButtonItem()
        {
            label = "WAYPOINT ONE",
            callback = TrackController.Instance.RepositionWaypoint1
        });
        admin.col1.Add(new ButtonItem()
        {
            label = "WAYPOINT TWO",
            callback = TrackController.Instance.RepositionWaypoint2
        });
        admin.col1.Add(new ButtonItem()
        {
            label = "WAYPOINT THREE",
            callback = TrackController.Instance.RepositionWaypoint3
        });
        admin.col1.Add(new ButtonItem()
        {
            label = "WAYPOINT FOUR",
            callback = TrackController.Instance.RepositionWaypoint4
        });
        admin.col1.Add(new SeperatorItem());
        if (TrackController.Instance.predefinedPaths != null && TrackController.Instance.predefinedPaths.Length > 0)
        {
            admin.col1.Add(new ButtonItem()
            {
                label = "PREDEFINED PATH ONE",
                callback = () => TrackController.Instance.StartPredefined(0)
            });
        }
        if (TrackController.Instance.predefinedPaths != null && TrackController.Instance.predefinedPaths.Length > 1)
        {
            admin.col1.Add(new ButtonItem()
            {
                label = "PREDEFINED PATH TWO",
                callback = () => TrackController.Instance.StartPredefined(1)
            });
        }
        if (TrackController.Instance.predefinedPaths != null && TrackController.Instance.predefinedPaths.Length > 2)
        {
            admin.col1.Add(new ButtonItem()
            {
                label = "PREDEFINED PATH THREE",
                callback = () => TrackController.Instance.StartPredefined(2)
            });
        }
        if (TrackController.Instance.predefinedPaths != null && TrackController.Instance.predefinedPaths.Length > 3)
        {
            admin.col1.Add(new ButtonItem()
            {
                label = "PREDEFINED PATH FOUR",
                callback = () => TrackController.Instance.StartPredefined(3)
            });
        }
        


        var trafEnable = new ToggleItem()
        {
            label = "Enable/Disable",
            value = false,
            get = () =>
            {
                if (settings.traffic == null)
                    return false;
                else
                    return settings.traffic.GetState();
            },
            set = (b) =>
            {
                if (settings.traffic != null)
                    settings.traffic.SetTraffic(b);
            }
        };

        admin.col2.Add(trafEnable);
        settings.updateEachFrame.Add(trafEnable);

        var auto = new ToggleItem()
        {
            label = "Autoplay",
            value = TrackController.Instance.IsInAutoPath(),
            get = () => TrackController.Instance.IsInAutoPath(),
            set = (b) =>
            {
                TrackController.Instance.AutoPath();
            }
        };
        admin.col3.Add(auto);
        settings.updateEachFrame.Add(auto);

        //Weather + cam
       var weatherCam = new AdminCategory("TIME", "WEATHER", "DISPLAY CONFIGURATION", "CAMERA CONFIGURATION");
        /*
        var tod = new SliderItemTime()
        {
            label = "Time of Day",
            min = 0f,
            max = 24f,
            value = AdminSettings.Instance.timeOfDay,
            get = () => timeOfDay.Cycle.Hour,
            set = (f) => settings.SetTimeOfDay(f)
        };
        weatherCam.col0.Add(tod);
        settings.updateEachFrame.Add(tod);
        weatherCam.col0.Add(new ToggleItem()
        {
            label = "Progess Time",
            value = AdminSettings.Instance.progressTime,
            get = () => timeOfDay.Components.Time.enabled,
            set = (b) => timeOfDay.Components.Time.enabled = b
        });
        var weather = new RadioItem()
        {
            label = "Weather",
            labels = new string[] {"Clear", "Storm", "Overcast"},
            selected = 0,
            get = () => AdminSettings.Instance.weather,
            set = (i) =>
            {
//                AdminSettings.Instance.weather = i;
//                var newWeather = AdminSettings.Instance.GetWeather();
//                settings.weather.currentWeather = newWeather;
//                settings.weather.ChangeWeather(newWeather);
            }
        };
        
        weatherCam.col1.Add(weather);
        settings.updateEachFrame.Add(weather);
        var wetRoad = new ToggleItem()
        {
            label = "Wet Road",
            value = false,
            get = () => WetRoads.instance.IsWet(),
            set = (t) => WetRoads.instance.ToggleWet(t)
        };
        weatherCam.col1.Add(wetRoad);
        settings.updateEachFrame.Add(wetRoad);
        weatherCam.col1.Add(new ToggleItem()
        {
            label = "Sun Shafts",
            value = false,
            get = () => AdminSettings.Instance.sunshafts,
            set = (b) =>
            {
                AdminSettings.Instance.sunshafts = b;
                TrackController.Instance.driverCam.center.GetComponent<TOD_SunShafts>().enabled = b;
            }
        });
        
        weatherCam.col1.Add(new ToggleItem()
        {
            label = "Enable Fog",
            value = false,
            get = () => AdminSettings.Instance.fog,
            set = (b) => AdminSettings.Instance.fog = b
        });
        */
        weatherCam.col2.Add(new ButtonItem()
        {
            label = "PARABOLIC PROJECTION",
            callback = () =>
            {
                AdminSettings.Instance.displayType = AdminScreen.DisplayType.PARABOLIC;
                settings.SetDisplay();
            }
        });
        weatherCam.col2.Add(new ButtonItem()
        {
            label = "24\" MONITOR CONFIGURATION",
            callback = () =>
            {
                AdminSettings.Instance.displayType = AdminScreen.DisplayType.TENTYFOURINCH;
                settings.SetDisplay();
            }
        });
        weatherCam.col2.Add(new ButtonItem()
        {
            label = "55\" MONITOR CONFIGURATION",
            callback = () =>
            {
                AdminSettings.Instance.displayType = AdminScreen.DisplayType.FIFTYFIVEINCH;
                settings.SetDisplay();
            }
        });
        weatherCam.col2.Add(new ButtonItem()
        {
            label = "FLAT PLANE",
            callback = () =>
            {
                AdminSettings.Instance.displayType = AdminScreen.DisplayType.FLAT;
                settings.SetDisplay();
            }
        });
        weatherCam.col2.Add(new SeperatorItem());
        weatherCam.col2.Add(new ButtonItem()
        {
            label = "LOAD DEFAULTS",
            callback = LoadMisc
        });
        weatherCam.col2.Add(new ButtonItem()
        {
            label = "SAVE CURRENT SETTINGS AS DEFAULT",
            callback = SaveMisc
        });
        weatherCam.col2.Add(new SeperatorItem());
        weatherCam.col2.Add(new ToggleItem()
        {
            label = "Display System Stats",
            value = false
        });

        weatherCam.col3.Add(new SliderItem()
        {
            label = "Field of View",
            min = 10f,
            max = 125f,
            value = AdminSettings.Instance.fov,
            get = () => AdminSettings.Instance.fov,
            set = (f) =>
            {
                driverCam.SetFoV(f);
                AdminSettings.Instance.fov = f;
            }
        });
        weatherCam.col3.Add(new SliderItem()
        {
            label = "Camera Clipping",
            min = 1f,
            max = 15000f,
            value = AdminSettings.Instance.camFarClip,
            get = () => AdminSettings.Instance.camFarClip,
            set = (f) => settings.SetFarClip(f)
        });

        //Audio
        var audio = new AdminCategory("AUDIO", "", "", "");
        audio.col0.Add(new AudioSliderItem()
        {
            label = "Music",
            min = 0f,
            max = 1f,
            value = AdminSettings.Instance.musicVol,
            get = () => AudioController.Instance.MusicVolume,
            set = (f) => AudioController.Instance.MusicVolume = f
        });
        audio.col0.Add(new AudioSliderItem()
        {
            label = "Foley",
            min = 0f,
            max = 1f,
            value = AdminSettings.Instance.foleyVol,
            get = () => AudioController.Instance.FoleyVolume,
            set = (f) => AudioController.Instance.FoleyVolume = f
        });
        audio.col0.Add(new AudioSliderItem()
        {
            label = "Vehicle",
            min = 0f,
            max = 1f,
            value = AdminSettings.Instance.vehicleVol,
            get = () => AudioController.Instance.VehicleVolume,
            set = (f) => AudioController.Instance.VehicleVolume = f
        });


        //Vehicle settings
        var vehicle = new AdminCategory("VEHICLE CONFIGURATION", "", "", "", new float[] {0f, -97f, -97f, -97f});
       /* vehicle.col0.Add(new SliderItem()
        {
            label = "Car Mass",
            min = 100f,
            max = 5000f,
            value = configurator.Mass,
            get = () => configurator.Mass,
            set = (f) => configurator.Mass = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Forward Grip",
            min = 100f,
            max = 3000f,
            value = configurator.ForwardGrip,
            get = () => configurator.ForwardGrip,
            set = (f) => configurator.ForwardGrip = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Forward GripRange",
            min = 0.5f,
            max = 20f,
            value = configurator.ForwardGripRange,
            get = () => configurator.ForwardGripRange,
            set = (f) => configurator.ForwardGripRange = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Forward Drift",
            min = 10f,
            max = 1500f,
            value = configurator.ForwardDrift,
            get = () => configurator.ForwardDrift,
            set = (f) => configurator.ForwardDrift = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Forward Drift Slope",
            min = 0f,
            max = 1f,
            value = configurator.ForwardDriftSlope,
            get = () => configurator.ForwardDriftSlope,
            set = (f) => configurator.ForwardDriftSlope = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Sideways Drift",
            min = 10f,
            max = 1000f,
            value = configurator.SidewaysDrift,
            get = () => configurator.SidewaysDrift,
            set = (f) => configurator.SidewaysDrift= f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Sideways Grip",
            min = 50f,
            max = 1500f,
            value = configurator.SidewaysGrip,
            get = () => configurator.SidewaysGrip,
            set = (f) => configurator.SidewaysGrip = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Sideways Grip Range",
            min = 0.5f,
            max = 20f,
            value = configurator.SidewaysGripRange,
            get = () => configurator.SidewaysGripRange,
            set = (f) => configurator.SidewaysGripRange = f
        });
        vehicle.col0.Add(new SliderItem()
        {
            label = "Brake Balance",
            min = 0f,
            max = 1f,
            value = configurator.BrakeBalance,
            get = () => configurator.BrakeBalance,
            set = (f) => configurator.BrakeBalance = f
        });


        vehicle.col1.Add(new SliderItem()
        {
            label = "Motor Max",
            min = 0.5f,
            max = 8f,
            value = configurator.MotorMax,
            get = () => configurator.MotorMax,
            set = (f) => configurator.MotorMax = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Brake Max",
            min = 0.5f,
            max = 8f,
            value = configurator.BrakeMax,
            get = () => configurator.BrakeMax,
            set = (f) => configurator.BrakeMax = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Motor Perf. Peak",
            min = 1f,
            max = 20f,
            value = configurator.MotorPerformancePeak,
            get = () => configurator.MotorPerformancePeak,
            set = (f) => configurator.MotorPerformancePeak = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Motor Force Factor",
            min = 0.1f,
            max = 5f,
            value = configurator.MotorForceFactor,
            get = () => configurator.MotorForceFactor,
            set = (f) => configurator.MotorForceFactor = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Motor Balance",
            min = 0f,
            max = 1f,
            value = configurator.MotorBalance,
            get = () => configurator.MotorBalance,
            set = (f) => configurator.MotorBalance = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Sdwys Drift Friction",
            min = 0.01f,
            max = 1f,
            value = configurator.SidewaysDriftFriction,
            get = () => configurator.SidewaysDriftFriction,
            set = (f) => configurator.SidewaysDriftFriction = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Static Friction Max",
            min = 100f,
            max = 4000f,
            value = configurator.StaticFrictionMax,
            get = () => configurator.StaticFrictionMax,
            set = (f) => configurator.StaticFrictionMax = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Max Steer Angle",
            min = 10f,
            max = 80f,
            value = configurator.MaxSteerAngle,
            get = () => configurator.MaxSteerAngle,
            set = (f) => configurator.MaxSteerAngle = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Front Sideways Grip",
            min = 0.2f,
            max = 3f,
            value = configurator.FrontSidewaysGrip,
            get = () => configurator.FrontSidewaysGrip,
            set = (f) => configurator.FrontSidewaysGrip = f
        });
        vehicle.col1.Add(new SliderItem()
        {
            label = "Sideways Drift Slope",
            min = 0f,
            max = 3f,
            value = configurator.SidewaysDriftSlope,
            get = () => configurator.SidewaysDriftSlope,
            set = (f) => configurator.SidewaysDriftSlope = f
        });


        vehicle.col2.Add(new SliderItem()
        {
            label = "Air Drag",
            min = 0.1f,
            max = 30f,
            value = configurator.AirDrag,
            get = () => configurator.AirDrag,
            set = (f) => configurator.AirDrag = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Friction Drag",
            min = 0.1f,
            max = 50f,
            value = configurator.FrictionDrag,
            get = () => configurator.FrictionDrag,
            set = (f) => configurator.FrictionDrag = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Rolling Friction Slip",
            min = 0.01f,
            max = 1f,
            value = configurator.RollingFrictionSlip,
            get = () => configurator.RollingFrictionSlip,
            set = (f) => configurator.RollingFrictionSlip = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Anti Roll Bias",
            min = 0.01f,
            max = 1f,
            value = configurator.AntiRollBias,
            get = () => configurator.AntiRollBias,
            set = (f) => configurator.AntiRollBias = f

        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Anti Roll Factor",
            min = 0.01f,
            max = 2f,
            value = configurator.AntiRollFactor,
            get = () => configurator.AntiRollFactor,
            set = (f) => configurator.AntiRollFactor = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Anti Roll Front",
            min = 100f,
            max = 20000f,
            value = configurator.AntiRollFront,
            get = () => configurator.AntiRollFront,
            set = (f) => configurator.AntiRollFront = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Anti Roll Back",
            min = 100f,
            max = 20000f,
            value = configurator.AntiRollBack,
            get = () => configurator.AntiRollBack,
            set = (f) => configurator.AntiRollBack = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Rear Sideways Grip",
            min = 0.2f,
            max = 3f,
            value = configurator.RearSidewaysGrip,
            get = () => configurator.RearSidewaysGrip,
            set = (f) => configurator.RearSidewaysGrip = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "Brake Force Factor",
            min = 0.1f,
            max = 5f,
            value = configurator.BrakeForceFactor,
            get = () => configurator.BrakeForceFactor,
            set = (f) => configurator.BrakeForceFactor = f
        });
        vehicle.col2.Add(new SliderItem()
        {
            label = "AutoSteer Level",
            min = 0f,
            max = 2f,
            value = configurator.AutoSteerLevel,
            get = () => configurator.AutoSteerLevel,
            set = (f) => configurator.AutoSteerLevel = f
        });



        vehicle.col3.Add(new SliderItem()
        {
            label = "Headlight Angle",
            min = -20f,
            max = 10f,
            value = configurator.HeadlightAngle,
            get = () => configurator.HeadlightAngle,
            set = (f) => configurator.HeadlightAngle = f
        });
        vehicle.col3.Add(new SliderItem()
        {
            label = "Force Feedback mult.",
            min = 0f,
            max = 4f,
            value = configurator.ForceFeedbackMultiplier,
            get = () => configurator.ForceFeedbackMultiplier,
            set = (f) => configurator.ForceFeedbackMultiplier = f
        });
        vehicle.col3.Add(new SliderItem()
        {
            label = "Force Feedback slip coeff.",
            min = 0.1f,
            max = 20f,
            value = configurator.ForceFeedbackSlipCoefficient,
            get = () => configurator.ForceFeedbackSlipCoefficient,
            set = (f) => configurator.ForceFeedbackSlipCoefficient = f
        });
        vehicle.col3.Add(new SliderItem()
        {
            label = "FFB Damper Max Speed",
            min = 0.1f,
            max = 50f,
            value = configurator.ForceFeedbackDamperMaxSpeed,
            get = () => configurator.ForceFeedbackDamperMaxSpeed,
            set = (f) => configurator.ForceFeedbackDamperMaxSpeed = f
        });
        vehicle.col3.Add(new SliderItem()
        {
            label = "FFB Damper Strength",
            min = 0f,
            max = 100f,
            value = configurator.ForceFeedbackDamperStrength,
            get = () => configurator.ForceFeedbackDamperStrength,
            set = (f) => configurator.ForceFeedbackDamperStrength = f
        });

        vehicle.col3.Add(new ToggleItem()
        {
            label = "Enable Headlights",
            value = false,
            get = () => configurator.EnableHeadlights,
            set = (b) => configurator.EnableHeadlights = b
        });
        */
        vehicle.col3.Add(new ButtonItem()
        {
            label = "LOAD DRY DEFAULTS",
            callback = () => LoadCar(false)
        });
        vehicle.col3.Add(new ButtonItem()
        {
            label = "SAVE CURRENT SETTINGS AS DRY DEFAULT",
            callback = () => SaveCar(false)
        });        
        vehicle.col3.Add(new ButtonItem()
        {
            label = "LOAD WET DEFAULTS",
            callback = () => LoadCar(true)
        });
        vehicle.col3.Add(new ButtonItem()
        {
            label = "SAVE CURRENT SETTINGS AS WET DEFAULT",
            callback = () => SaveCar(true)
        });

        var endScene = new AdminCategory("END SCENE", "", "", "");
        endScene.col0.Add(new ButtonItem()
        {
            label = "CONFIRM END SCENE",
            callback = TrackController.Instance.ConfirmEndScene
        });
        endScene.col0.Add(new ButtonItem()
        {
            label = "CANCEL END SCENE",
            callback = () => currentCategory = 0
        });

        var map = new AdminCategory("MAP", "", "", "");
        var mapItem = new MapItem()
        {
            environment =  AppController.Instance.currentSessionSettings.selectedEnvironment == Environment.NONE  ? settings.mapEnvironment : AppController.Instance.currentSessionSettings.selectedEnvironment
        };
        map.col0.Add(mapItem);
        settings.updateEachFrame.Add(mapItem);
       // var infractions = new InfractionsItem();
       // map.col0.Add(infractions);
       // settings.updateEachFrame.Add(infractions);


        adminCategories[0] = admin;
        adminCategories[1] = weatherCam;
        adminCategories[2] = map;
        adminCategories[3] = audio;
        adminCategories[4] = vehicle;
        adminCategories[5] = endScene;


      //  LoadCar(false);   --old configurator - disable for now
        LoadMisc();
    }
}
