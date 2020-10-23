/*
 *  This is the preset script.
 *  It is used for saving preset data.
 *  Basically everything except tracking settings.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;

[System.Serializable]
public struct PropSettings 
{
    public string propName;
    public SerializableVector3 scale;
    public SerializableVector3 positionOffset;
    public SerializableVector3 rotationOffset;
    public HumanBodyBones boneAttachedTo;
    public bool attachedToSomething;
}
[System.Serializable]
public class Preset
{
    private string settingsPath = Application.persistentDataPath;
    public string presetName;
    public string avatar;
    public string environment;
    //public Prop[] props;
    public PropSettings[] propSettings;

    /*public string headTracking;
    public string handTracking;*/

    public SerializableVector3 headPosition;
    public SerializableVector3 headRotation;
    public SerializableVector3 leftHandPosition;
    public SerializableVector3 leftHandRotation;
    public SerializableVector3 rightHandPosition;
    public SerializableVector3 rightHandRotation;

    public string dateTime;

    public bool SavePreset()
    {
        // Get all things in scene and save to preset
        Prop[] props = Object.FindObjectsOfType<Prop>();
        propSettings = new PropSettings[props.Length];
        for(int i = 0; i < props.Length; i++)
        {
            propSettings[i].propName = props[i].name;
            propSettings[i].scale = props[i].scale;
            propSettings[i].positionOffset = props[i].positionOffset;
            propSettings[i].rotationOffset = props[i].rotationOffset;
            propSettings[i].boneAttachedTo = props[i].attachedBone;
            propSettings[i].attachedToSomething = props[i].attachedToSomething;
        }
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
            if(file.Name == name + ".preset.vstream")
            {
                JsonUtility.FromJsonOverwrite(File.ReadAllText(settingsPath + "/Presets/" + name + ".preset.vstream"), this);
                // If settings file couldn't be loaded!!!
                /*if (retrieved == null)
                {
                    return false;
                }*/

                Settings.Instance.presetName = presetName;
                Settings.Instance.avatar = avatar;
                Settings.Instance.environment = environment;
                Settings.Instance.props = propSettings;
                // To Do: Avatar, environment settings

                VSAssetManager.Instance.Load(VSAssetManager.AssetType.Avatar, avatar);


                VSAssetManager.Instance.Load(VSAssetManager.AssetType.Environment, environment);
                // Spawn props and apply their settings
                foreach (PropSettings prop in propSettings)
                {
                    GameObject loadedObject = VSAssetManager.Instance.Load(VSAssetManager.AssetType.Prop, prop.propName);
                    Prop propInScene = loadedObject.GetComponent<Prop>();
                    propInScene.scale = prop.scale;
                    propInScene.positionOffset = prop.positionOffset;
                    propInScene.rotationOffset = prop.rotationOffset;
                    propInScene.attachedBone = prop.boneAttachedTo;
                    propInScene.attachedToSomething = prop.attachedToSomething;
                    propInScene.UpdateProp();
                }

                //TODO: Set things in scene
                Settings.Instance.headPosition = headPosition;
                Settings.Instance.headRotation = headRotation;
                Settings.Instance.leftHandPosition = leftHandPosition;
                Settings.Instance.leftHandRotation = leftHandRotation;
                Settings.Instance.rightHandPosition = rightHandPosition;
                Settings.Instance.rightHandRotation = rightHandRotation;
                Settings.Instance.SaveSettings();
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
