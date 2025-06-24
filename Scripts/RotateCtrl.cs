using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class RotateCtrl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public int m_index = 0;
    public static bool[] m_isRotating = new bool[2] { false, false };
    public void OnPointerDown(PointerEventData eventData)
    {
        m_isRotating[m_index] = true;
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        m_isRotating[m_index] = false;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_isRotating[m_index] = false;
    }
}
