using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//默认的button在点击后处于选中状态，解除之前鼠标进入无法高亮，因此在此处重载屏蔽选中事件的处理
public class UI_MyButton : Button
{
    [SerializeField] private SoundType m_soundType = SoundType.ButtonPress;
    public override void OnSelect(BaseEventData eventData)
    {
        
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable || eventData.button == PointerEventData.InputButton.Right) return;
        SoundManager.Play(m_soundType);
        base.OnPointerDown(eventData);
    }
}
