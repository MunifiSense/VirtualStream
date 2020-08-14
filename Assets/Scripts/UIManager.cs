using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UIManager : MonoBehaviour
{
    public RectTransform windowSelectPanel;
    public GameObject avatarPanel;
    public GameObject environmentPanel;
    public GameObject propPanel;
    public GameObject profilePanel;

    public Button importAvatar;
    public Button importEnvironment;
    public Button importProp;
    public Button saveProfile;

    public GameObject menuObject;
    public GameObject profileObject;
    public GameObject profileNameEntry;
    public InputField profileName;
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
        saveProfile.onClick.AddListener(() => {
            ValidateInput();
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
        if(type == "profile")
        {
            menuObjectToUse = profileObject;
            DeleteChildren(profilePanel);
        }
        foreach (FileInfo file in files)
        {
            AssetBundle asset = AssetBundle.LoadFromFile(file.FullName);
            GameObject uiObject = Instantiate(menuObjectToUse);
            VSButton button = uiObject.GetComponent<VSButton>();
            string objectName = asset.name.Replace("."+type+".vstream","");
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
                uiObject.transform.parent = propPanel.transform;
            }
            else if(type == "profile")
            {
                button.type = VSAssetManager.AssetType.Profile;
                button.objectName = objectName;
                button.SetButton();
                uiObject.transform.parent = profilePanel.transform;
            }

            uiObject.transform.localScale = Vector3.one;
            button.gameObject.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>().text = objectName;
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
}
