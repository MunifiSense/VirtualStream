using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Preset
{
    private string settingsPath = Application.persistentDataPath;
    public string presetName;
    public string avatar;
    public string environment;
    public Prop[] props;
    public bool eyeTracking;
    public bool eyeBlinking;

    /*public string headTracking;
    public string handTracking;*/

    public SerializableVector3 leftHandPosition;
    public SerializableVector3 leftHandRotation;
    public SerializableVector3 rightHandPosition;
    public SerializableVector3 rightHandRotation;

    public string dateTime;

    public bool SavePreset()
    {
        dateTime = System.DateTime.Now.ToString();
        File.WriteAllText(settingsPath + "/Presets/" + presetName + ".preset.vstream", JsonUtility.ToJson(this));
        return true;
    }

    public bool LoadPreset(string name)
    {
        DirectoryInfo dir = new DirectoryInfo(settingsPath + "/Presets");
        FileInfo[] files = dir.GetFiles("*.preset.vstream");
        foreach (FileInfo file in files)
        {
            if(file.Name == name+ ".preset.vstream")
            {
                Preset retrieved = JsonUtility.FromJson<Preset>(File.ReadAllText(settingsPath + "/Presets/" + presetName + ".preset.vstream"));
                // If settings file couldn't be loaded!!!
                if (retrieved == null)
                {
                    return false;
                }

                Settings.Instance.presetName = retrieved.presetName;
                Settings.Instance.avatar = retrieved.avatar;
                Settings.Instance.environment = retrieved.environment;
                Settings.Instance.props = retrieved.props;

                VSAssetManager.Instance.Load(VSAssetManager.AssetType.Avatar, avatar);
                VSAssetManager.Instance.Load(VSAssetManager.AssetType.Environment, environment);

                Settings.Instance.eyeTracking = retrieved.eyeTracking;
                Settings.Instance.eyeBlinking = retrieved.eyeBlinking;

                // Not sure if setting tracking by preset is a good idea...
                /*
                headTracking = retrieved.headTracking;
                handTracking = retrieved.handTracking;

                Tracking.TrackingType head = Tracking.TrackingType.None;
                Tracking.TrackingType hand = Tracking.TrackingType.None;

                if (headTracking == "OpenCV")
                {
                    head = Tracking.TrackingType.OpenCV;
                }
                else if (headTracking == "SteamVR")
                {
                    head = Tracking.TrackingType.SteamVR;
                }
                else if (headTracking == "TobiiEyeTracker")
                {
                    head = Tracking.TrackingType.TobiiEyeTracker;
                }

                Tracking.Instance.SetHead(head);

                if (Settings.Instance.headTracker == "SteamVR")
                {
                    hand = Tracking.TrackingType.SteamVR;
                }
                else if (Settings.Instance.headTracker == "LeapMotion")
                {
                    hand = Tracking.TrackingType.LeapMotion;
                }

                Tracking.Instance.SetHands(hand);*/
                //TODO: Set things in scene
                Settings.Instance.leftHandPosition = retrieved.leftHandPosition;
                Settings.Instance.leftHandRotation = retrieved.leftHandRotation;
                Settings.Instance.rightHandPosition = retrieved.rightHandPosition;
                Settings.Instance.rightHandRotation = retrieved.rightHandRotation;
                Settings.Instance.ApplySettings();
                return true;
            }
        }
        return false;
    }

    /*public List<Preset> ListPresets()
    {
        List<Preset> presets = new List<Preset>();
        DirectoryInfo dir = new DirectoryInfo(settingsPath + "/Presets");
        FileInfo[] files = dir.GetFiles("*.preset.vstream");
        foreach (FileInfo file in files)
        {
            Preset tempPreset = new Preset();
            //tempPreset.LoadPreset(file.Name);
            presets.Add(tempPreset);
        }

        return presets;
    }*/
}
