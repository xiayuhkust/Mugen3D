using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_MaterialRowUI : MonoBehaviour
{
    public Text[] m_text = new Text[2];
    public Button m_buttonColor;
    public Slider m_sliderRough;
    public Text m_textRough;
    public Slider m_sliderMetal;
    public Text m_textMetal;
    [HideInInspector] public int m_index = 0;
    [HideInInspector] public UI_MakeNPCModelUI m_baseUI = null;

    public void ButtonColorPress()
    {
        if (m_baseUI != null) m_baseUI.ButtonMatColorPress(m_index);
    }
    public void ButtonMaterialPress()
    {
        if (m_baseUI != null) m_baseUI.ButtonMaterialPress(m_index);
    }
    public void SliderRoughChange()
    {
        if (m_baseUI != null) m_baseUI.SliderRoughChange(m_index, m_sliderRough.value);
    }
    public void SliderMetalChange()
    {
        if (m_baseUI != null) m_baseUI.SliderMetalChange(m_index, m_sliderMetal.value);
    }
}
