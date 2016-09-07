/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */

using UnityEngine;
using System.Collections;

[System.Serializable]
public class SelectChoiceUI {
    public string name;
    public int selectIndex;
    public Texture2D texture;
    public Rect position;
    public Rect textPosition;
    public Rect checkPosition;
   
}

public class SelectAdminUI : MonoBehaviour {

    public Texture2D background;
    public Texture2D logos;
    public Texture2D checkTex;
    public Texture2D cancelTex;
    public Texture2D rolloverTex;
   
    public Rect logoRect;

    public GUIStyle textOn;
    public GUIStyle textOff;
    public GUIStyle thumbText;

    private Rect fullScreen = new Rect(0, 0, 1920f, 1080f);
    public Rect titleLabel;
    public Rect additionalControlsLabel;
    public Rect infractionsLabel;
    public Rect selectVehicleLabel;
    public Rect selectSceneLabel;
    public SelectChoiceUI[] cars;
    public SelectChoiceUI[] environments;


    public CarSelectController carSelect;
    public EnvironmentSelectController environmentSelect;
    public AdminScreen mainAdmin;

    public Color dimmedColor;
    private Color currentEnvColor;
    private Color currentVehicleColor;
    private float currentEnvAlpha = 0f;

    private bool isAdditional = false;
    private bool isInfractions = false;

    private AdminCategory additionalCat;
    private AdminCategory infractionsCat;
    private InfractionsItem infractionsItem;

    public GUIStyle labelStyle;
    public GUIStyle minMaxStyle;
    public GUIStyle valueStyle;
    public GUIStyle sliderStyle;
    public GUIStyle sliderThumbStyle;
    public Texture2D minAudioTex;
    public Texture2D maxAudioTex;
    public GUIStyle infractionsStyle;
    public GUIStyle infractionsTitleStyle;

    private InfractionReviewUI reviewUI;

    void Awake () {

        reviewUI = GetComponent<InfractionReviewUI>();

        AdminItem.minMaxStyle = minMaxStyle;
        AdminItem.labelStyle = labelStyle;
        AdminItem.valueStyle = valueStyle;
        AdminItem.sliderStyle = sliderStyle;
        AdminItem.sliderThumbStyle = sliderThumbStyle;
        AdminItem.minAudioTex = minAudioTex;
        AdminItem.maxAudioTex = maxAudioTex;
        AdminItem.infractionsStyle = infractionsStyle;
        AdminItem.infractionsTitleStyle = infractionsTitleStyle;
        
        mainAdmin.hidden = true;
        isAdditional = false;
        isInfractions = false;


        additionalCat = new AdminCategory("AUDIO", "", "", "");
        additionalCat.col0.Add(new AudioSliderItem()
        {
            label = "Music",
            min = 0f,
            max = 1f,
            value = AdminSettings.Instance.musicVol,
            get = () => AudioController.Instance.MusicVolume,
            set = (f) => AudioController.Instance.MusicVolume = f
        });
        additionalCat.col0.Add(new AudioSliderItem()
        {
            label = "Foley",
            min = 0f,
            max = 1f,
            value = AdminSettings.Instance.foleyVol,
            get = () => AudioController.Instance.FoleyVolume,
            set = (f) => AudioController.Instance.FoleyVolume = f
        });
        additionalCat.col0.Add(new AudioSliderItem()
        {
            label = "Vehicle",
            min = 0f,
            max = 1f,
            value = AdminSettings.Instance.vehicleVol,
            get = () => AudioController.Instance.VehicleVolume,
            set = (f) => AudioController.Instance.VehicleVolume = f
        });


        additionalCat.Calculate();


        infractionsCat = new AdminCategory("Infractions", "", "", "");
        infractionsItem = new InfractionsItem();
        infractionsCat.col0.Add(infractionsItem);
        infractionsCat.Calculate();
        infractionsItem.UpdateValue();


        //hide at start
        hidden = true;
      

    }

    private bool hitAdditional = false;
    private bool hitInfractions = false;
    void Update()
    {
        if (hitAdditional)
        {
            isAdditional = true;
            isInfractions = false;
        }
        else if (hitInfractions)
        {
            isAdditional = false;
            isInfractions = true;
        }
        else
        {
            isAdditional = false;
            isInfractions = false;
        }

        if (carSelect.enabled)
            currentEnvAlpha = Mathf.MoveTowards(currentEnvAlpha, 1f, Time.deltaTime * 1.2f);
        else
            currentEnvAlpha = Mathf.MoveTowards(currentEnvAlpha, 0f, Time.deltaTime * 1.2f);

        currentEnvColor = Color.Lerp(Color.white, dimmedColor, currentEnvAlpha);
        currentVehicleColor = Color.Lerp(Color.white, dimmedColor, 1 - currentEnvAlpha);

        if (Input.GetKeyDown(KeyCode.F1))
        {
            hidden = !hidden;
        }
        
    }

    void OnEnable()
    {
        RemoteAdminController.OnSelectCar += carSelect.Trigger;
        RemoteAdminController.OnSelectScene += environmentSelect.Trigger;
        RemoteAdminController.OnSelectBack += environmentSelect.Back;
        RemoteAdminController.OnSelectInfraction += SelectScreenshot;
        RemoteAdminController.OnClearInfraction += ClearScreenshot;
    }

    void OnDisable()
    {
        RemoteAdminController.OnSelectCar -= carSelect.Trigger;
        RemoteAdminController.OnSelectScene -= environmentSelect.Trigger;
        RemoteAdminController.OnSelectBack -= environmentSelect.Back;
        RemoteAdminController.OnSelectInfraction -= SelectScreenshot;
        RemoteAdminController.OnClearInfraction -= ClearScreenshot;
    }
 

    void OnDestroy()
    {
        infractionsItem.Cleanup();

        if (currentRemoteInfractionTex != null)
        {
            Object.Destroy(currentRemoteInfractionTex);
        }
    }

    private bool hidden = true;

    private int currentRemoteInfraction;
    private Texture2D currentRemoteInfractionTex;


    private void SelectScreenshot(int id)
    {

        if (currentRemoteInfractionTex != null)
            Object.Destroy(currentRemoteInfractionTex);

        try
        {
            byte[] data = System.IO.File.ReadAllBytes(Application.temporaryCachePath + "/" + id + ".jpg");
            currentRemoteInfractionTex = new Texture2D(1248, 270, TextureFormat.RGB24, false);
            currentRemoteInfractionTex.LoadImage(data);

            reviewUI.Enable(id);
            reviewUI.screenshot = currentRemoteInfractionTex;

        }
        catch (System.Exception e)
        {
            Debug.Log("Error loading screenshot " + id + ": " + e.Message);
            reviewUI.Disable();
        }

    }

    private void ClearScreenshot()
    {
        reviewUI.Disable();
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
        GUI.BeginGroup(new Rect((Screen.width / (Screen.height/1080f)) - 1920f, 0, 1920, 1080));

        //GUILayout.BeginArea(new Rect(0, 0, 1920f, 1080f));

        GUI.DrawTexture(fullScreen, background);

        GUI.DrawTexture(logoRect, logos);

        GUI.Label(titleLabel, "VEHICLE & SCENE SELECT", isAdditional || isInfractions ? textOff : textOn);
        if (Event.current.type == EventType.mouseDown && titleLabel.Contains(Event.current.mousePosition))
        {
            hitAdditional = false;
            hitInfractions = false;
        }


        GUI.Label(additionalControlsLabel, "ADDITIONAL CONTROLS", isAdditional ? textOn : textOff);
        if (Event.current.type == EventType.mouseDown && additionalControlsLabel.Contains(Event.current.mousePosition))
        {
            hitAdditional = true;
            hitInfractions = false;
        }

        GUI.Label(infractionsLabel, "INFRACTIONS", isInfractions ? textOn : textOff);
        if (Event.current.type == EventType.mouseDown && infractionsLabel.Contains(Event.current.mousePosition))
        {
            hitAdditional = false;
            hitInfractions = true;
        }

        if (isAdditional)
        {
            additionalCat.Render();
            GUI.EndGroup();
        }
        else if (isInfractions)
        {
            infractionsCat.Render();
            GUI.EndGroup();
        }
        else
        {

            GUI.color = currentVehicleColor;
            GUI.Label(selectVehicleLabel, "SELECT VEHICLE", textOn);


            foreach (var car in cars)
            {

                if (carSelect.currentChoice != car.selectIndex)
                {
                    GUI.color = currentVehicleColor;
                }
                else
                {
                    GUI.color = Color.white;
                }

                GUI.DrawTexture(car.position, car.texture);

                GUI.Label(car.textPosition, car.name, thumbText);


                if (carSelect.enabled && carSelect.CanSelect() && car.position.Contains(Event.current.mousePosition))
                {
                    GUI.DrawTexture(car.position, rolloverTex, ScaleMode.StretchToFill, true);
                    if (Event.current.type == EventType.MouseDown)
                    {
                        carSelect.Trigger(car.selectIndex);
                    }
                }


                if (carSelect.enabled && carSelect.hasSelected() && carSelect.currentChoice == car.selectIndex)
                {
                    GUI.DrawTexture(car.checkPosition, checkTex);
                }
                else if (environmentSelect.enabled && carSelect.currentChoice == car.selectIndex)
                {
                    if (car.position.Contains(Event.current.mousePosition))
                    {
                        GUI.DrawTexture(car.checkPosition, cancelTex);
                        if (Event.current.type == EventType.MouseDown)
                        {
                            environmentSelect.Back();
                        }
                    }
                    else
                        GUI.DrawTexture(car.checkPosition, checkTex);
                }

                GUI.color = Color.white;


            }

            GUI.color = currentEnvColor;


            GUI.Label(selectSceneLabel, "SELECT SCENE", textOn);

            foreach (var env in environments)
            {
                GUI.DrawTexture(env.position, env.texture);
                GUI.Label(env.textPosition, env.name, thumbText);
                if (environmentSelect.enabled == true && environmentSelect.CanSelect() && env.position.Contains(Event.current.mousePosition))
                {
                    GUI.DrawTexture(env.position, rolloverTex, ScaleMode.StretchToFill, true);
                    if (Event.current.type == EventType.MouseDown)
                    {
                        environmentSelect.Trigger(env.selectIndex);
                    }
                }

                if (environmentSelect.enabled == true && !environmentSelect.CanSelect())
                {
                    GUI.DrawTexture(env.checkPosition, checkTex);
                }
            }

            GUI.EndGroup();
            GUI.color = Color.white;
            GUI.matrix = oldMatrix;
        }
    }

}
