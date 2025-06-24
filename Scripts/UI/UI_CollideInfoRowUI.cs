using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CollideInfoRowUI : MonoBehaviour, UI_FixedRow
{
    public Image m_imageBack;
    public Text[] m_textInfo = new Text[3];
    [HideInInspector] public UI_FixedScrollRect m_rect;
    [HideInInspector] public int m_index = 0;

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
