using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_CommandInputBoxUI : MonoBehaviour
{
    public Text m_textInput;
    public Button m_buttonEdit;
    [HideInInspector] public UI_MakeCommandUI m_baseUI;
    [HideInInspector] public int m_index = 0;

    public void ButtonAddPress()
    {
        m_baseUI.ButtonAddPress(m_index);
    }
    public void ButtonEditPress()
    {
        m_baseUI.ButtonEditPress(m_index);
    }
    
}
