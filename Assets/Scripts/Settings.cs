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
    public int windowMode;

    // Last used preset
    //public string lastLoadedPreset;

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
    public PropSettings[] props;

    // Use eye tracking?
    public bool eyeTracking;

    // Use eye blinking?
    public bool eyeBlinking;

    // Use hand tracking?
    //public bool handTracking;

    // Use head tracking?
    //public bool headTracking;

    // Webcam
    public string webcam;

    // Avatar floor height; probably put this in the avatar component?
    public double floorHeight;
    public double handHeight;
    public double armSpread;
    public double avatarScale;

    // SteamVR Tracking calibrations
    public SerializableVector3 headPosition;
    public SerializableVector3 headRotation;
    public SerializableVector3 leftHandPosition;
    public SerializableVector3 leftHandRotation;
    public SerializableVector3 rightHandPosition;
    public SerializableVector3 rightHandRotation;

    // SteamVR Tracker Index
    public int SteamVRHeadTracker;
    public int SteamVRLeftHandTracker;
    public int SteamVRRightHandTracker;

    // For when hands are detected on the webcam
    public double webcamHandHeight;

    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
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

            if (!Directory.Exists(assetPath + "/Presets"))
            {
                //if it doesn't, create it
                Directory.CreateDirectory(assetPath + "/Presets");

            }
            DefaultSettings();
            SaveSettings();
            ApplySettings();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if( screenWidth != Screen.width || screenHeight != Screen.height)
        {
            //fullscreen = Screen.fullScreen;
            screenWidth = Screen.width;
            screenHeight = Screen.height;
            SaveSettings();
        }

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
        //Settings retrieved = new Settings();
        try
        {
            JsonUtility.FromJsonOverwrite(File.ReadAllText(settingsPath + "/settings.cfg"), this);
        }
        catch(Exception e)
        {
            DefaultSettings();
            SaveSettings();
            return false;
        }
        /*
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
        screenWidth         = retrieved.screenWidth;
        screenHeight        = retrieved.screenHeight;
        windowMode          = retrieved.windowMode;
        //lastLoadedPreset    = retrieved.lastLoadedPreset;
        assetPath           = retrieved.assetPath;
        presetName          = retrieved.presetName;
        avatar              = retrieved.avatar;
        environment         = retrieved.environment;
        Prop[] props        = retrieved.props;
        eyeTracking         = retrieved.eyeTracking;
        eyeBlinking         = retrieved.eyeBlinking;
        headTracker         = retrieved.headTracker;
        handTracker         = retrieved.handTracker;
        handTracking        = retrieved.handTracking;
        headTracking        = retrieved.headTracking;
        headPosition        = retrieved.headPosition;
        headRotation        = retrieved.headRotation;
        leftHandPosition    = retrieved.leftHandPosition;
        leftHandRotation    = retrieved.leftHandRotation;
        rightHandPosition   = retrieved.rightHandPosition;
        rightHandRotation   = retrieved.rightHandRotation;
        floorHeight         = retrieved.floorHeight;
        handHeight          = retrieved.handHeight;
        armSpread           = retrieved.armSpread;
        avatarScale         = retrieved.avatarScale;
        quality             = retrieved.quality;
        screenWidth         = retrieved.screenWidth;
        screenHeight        = retrieved.screenHeight;
        windowMode          = retrieved.windowMode;
        webcam              = retrieved.webcam;
        framerate           = retrieved.framerate;
        webcamHandHeight    = retrieved.webcamHandHeight;
        */
        return true;
    }

    public void ApplySettings()
    {
        Application.targetFrameRate = framerate;
        QualitySettings.SetQualityLevel(quality);
        Screen.SetResolution(screenWidth, screenHeight, (FullScreenMode)3);
        VSAssetManager.Instance.assetPath = assetPath;
        GameObject tempAvatar = VSAssetManager.Instance.Load(VSAssetManager.AssetType.Avatar, avatar);
        //((OpenCVTracking)(Resources.FindObjectsOfTypeAll(typeof(OpenCVTracking))[0])).SetWebcamDevice(webcam);
        Tracking.Instance.UpdateTrackerList();
        if (headTracker == "Webcam")
        {
            Tracking.Instance.SetHead(Tracking.TrackingType.OpenCV);
        }
        else if (headTracker == "SteamVR")
        {
            Tracking.Instance.SetHead(Tracking.TrackingType.SteamVR);
        }
        else if (headTracker == "Tobii Eye Tracker")
        {
            Tracking.Instance.SetHead(Tracking.TrackingType.TobiiEyeTracker);
        }

        if (handTracker == "SteamVR")
        {
            Tracking.Instance.SetHands(Tracking.TrackingType.SteamVR);
        }
        else if (handTracker == "Webcam")
        {
            Tracking.Instance.SetHands(Tracking.TrackingType.OpenCV);
        }
        else if (handTracker == "Leap Motion")
        {
            Tracking.Instance.SetHands(Tracking.TrackingType.LeapMotion);
        }

        if(headTracker == "SteamVR" || handTracker == "SteamVR")
        {
            UIManager.Instance.EnableCalibrationPanel();
        }
        else
        {
            UIManager.Instance.DisableCalibrationPanel();
        }

        VSAssetManager.Instance.Load(VSAssetManager.AssetType.Environment, environment);

        // Load props if not in scene
        Prop[] sceneProps = FindObjectsOfType<Prop>();
        bool propFound = false;
        if(sceneProps.Length > 0)
        {
            foreach (PropSettings prop in props)
            {
                propFound = false;
                foreach (Prop sceneProp in sceneProps)
                {
                    if(prop.propName == sceneProp.name)
                    {
                        propFound = true;
                        VSAssetManager.Instance.AdjustProp(sceneProp, prop.boneAttachedTo, prop.attachedToSomething, prop.positionOffset, prop.rotationOffset, prop.scale);
                        break;
                    }
                }
                // Prop isn't in scene! New prop!
                if (!propFound)
                {
                    VSAssetManager.Instance.Load(VSAssetManager.AssetType.Prop, prop.propName);
                }
            }
        }
        else
        {
            foreach (PropSettings prop in props)
            {
                Prop propInScene = VSAssetManager.Instance.Load(VSAssetManager.AssetType.Prop, prop.propName).GetComponent<Prop>();
                propInScene.scale = prop.scale;
                propInScene.positionOffset = prop.positionOffset;
                propInScene.rotationOffset = prop.rotationOffset;
                propInScene.attachedBone = prop.boneAttachedTo;
                propInScene.attachedToSomething = prop.attachedToSomething;
                propInScene.UpdateProp();
            }
        }
        // Set SteamVR devices
        Tracking.Instance.SetSteamVRTrackerVRIndex("head", SteamVRHeadTracker);
        Tracking.Instance.SetSteamVRTrackerVRIndex("leftHand", SteamVRLeftHandTracker);
        Tracking.Instance.SetSteamVRTrackerVRIndex("rightHand", SteamVRRightHandTracker);
        // Set SteamVR Tracking offsets
        Tracking.Instance.headPositionOffset = headPosition;
        Tracking.Instance.headRotationOffset = headRotation;
        Tracking.Instance.leftHandPositionOffset = leftHandPosition;
        Tracking.Instance.leftHandRotationOffset = leftHandRotation;
        Tracking.Instance.rightHandPositionOffset = rightHandPosition;
        Tracking.Instance.rightHandRotationOffset = rightHandRotation;
        Tracking.Instance.ApplySteamVROffsets();

        // Resolution and stuff
        Screen.SetResolution(screenWidth, screenHeight, (FullScreenMode)windowMode, framerate);
        QualitySettings.SetQualityLevel(quality);

        UIManager.Instance.webcamButton.SetActive(headTracker == "Webcam" || handTracker == "Webcam");

        Tracking.Instance.SetLegHeight(floorHeight);
        Tracking.Instance.SetHandHeight(handHeight);
        Tracking.Instance.SetArmSpread(armSpread);
        tempAvatar.transform.localScale = new Vector3((float)avatarScale, (float)avatarScale, (float)avatarScale);
    }
    
    public void DefaultSettings()
    {
        // Create a new settings file
        framerate = 60;
        quality = 2;
        screenWidth = Screen.currentResolution.width;
        screenHeight = Screen.currentResolution.height;
        windowMode = (int)FullScreenMode.Windowed;
        //lastLoadedPreset = "";
        assetPath = Application.persistentDataPath;
        VSAssetManager.Instance.assetPath = assetPath;
        presetName = "";
        RestoreDefaultAvatar();
        RestoreDefaultEnvironment();
        RestoreDefaultProp();
        avatar = "sample-yell";
        VSAssetManager.Instance.Load(VSAssetManager.AssetType.Avatar, avatar);
        environment = "";
        Prop[] props = new Prop[0];
        handTracker = "None";
        headTracker = "None";
        eyeTracking = false;
        eyeBlinking = false;
        //headTracking = true;
        //handTracking = true;
        leftHandPosition = Vector3.zero;
        leftHandRotation = Vector3.zero;
        rightHandPosition = Vector3.zero;
        rightHandRotation = Vector3.zero;
        floorHeight = 0.0;
        handHeight = 0.788;
        armSpread = 0.3;
        avatarScale = 1.0;
        screenWidth = 1280;
        screenHeight = 720;
        windowMode = 3;
        webcam = "None";
        webcamHandHeight = 0.888;
    }

    public void SaveProps()
    {
        GameObject[] sceneProps = GameObject.FindGameObjectsWithTag("Prop");
        props = new PropSettings[sceneProps.Length];
        for (int i = 0; i < sceneProps.Length; i++)
        {
            props[i].propName = sceneProps[i].GetComponent<Prop>().name;
            props[i].scale = sceneProps[i].GetComponent<Prop>().scale;
            props[i].positionOffset = sceneProps[i].GetComponent<Prop>().positionOffset;
            props[i].rotationOffset = sceneProps[i].GetComponent<Prop>().rotationOffset;
            props[i].boneAttachedTo = sceneProps[i].GetComponent<Prop>().attachedBone;
            props[i].attachedToSomething = sceneProps[i].GetComponent<Prop>().attachedToSomething;
        }
    }

    public void RestoreDefaultAvatar()
    {
        File.Copy(Application.streamingAssetsPath + "/avatar/sample-yell.avatar.vstream", assetPath + "/Avatars/sample-yell.avatar.vstream", true);
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