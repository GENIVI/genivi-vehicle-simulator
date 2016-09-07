/*
 * Copyright (C) 2016, Jaguar Land Rover
 * This program is licensed under the terms and conditions of the
 * Mozilla Public License, version 2.0.  The full text of the
 * Mozilla Public License is at https://www.mozilla.org/MPL/2.0/
 */


#region named containers
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

public class DefaultAppSettings : AppSettings, IXmlSerializable
{
    public DefaultAppSettings()
        : base()
    {
        inputControllerBackup = typeof(KeyboardInputController); 
        inputController = typeof(SteeringWheelInputController);
        xResolution = 5760;
        yResolution = 1080;
        fullscreen = true;
        showDebug = false;
        scaleUi = false;

        projectorBlend = false;

        environmentSelectScene = "EnvironmentSelect";
        carSelectScene = "NewCarSelect_2";
        roadSideSelectScene = "RoadSideSelect";
        brandSelectScene = "BrandSelect";
        urbanDrivingScene = "Environment_SanFrancisco02";
        scenicDrivingScene = "Environment_Yosemite";
        coastalDrivingScene = "Environment_PCH";

        gasAxis = "Z";
        brakeAxis = "X";
        minGas = -32768;
        maxGas = 32768;
        minBrake = -32768;
        maxBrake = 32767;
        showConfigurator = true;

        FFBMultiplier = 1.75f;

        
    }

    public DefaultAppSettings(SerializationInfo info, StreamingContext context) : this()
    {           
        inputController = System.Type.GetType(info.GetString("inputController"));
        inputControllerBackup = System.Type.GetType(info.GetString("inputControllerBackup"));
        xResolution = info.GetInt32("xResolution");
        yResolution = info.GetInt32("yResolution");
        fullscreen = info.GetBoolean("fullscreen");
        showDebug = info.GetBoolean("showDebug");
    }

    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        info.AddValue("inputController", (string)inputController.AssemblyQualifiedName);
        info.AddValue("inputControllerBackup", (string)inputControllerBackup.AssemblyQualifiedName);
        info.AddValue("xResolution", xResolution);
        info.AddValue("yResolution", yResolution);
        info.AddValue("fullscreen", fullscreen);
        info.AddValue("showDebug", showDebug);
    }

    public System.Xml.Schema.XmlSchema GetSchema()
    {
        return null;
    }

    public void ReadXml(System.Xml.XmlReader reader)
    {
        reader.MoveToContent();
        reader.ReadStartElement();
        inputController = System.Type.GetType(reader.ReadElementString("inputController"));
        inputControllerBackup = System.Type.GetType(reader.ReadElementString("inputControllerBackup"));
        xResolution = System.Convert.ToInt32(reader.ReadElementString("xResolution"));
        yResolution = System.Convert.ToInt32(reader.ReadElementString("yResolution"));
        fullscreen = System.Convert.ToBoolean(reader.ReadElementString("fullscreen"));
        
        showDebug = System.Convert.ToBoolean(reader.ReadElementString("showDebug"));
        showConfigurator = System.Convert.ToBoolean(reader.ReadElementString("showConfigurator"));
        projectorBlend = System.Convert.ToBoolean(reader.ReadElementString("projectorBlend"));
        brakeAxis = reader.ReadElementString("brakeAxis");
        gasAxis = reader.ReadElementString("gasAxis");
        minBrake = System.Convert.ToInt32(reader.ReadElementString("minBrake"));
        maxBrake = System.Convert.ToInt32(reader.ReadElementString("maxBrake"));
        minGas = System.Convert.ToInt32(reader.ReadElementString("minGas"));
        maxGas = System.Convert.ToInt32(reader.ReadElementString("maxGas"));
        FFBMultiplier = System.Convert.ToSingle(reader.ReadElementString("FFBMultiplier"));
        reader.ReadEndElement();
    }

    public void WriteXml(System.Xml.XmlWriter writer)
    {
        writer.WriteElementString("inputController", (string)inputController.AssemblyQualifiedName);
        writer.WriteElementString("inputControllerBackup", (string)inputControllerBackup.AssemblyQualifiedName);
        writer.WriteElementString("xResolution", xResolution.ToString());
        writer.WriteElementString("yResolution", yResolution.ToString());
        writer.WriteElementString("fullscreen", fullscreen.ToString());
        writer.WriteElementString("showDebug", showDebug.ToString());
        writer.WriteElementString("showConfigurator", showConfigurator.ToString());
        writer.WriteElementString("projectorBlend", projectorBlend.ToString());
        writer.WriteElementString("brakeAxis", brakeAxis.ToString());
        writer.WriteElementString("gasAxis", gasAxis.ToString());
        writer.WriteElementString("minBrake", minBrake.ToString());
        writer.WriteElementString("maxBrake", maxBrake.ToString());
        writer.WriteElementString("minGas", minGas.ToString());
        writer.WriteElementString("maxGas", maxGas.ToString());
        writer.WriteElementString("FFBMultiplier", FFBMultiplier.ToString());
    }
}

public class DevAppSettings : DefaultAppSettings
{
    public DevAppSettings()
        : base()
    {
        showDebug = true;
    }
}

public class ProductionAppSettings : DefaultAppSettings
{
    public ProductionAppSettings()
        : base()
    {
        inputController = typeof(SteeringWheelInputController);
        inputControllerBackup = typeof(KeyboardInputController);
        scaleUi = false;
        projectorBlend = true;
        fullscreen = true;
        xResolution = 4992;
    }
}

#endregion


public class SmallAppSettings : DevAppSettings
{
    public SmallAppSettings()
        : base()
    {
        xResolution = 1920;
        yResolution = 270;
        fullscreen = false;
    }
}

#region G27 at various resolutions

public class SteeringWheelSettings : DefaultAppSettings
{
    public SteeringWheelSettings()
        : base()
    {
        inputController = typeof(SteeringWheelInputController);
        inputControllerBackup = typeof(KeyboardInputController);
    }
}

public class SteeringWheelFull : SteeringWheelSettings
{
    public SteeringWheelFull() : base() {
        xResolution = 5760;
        yResolution = 1080;
    }
}

public class SteeringWheelHalf : SteeringWheelSettings
{
    public SteeringWheelHalf()
        : base()
    {
        xResolution = 2880;
        yResolution = 1080;
    }
}

public class SteeringWheelQuarter : SteeringWheelSettings
{
    public SteeringWheelQuarter()
        : base()
    {
        xResolution = 1440;
        yResolution = 270;
    }
}
#endregion

#region abstract settings base class

public abstract class AppSettings
{

    public static AppSettings GetSettingsFromName(string settingsName)
    {
        var IDs = new Dictionary<string, AppSettings>() {
            {"DEV", new DevAppSettings()},
            {"PRODUCTION", new ProductionAppSettings()},
            {"CUSTOM", new DefaultAppSettings()}
        };
        
        if(IDs.ContainsKey(settingsName))
            return IDs[settingsName];
        else 
            return new DefaultAppSettings();


    }
    public System.Type inputController;
    public System.Type inputControllerBackup;
    public int xResolution;
    public int yResolution;
    public bool fullscreen;
    public bool showDebug;
    public bool scaleUi;
    public bool projectorBlend;
    public bool showConfigurator;
    public string environmentSelectScene;
    public string carSelectScene;
    public string roadSideSelectScene;
    public string brandSelectScene;
    public string urbanDrivingScene;
    public string scenicDrivingScene;
    public string coastalDrivingScene;
    public string gasAxis;
    public string brakeAxis;
    public int maxGas;
    public int minGas;
    public int maxBrake;
    public int minBrake;
    public float FFBMultiplier;
}
#endregion