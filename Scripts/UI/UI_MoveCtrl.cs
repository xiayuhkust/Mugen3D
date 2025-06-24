using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum TKDirection//朝向
{
    Up,
    Down,
    Left,
    Right
}
public class UI_MoveCtrl : MonoBehaviour
{
    public TKDirection m_pos = TKDirection.Up;
    private RectTransform m_rectTransform = null;
    private Vector3 m_originalPos;
    private void Start()
    {
        if (m_rectTransform == null)
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_originalPos = m_rectTransform.localPosition;
        }
    }
    private void OnEnable()
    {
        if (m_rectTransform == null)
        {
            m_rectTransform = GetComponent<RectTransform>();
            m_originalPos = m_rectTransform.localPosition;
        }
        switch(m_pos)
        {
            case TKDirection.Up: m_rectTransform.localPosition = m_originalPos + new Vector3(0, m_rectTransform.sizeDelta.y * 0.5f * GlobalAssist.m_screenScale, 0); break;
            case TKDirection.Down: m_rectTransform.localPosition = m_originalPos + new Vector3(0, -m_rectTransform.sizeDelta.y * 0.5f * GlobalAssist.m_screenScale, 0); break;
            case TKDirection.Left: m_rectTransform.localPosition = m_originalPos + new Vector3(-m_rectTransform.sizeDelta.x * 0.5f * GlobalAssist.m_screenScale, 0, 0); break;
            case TKDirection.Right: m_rectTransform.localPosition = m_originalPos + new Vector3(m_rectTransform.sizeDelta.x * 0.5f * GlobalAssist.m_screenScale, 0, 0); break;
        }
        m_rectTransform.DOLocalMove(m_originalPos, 0.3f).SetUpdate(true);
    }

}
