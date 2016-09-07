/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class UILabel {
    public string text;
    public Rect location;
}

[System.Serializable]
public class UIRule {

}

[System.Serializable]
public class UITexture
{
    public Rect location;
    public Texture2D tex;
}

public class AdminCategory
{

    private const float ITEMS_Y_START = 200f;

    public string subcat0;
    public string subcat1;
    public string subcat2;
    public string subcat3;

    public List<AdminItem> col0;
    public List<AdminItem> col1;
    public List<AdminItem> col2;
    public List<AdminItem> col3;

    private float[] offset;

    public AdminCategory(string c0, string c1, string c2, string c3, float[] offset = null)
    {
        col0 = new List<AdminItem>();
        col1 = new List<AdminItem>();
        col2 = new List<AdminItem>();
        col3 = new List<AdminItem>();

        subcat0 = c0;
        subcat1 = c1;
        subcat2 = c2;
        subcat3 = c3;

        if (offset == null)
            this.offset = new float[] { 0f, 0f, 0f, 0f };
        else
            this.offset = offset;
    }

    public void Calculate()
    {
        float offsetX = 0f;
        float offsetY = ITEMS_Y_START + offset[0];
        foreach (var i in col0)
        {
            i.Calculate(new Vector2(offsetX, offsetY));
            offsetY += i.GetHeight() + 1f;
        }

        offsetX = 480f;
        offsetY = ITEMS_Y_START + offset[1];

        foreach (var i in col1)
        {
            i.Calculate(new Vector2(offsetX, offsetY));
            offsetY += i.GetHeight() + 1f;
        }

        offsetX = 960f;
        offsetY = ITEMS_Y_START + offset[2];

        foreach (var i in col2)
        {
            i.Calculate(new Vector2(offsetX, offsetY));
            offsetY += i.GetHeight() + 1f;
        }

        offsetX = 1440f;
        offsetY = ITEMS_Y_START + offset[3];

        foreach (var i in col3)
        {
            i.Calculate(new Vector2(offsetX, offsetY));
            offsetY += i.GetHeight() + 1f;
        }
    }

    public void Render()
    {
        foreach (var i in col0)
        {
            i.Render();
        }

        foreach (var i in col1)
        {
            i.Render();
        }

        foreach (var i in col2)
        {
            i.Render();
        }

        foreach (var i in col3)
        {
            i.Render();
        }
    }

    public void UpdateValues()
    {
        foreach (var item in col0)
        {
            item.UpdateValue();
        }
        foreach (var item in col1)
        {
            item.UpdateValue();
        }
        foreach (var item in col2)
        {
            item.UpdateValue();
        }
        foreach (var item in col3)
        {
            item.UpdateValue();
        }
    }
}

public abstract class AdminItem
{

    public static GUIStyle minMaxStyle;
    public static GUIStyle valueStyle;
    public static GUIStyle sliderStyle;
    public static GUIStyle sliderThumbStyle;
    public static GUIStyle toggleStyle;
    public static Texture2D whiteTex;
    public static Texture2D minAudioTex;
    public static Texture2D maxAudioTex;
    public static Texture2D radioBgTex;
    public static GUIStyle buttonStyle;
    public static GUIStyle labelStyle;
    public static GUIStyle toolbarStyle;
    public static GUIStyle infractionsStyle;
    public static GUIStyle infractionsTitleStyle;
    public static Texture2D sfMap;
    public static Texture2D yosMap;
    public static Texture2D pchMap;
    public static Texture2D mapIndicator;
    protected Rect calculatedLabelRect;



    public virtual void Calculate(Vector2 topLeft) {
        Rect r = GenLabelRect();
        calculatedLabelRect = new Rect(r.xMin + topLeft.x, r.yMin + topLeft.y, r.width, r.height);
    }

    public virtual Rect GenLabelRect()
    {
        return new Rect(55f, 5f, 100f, 23f);
    }

    public abstract float GetHeight();

    public string label;

    public virtual void Render() {
        GUI.Label(calculatedLabelRect, label, labelStyle);
    }

    public virtual void UpdateValue()
    {

    }
}


public class MapItem : AdminItem
{
    private Rect calculatedMapRect;
    private Vector3 currentPosition;
    private Vector3 currentRotation;
    public Environment environment;
    private Rect indicatorRect;
    private Vector2 indicatorPivot;
    private Vector3 tlMap;
    private Vector3 brMap;

    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        calculatedMapRect = new Rect(0, 380, 1440, 700);

        indicatorRect = new Rect(0,
                        380,
                        80, 66);

        indicatorPivot = new Vector2(40, 413);

        switch (environment)
        {
            case Environment.COASTAL:
                tlMap = new Vector3(1481f, 0f, 4559f);
                brMap = new Vector3(-2354f, 0f, -3331f);
                break;
            case Environment.SCENIC:
                tlMap = new Vector3(-6915f, 0f, 2589f);
                brMap = new Vector3(5120f, 0f, -3249f);
                break;
            case Environment.URBAN:
                tlMap = new Vector3(90.5f, 0f, 2330f);
                brMap = new Vector3(6327.5f, 0f, -700f);
                break;

        }

    }

    public override float GetHeight()
    {
        return 1080f;
    }

    public override void Render()
    {
        switch(environment) {
            case Environment.COASTAL:
                GUI.DrawTexture(calculatedMapRect, pchMap);
                break;
            case Environment.SCENIC:
                GUI.DrawTexture(calculatedMapRect, yosMap);
                break;
            case Environment.URBAN:
                GUI.DrawTexture(calculatedMapRect, sfMap);
                break;
            
        }

        //draw indicator
        //Matrix4x4 oldMat = GUI.matrix;
        //GUIUtility.RotateAroundPivot(currentRotation.y, indicatorPivot);

        //Matrix4x4 matrix = GUI.matrix;
        //GUI.matrix = Matrix4x4.identity;
        //Matrix4x4 lhs = Matrix4x4.TRS(indicatorPivot, Quaternion.Euler(0f, 0f, currentRotation.y), Vector3.one) * Matrix4x4.TRS(-indicatorPivot, Quaternion.identity, Vector3.one);
        //GUI.matrix = lhs * matrix;


        float xOff = 0f;

        if (AdminSettings.Instance.displayType == AdminScreen.DisplayType.PARABOLIC)
        {
            GUI.BeginGroup(new Rect(-3072, 0, 4992, 1080));
            xOff = 3072;
        }
        else
        {
            GUI.BeginGroup(new Rect(-3840, 0, 5760, 1080));
            xOff = 3840;
        }

        GUIUtility.RotateAroundPivot(currentRotation.y, new Vector2(indicatorPivot.x + xOff * (Screen.height/ 1080f), indicatorPivot.y));
        GUI.DrawTexture(new Rect(indicatorRect.x + xOff, indicatorRect.y, indicatorRect.width, indicatorRect.height), mapIndicator);

        GUI.EndGroup();
        //GUI.matrix = oldMat;
    }

    public override void UpdateValue()
    {
        base.UpdateValue();
        currentPosition = TrackController.Instance.car.transform.position;
        currentRotation = TrackController.Instance.car.transform.rotation.eulerAngles - Vector3.up * 90f;

        float xPc = (currentPosition.x - tlMap.x) / (brMap.x - tlMap.x);
        float zPc = (currentPosition.z - brMap.z) / (tlMap.z - brMap.z);

        //PCH is weird and flipped
        if (environment == Environment.COASTAL)
        {
            zPc = (currentPosition.x - brMap.x) / (tlMap.x - brMap.x);
            xPc = 1 - (currentPosition.z - brMap.z) / (tlMap.z - brMap.z);
            currentRotation.y -= 90f;
        }

        indicatorRect.x = (xPc * 1440) - 40;
        indicatorRect.y = 380 + (1f - zPc) * 700 - 33;

        indicatorPivot.x = (indicatorRect.x + 32) * Screen.height / 1080f;
        indicatorPivot.y = (indicatorRect.y + 33) * Screen.height / 1080f;

        //store for remote!
        TrackController.Instance.mapXPos = xPc;
        TrackController.Instance.mapZPos = zPc;
    }

}


public class ButtonItem : AdminItem {


    private Rect calculatedButtonRect;


    public System.Action callback;
    
    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        calculatedButtonRect = new Rect(topLeft.x + 57, topLeft.y + 12f, 366f, 50f);
    }

    public override void Render()
    {
        if (GUI.Button(calculatedButtonRect, label, buttonStyle) && callback != null)
        {
            callback();
        }
    }

    public override float GetHeight()
    {
        return 74f;
    }

}

public class SeperatorItem : AdminItem
{
    private Rect calculatedRuleRect;

    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        calculatedRuleRect = new Rect(topLeft.x + 40f, topLeft.y + 12f, 400f, 1f);
    }

    public override void Render()
    {
        GUI.DrawTexture(calculatedRuleRect, whiteTex);
    }

    public override float GetHeight()
    {
        return 25f;
    }
}

public class ToggleItem : AdminItem
{
    protected Rect calculatedToggleRect;

    public bool value;

    public System.Action<bool> set;
    public System.Func<bool> get;

    public override void UpdateValue()
    {
        if (get != null)
            value = get();

    }

    public override Rect GenLabelRect()
    {
        return new Rect(55f, 10f, 200f, 26f);
    }

    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        calculatedToggleRect = new Rect(topLeft.x + 356f, topLeft.y + 15f, 64f, 32f);
    }

    public override void Render()
    {
        base.Render();
        bool oldVal = value;
        value = GUI.Toggle(calculatedToggleRect, value, GUIContent.none, toggleStyle);

        if (oldVal != value && set != null)
            set(value);
            
    }

    public override float GetHeight()
    {
        return 62f;
    }


}

public class InfractionsItem : AdminItem
{

    private Rect[] calculatedRects;
    private string[] labelStrings;
    private const float COL1 = 55f;
    private const float COL2 = 87f;
    private const float COL3 = 140f;
    private const float COL4 = 240f;
    private const float COL5 = 370f;
    private const float ROW = 25f;

    private Rect screenshotRect;
    private Texture2D currentScreenshot;

    private void SelectScreenshot(int id)
    {
        if (currentScreenshot != null)
            Object.Destroy(currentScreenshot);

        try 
        {
            byte[] data = System.IO.File.ReadAllBytes(Application.temporaryCachePath + "/" + id + ".jpg");
            currentScreenshot = new Texture2D(1248, 270, TextureFormat.RGB24, false);
            currentScreenshot.LoadImage(data);
        }
        catch (System.Exception e)
        {
            Debug.Log("Error loading screenshot " + id + ": " + e.Message);
        }
        
    }

    public void Cleanup()
    {
        if(currentScreenshot != null)
        {
            Object.Destroy(currentScreenshot);
        }
    }

    private class Infraction
    {
        public DrivingInfraction infraction;
        public Rect[] rects;
        public string[] strings;
        public Rect selectionRect;
    }

    private List<Infraction> infractions;

    public override void Render()
    {
        for (int i = 0; i < labelStrings.Length; i++)
        {
            GUI.Label(calculatedRects[i], labelStrings[i], infractionsTitleStyle);
        }

        foreach (var i in infractions)
        {

            if (Event.current.type == EventType.mouseDown && i.selectionRect.Contains(Event.current.mousePosition))
            {
                SelectScreenshot(i.infraction.id);
            }

            for(int c = 0; c < i.strings.Length; c++) {
                GUI.Label(i.rects[c], i.strings[c], infractionsStyle);
            }
        }

        if (currentScreenshot != null)
        {
            GUI.DrawTexture(screenshotRect, currentScreenshot);
        }

    }

  

    public InfractionsItem()
    {
        infractions = new List<Infraction>();
    }

    public override void UpdateValue()
    {
        var newInfractions = InfractionController.Instance.GetInfractions();
        if (newInfractions.Count > infractions.Count)
        {
            for (int i = infractions.Count; i < newInfractions.Count; i++)
            {
                var newInfraction = newInfractions[i];

                var itemRects = new Rect[] { 
                        new Rect(calculatedRects[0].x, calculatedRects[0].y + ROW * (i + 1), calculatedRects[0].width, ROW),
                        new Rect(calculatedRects[1].x, calculatedRects[1].y + ROW * (i + 1), calculatedRects[1].width, ROW),
                        new Rect(calculatedRects[2].x, calculatedRects[2].y + ROW * (i + 1), calculatedRects[2].width, ROW),
                        new Rect(calculatedRects[3].x, calculatedRects[3].y + ROW * (i + 1), calculatedRects[3].width, ROW),
                        new Rect(calculatedRects[4].x, calculatedRects[4].y + ROW * (i + 1), calculatedRects[4].width, ROW),
                    };

                infractions.Add(new Infraction()
                {
                    infraction = newInfraction,
                    strings = new string[] { 
                        newInfraction.id.ToString(),
                        newInfraction.type,
                        newInfraction.systemTime.ToShortTimeString(),
                        System.TimeSpan.FromSeconds(newInfraction.sessionTime).ToString(),
                        (newInfraction.speed * NetworkedCarConsole.MPSTOMPH).ToString("F2")
                    },
                    rects = itemRects,
                    selectionRect = new Rect(itemRects[0].x, itemRects[0].y, itemRects[4].x + itemRects[4].width - itemRects[0].x, ROW )                   
                });
            }
        }
    }

    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        calculatedRects = new Rect[5];
        calculatedRects[0] = new Rect(topLeft.x + COL1, topLeft.y + ROW, COL2 - COL1, ROW);
        calculatedRects[1] = new Rect(topLeft.x + COL2, topLeft.y + ROW, COL3 - COL2, ROW);
        calculatedRects[2] = new Rect(topLeft.x + COL3, topLeft.y + ROW, COL4 - COL3, ROW);
        calculatedRects[3] = new Rect(topLeft.x + COL4, topLeft.y + ROW, COL5 - COL4, ROW);
        calculatedRects[4] = new Rect(topLeft.x + COL5, topLeft.y + ROW, 480 - COL1, ROW);

        labelStrings = new string[] { "ID", "TYPE", "SYSTEM TIME", "SESSION TIME", "VEHICLE SPEED" };

        screenshotRect = new Rect(480, 270, 1248, 270);
    }

    public override float GetHeight()
    {
        return 0f;
    } 
}





public class RadioItem : AdminItem
{
    public string[] labels;
    public int selected = 0;

    private Rect calculatedToolbarRect;
    private Rect calculatedBgRect;

    public System.Action<int> set;
    public System.Func<int> get;

    public override void UpdateValue()
    {
        if (get != null)
            selected = get();

    }

    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        calculatedToolbarRect = new Rect(topLeft.x + 30f, topLeft.y + 35f, 350f, 55f);
        calculatedBgRect = new Rect(topLeft.x + 63f, topLeft.y + 44f, 283f, 6f);
    }

    public override void Render()
    {
        base.Render();

        GUI.DrawTexture(calculatedBgRect, radioBgTex);

        int oldSelected = selected;
        selected = GUI.Toolbar(calculatedToolbarRect, selected, labels, toolbarStyle);

        if (selected != oldSelected && set != null)
            set(selected);
    }

    public override float GetHeight()
    {
        return 96f;
    }

    public override Rect GenLabelRect()
    {
        return new Rect(55f, 0f, 200f, 24f);
    }
}

public class SliderItem : AdminItem {
    public float value;
    public float min;
    public float max;

    protected Rect calculatedSliderRect;
    protected Rect calculatedMinRect;
    protected Rect calculatedMaxRect;
    protected Rect calculatedValueRect;


    public System.Action<float> set;
    public System.Func<float> get;

    public override void UpdateValue()
    {
        if (get != null)
            value = get();
    }


    public virtual Rect GenMinRect()
    {
        return new Rect(54f, 40f, 56f, 25f);
    }

    public virtual Rect GenMaxRect()
    {
        return new Rect(370f, 40f, 56f, 25f);
    }


    public override void Calculate(Vector2 topLeft)
    {
        base.Calculate(topLeft);
        Rect min = GenMinRect();
        Rect max = GenMaxRect();
        calculatedMinRect = new Rect(min.xMin + topLeft.x, min.yMin + topLeft.y, min.width, min.height);
        calculatedMaxRect = new Rect(max.xMin + topLeft.x, max.yMin + topLeft.y, max.width, max.height);

        calculatedValueRect = new Rect(topLeft.x + 200f, topLeft.y + 5f, 80f, 23f);
        calculatedSliderRect = new Rect(topLeft.x + 110f, topLeft.y + 55f, 260f, 44f);
    }

    public override void Render()
    {
        base.Render();
        RenderMinMax();
        RenderLabel();   
        float oldVal = value;
        value = GUI.HorizontalSlider(calculatedSliderRect, value, min, max, sliderStyle, sliderThumbStyle);
        if (value != oldVal && set != null)
        {
            set(value);
        }

    }

    protected virtual void RenderLabel()
    {
        GUI.Label(calculatedValueRect, value.ToString(), valueStyle);
    }

    protected virtual void RenderMinMax()
    {
        GUI.Label(calculatedMinRect, min.ToString(), minMaxStyle);
        GUI.Label(calculatedMaxRect, max.ToString(), minMaxStyle);
    }

    public override float GetHeight()
    {
        return 96f;
    }
}

public class SliderItemTime : SliderItem {

    protected override void RenderLabel() {
        int hour = Mathf.FloorToInt(value);
        GUI.Label(calculatedValueRect, hour.ToString("D2") + ":" +  Mathf.FloorToInt((value - hour) * 60).ToString("D2"), valueStyle);
    }

}

public class AudioSliderItem : SliderItem
{
    public override Rect GenMinRect()
    {
        return new Rect(79f, 52f, 11f, 20f);
    }

    public override Rect GenMaxRect()
    {
        return new Rect(378f, 51f, 23f, 22f);
    }

    protected override void RenderMinMax()
    {
        GUI.DrawTexture(calculatedMinRect, minAudioTex);
        GUI.DrawTexture(calculatedMaxRect, maxAudioTex);
    }
}



public partial class DriveAdminUI : MonoBehaviour {

    public Texture2D white;
    public UITexture background;

    public GUIStyle categoryOnStyle;
    public GUIStyle categoryOffStyle;
    public GUIStyle subCatStyle;





    public UILabel[] categories;

    public Rect subcat0Rect;
    public Rect subcat1Rect;
    public Rect subcat2Rect;
    public Rect subcat3Rect;

    private int tempCategory = 0;

    public int currentCategory = 0;

    private AdminCategory[] adminCategories;

    public GUIStyle minMaxStyle;
    public GUIStyle valueStyle;
    public GUIStyle sliderStyle;
    public GUIStyle sliderThumbStyle;
    public GUIStyle toggleStyle;
    public Texture2D whiteTex;
    public Texture2D minAudioTex;
    public Texture2D maxAudioTex;
    public Texture2D radioBgTex;
    public GUIStyle buttonStyle;
    public GUIStyle labelStyle;
    public GUIStyle toolbarStyle;
    public GUIStyle infractionsStyle;
    public Texture2D sfMap;
    public Texture2D yosMap;
    public Texture2D pchMap;
    public Texture2D mapIndicator;


    private bool hidden = true;

    public bool disableTrigger = false;

    void Awake()
    {
        AdminItem.whiteTex = white;
        AdminItem.minMaxStyle = minMaxStyle;
        AdminItem.valueStyle = valueStyle;
        AdminItem.sliderStyle = sliderStyle;
        AdminItem.sliderThumbStyle = sliderThumbStyle;
        AdminItem.toggleStyle = toggleStyle;
        AdminItem.minAudioTex = minAudioTex;
        AdminItem.maxAudioTex = maxAudioTex;
        AdminItem.radioBgTex = radioBgTex;
        AdminItem.buttonStyle = buttonStyle;
        AdminItem.labelStyle = labelStyle;
        AdminItem.toolbarStyle = toolbarStyle;
        AdminItem.sfMap = sfMap;
        AdminItem.yosMap = yosMap;
        AdminItem.pchMap = pchMap;
        AdminItem.infractionsStyle = infractionsStyle;
        AdminItem.mapIndicator = mapIndicator;

        //hide at startup
        hidden = true;
    }

    public void EndScene()
    {
        currentCategory = 5;
    }

    void OnEnable()
    {
        AppController.Instance.AdminInput.ToggleConsole += ToggleConfigurator;

        RemoteAdminController.OnSetTraffic += SetTraffic;
        RemoteAdminController.OnSetWeather += SetWeather;
        RemoteAdminController.OnSetWetRoads += SetWetRoads;
        RemoteAdminController.OnSetFoV += SetFoV;
        RemoteAdminController.OnSetCamFarClip += SetFarClip;
    }

    void SetTraffic(bool b)
    {
        if (settings.traffic != null)
            settings.traffic.SetTraffic(b);
    }

    void SetWeather(string weather)
    {
 //       int i = 0;
 //       if (weather == "clear")
 //           i = 0;
 //       else if (weather == "stormy")
 //           i = 1;
 //       else if (weather == "overcast")
 //           i = 2;

  //      AdminSettings.Instance.weather = i;
  //      var newWeather = AdminSettings.Instance.GetWeather();
  //      settings.weather.currentWeather = newWeather;
  //      settings.weather.ChangeWeather(newWeather);

    }

    void SetWetRoads(bool w)
    {
        WetRoads.instance.ToggleWet(w);
    }


    void SetFoV(float f)
    {
        settings.driverCam.SetFoV(f);
        AdminSettings.Instance.fov = f;
    }

    void SetFarClip(float f)
    {
        settings.SetFarClip(f);
    }

    void OnDisable()
    {
        if(AppController.IsInstantiated && AppController.Instance.AdminInput != null)
            AppController.Instance.AdminInput.ToggleConsole += ToggleConfigurator;
       
        RemoteAdminController.OnSetTraffic -= SetTraffic;
        RemoteAdminController.OnSetWeather -= SetWeather;
        RemoteAdminController.OnSetWetRoads -= SetWetRoads;
        RemoteAdminController.OnSetFoV -= SetFoV;
        RemoteAdminController.OnSetCamFarClip -= SetFarClip;
    }

    void ToggleConfigurator()
    {
        AppController.Instance.appSettings.showConfigurator = !AppController.Instance.appSettings.showConfigurator;
    }

    void OnGUI()
    {
        if (hidden)
            return;

        Matrix4x4 oldMatrix = GUI.matrix;

        /*
        if (AdminSettings.Instance.displayType == AdminScreen.DisplayType.PARABOLIC)
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 4992f, Screen.height / 1080f, 1f));
            GUI.BeginGroup(new Rect(3072, 0, 1920, 1080));
        }
        else
        {
            GUI.matrix = Matrix4x4.Scale(new Vector3(Screen.width / 5760f, Screen.height / 1080f, 1f));
            GUI.BeginGroup(new Rect(3840, 0, 1920, 1080));
        }
        */
        OnGUIScaler.ScaleUI();

        GUI.DrawTexture(background.location, background.tex);

        
        for (int i = 0; i < categories.Length; i++)
        {
            if (i != 4 || AppController.Instance.appSettings.showConfigurator)
            {
                GUI.Label(categories[i].location, categories[i].text, i == currentCategory ? categoryOnStyle : categoryOffStyle);
                if (Event.current.type == EventType.mouseDown && categories[i].location.Contains(Event.current.mousePosition))
                {
                    tempCategory = i;
                }
            }
        }

        var current = adminCategories[currentCategory];

        if (!string.IsNullOrEmpty(current.subcat0))
        {
            GUI.Label(subcat0Rect, current.subcat0, subCatStyle);
        }
        if (!string.IsNullOrEmpty(current.subcat1))
        {
            GUI.Label(subcat1Rect, current.subcat1, subCatStyle);
        }
        if (!string.IsNullOrEmpty(current.subcat2))
        {
            GUI.Label(subcat2Rect, current.subcat2, subCatStyle);
        }
        if (!string.IsNullOrEmpty(current.subcat3))
        {
            GUI.Label(subcat3Rect, current.subcat3, subCatStyle);
        }


        current.Render();

        GUI.EndGroup();

        GUI.matrix = oldMatrix;

    }

    
    void Update () {
        currentCategory = tempCategory;


        AdminItem.whiteTex = white;
        AdminItem.minMaxStyle = minMaxStyle;
        AdminItem.valueStyle = valueStyle;
        AdminItem.sliderStyle = sliderStyle;
        AdminItem.sliderThumbStyle = sliderThumbStyle;
        AdminItem.toggleStyle = toggleStyle;
        AdminItem.minAudioTex = minAudioTex;
        AdminItem.maxAudioTex = maxAudioTex;
        AdminItem.buttonStyle = buttonStyle;
        AdminItem.labelStyle = labelStyle;
        AdminItem.toolbarStyle = toolbarStyle;
        AdminItem.sfMap = sfMap;
        AdminItem.yosMap = yosMap;
        AdminItem.pchMap = pchMap;
        AdminItem.infractionsStyle = infractionsStyle;


        if (!disableTrigger && Input.GetKeyDown(KeyCode.F1))
        {
            hidden = !hidden;
        }
        
       
    }
}
