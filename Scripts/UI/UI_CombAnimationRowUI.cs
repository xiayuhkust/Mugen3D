using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

public class UI_CombAnimationRowUI : MonoBehaviour, IPointerClickHandler
{
    public Text[] m_text = new Text[2];
    public InputField m_inputFieldFadeTime;
    public InputField m_inputFieldOffTime;
    public InputField m_inputFieldEndTime;
    [HideInInspector] public int m_index = 0;
    [HideInInspector] public UI_EditStateDefUI m_baseUI = null;
    [HideInInspector] public UnityEvent leftClick;
    [HideInInspector] public UnityEvent rightClick;
    private void Start()
    {
        leftClick.AddListener(new UnityAction(ButtonLeftClick));
        rightClick.AddListener(new UnityAction(ButtonRightClick));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            leftClick.Invoke();
        else if (eventData.button == PointerEventData.InputButton.Right)
            rightClick.Invoke();
    }

    public void ButtonUpPress()
    {

        if (m_baseUI != null) m_baseUI.CardAnimationUpPress(m_index);
    }
    public void ButtonDownPress()
    {

        if (m_baseUI != null) m_baseUI.CardAnimationDownPress(m_index);
    }
    private void ButtonLeftClick()
    {

    }

    private void ButtonRightClick()
    {

        if (m_baseUI != null) m_baseUI.DeleteCardAnimation(m_index);
    }
    public void EndEditFadeTime()
    {
        if (m_inputFieldFadeTime.text == "-" || m_inputFieldFadeTime.text == "") m_inputFieldFadeTime.text = "0";
        m_baseUI.m_listAnimation[m_index].m_fadeTime = Convert.ToSingle(m_inputFieldFadeTime.text);
        if (m_baseUI.m_listAnimation[m_index].m_fadeTime < 0) m_baseUI.m_listAnimation[m_index].m_fadeTime = 0;
        else if (m_baseUI.m_listAnimation[m_index].m_fadeTime > 0.5f) m_baseUI.m_listAnimation[m_index].m_fadeTime = 0.5f;
        m_inputFieldFadeTime.SetTextWithoutNotify(m_baseUI.m_listAnimation[m_index].m_fadeTime.ToString());
    }
    public void EndEditOffTime()
    {
        if (m_inputFieldOffTime.text == "-" || m_inputFieldOffTime.text == "") m_inputFieldOffTime.text = "0";
        m_baseUI.m_listAnimation[m_index].m_offTime = Convert.ToSingle(m_inputFieldOffTime.text);
        if (m_baseUI.m_listAnimation[m_index].m_offTime < 0) m_baseUI.m_listAnimation[m_index].m_offTime = 0;
        else if (m_baseUI.m_listAnimation[m_index].m_offTime > 0.9f) m_baseUI.m_listAnimation[m_index].m_offTime = 0.9f;
        m_inputFieldOffTime.SetTextWithoutNotify(m_baseUI.m_listAnimation[m_index].m_offTime.ToString());
    }
    public void EndEditEndTime()
    {
        if (m_inputFieldEndTime.text == "-" || m_inputFieldEndTime.text == "") m_inputFieldEndTime.text = "0";
        m_baseUI.m_listAnimation[m_index].m_endTime = Convert.ToSingle(m_inputFieldEndTime.text);
        if (m_baseUI.m_listAnimation[m_index].m_endTime < 0) m_baseUI.m_listAnimation[m_index].m_endTime = 0;
        else if (m_baseUI.m_listAnimation[m_index].m_endTime > 0.9f) m_baseUI.m_listAnimation[m_index].m_endTime = 0.9f;
        m_inputFieldEndTime.SetTextWithoutNotify(m_baseUI.m_listAnimation[m_index].m_endTime.ToString());
    }
}
