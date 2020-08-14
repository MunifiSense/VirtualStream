using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DeleteButton : MonoBehaviour
{
    public VSAssetManager.AssetType type;
    public Button confirmation;
    public TMPro.TextMeshProUGUI text;
    // Start is called before the first frame update
    void Start()
    {
        confirmation.onClick.AddListener(() => {
            VSAssetManager.Instance.Delete(type, VSAssetManager.Instance.selected);
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
            text.text = "Are you sure you want to delete " + VSAssetManager.Instance.selected + " ?";
            GetComponent<Button>().interactable = true;
        }
    }
}
