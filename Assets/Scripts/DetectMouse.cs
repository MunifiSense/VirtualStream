using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class DetectMouse : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public bool mouseIsOver = false;

    public void OnEnable()
    {
        mouseIsOver = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseIsOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        mouseIsOver = false;
    }
}
