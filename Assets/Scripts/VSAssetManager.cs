using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class VSAssetManager : Singleton<VSAssetManager>
{
    public enum AssetType
    {
        None,
        Avatar,
        Environment,
        Prop,
        Profile
    }
    public string assetPath;
    public string selected;
    private UIManager UIManager;
    private Tracking tracking;
    protected VSAssetManager() { }
    public void Start()
    {
        UIManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        tracking = GameObject.Find("Tracking").GetComponent<Tracking>();
    }
    public bool Import(AssetType type)
    {
        string[] files = SFB.StandaloneFileBrowser.OpenFilePanel("Select " + type.ToString() + " File", "", type.ToString().ToLower() + ".vstream", true);
        foreach(string file in files)
        {
            string[] fileStuff = file.Split('\\');
            string filename = fileStuff[fileStuff.Length-1];
            //TODO: Check if file already exists and get user input
            // For now just overwrites
            File.Copy(file, assetPath+"/" + type.ToString() + "s/"+filename, true);
        }
        UIManager.Populate(type.ToString().ToLower());
        return true;
    }

    public bool SaveProfile(string profileName)
    {
        Preset newPreset = new Preset();
        newPreset.presetName = profileName;
        newPreset.eyeBlinking = Settings.Instance.eyeBlinking;
        newPreset.eyeTracking = Settings.Instance.eyeTracking;
        newPreset.leftHandPosition = Settings.Instance.leftHandPosition;
        newPreset.rightHandPosition = Settings.Instance.rightHandPosition;
        newPreset.props = Settings.Instance.props;
        newPreset.avatar = Settings.Instance.avatar;
        newPreset.environment = Settings.Instance.environment;
        newPreset.SavePreset();
        return true;
    }

    public bool Delete(AssetType type, string name)
    {
        DirectoryInfo dir = new DirectoryInfo(assetPath + "/" + type.ToString() + "s");
        FileInfo[] files = dir.GetFiles("*." + type.ToString().ToLower() + ".vstream");
        foreach (FileInfo file in files)
        {
            if(file.Name == name + "." + type.ToString().ToLower() + ".vstream")
            {
                File.Delete(assetPath+ "/" + file.Name);
                UIManager.Populate(type.ToString().ToLower());
                return true;
            }
        }
        return false;
    }

    public GameObject Load(AssetType type, string name)
    {
        // Profiles
        if(type == AssetType.Profile)
        {
            Preset newPreset = new Preset();
            newPreset.LoadPreset(name);
            return null;
        }

        // Environments
        if (name == "None" && type == AssetType.Environment)
        {
            Destroy(GameObject.FindGameObjectWithTag("Environment"));
            GameObject mainCam = GameObject.FindGameObjectWithTag("MainCamera");
            mainCam.transform.position = new Vector3(0f, 0.7f, -1.5f);
            mainCam.transform.rotation = Quaternion.Euler(0, 0, 0);
            return null;
        }
        DirectoryInfo dir = new DirectoryInfo(assetPath + "/" + type.ToString() + "s");
        FileInfo[] files = dir.GetFiles("*." + type.ToString().ToLower() + ".vstream");
        GameObject loadedAsset = null;
        foreach (FileInfo file in files)
        {
            if (file.Name == name + "." + type.ToString().ToLower() + ".vstream")
            {
                AssetBundle asset = AssetBundle.LoadFromFile(file.FullName);
                string objectName = asset.name.Replace("." + type.ToString().ToLower() + ".vstream", "");
                GameObject tempObject = asset.LoadAsset<GameObject>(objectName);
                asset.Unload(false);
                //TODO: Load thing
                switch (type)
                {
                    // Load an avatar
                    case AssetType.Avatar:
                        GameObject newObject = Instantiate(tempObject, new Vector3(0, 0, 0), Quaternion.Euler(0f, 180f, 0f));
                        GameObject oldAvatar = GameObject.FindGameObjectWithTag("Player");
                        Destroy(oldAvatar);
                        newObject.tag = "Player";
                        newObject.AddComponent<RootMotion.FinalIK.VRIK>();
                        Tracking.TrackingType head = Tracking.TrackingType.None;
                        Tracking.TrackingType hand = Tracking.TrackingType.None;

                        if (Settings.Instance.headTracker == "None")
                        {
                            head = Tracking.TrackingType.None;
                        }
                        else if (Settings.Instance.headTracker == "OpenCV")
                        {
                            head = Tracking.TrackingType.OpenCV;
                        }
                        else if (Settings.Instance.headTracker == "SteamVR")
                        {
                            head = Tracking.TrackingType.SteamVR;
                        }
                        else if (Settings.Instance.headTracker == "TobiiEyeTracker")
                        {
                            head = Tracking.TrackingType.TobiiEyeTracker;
                        }

                        tracking.SetHead(head);
                        if (Settings.Instance.handTracker == "None")
                        {
                            hand = Tracking.TrackingType.None;
                        }
                        else if (Settings.Instance.handTracker == "SteamVR")
                        {
                            hand = Tracking.TrackingType.SteamVR;
                        }
                        else if (Settings.Instance.handTracker == "LeapMotion")
                        {
                            hand = Tracking.TrackingType.LeapMotion;
                        }

                        tracking.SetHands(hand);

                        // If there's an environment loaded
                        GameObject env = GameObject.FindGameObjectWithTag("Environment");
                        if(env != null)
                        {
                            Transform newObjT = env.GetComponent<VS_EnvironmentDescriptor>().avatarLocation.transform;
                            newObjT.Translate(new Vector3(0, -1f, 0));
                            newObjT.RotateAround(newObjT.position, newObjT.up, 180);
                            Tracking.Instance.noneObject.transform.position = newObjT.position + new Vector3(0, (float)Settings.Instance.floorHeight, 0);
                            Tracking.Instance.steamVRObject.transform.position = newObjT.position + new Vector3(0, (float)Settings.Instance.floorHeight, 0);
                            Tracking.Instance.openCVObject.transform.position = newObjT.position + new Vector3(0, (float)Settings.Instance.floorHeight, 0);
                            newObject.transform.position = newObjT.position;

                            Tracking.Instance.noneObject.transform.rotation = newObjT.rotation;
                            Tracking.Instance.openCVObject.transform.rotation = newObjT.rotation;
                            Tracking.Instance.steamVRObject.transform.rotation = newObjT.rotation;
                            Tracking.Instance.steamVRObject.transform.RotateAround(Tracking.Instance.steamVRObject.transform.position, Tracking.Instance.steamVRObject.transform.up, 180);
                            newObject.transform.rotation = newObjT.rotation;
                        }
                        loadedAsset = newObject;
                        break;
                    // Load an environment
                    case AssetType.Environment:
                        GameObject newObjectE = Instantiate(tempObject, tempObject.transform.position, tempObject.transform.rotation);
                        GameObject camera = GameObject.FindGameObjectWithTag("MainCamera");
                        GameObject oldEnvironment = GameObject.FindGameObjectWithTag("Environment");
                        newObjectE.tag = "Environment";
                        GameObject avatar = GameObject.FindGameObjectWithTag("Player");
                        // Can maybe do this in export script instead
                        newObjectE.GetComponent<VS_EnvironmentDescriptor>().avatarLocation.SetActive(false);
                        if (avatar != null)
                        {
                            Transform newObjT = newObjectE.GetComponent<VS_EnvironmentDescriptor>().avatarLocation.transform;
                            newObjT.Translate(new Vector3(0,-1f,0));
                            newObjT.RotateAround(newObjT.position, newObjT.up, 180);
                            Tracking.Instance.noneObject.transform.position = newObjT.position + new Vector3(0, (float)Settings.Instance.floorHeight, 0);
                            Tracking.Instance.steamVRObject.transform.position = newObjT.position + new Vector3(0, (float)Settings.Instance.floorHeight, 0);
                            Tracking.Instance.openCVObject.transform.position = newObjT.position + new Vector3(0, (float)Settings.Instance.floorHeight, 0);
                            avatar.transform.position = newObjT.position; 

                            Tracking.Instance.noneObject.transform.rotation = newObjT.rotation;
                            Tracking.Instance.openCVObject.transform.rotation = newObjT.rotation;
                            Tracking.Instance.steamVRObject.transform.rotation = newObjT.rotation;
                            Tracking.Instance.steamVRObject.transform.RotateAround(Tracking.Instance.steamVRObject.transform.position, Tracking.Instance.steamVRObject.transform.up, 180);
                            avatar.transform.rotation = newObjT.rotation;
                        }

                        GameObject newCamera = newObjectE.GetComponent<VS_EnvironmentDescriptor>().camera.gameObject;
                        camera.transform.position = newCamera.transform.position;
                        camera.transform.rotation = newCamera.transform.rotation;
                        Destroy(newCamera);
                        loadedAsset = newObjectE;
                        break;
                    case AssetType.Prop:
                        GameObject newObjectP = Instantiate(tempObject, tempObject.transform.position, tempObject.transform.rotation);
                        newObjectP.tag = "Prop";
                        VS_PropDescriptor.ObjectType propType = newObjectP.GetComponent<VS_PropDescriptor>().type;
                        Prop prop = newObjectP.AddComponent<Prop>();
                        GameObject avatarProp = GameObject.FindGameObjectWithTag("Player");
                        prop.name = objectName;
                        switch (propType) {
                            case VS_PropDescriptor.ObjectType.Head:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.Head;
                                break;
                            case VS_PropDescriptor.ObjectType.Hip:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.Hips;
                                break;
                            case VS_PropDescriptor.ObjectType.LeftHand:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftHand);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.LeftHand;
                                break;
                            case VS_PropDescriptor.ObjectType.LeftShoulder:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftShoulder);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.LeftShoulder;
                                break;
                            case VS_PropDescriptor.ObjectType.Neck:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Neck);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.Neck;
                                break;
                            case VS_PropDescriptor.ObjectType.RightHand:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.RightHand;
                                break;
                            case VS_PropDescriptor.ObjectType.RightShoulder:
                                newObjectP.transform.parent = avatarProp.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightShoulder);
                                prop.attachedToSomething = true;
                                prop.attachedBone = HumanBodyBones.RightShoulder;
                                break;
                            case VS_PropDescriptor.ObjectType.Other:
                                // probably nothing
                                prop.attachedToSomething = false;
                                break;
                        }
                        newObjectP.transform.localPosition = Vector3.zero;
                        newObjectP.transform.localEulerAngles = Vector3.zero;
                        prop.scale = newObjectP.transform.localScale;
                        loadedAsset = newObjectP;

                        break;
                }
                return loadedAsset;
            }
        }
        return null;
    }

    public void UnequipProp(string propName)
    {
        Prop[] props = FindObjectsOfType<Prop>();
        foreach(Prop prop in props)
        {
            if(prop.name == propName)
            {
                Destroy(prop.gameObject);
                return;
            }
        }
    }

    public void AdjustProp(Prop prop, HumanBodyBones attachedBone,  bool attachedToSomething, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        prop.attachedBone = attachedBone;
        prop.attachedToSomething = attachedToSomething;
        prop.positionOffset = position;
        prop.rotationOffset = rotation;
        prop.scale = scale;

        prop.UpdateProp();
    }
}
