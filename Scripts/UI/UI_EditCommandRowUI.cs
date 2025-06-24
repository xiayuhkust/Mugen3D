using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EditCommandRowUI : MonoBehaviour, UI_FixedRow
{
    public Image m_imageBack;
    public Text m_textNo;
    public Text m_textName;
    public Text m_textDuration;
    public Text m_textInput;
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
