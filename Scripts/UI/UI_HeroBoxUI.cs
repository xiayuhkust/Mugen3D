using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_HeroBoxUI : MonoBehaviour, UI_FixedRow
{
    public Image m_imageBack;
    public Image m_imageIcon;
    public Text m_textName;
    [HideInInspector] public UI_FixedScrollRect m_rect;
    [HideInInspector] public int m_index;
    public void ButtonPress()
    {
        m_rect.RowPress(0, m_index);
    }
    //UI_FixedRow接口的实现
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
    public float GetPosY()
    {
        return transform.position.y;
    }
}
