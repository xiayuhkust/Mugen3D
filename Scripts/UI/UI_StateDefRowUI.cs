using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_StateDefRowUI : MonoBehaviour, UI_FixedRow
{
    public Image m_imageBack;
    public Text m_textID;
    public Text m_textName;
    [HideInInspector] public int m_tag = 0;//标记在哪个列表，0-基本状态，1-常驻状态，2-基本招式，3-必杀技，4-大招，5-其它
    [HideInInspector] public int m_index = 0;
    [HideInInspector] public UI_FixedScrollRect m_rect;

    public void ButtonPress()
    {
        m_rect.RowPress(m_tag, m_index);
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
