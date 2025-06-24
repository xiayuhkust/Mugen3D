using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
//重载，实现点击音效
public class UI_MyToggle : Toggle
{
    public override void OnPointerDown(PointerEventData eventData)
    {
        if (!interactable || eventData.button == PointerEventData.InputButton.Right) return;
        SoundManager.Play(SoundType.TogglePress);
    }
}
