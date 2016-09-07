/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

public class DrivingInfraction
{
    public int id;
    public string type;
    public System.DateTime systemTime;
    public float sessionTime;
    public float speed;
}

public class InfractionController : Singleton<InfractionController>
{
    public string lastScene = "";

    private List<DrivingInfraction> infractions;

    public InfractionController()
    {
        infractions = new List<DrivingInfraction>();
    }

    public void Clear()
    {
        infractions.Clear();
    }

    public void SetLastScene(Environment env)
    {
        switch (env)
        {
            case Environment.COASTAL:
                lastScene = "Coastal";
                break;
            case Environment.SCENIC:
                lastScene = "Scenic";
                break;
            case Environment.URBAN:
                lastScene = "Urban";
                break;
        }
    }

    public bool AddInfraction(DrivingInfraction infraction)
    {
        if (infractions.Count < 30)
        {
            infractions.Add(infraction);
            return true;
        }
        return false;
    }

    public List<DrivingInfraction> GetInfractions()
    {
        return infractions;
    }

    public void WriteLog()
    {
        string filePath = Application.dataPath + "/infractionLogs/" + System.DateTime.Now.ToString("dd-MMM-yyyy_HH-mm-ss") + ".infractionlog";

        //editor writes to one level above to avoid storing in source control
        if(Application.isEditor)
            filePath = Application.dataPath + "/../infractionLogs/" + System.DateTime.Now.ToString("dd-MMM-yyyy_HH-mm-ss") + ".infractionlog";

        //create the log dir if it doesn't already exist
        var fi = new System.IO.FileInfo(filePath);
        fi.Directory.Create();

        string logText = "";
        foreach (var i in infractions)
        {
            logText += i.id + "," + i.type + "," + i.systemTime.ToLongTimeString() + "," + i.sessionTime + "," + i.speed + "\n";
        }
        System.IO.File.WriteAllText(filePath, logText);
        
    }

    public static string GetFullInfractionType(DrivingInfraction d) {
        string infractionText = "";
        switch (d.type)
        {
            case "LANE":
                infractionText = "Lane Infraction";
                break;
            case "ENV":
                infractionText = "Environment Collision";
                break;
            case "TRAF":
                infractionText = "Vehicle Collision";
                break;
            case "OBS":
                infractionText = "Obstacle Collision";
                break;
            case "STOP":
                infractionText = "Stop Sign Infraction";
                break;
            case "LIGHT":
                infractionText = "Traffic Light Infraction";
                break;
            default:
                infractionText = "Infraction";
                break;
        }
        return infractionText;
    }
}

public class ScreenshotSaveTaskInfo
{
    public byte[] imageBytes;
    public string saveLocation;
}

public class TrackController : UnitySingleton<TrackController> {

    public GameObject car;
    public CarAutoPath autoPath;

    public GameObject defaultCar;
    public GameObject carSpawnTarget;

    public DriveSceneAdmin adminScreen;

    public DriverCamera driverCam;

    public List<GameObject> obstacles;

    public GameObject joinerPaths;

    public float obstacleSpawnDist = 40;

    public GameObject[] repositionWaypoints;

    public bool hasTraffic = false;

    public float obstacleDist = 30f;

    public TrafSpawner sftraffic;
    public TrafSpawnerPCH pchtraffic;

    public GameObject[] predefinedPaths;

    private DriveAdminUI driveAdmin;

    private GameObject slipMeter;


    public Texture2D infractionAlert;
    public AudioClip infractionsAlertSound;
    public GUIStyle infractionsAlertText;

    public List<DrivingInfraction> GetInfractions()
    {
        return InfractionController.Instance.GetInfractions();
    }

    private float infractionsAlertTimer = -500f;

    private const float infractionsAlertTime = 4f;

    private RenderTexture screenshotTarget;
    private string infractionText = "";

    private const int NUM_SELECT_LIGHTMAPS = 13;

    [HideInInspector]
    public float mapXPos = 0f;
    
    [HideInInspector]
    public float mapZPos = 0f;

    public void AddInfraction(DrivingInfraction d)
    {

        infractionsAlertTimer = Time.time;
        if(infractionsAlertSound != null)
            AudioController.Instance.PlayClip(infractionsAlertSound);

        switch (d.type)
        {
            case "LANE":
                infractionText = "Lane Infraction";
                break;
            case "ENV":
                infractionText = "Environment Collision";
                break;
            case "TRAF":
                infractionText = "Vehicle Collision";
                break;
            case "OBS":
                infractionText = "Obstacle Collision";
                break;
            case "STOP":
                infractionText = "Stop Sign Infraction";
                break;
            case "LIGHT":
                infractionText = "Traffic Light Infraction";
                break;
            default:
                infractionText = "Infraction";
                break;
        }

        if (InfractionController.Instance.AddInfraction(d))
        {
            StartCoroutine(SaveScreenshot(d.id.ToString()));
        }

    }

 
    private IEnumerator SaveScreenshot(string id)
    {
        yield return new WaitForEndOfFrame();
      
        //Graphics.Blit(null, screenshotTarget);
        //Camera cam = driverCam.camera;
        //cam.targetTexture = screenshotTarget;
        //cam.Render();
        //cam.targetTexture = null;
        driverCam.center.GetComponent<CopyToScreenRT>().render = true;
        yield return null;
        yield return new WaitForEndOfFrame();

        Texture2D screenshot = new Texture2D(1248, 270, TextureFormat.RGB24, false);
        RenderTexture.active = screenshotTarget;
        screenshot.ReadPixels(new Rect(0, 0, 1248, 270), 0, 0, false);
        RenderTexture.active = null;
        yield return null;

       // screenshot.ReadPixels(new Rect(Mathf.RoundToInt((Screen.width - width) / 2), 0, width, Screen.height), 0, 0, false);
        screenshot.Apply();
       // Graphics.Blit(Camera.main.targetTexture, screenshotTarget);
        
       var bytes = screenshot.EncodeToJPG();
       // screenshotTarget.colorBuffer
        try
        {
           var path = Application.temporaryCachePath + System.IO.Path.DirectorySeparatorChar + id + ".jpg";
           Debug.Log(path);
           ThreadPool.QueueUserWorkItem(new WaitCallback(SaveImageThreaded), new ScreenshotSaveTaskInfo()
           {
               imageBytes = bytes,
               saveLocation = path
           });
        }
        catch (System.Exception e)
        {
            Debug.Log("Error writing screensot: " + id + ".jpg\n" + e.Message);
        }
        finally
        {
            Destroy(screenshot);
        }
    }

    static void SaveImageThreaded(System.Object screenshot)
    {
        ScreenshotSaveTaskInfo info = (ScreenshotSaveTaskInfo)screenshot;
        System.IO.File.WriteAllBytes(info.saveLocation, info.imageBytes);
    }

    IEnumerator WatchForExit(GameObject origin)
    {
        yield return new WaitForFixedUpdate();
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;
        yield return new WaitForFixedUpdate();
        yield return null;
        Physics.IgnoreLayerCollision(LayerMask.NameToLayer("PlayerCar"), LayerMask.NameToLayer("Default"), false);
        while(Vector3.Distance(car.transform.position, origin.transform.position) < 50f) {
            yield return null;
        }
        car.transform.parent = null;
        driverCam.transform.parent = null;
        Destroy(origin);
    }

    private GUIStyle speedDebugStyle;

    public bool disabled = false;

    public void Start()
    {
        screenshotTarget = new RenderTexture(1248, 270, 0, RenderTextureFormat.ARGB32);
        screenshotTarget.Create();

        InfractionController.Instance.Clear();

        speedDebugStyle = new GUIStyle();
        speedDebugStyle.fontSize = 35;
        speedDebugStyle.normal.textColor = Color.white;

        if (disabled)
            return;
        
        //instantiate correct car (from AppController if set, otherwise default)
        if(GameObject.Find("SelectSceneOrigin") != null)
        {
            //kill scene cam since and use the select scene one
            var origin = GameObject.Find("SelectSceneOrigin");
            driverCam.gameObject.SetActive(false);
            Destroy(driverCam.gameObject);

            driverCam = GameObject.Find("SelectSceneOrigin/DriverCamera").GetComponent<DriverCamera>();
            Destroy(driverCam.GetComponent<MoveToTarget>());

            car = origin.GetComponentInChildren<CarSelectController>().GetCurrentCar();


            var pathController = car.GetComponent<VehiclePathController>();
            pathController.enabled = false;
            pathController.Clear();

            var cont = car.GetComponent<VehicleController>();
            cont.enabled = true;
            car.GetComponent<VehicleInputController>().enabled = true;

            car.transform.localPosition = MasterSelectController.carLoadPosition;
            car.transform.localRotation = MasterSelectController.carLoadRotation;
            car.GetComponent<Rigidbody>().velocity = Vector3.zero;
            car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;


            //car.GetComponent<CarCameras>().enabled = true;

            //destroy the non-selected car models
            foreach(Transform c in GameObject.Find("SelectSceneOrigin/Available Car Models").transform)
            {
                if(c.gameObject != car)
                    Destroy(c.gameObject);
            }

           // var todcam = driverCam.center.AddComponent<TOD_Camera>();
           // todcam.DomeScaleToFarClip = true;
           // todcam.DomeScaleFactor = 0.95f;

            //init headlights
            var headlights = car.GetComponent<CarHeadlights>();
            if (headlights == null)
            {
                Debug.LogWarning("No CarHeadlights script attached to selected car");
            }
            else
            {
                headlights.enabled = true;
            }
                        
            StartCoroutine(WatchForExit(origin));

            //assign sunshafts values
           // driverCam.center.GetComponent<TOD_SunShafts>().sky = GameObject.Find("Sky Dome").GetComponent<TOD_Sky>();


            //reload the select scene lightmaps
            LightmapData[] data = new LightmapData[NUM_SELECT_LIGHTMAPS];

            for (int i = 0; i < NUM_SELECT_LIGHTMAPS; i++)
            {
                data[i] = new LightmapData()
                {
                    lightmapFar = Resources.Load<Texture2D>("CarSelect_Lightmaps/LightmapFar-" + i)
                };
            }
            LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
            LightmapSettings.lightmaps = data;


        }
        else
        {
            car = GameObject.Instantiate(defaultCar, carSpawnTarget.transform.position, carSpawnTarget.transform.rotation) as GameObject;
            car.GetComponent<VehicleInputController>().enabled = true;
        }

        //init FFB
        if (car.GetComponent<ForceFeedback>() != null)
            car.GetComponent<ForceFeedback>().enabled = true;

        //var settings = car.GetComponent<CarCameras>();
        //driverCam.Init(settings);
        var cameraPos = car.GetComponent<VehicleController>().cameraPosition;
        driverCam.Init(cameraPos);
        driverCam.ViewInside();


        //init admin screen
        var admin = GameObject.Find("DriveAdmin");

        var adminUISettings = admin.GetComponent<DriveAdminUISettings>();
        adminUISettings.driverCam = driverCam;
        adminUISettings.traffic = sftraffic;
        if (adminUISettings.traffic == null)
            adminUISettings.traffic = pchtraffic;

 //       adminUISettings.weather = GameObject.FindObjectOfType<WeatherSystem>();
        adminUISettings.Init();

        var adminUI = admin.GetComponent<DriveAdminUI>();
        adminUI.Init();

        driveAdmin = adminUI;

        //Add configurator and init
        var configurator = car.AddComponent<VehicleConfigurator>();
        configurator.Init(car);
        configurator.LoadSettings();

        //Add infraction recorder
        car.AddComponent<InfractionRecorder>();

        //init new admin screen
        var newAdmin = GameObject.Find("AdminScreen");
        newAdmin.GetComponent<NewAdminScreen>().Init();

        //add CANDataCollector
        car.AddComponent<CANDataCollector>();

        //init SlipMeter
        //slipMeter = Resources.Load<GameObject>("SlipMeter");
        //GameObject.Instantiate(slipMeter);

        //init terain wetness watcher
        GameObject TerrainWetness = Resources.Load<GameObject>("TerrainWetness");
        GameObject.Instantiate(TerrainWetness);

        AppController.Instance.UserInput.Init();

        //add fog for SF
        if (AppController.Instance.currentSessionSettings.selectedEnvironment == Environment.URBAN)
        {
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
        }
        else
        {
            RenderSettings.fog = false;
        }
    
        //add stoopid hack to fix broken screenshot framebuffer with posteffects
        var rt = driverCam.center.AddComponent<CopyToScreenRT>();
        rt.target = screenshotTarget;

        //enable vehicle audio
        if (car.GetComponent<VehicleAudioController>())
        {
            car.GetComponent<VehicleAudioController>().enabled = true;
        }
  

        //short delay before enabling the ipad screen
        StartCoroutine(Functional.DoAfter(1f, () => RemoteAdminController.Instance.SendMessage(RemoteAdminController.SendMessageType.START_DRIVE_SCENE)));
    }


    private bool inPredefinedPath = false;

    private SplinePath currentPath;

    public int predefinedArrowsToShow = 10;
    public float predefinedArrowSeperation = 4f;

    public GameObject Arrow;

    public void TriggerPredefined(int p)
    {
        if (predefinedPaths != null && predefinedPaths.Length > p)
        {
            StartPredefined(p);
        }
    }
    

    private class PlacedArrow
    {
        public GameObject go;
        public float percent;
    }

    private List<PlacedArrow> placedArrows;
    private PlacedArrow finalArrow;

    private float arrowStep;

    public void StartPredefined(int p)
    {
        if (inPredefinedPath)
        {
            foreach (var a in placedArrows)
            {
                Destroy(a.go);
            }
            placedArrows.Clear();
            inPredefinedPath = false;
            return;
        }

        RepositionToWaypoint(p);

        placedArrows = new List<PlacedArrow>();
        inPredefinedPath = true;
        currentPath = repositionWaypoints[p].GetComponent<SplinePath>();

        arrowStep = predefinedArrowSeperation / currentPath.TotalDistance;
        float currentSpot = 0f;

        for (int i = 0; i < predefinedArrowsToShow; i++)
        {
            Vector3 position = currentPath.PointAtRealPercent(currentSpot);
            GameObject go = GameObject.Instantiate(Arrow, position,
                Quaternion.LookRotation(currentPath.PointAtRealPercent(currentSpot + arrowStep / 3f) - position)) as GameObject;
            var arrow = new PlacedArrow()
            {
                go = go,
                percent = currentSpot
            };

            RaycastHit hit;
            Physics.Raycast(go.transform.position + Vector3.up * 0.5f, -go.transform.up, out hit, 100f, ~(1 << LayerMask.NameToLayer("obstacle") | 1 << LayerMask.NameToLayer("Traffic") | 1 << LayerMask.NameToLayer("PlayerCar")));
            go.transform.position = new Vector3(go.transform.position.x, hit.point.y, go.transform.position.z);


            placedArrows.Add(arrow);
            currentSpot += arrowStep;
            finalArrow = arrow;
        }

    }

    public void Update()
    {
        if (inPredefinedPath)
        {
            
            if (Vector3.Distance(car.transform.position, placedArrows[0].go.transform.position) > Vector3.Distance(car.transform.position, placedArrows[2].go.transform.position))
            {
                Vector3 testSpot = currentPath.PointAtRealPercent(finalArrow.percent + arrowStep);
                PlacedArrow arrow = placedArrows[0];
                placedArrows.RemoveAt(0);
                arrow.go.transform.position = testSpot;
                arrow.percent = finalArrow.percent + arrowStep;
                arrow.go.transform.rotation = Quaternion.LookRotation(currentPath.PointAtRealPercent(finalArrow.percent + arrowStep + arrowStep / 3f) - testSpot);

                RaycastHit hit;
                Physics.Raycast(arrow.go.transform.position + Vector3.up * 0.5f, -arrow.go.transform.up, out hit, 100f, ~(1 << LayerMask.NameToLayer("obstacle") | 1 << LayerMask.NameToLayer("Traffic") | 1 << LayerMask.NameToLayer("PlayerCar")));
                arrow.go.transform.position = new Vector3(arrow.go.transform.position.x, hit.point.y, arrow.go.transform.position.z);

                finalArrow = arrow;
                placedArrows.Add(arrow);
            }



        }

        if(adminScreen != null)
            adminScreen.carPosition = car.transform.position;


    }



    public void OnEnable() {
        AppController.Instance.AdminInput.ChangeCamView += ChangeCamView;
        AppController.Instance.AdminInput.ControlNumeric1 += TriggerObstacle1;
        AppController.Instance.AdminInput.ControlNumeric2 += TriggerObstacle2;
        AppController.Instance.AdminInput.ControlNumeric3 += TriggerObstacle3;
        AppController.Instance.AdminInput.ControlNumeric4 += TriggerObstacle4;
        AppController.Instance.AdminInput.ControlNumeric5 += TriggerObstacle5;
        AppController.Instance.AdminInput.ControlNumeric0 += KillObstacles;
        AppController.Instance.AdminInput.EndScene += EndScene;
        AppController.Instance.AdminInput.ConfirmPopup += ConfirmEndScene;
        AppController.Instance.AdminInput.CancelPopup += CancelEndScene;
        AppController.Instance.AdminInput.Reposition += RepositionUser;
        AppController.Instance.AdminInput.ToggleStats += ToggleStats;
        AppController.Instance.AdminInput.ToggleConsole += ToggleConsole;
        AppController.Instance.AdminInput.ShiftNumeric1 += RepositionWaypoint1;
        AppController.Instance.AdminInput.ShiftNumeric2 += RepositionWaypoint2;
        AppController.Instance.AdminInput.ShiftNumeric3 += RepositionWaypoint3;
        AppController.Instance.AdminInput.ShiftNumeric4 += RepositionWaypoint4;
        AppController.Instance.AdminInput.ShiftNumeric5 += RepositionWaypoint5; 
        AppController.Instance.AdminInput.ShiftNumeric6 += RepositionWaypoint6;
        AppController.Instance.AdminInput.ShiftNumeric7 += RepositionWaypoint7;
        AppController.Instance.AdminInput.ShiftNumeric8 += RepositionWaypoint8;
        AppController.Instance.AdminInput.ShiftNumeric9 += RepositionWaypoint9;
        AppController.Instance.AdminInput.SelectEnvironment1 += AutoPath;


        //remote actions
        RemoteAdminController.OnTriggerObstacle += TriggerObstacle;
        RemoteAdminController.OnTriggerWaypoint += RepositionToWaypoint;
        RemoteAdminController.OnTriggerPredefinedPath += TriggerPredefined;
        RemoteAdminController.OnSetAutoPlay += SetAutoPath;
        RemoteAdminController.OnEndScene += ConfirmEndScene;
        RemoteAdminController.OnRepositionVehicle += RepositionUser;
   
    }

    public void OnDisable()
    {
        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
        {
            AppController.Instance.AdminInput.ChangeCamView -= ChangeCamView;
            AppController.Instance.AdminInput.ControlNumeric1 -= TriggerObstacle1;
            AppController.Instance.AdminInput.ControlNumeric2 -= TriggerObstacle2;
            AppController.Instance.AdminInput.ControlNumeric3 -= TriggerObstacle3;
            AppController.Instance.AdminInput.ControlNumeric4 -= TriggerObstacle4;
            AppController.Instance.AdminInput.ControlNumeric5 -= TriggerObstacle5;
            AppController.Instance.AdminInput.ControlNumeric0 -= KillObstacles;
            AppController.Instance.AdminInput.EndScene -= EndScene;
            AppController.Instance.AdminInput.ConfirmPopup -= ConfirmEndScene;
            AppController.Instance.AdminInput.CancelPopup -= CancelEndScene;
            AppController.Instance.AdminInput.Reposition -= RepositionUser;
            AppController.Instance.AdminInput.ToggleStats -= ToggleStats;
            AppController.Instance.AdminInput.ToggleConsole -= ToggleConsole;
            AppController.Instance.AdminInput.ShiftNumeric1 -= RepositionWaypoint1;
            AppController.Instance.AdminInput.ShiftNumeric2 -= RepositionWaypoint2;
            AppController.Instance.AdminInput.ShiftNumeric3 -= RepositionWaypoint3;
            AppController.Instance.AdminInput.ShiftNumeric4 -= RepositionWaypoint4;
            AppController.Instance.AdminInput.ShiftNumeric5 -= RepositionWaypoint5;
            AppController.Instance.AdminInput.ShiftNumeric6 -= RepositionWaypoint6;
            AppController.Instance.AdminInput.ShiftNumeric7 -= RepositionWaypoint7;
            AppController.Instance.AdminInput.ShiftNumeric8 -= RepositionWaypoint8;
            AppController.Instance.AdminInput.ShiftNumeric9 -= RepositionWaypoint9;
            AppController.Instance.AdminInput.SelectEnvironment1 -= AutoPath;
        }

        RemoteAdminController.OnTriggerObstacle -= TriggerObstacle;
        RemoteAdminController.OnTriggerWaypoint -= RepositionToWaypoint;
        RemoteAdminController.OnTriggerPredefinedPath -= TriggerPredefined;
        RemoteAdminController.OnSetAutoPlay -= SetAutoPath;
        RemoteAdminController.OnEndScene -= ConfirmEndScene;
        RemoteAdminController.OnRepositionVehicle -= RepositionUser;
    }


    protected override void OnDestroy()
    {
        base.OnDestroy();
        screenshotTarget.Release();
        Resources.UnloadUnusedAssets();
    }

    #region end scene

    public bool isShowingDebugDialog = false;

    public void EndScene()
    {
        //adminScreen.isShowingEndScene = true;
        driveAdmin.EndScene();
    }

    public void ConfirmEndScene()
    {
        InfractionController.Instance.WriteLog();
        DestroyOnLoadManager.Instance.Destroy();
        AppController.Instance.UserInput.CleanUp();
        AppController.Instance.LoadCarSelect();

        RemoteAdminController.Instance.SendMessage(RemoteAdminController.SendMessageType.END_DRIVE_SCENE, InfractionController.Instance.GetInfractions());
        
    }

    public void CancelEndScene()
    {
        adminScreen.isShowingEndScene = false;
    }

    public void ToggleStats()
    {
        isShowingDebugDialog = !isShowingDebugDialog;
    }

    public void OnGUI()
    {
        Matrix4x4 oldMatrix = GUI.matrix;

        
        if (AdminSettings.Instance.displayType == AdminScreen.DisplayType.PARABOLIC)
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.height / 1080f, Screen.height / 1080f, 1f));
        }
        else
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.height / 1080f, Screen.height / 1080f, 1f));
        }
        


        if (Time.time - infractionsAlertTimer < infractionsAlertTime + 1f)
        {
            if ((Time.time - infractionsAlertTimer) < infractionsAlertTime)
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01((Time.time - infractionsAlertTimer) * 2));
            else 
                GUI.color = new Color(1f, 1f, 1f, 1f - Mathf.Clamp01((Time.time - infractionsAlertTimer - infractionsAlertTime) * 2));

            GUI.DrawTexture(new Rect(40f, 912f, 128f, 128f), infractionAlert);

            GUI.Label(new Rect(168f, 1006f, 300f, 40f), infractionText, infractionsAlertText);

            GUI.color = Color.white;
        }

        if(car != null)
            GUI.Label(new Rect(2880f, 1000f, 400f, 50f), (car.GetComponent<Rigidbody>().velocity.magnitude * NetworkedCarConsole.MPSTOMPH).ToString("F2") + " MPH", speedDebugStyle);

        if(isShowingDebugDialog)
        {
            GUI.Box(new Rect(40, 90, 300, 150), "Car Stats");
            GUI.Label(new Rect(50, 115, 200, 50), "Steering Angle: " + AppController.Instance.UserInput.GetSteerInput());
            GUI.Label(new Rect(50, 155, 200, 50), "Car Speed: " + car.GetComponent<Rigidbody>().velocity.magnitude);
            GUI.Label(new Rect(50, 205, 200, 50), "Car Position: " + car.transform.position.ToString()); 
        }

        GUI.matrix = oldMatrix;
    }

    #endregion

    /*
    public RAPathNode GetCurrentRoadPathNode()
    {

        RaycastHit[] hits = Physics.RaycastAll(car.transform.position, Vector3.down, 10f, ~(1 << LayerMask.NameToLayer("PlayerCar")));
        
        for(int i = 0; i < hits.Length; i++) {
            RaycastHit hit = hits[i];

            
            
            var t = hit.collider.transform;
            //bubble up looking for an RARoad
            while(t.transform.parent != null)
            {
                
                t = t.transform.parent;
                Debug.Log("searching " + t.name);
                if(t.GetComponent<RARoad>() != null)
                {
                    var road = t.GetComponent<RARoad>();
                    var l1 = t.Find("lane1");
                    var l2 = t.Find("lane2");

                    Vector3 carPos = car.transform.position;

                    Transform min = null;
                    float minDist = 999999;
                    foreach(Transform node in l1)
                    {
                        var d = Vector3.Distance(node.position, carPos);
                        if( d < minDist)
                        {
                            minDist = d;
                            min = node;
                            if(minDist <= 10f)
                                break;
                        }
                    }

                    Transform min2 = null;
                    minDist = 999999;
                    foreach(Transform node in l2)
                    {
                        var d = Vector3.Distance(node.position, carPos);
                        if(d < minDist)
                        {
                            minDist = d;
                            min2 = node;
                            if(minDist <= 10f)
                                break;
                        }
                    }

                    var tan1 = min.GetComponent<RAPathNode>().next.transform.position - min.transform.position;
                    var tan2 = min2.GetComponent<RAPathNode>().next.transform.position - min.transform.position;

                    if(Vector3.Angle(car.transform.forward, tan1) < Vector3.Angle(car.transform.forward, tan2))
                    {
                        return min.GetComponent<RAPathNode>();
                    }
                    else
                    {
                        return min2.GetComponent<RAPathNode>();
                    }

                }
            }

        }
        Debug.Log("Couldn't find road collider via raycast");
        return null;
    }
    */

    public RoadPathNode GetCurrentRoadPathNode()
    {
        
        Transform r = GameObject.Find("roads").transform;

        RoadPathNode min = null;
        float minDist = 999999;

        RoadPathNode min2 = null;
        float minDist2 = 999999;
        foreach(Transform t in r) {
            if(t.GetComponent<RoadPath>() != null)
            {
                var road = t.GetComponent<RoadPath>();

                Vector3 carPos = car.transform.position;
              
                foreach(var node in road.pathNodesLane1)
                {
                    var d = Vector3.Distance(node.position, carPos);
                    if(d < minDist)
                    {
                        minDist = d;
                        min = node;
                    }
                }


                foreach(var node in road.pathNodesLane2)
                {
                    var d = Vector3.Distance(node.position, carPos);
                    if(d < minDist2)
                    {
                        minDist2 = d;
                        min2 = node;
                    }
                }


                

            }
            

        }

        if(Vector3.Angle(car.transform.forward, min.tangent) < Vector3.Angle(car.transform.forward, min2.tangent))
        {
            return min;
        }
        else
        {
            return min2;

        }
    }

    private float GetLinePointDist(Vector3 p, Vector3 A, Vector3 B)
    {
        Vector3 ap = p - A;
        Vector3 ab = (B - A).normalized;

        return Vector3.Distance(A + (ab * Vector3.Dot(ap, ab)), p);

    }

    private float GetLinePointDistFromTan(Vector3 p, Vector3 A, Vector3 ab)
    {
        Vector3 ap = p - A;

        return Vector3.Distance(A + (ab * Vector3.Dot(ap, ab)), p);

    }

    private float GetLineSegmentPointDist(Vector2 v, Vector2 w, Vector2 p)
    {
        // Return minimum distance between line segment vw and point p
        float l2 = (w - v).sqrMagnitude;  // i.e. |w-v|^2 -  avoid a sqrt
        if (l2 == 0.0) return Vector2.Distance(p, v);   // v == w case
        // Consider the line extending the segment, parameterized as v + t (w - v).
        // We find projection of point p onto the line. 
        // It falls where t = [(p-v) . (w-v)] / |w-v|^2
        float t = Vector2.Dot(p - v, w - v) / l2;
        
        if (t < 0.0) return Vector2.Distance(p, v);       // Beyond the 'v' end of the segment
        else if (t > 1.0) return Vector2.Distance(p, w);  // Beyond the 'w' end of the segment
        Vector2 projection = v + t * (w - v);  // Projection falls on the segment
        return Vector2.Distance(p, projection);
    }

    public Texture2D lanesTexture;

    //returnes (-1, -1) if out of bounds
    private int[] GetIndices(Vector3 p)
    {
        Vector3 bl = new Vector3(721f, 10f, -22f);
        Vector3 tr = new Vector3(2419, 10f, 1774f);
        int[] indices = new int[2];
        indices[0] = Mathf.RoundToInt(((p.x - bl.x) / (tr.x - bl.x)) * 4096);
        indices[1] = Mathf.RoundToInt(((p.z - bl.z) / (tr.z - bl.z)) * 4096);

        if (indices[0] < 0 || indices[0] > 4095 || indices[1] < 0 || indices[1] > 4095)
        {
            indices[0] = -1;
            indices[1] = -1;
        }
        
        return indices;
    }

    private float LoopingDiff(float a, float b, float max)
    {
        return Mathf.Min(Mathf.Abs(a - b), Mathf.Abs((a + max) - b), Mathf.Abs(a - (b + max)));
    }

    public enum OutsideLane {INSIDE, OUTSIDE, UNSURE};

    public OutsideLane CheckForOutsideLaneSF()
    {
        int[] carIndices = GetIndices(car.transform.position);

        if (carIndices[0] < 0)
            return OutsideLane.UNSURE;

        float c = lanesTexture.GetPixel(carIndices[0], carIndices[1]).a;

        if (c < 0.08f)
            return OutsideLane.UNSURE;
        else if (c < 0.2f)
            return OutsideLane.OUTSIDE;

        c -= 0.2f;

        float carRot = Quaternion.LookRotation(car.transform.forward).eulerAngles.y;
        if(carRot < 0f)
            carRot += 360f;
        if(carRot > 360f)
            carRot -= 360f;
        
        carRot = carRot/ 360f * 0.8f;

        if (LoopingDiff(c, carRot, 0.8f) > (100f / 360f) * 0.8f)
            return OutsideLane.OUTSIDE;

        return OutsideLane.INSIDE;


    }

    public bool CheckForOutsideLane(Transform roadPavement)
    {
        var t = roadPavement;
        RoadPathNodeInfractions min = null;
        float minDist = 999999;
        RoadPathNodeInfractions min2 = null;
        float minDist2 = 999999;

        if (t.GetComponent<RoadPath>() == null)
        {
            return false;
        }

        var road = t.GetComponent<RoadPath>();

        if (road.infractionPathNodesLane1 == null || road.infractionPathNodesLane1.Length == 0 ||
            road.infractionPathNodesLane2 == null || road.infractionPathNodesLane2.Length == 0)
        {
            return false;
        }

        Vector3 carPos = car.transform.position;

        foreach (RoadPathNodeInfractions node in road.infractionPathNodesLane1)
        {
            var d = Vector3.Distance(node.position, carPos);
            if (d < minDist)
            {
                minDist = d;
                min = node;
            }
        }


        foreach (RoadPathNodeInfractions node in road.infractionPathNodesLane2)
        {
            var d = Vector3.Distance(node.position, carPos);
            if (d < minDist2)
            {
                minDist2 = d;
                min2 = node;
            }
        }

        float d1 = 0f;
        float d2 = 0f;

        d1 = GetLinePointDistFromTan(carPos, min.position, min.tangent);
        d2 = GetLinePointDistFromTan(carPos, min2.position, min2.tangent);

        Vector3 tan = min.tangent;
        if (d2 < d1)
        {
            tan = min2.tangent;
            if (!min2.doubleYellow)
                return false;
        }
        else
        {
            if (!min.doubleYellow)
                return false;
        }

        if (Vector3.Angle(car.transform.forward, tan) > 100)
        {
            return true;
        }

        return false;
    }


    public Vector3[] GetObstacleSpawnPosition(RoadPathNode carNode, float distance)
    {
  
        if(carNode.obstacleSpawnPosition == Vector3.zero)
            return new Vector3[0];        

        return new Vector3[] { carNode.obstacleSpawnPosition + (Quaternion.Euler(0, 90f, 0) * carNode.obstacleSpawnTangent).normalized * distance, carNode.obstacleSpawnPosition };

    }

    public Vector3[] GetCarResetPosition(RoadPathNode carNode)
    {
        return new Vector3[] { carNode.position, carNode.tangent };
    }

    private void TriggerObstacle(int idx)
    {
        //-1 means destroy from remote admin
        if(idx < 0)
        {
            KillObstacles();
            return;
        }

        if(obstacles.Count > idx)
        {
            if(hasTraffic)
            {
                Vector3[] spot;
                if(obstacles[idx].GetComponent<BaseObstacle>().SpawnAnywhere())
                {
                    spot = GetObstacleSpawnPositionTraffic();
                } else {
                    spot = GetNextIntersectionTaffic();
                }

                if(spot[0] != Vector3.zero)
                {
                    GameObject obs = GameObject.Instantiate(obstacles[idx], Vector3.zero, Quaternion.identity) as GameObject;
                    BaseObstacle ob = obs.GetComponent<BaseObstacle>();



                    obs.transform.position = spot[0];
                    obs.transform.LookAt(spot[1]);

                    if (ob.spawnInLane)
                    {
                        ob.transform.position = spot[1]; 
                        RaycastHit hit;
                        Physics.Raycast(ob.transform.position + Vector3.up * 5, -transform.up, out hit, 100f, ~(1 << LayerMask.NameToLayer("obstacle") | 1 << LayerMask.NameToLayer("Traffic")));
                        ob.transform.position = new Vector3(ob.transform.position.x, hit.point.y, ob.transform.position.z);

                    }
                    else
                        obs.transform.position = obs.transform.position - obs.transform.forward * ob.offRoadDistance;
                }

            }
            else
            {

                var pathNode = GetCurrentRoadPathNode();
                if(pathNode != null)
                {
                    GameObject obs = GameObject.Instantiate(obstacles[idx], Vector3.zero, Quaternion.identity) as GameObject;
                    BaseObstacle ob = obs.GetComponent<BaseObstacle>();
                    Vector3[] spawnPosition = GetObstacleSpawnPosition(pathNode, ob.offRoadDistance);
                    if(spawnPosition.Length > 0)
                    {

                        obs.transform.position = spawnPosition[0];
                        obs.transform.LookAt(spawnPosition[1]);

                        if (ob.spawnInLane)
                        {
                            obs.transform.position = spawnPosition[1];
                            RaycastHit hit;
                            Physics.Raycast(ob.transform.position + Vector3.up * 5, -transform.up, out hit, 100f, ~(1 << LayerMask.NameToLayer("obstacle") | 1 << LayerMask.NameToLayer("Traffic")));
                            ob.transform.position = new Vector3(ob.transform.position.x, hit.point.y, ob.transform.position.z);
                        }
                    }
                    else
                    {
                        Destroy(obs);
                    }
                }
            }
        }
    }

    public void TriggerObstacle1()
    {
        TriggerObstacle(0);            
    }

    public void TriggerObstacle2()
    {
        TriggerObstacle(1);
    }

    public void TriggerObstacle3()
    {
        TriggerObstacle(2);
    }

    public void TriggerObstacle4()
    {
        TriggerObstacle(3);
    }

    public void TriggerObstacle5()
    {
        TriggerObstacle(4);
    }

    public void RepositionWaypoint1()
    {
        RepositionToWaypoint(0);
    }

    public void RepositionWaypoint2()
    {
        RepositionToWaypoint(1);
    }

    public void RepositionWaypoint3()
    {
        RepositionToWaypoint(2);
    }

    public void RepositionWaypoint4()
    {
        RepositionToWaypoint(3);
    }

    public void RepositionWaypoint5()
    {
        RepositionToWaypoint(4);
    }

    public void RepositionWaypoint6()
    {
        RepositionToWaypoint(5);
    }

    public void RepositionWaypoint7()
    {
        RepositionToWaypoint(6);
    }
    public void RepositionWaypoint8()
    {
        RepositionToWaypoint(7);
    }
    public void RepositionWaypoint9()
    {
        RepositionToWaypoint(8);
    }
    public void ChangeCamView()
    {
        driverCam.SwitchView();
    }


    public void ToggleConsole()
    {
    }

    public void KillObstacles()
    {
        var obstacles = GameObject.FindGameObjectsWithTag("Obstacle");
        foreach (var o in obstacles)
        {
            GameObject.Destroy(o);
        }
    }

    private TrafEntry GetCurrentTrafEntry()
    {
        Vector3 carPos = car.transform.position;

        GameObject traf = GameObject.Find("SFTraffic");
        var trafSystem = traf.GetComponent<TrafSystem>();
        float minDist = 999999999f;
        TrafEntry minEntry = null;
        foreach(var trafEntry in trafSystem.entries)
        {
            foreach(var e in trafEntry.waypoints)
            {
                float d = Vector3.Distance(e, carPos);
                if(d < minDist)
                {
                    minDist = d;
                    minEntry = trafEntry;
                }
            }
        }

        return minEntry;
    }

    private Vector3[] GetObstacleSpawnPositionTraffic()
    {
        Vector3 carPos = car.transform.position;

        GameObject traf = GameObject.Find("SFTraffic");
        var trafSystem = traf.GetComponent<TrafSystem>();

        float minDist = 9999999f;
        float minInterp = 0f;
        TrafEntry minEntry = null;
        foreach(var trafEntry in trafSystem.entries)
        {
            for(float i = 0; i < 1; i += 0.02f)
            {
                InterpolatedPosition pos = trafEntry.GetInterpolatedPosition(i);
                float d = Vector3.Distance(pos.position, carPos);
                if(d < minDist)
                {
                    minEntry = trafEntry;
                    minDist = d;
                    minInterp = i;
                }
            }
        }

        for(float i = minInterp; i < 1; i += 0.01f)
        {
            InterpolatedPosition pos = minEntry.GetInterpolatedPosition(i);
            float d = Vector3.Distance(pos.position, carPos);
            if(d > obstacleDist)
            {
                Vector3 tangent = minEntry.GetPoints()[pos.targetIndex] - pos.position;
                Vector3 oppositeTangent = Vector3.zero;
                if (minEntry.subIdentifier <= 1)
                {
                    var opposite = trafSystem.GetEntry(minEntry.identifier, 2);
                    var oppPos = opposite.GetInterpolatedPosition(i);
                    oppositeTangent = opposite.GetPoints()[oppPos.targetIndex] - oppPos.position;
                }
                else
                {
                    var opposite = trafSystem.GetEntry(minEntry.identifier, 1);
                    var oppPos = opposite.GetInterpolatedPosition(i);
                    oppositeTangent = opposite.GetPoints()[oppPos.targetIndex] - oppPos.position;
                }

                if (Vector3.Angle(oppositeTangent, tangent) > 90f)
                {
                    //two way street
                    return new Vector3[] { pos.position + (Quaternion.Euler(0, 90f, 0) * tangent).normalized * (2f + 4f * (minEntry.subIdentifier == 1 || minEntry.subIdentifier == 2 ? 1 : 0)), pos.position };
                }
                else
                {
                    //one way
                    return new Vector3[] { pos.position + (Quaternion.Euler(0, minEntry.subIdentifier <= 1 ? -90f : 90f, 0) * tangent).normalized * (2f + 4f * (minEntry.subIdentifier == 1 || minEntry.subIdentifier == 2 ? 1 : 0)), pos.position };
                }
            }
        }

        return new Vector3[] {Vector3.zero };
    }

    private Vector3[] GetNextIntersectionTaffic()
    {
        Vector3 carPos = car.transform.position;

        GameObject traf = GameObject.Find("SFTraffic");
        var trafSystem = traf.GetComponent<TrafSystem>();

        var minEntry = GetCurrentTrafEntry();

        float minAngle = 720f;
        Vector3 minSpot = Vector3.zero;
        Vector3 minLookAt = Vector3.zero;
        float minDist = 0;
        foreach(TrafEntry entry in (trafSystem.entries.Where(e => e.identifier == minEntry.identifier)))
        {
            minDist = 0;
            int min = 0;
            for(int i = 0; i < entry.waypoints.Count; i++)
            {
                Vector3 e = entry.waypoints[i];
                float d = Vector3.Distance(e, carPos);
                if(d < minDist)
                {
                    minDist = d;
                    min = i;
                }
            }

            //get tangent
            Vector3 tan = Vector3.zero;
            if(min < entry.waypoints.Count - 1)
            {
                tan = entry.waypoints[min + 1] - entry.waypoints[min];
            }
            else
            {
                tan = entry.waypoints[min] - entry.waypoints[min - 1];
            }

            var angle = Vector3.Angle(car.transform.forward, tan);
            if(angle < minAngle)
            {
                minAngle = angle;
                var newNode = trafSystem.roadGraph.GetNode(entry.identifier, entry.subIdentifier).SelectRandom();
                if(newNode == null)
                {
                    return new Vector3[] {Vector3.zero, Vector3.zero};
                }
                var spot = trafSystem.GetEntry(newNode.id, newNode.subId).waypoints[0];
                minSpot = spot + (Quaternion.Euler(0, -90f, 0) * tan).normalized * (2f + 4f * entry.subIdentifier);
                minLookAt = spot;
            }

        }
        return new Vector3[] { minSpot, minLookAt };
    }

    private Vector3[] GetCurrentCarSpotTraffic()
    {
        Vector3 carPos = car.transform.position;

        GameObject traf = GameObject.Find("SFTraffic");
        var trafSystem = traf.GetComponent<TrafSystem>();

        var minEntry = GetCurrentTrafEntry();

        float minAngle = 720f;
        Vector3 minSpot = Vector3.zero;
        Vector3 minTan = Vector3.zero;
        float minDist = 0;
        foreach(TrafEntry entry in (trafSystem.entries.Where(e => e.identifier == minEntry.identifier))) {
            minDist = 0;
            int min = 0;
            for(int i = 0; i < entry.waypoints.Count; i++ )
            {
                Vector3 e = entry.waypoints[i];
                float d = Vector3.Distance(e, carPos);
                if(d < minDist)
                {
                    minDist = d;
                    min = i;
                }
            }

            //get tangent
            Vector3 tan = Vector3.zero;
            if(min < entry.waypoints.Count - 1)
            {
                tan = entry.waypoints[min + 1] - entry.waypoints[min];
            }
            else
            {
                tan = entry.waypoints[min] - entry.waypoints[min - 1];
            }

            var angle = Vector3.Angle(car.transform.forward, tan);
            if(angle < minAngle)
            {
                minAngle = angle;
                minSpot = entry.waypoints[min];
                minTan = tan;
            }

        }
        return new Vector3[] {minSpot, minTan};

    }

    private bool isInAutoPath = false;

    public bool IsInAutoPath()
    {
        return isInAutoPath;
    }

    public void SetAutoPath(bool status)
    {
        if (IsInAutoPath() != status)
        {
            AutoPath();
        }
    }

    public void AutoPath()
    {
        if (hasTraffic)
            return;

        if (GameObject.Find("AutoPath") == null)
        {
            Debug.Log("No auto path found");
            return;
        }

        if (isInAutoPath)
        {
            RepositionUser();
            isInAutoPath = false;
            return;
        }
        else
        {
            isInAutoPath = true;
        }

        var ap = car.gameObject.GetComponent<CarExternalInputAutoPathAdvanced>();
        if (ap != null) {
            ap.enabled = false;
            Destroy(ap);
        }

        car.GetComponent<VehicleInputController>().enabled = false;
        car.transform.position = autoPath.transform.position;
        car.transform.rotation = autoPath.transform.rotation;
        car.GetComponent<Rigidbody>().velocity = Vector3.zero;
        car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

        var newAp = car.AddComponent<CarExternalInputAutoPathAdvanced>();
        newAp.waypointThreshold = 4f;
        newAp.maxSpeed = 15f;
        newAp.maxBrake = 1f;
        newAp.maxThrottle = 0.7f;
        newAp.steerSpeed = 20f;
        newAp.throttleSpeed = 1f;
        newAp.brakeSpeed = 0.5f;

        newAp.normalAdd = 0f;
        newAp.pathRadius = 0.35f;

        newAp.path = autoPath;
        newAp.Init();
            
    }       

    public void RepositionUser() {
        hasTraffic = GameObject.Find("SFTraffic") != null;

        var ap = car.gameObject.GetComponent<CarExternalInputAutoPathAdvanced>();
        if (ap != null)
        {
            ap.enabled = false;
            Destroy(ap);
            isInAutoPath = false;
        }

        if(hasTraffic)
        {
            var spot = GetCurrentCarSpotTraffic();

            //raycast to find better spot
            Vector3 spawnPos = spot[0];
            RaycastHit hit;
            Physics.Raycast(new Ray(spot[0] + Vector3.up * 10f, Vector3.down), out hit, 45f, ~(1 << LayerMask.NameToLayer("PlayerCar")));
            spawnPos.y = hit.point.y;
            car.transform.position = spawnPos;
            car.transform.LookAt(spawnPos + spot[1]);
            car.GetComponent<Rigidbody>().velocity = Vector3.zero;
            car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            //clear traffic if need be
            var colls = Physics.OverlapSphere(spot[0], 30f, 1 << LayerMask.NameToLayer("Traffic"));
            if(colls != null && colls.Length > 0)
            {
                for(var i = 0; i < colls.Length; i++)
                {
                    Destroy(colls[i].transform.root.gameObject);
                }
            }

        }
        else
        {
            var pathNode = GetCurrentRoadPathNode();
            if(pathNode != null)
            {
                Vector3[] spawnPosition = GetCarResetPosition(pathNode);
                if(spawnPosition.Length > 0)
                {
                    car.transform.position = spawnPosition[0];
                    if (spawnPosition[1] == Vector3.zero)
                        spawnPosition[1] = Vector3.forward;

                    car.transform.LookAt(spawnPosition[0] + spawnPosition[1]);
                    car.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
                }
                else
                {
                    RepositionToWaypoint(0);
                }
            }
            else
            {
                RepositionToWaypoint(0);
            }
        }
    }

    public void RepositionToWaypoint(int spot)
    {
        if(repositionWaypoints.Length > spot && repositionWaypoints[spot] != null)
        {

            var ap = car.gameObject.GetComponent<CarExternalInputAutoPathAdvanced>();
            if (ap != null)
            {
                ap.enabled = false;
                Destroy(ap);
            }

            car.transform.position = repositionWaypoints[spot].transform.position;
            car.transform.rotation = repositionWaypoints[spot].transform.rotation;
            car.GetComponent<Rigidbody>().velocity = Vector3.zero;
            car.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;

            if(hasTraffic)
            {
                //clear traffic if we need to
                var colls = Physics.OverlapSphere(repositionWaypoints[spot].transform.position, 30f, 1 << LayerMask.NameToLayer("Traffic"));
                if(colls != null && colls.Length > 0)
                {
                    for(var i = 0; i < colls.Length; i++)
                    {
                        Destroy(colls[i].transform.root.gameObject);
                    }
                }

            }

        }
    }

}
