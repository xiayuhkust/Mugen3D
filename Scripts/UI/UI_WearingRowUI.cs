using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class UI_WearingRowUI : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Text[] m_text = new Text[2];
    public Image m_image;
    [HideInInspector] public int m_index = 0;
    [HideInInspector] public UI_MakeNPCModelUI m_baseUI = null;
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

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (m_baseUI != null) m_baseUI.PreviewWearing(m_index);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (m_baseUI != null) m_baseUI.ClearPreview();
    }

    private void ButtonLeftClick()
    {
        if (m_baseUI != null) m_baseUI.WearingRowPress(m_index);
    }

    private void ButtonRightClick()
    {
        if (m_baseUI != null)
        {
            m_baseUI.ClearPreview();
            m_baseUI.TakeOffPart(m_index);
        }
    }
}
