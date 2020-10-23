using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public RectTransform windowSelectPanel;
    public GameObject mainPanel;
    public GameObject offsetPanel;
    public GameObject avatarPanel;
    public GameObject environmentPanel;
    public GameObject propPanel;
    public GameObject offsetPropPanel;
    public GameObject presetPanel;
    public GameObject adjustPropPanel;
    public GameObject trackingPanel;
    public GameObject settingsPanel;
    public GameObject calibrationButton;
    public GameObject trackingButton;

    public Button importAvatar;
    public Button importEnvironment;
    public Button importProp;
    public Button savePreset;

    public Button equipProp;
    public Button unequipProp;
    public Button adjustProp;
    public Button adjustPropConfirm;
    public Button calibrate;
    public Button startOffsetEditor;
    public Button finishOffsetEditor;
    public Button cancelOffsetEditor;
    public Button resetOffsetEditor;
    public Button startPropOffsetEditor;
    public Button finishPropOffsetEditor;
    public Button cancelPropOffsetEditor;
    public Button resetPropOffsetEditor;
    public TMP_InputField posX;
    public TMP_InputField posY;
    public TMP_InputField posZ;
    public TMP_InputField rotX;
    public TMP_InputField rotY;
    public TMP_InputField rotZ;
    public TMP_InputField scaleX;
    public TMP_InputField scaleY;
    public TMP_InputField scaleZ;
    public Toggle attachedToSomething;
    public TMP_Dropdown bonesDropdown;
    public TMP_Dropdown trackersListHead;
    public TMP_Dropdown trackersListLeftHand;
    public TMP_Dropdown trackersListRightHand;

    // Settings
    public TMP_Dropdown headTracker;
    public TMP_Dropdown handTracker;
    public TMP_Dropdown eyeTracking;
    public TMP_Dropdown eyeBlinking;
    public TMP_Dropdown renderQuality;
    public TMP_Dropdown windowMode;
    public TMP_Dropdown resolution;
    public TMP_Dropdown webcam;
    public TMP_InputField floorHeight;
    public TMP_InputField handHeight;
    public TMP_InputField avatarScale;
    public TMP_InputField frameRate;
    public TMP_InputField armSpread;
    public GameObject settingsWarnPanel;
    public Button settingsSave;
    public Button settingsClose;
    public Button settingsConfirm;
    public Button settingsNoSave;
    public Button testWebcam;
    public GameObject webcamButton;
    public GameObject webcamTestPanel;
    public TMP_InputField webcamHandHeight;
    public Button webcamTestingStart;
    public Button webcamTestingStop;
    public Button closeWebcamTesting;

    public GameObject menuObject;
    public GameObject presetObject;
    public GameObject presetNameEntry;
    public TMP_InputField presetName;
    //public Button presetOK;
    public float speed;
    public GameObject quitPanel;
    public Button quitConfirm;
    public Button exitApp;

    private static bool readyToQuit;
    // Start is called before the first frame update

    [RuntimeInitializeOnLoadMethod]
    static void RunOnStart()
    {
        Application.wantsToQuit += WantsToQuit;
    }

    void Start()
    {
        // Update the tracker list whenever there's a new device plugged in
        Valve.VR.SteamVR_Events.DeviceConnected.AddListener(delegate {
            UpdateTrackersList();
        });
        importAvatar.onClick.AddListener(() => {
            VSAssetManager.Instance.Import(VSAssetManager.AssetType.Avatar);
        });
        importEnvironment.onClick.AddListener(() => {
            VSAssetManager.Instance.Import(VSAssetManager.AssetType.Environment);
        });
        importProp.onClick.AddListener(() => {
            VSAssetManager.Instance.Import(VSAssetManager.AssetType.Prop);
        });
        unequipProp.onClick.AddListener(() => {
            VSAssetManager.Instance.UnequipProp(VSAssetManager.Instance.selected);
        });
        savePreset.onClick.AddListener(() => {
            ValidateInput();
            Populate("preset");
        });
        adjustPropConfirm.onClick.AddListener(() => {
            ValidateAndAdjust();
            propPanel.transform.parent.parent.parent.gameObject.SetActive(true);
        });
        string[] bones = System.Enum.GetNames(typeof(HumanBodyBones));
        List<string> bonesList = new List<string>();
        foreach (string bone in bones)
        {
            bonesList.Add(bone);
        }
        bonesDropdown.AddOptions(bonesList);

        trackersListHead.onValueChanged.AddListener(delegate {
            Tracking.Instance.SetSteamVRTracker("head", trackersListHead.value);
        });
        trackersListLeftHand.onValueChanged.AddListener(delegate {
            Tracking.Instance.SetSteamVRTracker("leftHand", trackersListLeftHand.value);
        });
        trackersListRightHand.onValueChanged.AddListener(delegate {
            Tracking.Instance.SetSteamVRTracker("rightHand", trackersListRightHand.value);
        });
        calibrate.onClick.AddListener(delegate
        {
            Tracking.Instance.CalibrateTracking();
        });
        startOffsetEditor.onClick.AddListener(delegate
        {
            OpenOffsetPanel();

        });
        cancelOffsetEditor.onClick.AddListener(delegate
        {
            CloseOffsetPanel();
            Tracking.Instance.ApplySteamVROffsets();

        });
        finishOffsetEditor.onClick.AddListener(delegate
        {
            CloseOffsetPanel();
            Tracking.Instance.SetOffsets();

        });
        resetOffsetEditor.onClick.AddListener(delegate
        {
            Tracking.Instance.ResetOffsets();

        });
        startPropOffsetEditor.onClick.AddListener(delegate
        {
            OpenPropOffsetPanel();
        });
        cancelPropOffsetEditor.onClick.AddListener(delegate
        {
            ClosePropOffsetPanelAndReset();
        });
        finishPropOffsetEditor.onClick.AddListener(delegate
        {
            ClosePropOffsetPanel();
        });
        resetPropOffsetEditor.onClick.AddListener(delegate
        {
            ApplyPropOffsets();
            GetPropValues();
        });
        settingsSave.onClick.AddListener(delegate
        {
            SaveSettings();
            settingsPanel.SetActive(false);
            windowSelectPanel.gameObject.SetActive(true);
        });
        settingsClose.onClick.AddListener(delegate
        {
            CheckSettings();
        });
        settingsConfirm.onClick.AddListener(delegate
        {
            SaveSettings();
            settingsWarnPanel.SetActive(false);
            settingsPanel.SetActive(false);
            windowSelectPanel.gameObject.SetActive(true);
        });
        settingsNoSave.onClick.AddListener(delegate
        {
            settingsPanel.SetActive(false);
            settingsWarnPanel.SetActive(false);
            windowSelectPanel.gameObject.SetActive(true);
        });
        testWebcam.onClick.AddListener(delegate
        {
            if(Settings.Instance.webcam != "None")
            {
                webcamTestPanel.SetActive(true);
            }
        });
        quitConfirm.onClick.AddListener(delegate
        {
            QuitApp();
        });
        exitApp.onClick.AddListener(delegate
        {
            QuitApp();
        });

        webcamTestingStart.onClick.AddListener(delegate
        {
            OpenCVTracking opencvTracking = Tracking.Instance.openCVObject.GetComponent<OpenCVTracking>();
            opencvTracking.SetTestingMode(true);
            opencvTracking.StartWebcamTracking();
        });
        webcamTestingStop.onClick.AddListener(delegate
        {
            OpenCVTracking opencvTracking = Tracking.Instance.openCVObject.GetComponent<OpenCVTracking>();
            opencvTracking.SetTestingMode(false);
            opencvTracking.StopWebcamTracking();
        });
        closeWebcamTesting.onClick.AddListener(delegate
        {
            OpenCVTracking opencvTracking = Tracking.Instance.openCVObject.GetComponent<OpenCVTracking>();
            opencvTracking.SetTestingMode(false);
            opencvTracking.StopWebcamTracking();
        });
    }

    // Update is called once per frame
    void Update()
    {
        // Left panel
        if (Input.mousePosition.x <= Screen.width * 0.05 && Input.mousePosition.x >= 0 && Input.mousePosition.y <= Screen.height && Input.mousePosition.y >= 0)
        {
            if (windowSelectPanel.transform.position.x < 82.0f)
            {
                windowSelectPanel.gameObject.SetActive(true);
                windowSelectPanel.transform.Translate(speed, 0f, 0f);
            }
        }
        else if(!windowSelectPanel.gameObject.GetComponent<DetectMouse>().mouseIsOver)
        {
            if (windowSelectPanel.transform.position.x > -100.0f)
            {
                windowSelectPanel.transform.Translate(-speed, 0f, 0f);
            }
            else
            {
                windowSelectPanel.gameObject.SetActive(false);
            }
        }
    }


    public void Populate(string type)
    {
        GameObject menuObjectToUse = new GameObject();
        VSAssetManager.Instance.selected = "";
        DirectoryInfo dir = new DirectoryInfo(VSAssetManager.Instance.assetPath + "/" + type + "s");
        if (dir == null)
        {
            return;
        }
        FileInfo[] files = dir.GetFiles("*." + type + ".vstream");
        if (type == "avatar")
        {
            menuObjectToUse = menuObject;
            DeleteChildren(avatarPanel);
        }
        if (type == "environment")
        {
            menuObjectToUse = menuObject;
            DeleteChildren(environmentPanel);
            GameObject uiObject = Instantiate(menuObject);
            VSButton button = uiObject.GetComponent<VSButton>();
            button.type = VSAssetManager.AssetType.Environment;
            button.objectName = "None";
            button.SetButton();
            uiObject.transform.parent = environmentPanel.transform;
            uiObject.transform.localScale = Vector3.one;
            button.gameObject.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = "None";
        }
        if (type == "prop")
        {
            menuObjectToUse = menuObject;
            DeleteChildren(propPanel);
        }
        if(type == "preset")
        {
            menuObjectToUse = presetObject;
            DeleteChildren(presetPanel);
        }
        foreach (FileInfo file in files)
        {
            GameObject uiObject = Instantiate(menuObjectToUse);
            VSButton button = uiObject.GetComponent<VSButton>();
            string objectName = file.Name.Replace("." + type + ".vstream", "");
            if (type == "avatar")
            {
                button.type = VSAssetManager.AssetType.Avatar;
                button.objectName = objectName;
                button.SetButton();
                uiObject.transform.parent = avatarPanel.transform;
            }
            else if(type == "environment")
            {
                button.type = VSAssetManager.AssetType.Environment;
                button.objectName = objectName;
                button.SetButton();
                uiObject.transform.parent = environmentPanel.transform;
            }
            else if(type == "prop")
            {
                button.type = VSAssetManager.AssetType.Prop;
                button.objectName = objectName;
                button.SetButton();
                button.GetComponent<Button>().onClick.AddListener(() => {
                    CheckEquipped();
                });
                uiObject.transform.parent = propPanel.transform;
            }
            else if(type == "preset")
            {
                button.type = VSAssetManager.AssetType.Preset;
                button.objectName = objectName;
                button.SetButton();
                uiObject.transform.parent = presetPanel.transform;
            }

            if(type == "preset")
            {
                uiObject.transform.localScale = Vector3.one;
                button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = objectName;
            }
            else
            {
                AssetBundle asset = AssetBundle.LoadFromFile(file.FullName);
                uiObject.transform.localScale = Vector3.one;
                button.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = objectName;
                GameObject preview = asset.LoadAsset<GameObject>(objectName);
                GameObject previewObject = Instantiate(preview, new Vector3(-1000, -1000, -1000), Quaternion.Euler(0f, 0f, 0f));
                button.gameObject.transform.GetChild(0).GetComponent<RawImage>().texture =
                RuntimePreviewGenerator.GenerateModelPreview(previewObject.transform, 256, 256);
                asset.Unload(true);
                Destroy(preview);
                Destroy(previewObject);
            }
        }
    }

    public void DeleteChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    public void ValidateInput()
    {
        if(!(presetName.text == ""))
        {
            presetNameEntry.SetActive(false);
            VSAssetManager.Instance.SavePreset(presetName.text);
        }
    }

    public void LoadSettings()
    {
        for (int i = 0; i < headTracker.options.Count; i++)
        {
            if (headTracker.options[i].text == Settings.Instance.headTracker)
            {
                headTracker.value = i;
            }
        }
        for (int i =0; i< handTracker.options.Count; i++)
        {
            if(handTracker.options[i].text == Settings.Instance.handTracker)
            {
                handTracker.value = i;
            }
        }
        if (Settings.Instance.eyeTracking)
        {
            eyeTracking.value = 1;
        }
        else
        {
            eyeTracking.value = 0;
        }
        if (Settings.Instance.eyeBlinking)
        {
            eyeBlinking.value = 1;
        }
        else
        {
            eyeBlinking.value = 0;
        }
        floorHeight.text = Settings.Instance.floorHeight.ToString();
        handHeight.text = Settings.Instance.handHeight.ToString();
        avatarScale.text = Settings.Instance.avatarScale.ToString();
        int currentResolution = 0;
        resolution.ClearOptions();
        List<string> options = new List<string>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {   if(!options.Contains(Screen.resolutions[i].width + " x " + Screen.resolutions[i].height))
            {
                options.Add(Screen.resolutions[i].width + " x " + Screen.resolutions[i].height);
                if (Screen.resolutions[i].width == Screen.width && Screen.resolutions[i].height == Screen.height)
                {
                    currentResolution = options.Count-1;
                }
            }
        }
        resolution.AddOptions(options);
        resolution.value = currentResolution;
        resolution.RefreshShownValue();
        windowMode.value = Settings.Instance.windowMode;
        renderQuality.value = Settings.Instance.quality;
         int currentWebcam = 0;
        webcam.ClearOptions();
        options = new List<string>() {"None"};
        for (int i = 0; i < WebCamTexture.devices.Length; i++)
        {
            options.Add(WebCamTexture.devices[i].name);
            if (WebCamTexture.devices[i].name == Settings.Instance.webcam)
            {
                currentWebcam = i+1;
            }
        }
        webcam.AddOptions(options);
        webcam.value = currentWebcam;
        webcam.RefreshShownValue();
        webcamHandHeight.text = Settings.Instance.webcamHandHeight.ToString();
        armSpread.text = Settings.Instance.armSpread.ToString();
    }

    public void CheckSettings()
    {
        bool settingEye = false;
        if (eyeTracking.captionText.text == "On")
        {
            settingEye = true;
        }
        bool settingBlink = false;
        if (eyeBlinking.captionText.text == "On")
        {
            settingBlink = true;
        }
        // Make sure all settings match. If not, ask user if they want to save.
        if (Settings.Instance.headTracker != headTracker.captionText.text ||
            Settings.Instance.handTracker != handTracker.captionText.text ||
            Settings.Instance.eyeTracking != settingEye ||
            Settings.Instance.eyeBlinking != settingBlink ||
            Settings.Instance.floorHeight != double.Parse(floorHeight.text) ||
            Settings.Instance.handHeight != double.Parse(handHeight.text) ||
            Settings.Instance.avatarScale != double.Parse(avatarScale.text) ||
            Settings.Instance.screenWidth != int.Parse(resolution.captionText.text.Split('x')[0].Trim()) ||
            Settings.Instance.screenHeight != int.Parse(resolution.captionText.text.Split('x')[1].Trim()) ||
            Settings.Instance.windowMode != windowMode.value ||
            Settings.Instance.quality != renderQuality.value ||
            Settings.Instance.framerate != int.Parse(frameRate.text) ||
            Settings.Instance.webcam != webcam.captionText.text ||
            Settings.Instance.webcamHandHeight != double.Parse(webcamHandHeight.text) ||
            Settings.Instance.armSpread != double.Parse(armSpread.text))
        {
            settingsWarnPanel.SetActive(true);
        }
        else
        {
            windowSelectPanel.gameObject.SetActive(true);
            settingsPanel.SetActive(false);
        }
    }

    public void SaveSettings()
    {
        bool settingEye = false;
        if (eyeTracking.captionText.text == "On")
        {
            settingEye = true;
        }
        bool settingBlink = false;
        if (eyeBlinking.captionText.text == "On")
        {
            settingBlink = true;
        }
        Settings.Instance.headTracker = headTracker.captionText.text;
        Settings.Instance.handTracker = handTracker.captionText.text;
        Settings.Instance.eyeTracking = settingEye;
        Settings.Instance.eyeBlinking = settingBlink;
        Settings.Instance.floorHeight = double.Parse(floorHeight.text);
        Settings.Instance.handHeight = double.Parse(handHeight.text);
        Settings.Instance.avatarScale = double.Parse(avatarScale.text);
        Settings.Instance.windowMode = windowMode.value;
        Settings.Instance.quality = renderQuality.value;
        Settings.Instance.screenWidth = int.Parse(resolution.captionText.text.Split('x')[0].Trim());
        Settings.Instance.screenHeight = int.Parse(resolution.captionText.text.Split('x')[1].Trim());
        Settings.Instance.framerate = int.Parse(frameRate.text);
        Settings.Instance.webcam = webcam.captionText.text;
        Settings.Instance.webcamHandHeight = double.Parse(webcamHandHeight.text);
        Settings.Instance.armSpread = double.Parse(armSpread.text);
        Settings.Instance.SaveSettings();
        Settings.Instance.ApplySettings();
    }

    public bool CheckEquipped()
    {
        // Check if selected prop is in scene
        Prop[] props = FindObjectsOfType<Prop>();
        foreach(Prop prop in props)
        {
            if(prop.name == VSAssetManager.Instance.selected)
            {
                // Set the buttons to active
                equipProp.interactable = false;
                unequipProp.interactable = true;
                adjustProp.interactable = true;
                return true;
            }
        }
        equipProp.interactable = true;
        unequipProp.interactable = false;
        adjustProp.interactable = false;
        return false;
    }
    public void GetPropValues()
    {
        Prop[] props = FindObjectsOfType<Prop>();
        foreach (Prop prop in props)
        {
            if (prop.name == VSAssetManager.Instance.selected)
            {
                // Set current prop values
                posX.text = prop.positionOffset.x.ToString();
                posY.text = prop.positionOffset.y.ToString();
                posZ.text = prop.positionOffset.z.ToString();
                rotX.text = prop.rotationOffset.x.ToString();
                rotY.text = prop.rotationOffset.y.ToString();
                rotZ.text = prop.rotationOffset.z.ToString();
                scaleX.text = prop.scale.x.ToString();
                scaleY.text = prop.scale.y.ToString();
                scaleZ.text = prop.scale.z.ToString();
                attachedToSomething.isOn = prop.attachedToSomething;

                for(int i=0; i< bonesDropdown.options.Count; i++)
                {
                    if(bonesDropdown.options[i].text == prop.attachedBone.ToString())
                    {
                        bonesDropdown.value = i;
                        return;
                    }
                }
            }
        }
    }
    // For manually adjusting prop offsets
    public void ValidateAndAdjust()
    {
        // Validate prop adjust things
        // Check if selected prop is in scene
        Prop[] props = FindObjectsOfType<Prop>();
        foreach (Prop prop in props)
        {
            if (prop.name == VSAssetManager.Instance.selected)
            {
                float num;
                // Make sure all values are floats...
                if(float.TryParse(posX.text, out num) 
                    && float.TryParse(posY.text, out num)
                    && float.TryParse(posZ.text, out num)
                    && float.TryParse(rotX.text, out num)
                    && float.TryParse(rotY.text, out num)
                    && float.TryParse(rotZ.text, out num)
                    && float.TryParse(scaleX.text, out num)
                    && float.TryParse(scaleY.text, out num)
                    && float.TryParse(scaleZ.text, out num))
                {
                    VSAssetManager.Instance.AdjustProp(prop,
                        (HumanBodyBones)bonesDropdown.value,
                        attachedToSomething,
                        new Vector3(float.Parse(posX.text), float.Parse(posY.text), float.Parse(posZ.text)),
                        new Vector3(float.Parse(rotX.text), float.Parse(rotY.text), float.Parse(rotZ.text)),
                        new Vector3(float.Parse(scaleX.text), float.Parse(scaleY.text), float.Parse(scaleZ.text)));

                    propPanel.SetActive(true);
                    adjustPropPanel.SetActive(false);
                    Settings.Instance.SaveProps();
                    Settings.Instance.SaveSettings();
                }
                return;
            }
        }
    }

    // For using gizmo to adjust prop offsets
    public void StartPropGizmo()
    {
        // Validate prop adjust things
        // Check if selected prop is in scene
        Camera.main.GetComponent<RuntimeGizmos.TransformGizmo>().enabled = true;
        Prop[] props = FindObjectsOfType<Prop>();
        foreach (Prop prop in props)
        {
            if (prop.name == VSAssetManager.Instance.selected)
            {
                prop.StartGizmoEdit();
                return;
            }
        }
    }

    // For using gizmo to adjust prop offsets
    public void EndPropGizmo()
    {
        // Validate prop adjust things
        // Check if selected prop is in scene
        Camera.main.GetComponent<RuntimeGizmos.TransformGizmo>().enabled = false;
        Prop[] props = FindObjectsOfType<Prop>();
        foreach (Prop prop in props)
        {
            if (prop.name == VSAssetManager.Instance.selected)
            {
                prop.EndGizmoEdit();
                rotX.text = (prop.transform.localEulerAngles.x).ToString();
                rotY.text = (prop.transform.localEulerAngles.y).ToString();
                rotZ.text = (prop.transform.localEulerAngles.z).ToString();
                posX.text = (prop.transform.localPosition.x).ToString();
                posY.text = (prop.transform.localPosition.y).ToString();
                posZ.text = (prop.transform.localPosition.z).ToString();
                scaleX.text = (prop.transform.localScale.x).ToString();
                scaleY.text = (prop.transform.localScale.y).ToString();
                scaleZ.text = (prop.transform.localScale.z).ToString();
                return;
            }
        }
    }

    public void ApplyPropOffsets()
    {
        // Validate prop adjust things
        // Check if selected prop is in scene
        Prop[] props = FindObjectsOfType<Prop>();
        foreach (Prop prop in props)
        {
            prop.ApplyOffsets();
        }
    }

    public void UpdateTrackersList()
    {
        Tracking.Instance.UpdateTrackerList();
        List<Tracking.TrackedObject> trackerList =  Tracking.Instance.GetTrackerList();
        List<string> trackerOptions = new List<string>();
        foreach(Tracking.TrackedObject trackedObject in trackerList)
        {
            trackerOptions.Add(trackedObject.name);
        }
        trackersListHead.ClearOptions();
        trackersListLeftHand.ClearOptions();
        trackersListRightHand.ClearOptions();
        trackersListHead.AddOptions(trackerOptions);
        trackersListLeftHand.AddOptions(trackerOptions);
        trackersListRightHand.AddOptions(trackerOptions);
    }

    public void OpenOffsetPanel()
    {
        trackingPanel.SetActive(false);
        offsetPanel.SetActive(true);
        Tracking.Instance.EnableGizmoEditingOnTrackers();
    }

    public void CloseOffsetPanel()
    {
        trackingPanel.SetActive(true);
        offsetPanel.SetActive(false);
        Tracking.Instance.DisableGizmoEditingOnTrackers();
    }

    public void OpenPropOffsetPanel()
    {
        adjustPropPanel.SetActive(false);
        offsetPropPanel.SetActive(true);
        StartPropGizmo();
    }

    public void ClosePropOffsetPanel()
    {
        offsetPropPanel.SetActive(false);
        adjustPropPanel.SetActive(true);
        EndPropGizmo();
    }

    public void ClosePropOffsetPanelAndReset()
    {
        offsetPropPanel.SetActive(false);
        adjustPropPanel.SetActive(true);
        EndPropGizmo();
        ApplyPropOffsets();
        GetPropValues();
    }

    public void EnableCalibrationPanel()
    {
        trackingButton.SetActive(true);
    }

    public void DisableCalibrationPanel()
    {
        trackingButton.SetActive(false);
    }

    public static bool WantsToQuit()
    {
        if (readyToQuit)
        {
            return true;
        }
        return false;
    }

    public void QuitApp()
    {
        readyToQuit = true;
        Application.Quit();
    }

    public void OnApplicationQuit()
    {
        quitPanel.SetActive(true);
    }
}
