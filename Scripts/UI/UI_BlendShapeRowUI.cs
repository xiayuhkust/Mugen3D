using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_BlendShapeRowUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image m_backImage;
    public Text[] m_text = new Text[3];
    public Slider m_slider;
    public Toggle m_toggle;
    [HideInInspector] public int m_index = 0;
    [HideInInspector] public UI_MakeNPCModelUI m_baseUI = null;

    public void SliderChange()
    {
        if (m_baseUI != null) m_baseUI.ChangeBlendShape(m_index, m_slider.value * (m_toggle.isOn ? 5f : 1f));
        m_text[2].text = m_slider.value.ToString("0");
    }
    public void ToggleChange()
    {
        
        if (m_baseUI != null) m_baseUI.ChangeBlendShape(m_index, m_slider.value * (m_toggle.isOn ? 5f : 1f));
    }
    public void ButtonResetPress()
    {
        
        m_slider.value = 0;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_baseUI != null) m_baseUI.PreviewBlendShapeName(m_index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        
    }
}
