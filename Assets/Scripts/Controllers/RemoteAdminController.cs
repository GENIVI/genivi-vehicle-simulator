/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Net;
using WebSocketSharp.Server;

public static class DateTimeJavaScript
{
    private static readonly long DatetimeMinTimeTicks =
       (new System.DateTime(1970, 1, 1, 0, 0, 0, System.DateTimeKind.Utc)).Ticks;

    public static long ToJavaScriptMilliseconds(this System.DateTime dt)
    {
        return (long)((dt.ToUniversalTime().Ticks - DatetimeMinTimeTicks) / 10000);
    }
}

public class AdminScreenSocket : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        RemoteAdminController.StaticInstance.ReceiveMessage(e.Data);
    }

    public void Broadcast(string message)
    {
        Sessions.Broadcast("Broadcasted " + message);
    }
}

[System.Serializable]
public class RemoteAdminMessage
{
    public string messageType;
    public string data;
}

public class RemoteAdminController : PersistentUnitySingleton<RemoteAdminController> {

    public enum SendMessageType {SELECT_CAR, SELECT_SCENE, REFRESH_STATE, START_DRIVE_SCENE, END_DRIVE_SCENE}
    public enum ReceiveMessageType {SELECT_CAR, SELECT_ENV, SET_FOLEY_VOL, SET_MUSIC_VOL, SET_VEHICLE_VOL}

    public static RemoteAdminController StaticInstance;

    public static event System.Action<int> OnSelectCar;
    public static event System.Action<int> OnSelectScene;
    public static event System.Action OnSelectBack;


    public static event System.Action<int> OnTriggerObstacle;
    public static event System.Action<int> OnTriggerWaypoint;
    public static event System.Action<int> OnTriggerPredefinedPath;
    public static event System.Action<bool> OnSetTraffic;
    public static event System.Action<bool> OnSetAutoPlay;

    public static event System.Action<string> OnSetWeather;
    public static event System.Action<bool> OnSetWetRoads;
    public static event System.Action<bool> OnSetSunShafts;
    public static event System.Action<float> OnSetCamFarClip;
    public static event System.Action<float> OnSetFoV;

    public static event System.Action OnEndScene;

    public static event System.Action<int> OnSelectInfraction;
    public static event System.Action OnClearInfraction;

    public static event System.Action<bool> OnSetProgressTime;
    public static event System.Action<float> OnSetTimeOfDay;

    public static event System.Action OnRepositionVehicle;

    private HttpServer httpServer;
    private WebSocketServer server;

   // public static event System.Action<ReceiveMessageType messageType OnAdminMessage()


    public static string tempCachePath;

    public void Start()
    {
        string buildType = ShowBuild.GetBuildType();
        if (buildType == "CONSOLE")
        {
            gameObject.SetActive(false);
            Destroy(gameObject);
            return;
        }

        tempCachePath = Application.temporaryCachePath;

        if (RemoteAdminController.Instance == this)
            StaticInstance = this;

        receivedMessages = new Queue<string>();
        debugLog = new Queue<string>();

        httpServer = new HttpServer(8087);
        httpServer.RootPath = Application.streamingAssetsPath + "/Admin";
        httpServer.OnGet += (sender, e) =>
        {

            var req = e.Request;
            var res = e.Response;

            var path = req.RawUrl;
            if (path == "/")
                path += "index.html";

            if (path.StartsWith("/Screenshots"))
            {
                res.ContentType = "image/jpeg";
                res.ContentEncoding = System.Text.Encoding.UTF8;
                int id = -1;
                try
                {
                    char[] p = new char[] {'/'};
                    id = int.Parse(path.Split(p)[2]);
                    byte[] data = System.IO.File.ReadAllBytes(tempCachePath + "/" + id + ".jpg");
                    res.WriteContent(data);
                    return;
                }
                catch (System.Exception ex)
                {
                    debugLog.Enqueue("Error loading screenshot " + id + ": " + ex.Message);
                    res.StatusCode = (int)HttpStatusCode.NotFound;
                    res.Abort();
                    return;
                }
            }

            var content = httpServer.GetFile(path);
            if (content == null)
            {
                res.StatusCode = (int)HttpStatusCode.NotFound;
                return;
            }

            if (path.EndsWith(".html"))
            {
                res.ContentType = "text/html";
                res.ContentEncoding = System.Text.Encoding.UTF8;
            }
            else if (path.EndsWith(".javascript"))
            {
                res.ContentType = "text/javascript";
                res.ContentEncoding = System.Text.Encoding.UTF8;
            }
            else if (path.EndsWith(".css"))
            {
                res.ContentType = "text/css";
                res.ContentEncoding = System.Text.Encoding.UTF8;
            }
            else if (path.EndsWith(".png"))
            {
                res.ContentType = "image/png";
                res.ContentEncoding = System.Text.Encoding.UTF8;
            }
            else if (path.EndsWith(".jpg"))
            {
                res.ContentType = "image/jpeg";
                res.ContentEncoding = System.Text.Encoding.UTF8;
            }

            //httpServer.Log.Debug("writing resonse: " + res.ToString());

            res.WriteContent(content);

            
        };


        //httpServer.Log.File = Application.streamingAssetsPath + "/Admin/http.log";
        //httpServer.Log.Level = LogLevel.Debug;
        httpServer.Start();

        server = new WebSocketServer(8088);


  //      server.OnConnect += (sender, e) => {
  //          debugLog.Enqueue("remote client connected");    
  //      };


        //server.Log.File = Application.streamingAssetsPath + "/Admin/websocket.log";
        //server.Log.Level = LogLevel.Debug;

        server.AddWebSocketService<AdminScreenSocket>("/api");


        server.Start();
        


        Debug.Log("started server");
    }

    //MOCKS ONLY!
    void OnEnable()
    {
        OnTriggerObstacle += (i) => Debug.Log("obstacle " + i);
        OnTriggerWaypoint += (i) => Debug.Log("waypoint " + i);
        OnTriggerPredefinedPath += (i) => Debug.Log("predefined " + i);
        OnSetTraffic += (i) => Debug.Log("traffic " + i);
        OnSetAutoPlay += (i) => Debug.Log("autoplay" + i);

        OnSetWeather += (i) => Debug.Log("weather" + i);
        OnSetSunShafts += (i) => Debug.Log("sunshafts" + i);
        OnSetWetRoads += (i) => Debug.Log("wetroads" + i);
        OnSetFoV += (i) => Debug.Log("fov" + i);
        OnSetCamFarClip += (i) => Debug.Log("far clip" + i);

        OnEndScene += () => Debug.Log("End scene");
    }

    public void SendMessage(SendMessageType message, float arg)
    {
        string msg = "{\"messageType\" : \"" + message.ToString() + "\", \"messageArgs\" : { \"float\" : " + arg + "}}";
        server.WebSocketServices.Broadcast(msg);
    }

    public void SendMessage(SendMessageType message, int arg)
    {
        string msg = "{\"messageType\" : \"" + message.ToString() + "\", \"messageArgs\" : { \"int\" : " + arg + "}}";
        server.WebSocketServices.Broadcast(msg);
    }

    public void SendMessage(SendMessageType message, string arg)
    {
        string msg = "{\"messageType\" : \"" + message.ToString() + "\", \"messageArgs\" : { \"string\" : \"" + arg + "\"}}";
        server.WebSocketServices.Broadcast(msg);
    }

    public void SendMessage(SendMessageType message, List<DrivingInfraction> infractions)
    {
        string msg = "{\"messageType\" : \"" + message.ToString() + "\", \"messageArgs\" : [";
        for (int i = 0; i < infractions.Count; i++ )
        {
            var inf = infractions[i];
            string obj = "{";
            obj += "\"id\" : " + inf.id;
            obj += ",\"type\" : \"" + inf.type + "\"";
            obj += ",\"sysTime\" : " + inf.systemTime.ToJavaScriptMilliseconds();
            obj += ",\"sessionTime\" : " + inf.sessionTime;
            obj += ",\"speed\" : " + (inf.speed * NetworkedCarConsole.MPSTOMPH).ToString("F2");
            obj += "}";

            if (i < infractions.Count - 1)
                obj += ",";

            msg += obj;
        }
        msg += "]}";
        server.WebSocketServices.Broadcast(msg);
    }

    public void SendMessage(SendMessageType message)
    {
        string msg = "{\"messageType\" : \"" + message.ToString() + "\", \"messageArgs\" : {}}";
        server.WebSocketServices.Broadcast(msg);
    }

    public void SendMessageRaw(SendMessageType message, string args)
    {
        string msg = "{\"messageType\" : \"" + message.ToString() + "\", \"messageArgs\" : " + args +"}";
        server.WebSocketServices.Broadcast(msg);
    }


    private Queue<string> receivedMessages;
    private Queue<string> debugLog;

    public void ReceiveMessage(string rawData)
    {
        receivedMessages.Enqueue(rawData);
    }

    public void ProcessMessage(string rawData)
    {
        var msg = JsonUtility.FromJson<RemoteAdminMessage>(rawData);
        Debug.Log("GOT: " + rawData);
        string msgType = msg.messageType;
        var data = msg.data;
        switch (msgType)
        {
            case "SELECT_CAR":
                SelectCar(int.Parse(data));
                break;
            case "SELECT_SCENE":
                SelectScene(int.Parse(data));
                break;
            case "SELECT_BACK":
                SelectBack();
                break;
            case "TRIGGER_OBSTACLE":
                TriggerObstacle(int.Parse(data));
                break;
            case "TRIGGER_WAYPOINT":
                TriggerWaypoint(int.Parse(data));
                break;
            case "TRIGGER_PREDEFINED_PATH":
                TriggerPredefinedPath(int.Parse(data));
                break;
            case "SET_TRAFFIC":
                SetTraffic(bool.Parse(data));
                break;
            case "SET_AUTOPLAY":
                SetAutoPlay(bool.Parse(data));
                break;
            case "SET_WEATHER":
                SetWeather(data);
                break;
            case "SET_SUN_SHAFTS":
                SetSunShafts(bool.Parse(data));
                break;
            case "SET_WET_ROADS":
                SetWetRoads(bool.Parse(data));
                break;
            case "SET_CAM_FOV":
                SetFoV(float.Parse(data));
                break;
            case "SET_CAM_FARCLIP":
                SetCamFarClip(float.Parse(data));
                break;
            case "END_DRIVE_SCENE":
                EndScene();
                break;
            case "SELECT_INFRACTION":
                SelectInfraction(int.Parse(data));
                break;
            case "CLEAR_INFRACTION":
                ClearInfraction();
                break;
            case "SET_PROGRESS_TIME":
                SetProgressTime(bool.Parse(data));
                break;
            case "SET_TIME_OF_DAY":
                SetTimeOfDay(float.Parse(data));
                break;
            case "REPOSITION_VEHICLE":
                RepositionVehicle();
                break;

        }
    }

    void Update()
    {
        while (receivedMessages.Count > 0)
        {
            ProcessMessage(receivedMessages.Dequeue());
        }

        while (debugLog.Count > 0)
        {
            Debug.Log(debugLog.Dequeue());
        }
        
    }

    void SelectInfraction(int id)
    {
        if (OnSelectInfraction != null)
            OnSelectInfraction(id);
    }

    void ClearInfraction()
    {
        if(OnClearInfraction != null)
        {
            OnClearInfraction();
        }
    }

    void SelectBack()
    {
        if (OnSelectBack != null)
            OnSelectBack();

    }

    void SelectCar(int car)
    {
        if (OnSelectCar != null)
            OnSelectCar(car);
    }

    void SelectScene(int scene)
    {
        if (OnSelectScene != null)
            OnSelectScene(scene);
    }

    void TriggerObstacle(int obs)
    {
        if (OnTriggerObstacle != null)
            OnTriggerObstacle(obs);
    }

    void TriggerWaypoint(int wp)
    {
        if (OnTriggerWaypoint != null)
            OnTriggerWaypoint(wp);
    }

    void TriggerPredefinedPath(int path)
    {
        if (OnTriggerPredefinedPath != null)
            OnTriggerPredefinedPath(path);
    }

    void SetTraffic(bool traf)
    {
        if (OnSetTraffic != null)
            OnSetTraffic(traf);
    }

    void SetAutoPlay(bool autoPlay)
    {
        if (OnSetAutoPlay != null)
            OnSetAutoPlay(autoPlay);
    }

    void SetWeather(string weather)
    {
        if (OnSetWeather != null)
            OnSetWeather(weather);
    }

    void SetWetRoads(bool wet)
    {
        if (OnSetWetRoads != null)
            OnSetWetRoads(wet);
    }

    void SetSunShafts(bool sunshafts)
    {
        if (OnSetSunShafts != null)
        {
            OnSetSunShafts(sunshafts);
        }
    }

    void SetFoV(float fov)
    {
        if (OnSetFoV != null)
            OnSetFoV(fov);
    }

    void SetCamFarClip(float clip)
    {
        if (OnSetCamFarClip != null)
            OnSetCamFarClip(clip);
    }

    void EndScene()
    {
        if (OnEndScene != null)
        {
            OnEndScene();
        }
    }

    void SetProgressTime(bool progress)
    {
        if (OnSetProgressTime != null)
        {
            OnSetProgressTime(progress);
        }
    }

    void SetTimeOfDay(float time)
    {
        if(OnSetTimeOfDay != null)
        {
            OnSetTimeOfDay(time);
        }
    }

    void RepositionVehicle()
    {
        if(OnRepositionVehicle != null)
        {
            OnRepositionVehicle();
        }
    }

    protected override void OnDestroy()
    {
        if(server != null)
            server.Stop();
        if(httpServer != null)
            httpServer.Stop();
        base.OnDestroy();
    }


}
