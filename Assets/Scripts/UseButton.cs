using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UseButton : MonoBehaviour
{
    public VSAssetManager.AssetType type;
    // Start is called before the first frame update
    void Start()
    {
        Button button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => {
            VSAssetManager.Instance.Load(type, VSAssetManager.Instance.selected);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if(VSAssetManager.Instance.selected == "")
        {
            GetComponent<Button>().interactable = false;
        }
        else
        {
            if(type == VSAssetManager.AssetType.Prop)
            {
                Prop[] props = FindObjectsOfType<Prop>();
                foreach (Prop prop in props)
                {
                    if (prop.name == VSAssetManager.Instance.selected)
                    {
                        GetComponent<Button>().interactable = true;
                        break;
                    }
                }
            }
            else
            {
                GetComponent<Button>().interactable = true;
            }
        }
    }
}
