using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ButtonRowUI : MonoBehaviour
{
    public Text m_textName;
    public GameObject m_selTag;
    [HideInInspector] public int m_index = 0;
    [HideInInspector] public UI_SelectAnimationUI m_baseUI;
    
    public void ButtonPress()
    {
        m_baseUI.ButtonAnimationClipTypePress(m_index);
    }
}
