using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class VSButton : MonoBehaviour
{
    public VSAssetManager.AssetType type;
    public string objectName;
    // Start is called before the first frame update

    public void SetButton()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(() => {
            VSAssetManager.Instance.selected = objectName; 
        });
    }
}
