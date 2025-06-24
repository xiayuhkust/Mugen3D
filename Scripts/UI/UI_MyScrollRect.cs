using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_MyScrollRect : ScrollRect
{
    public override void OnDrag(PointerEventData eventData)//此处重写拖动，以隐藏拖动滚动的功能
    {
        
    }
}
