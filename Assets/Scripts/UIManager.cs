using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

public class UIManager : MonoBehaviour
{
    public RectTransform windowSelectPanel;
    public GameObject avatarPanel;
    public GameObject environmentPanel;
    public GameObject propPanel;
    public GameObject profilePanel;
    public GameObject adjustPropPanel;

    public Button importAvatar;
    public Button importEnvironment;
    public Button importProp;
    public Button saveProfile;

    public Button equipProp;
    public Button unequipProp;
    public Button adjustProp;
    public Button adjustPropConfirm;
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

    public GameObject menuObject;
    public GameObject profileObject;
    public GameObject profileNameEntry;
    public TMP_InputField profileName;
    public Button profileOK;
    public float speed;
    // Start is called before the first frame update
    void Start()
    {
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
        saveProfile.onClick.AddListener(() => {
            ValidateInput();
        });
        adjustPropConfirm.onClick.AddListener(() => {
            ValidateAndAdjust();
        });
        string[] bones = System.Enum.GetNames(typeof(HumanBodyBones));
        List<string> bonesList = new List<string>();
        foreach (string bone in bones)
        {
            TMP_Dropdown.OptionData option = new TMP_Dropdown.OptionData();
            option.text = bone;
            bonesList.Add(bone);
        }
        bonesDropdown.AddOptions(bonesList);
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
        if(type == "profile")
        {
            menuObjectToUse = profileObject;
            DeleteChildren(profilePanel);
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
            else if(type == "profile")
            {
                button.type = VSAssetManager.AssetType.Profile;
                button.objectName = objectName;
                button.SetButton();
                uiObject.transform.parent = profilePanel.transform;
            }

            if(type == "profile")
            {
                uiObject.transform.localScale = Vector3.one;
                button.gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = objectName;
                return;
            }
            AssetBundle asset = AssetBundle.LoadFromFile(file.FullName);
            uiObject.transform.localScale = Vector3.one;
            button.gameObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = objectName;
            GameObject preview = asset.LoadAsset<GameObject>(objectName);
            GameObject previewObject = Instantiate(preview, new Vector3(-1000,-1000,-1000), Quaternion.Euler(0f, 0f, 0f));
            button.gameObject.transform.GetChild(0).GetComponent<RawImage>().texture =
            RuntimePreviewGenerator.GenerateModelPreview(previewObject.transform, 256, 256);
            asset.Unload(true);
            Destroy(preview);
            Destroy(previewObject);
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
        if(!(profileName.text == ""))
        {
            profileNameEntry.SetActive(false);
            VSAssetManager.Instance.SaveProfile(profileName.text);
        }
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
                }
                return;
            }
        }
    }
}
