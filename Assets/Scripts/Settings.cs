using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class Settings : Singleton<Settings>
{
    private string settingsPath;

    // Frame rate cap
    public int framerate;

    /* Quality levels:
     * VeryLow      0
     * Low          1
     * Medium       2
     * High         3
     * Very High    4
     * Ultra        5
    */
    public int quality;

    // Resolution width
    public int screenWidth;

    // Resolution hiehgt
    public int screenHeight;

    // Fullscreen?
    public bool fullscreen;

    // Last used preset
    public string lastLoadedPreset;

    // Asset storage location
    public string assetPath;

    // Hand tracking
    public string handTracker;

    // Head tracking
    public string headTracker;

    // Current preset
    public string presetName;

    // Current avatar
    public string avatar;

    // Current environment
    public string environment;

    // Current props in scene
    public Prop[] props;

    // Use eye tracking?
    public bool eyeTracking;

    // Use eye blinking?
    public bool eyeBlinking;

    // Avatar floor height; probably put this in the avatar component?
    public double floorHeight;
    public double handHeight;
    public double armSpread;
    public double avatarScale;

    // SteamVR Tracking calibrations
    public SerializableVector3 leftHandPosition;
    public SerializableVector3 leftHandRotation;
    public SerializableVector3 rightHandPosition;
    public SerializableVector3 rightHandRotation;

    // Start is called before the first frame update
    void Start()
    {
        settingsPath = Application.persistentDataPath;

        // If settings file exists
        if(File.Exists(settingsPath + "/settings.cfg"))
        {
            LoadSettings();
            ApplySettings();
        }
        else
        {
            // First run
            assetPath = Application.persistentDataPath;
            if (!Directory.Exists(assetPath + "/Avatars"))
            {
                //if it doesn't, create it
                Directory.CreateDirectory(assetPath + "/Avatars");

            }

            if (!Directory.Exists(assetPath + "/Environments"))
            {
                //if it doesn't, create it
                Directory.CreateDirectory(assetPath + "/Environments");

            }

            if (!Directory.Exists(assetPath + "/Props"))
            {
                //if it doesn't, create it
                Directory.CreateDirectory(assetPath + "/Props");

            }

            if (!Directory.Exists(assetPath + "/Profiles"))
            {
                //if it doesn't, create it
                Directory.CreateDirectory(assetPath + "/Profiles");

            }
            DefaultSettings();
            SaveSettings();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Screen.fullScreen != fullscreen || screenWidth != Screen.width || screenHeight != Screen.height)
        {
            fullscreen = Screen.fullScreen;
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            SaveSettings();
        }

    }

    // Save last used preset for next launch
    private void OnApplicationQuit()
    {
        //TODO: Set last loaded preset
        SaveSettings();
    }

    public bool SaveSettings()
    {
        /*if (System.IO.File.Exists(settingsPath + "/settings.cfg"))
        {
            System.IO.File.Create(settingsPath + "/settings.cfg");
        }*/

        File.WriteAllText(settingsPath + "/settings.cfg", JsonUtility.ToJson(this));
        return true;
    }

    public bool LoadSettings()
    {
        Settings retrieved = null;
        try
        {
            retrieved = JsonUtility.FromJson<Settings>(File.ReadAllText(settingsPath + "/settings.cfg"));
        }
        catch(Exception e)
        {
            DefaultSettings();
            SaveSettings();
            return false;
        }

        // If settings file couldn't be loaded!!!
        if (retrieved == null)
        {
            // Create a new settings file
            DefaultSettings();
            SaveSettings();
            return false;
        }

        // Settings file loaded
        framerate           = retrieved.framerate;
        quality             = retrieved.quality;
        screenWidth         = retrieved.quality;
        screenHeight        = retrieved.quality;
        fullscreen          = retrieved.fullscreen;
        lastLoadedPreset    = retrieved.lastLoadedPreset;
        assetPath           = retrieved.assetPath;
        presetName          = retrieved.presetName;
        avatar              = retrieved.avatar;
        environment         = retrieved.environment;
        Prop[] props        = retrieved.props;
        eyeTracking         = retrieved.eyeTracking;
        eyeBlinking         = retrieved.eyeBlinking;
        leftHandPosition    = retrieved.leftHandPosition;
        leftHandRotation    = retrieved.leftHandRotation;
        rightHandPosition   = retrieved.rightHandPosition;
        rightHandRotation   = retrieved.rightHandRotation;
        floorHeight         = retrieved.floorHeight;
        handHeight          = retrieved.handHeight;
        armSpread           = retrieved.armSpread;
        avatarScale         = retrieved.avatarScale;
        return true;
    }

    public void ApplySettings()
    {
        Application.targetFrameRate = framerate;
        QualitySettings.SetQualityLevel(quality);
        Screen.SetResolution(screenWidth, screenHeight, fullscreen);
        VSAssetManager.Instance.assetPath = assetPath;
        // Load new avatar if not same as current
        // Load new environment if not same as current
        // Load props if not in scene
    }
    
    public void DefaultSettings()
    {
        // Create a new settings file
        framerate = Application.targetFrameRate;
        quality = QualitySettings.GetQualityLevel();
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;
        fullscreen = Screen.fullScreen;
        lastLoadedPreset = "";
        assetPath = Application.persistentDataPath;
        VSAssetManager.Instance.assetPath = assetPath;
        presetName = "";
        RestoreDefaultAvatar();
        RestoreDefaultEnvironment();
        RestoreDefaultProp();
        avatar = "Sample-Yell";
        VSAssetManager.Instance.Load(VSAssetManager.AssetType.Avatar, avatar);
        environment = "";
        Prop[] props = new Prop[0];
        handTracker = "None";
        headTracker = "None";
        eyeTracking = true;
        eyeBlinking = true;
        leftHandPosition = Vector3.zero;
        leftHandRotation = Vector3.zero;
        rightHandPosition = Vector3.zero;
        rightHandRotation = Vector3.zero;
        floorHeight = 0.5;
        handHeight = 0.0;
        armSpread = 2.0;
        avatarScale = 1.0;
    }

    public void RestoreDefaultAvatar()
    {
        File.Copy(Application.streamingAssetsPath + "/avatar/Sample-Yell.avatar.vstream", assetPath + "/Avatars/Sample-Yell.avatar.vstream", true);
    }

    public void RestoreDefaultEnvironment()
    {
        File.Copy(Application.streamingAssetsPath + "/environment/sample-bedroom.environment.vstream", assetPath + "/Environments/sample-bedroom.environment.vstream", true);
    }

    public void RestoreDefaultProp()
    {
        File.Copy(Application.streamingAssetsPath + "/prop/sample-hat.prop.vstream", assetPath + "/Props/sample-hat.prop.vstream", true);
    }
}