using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class UI_FixedScrollRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Scrollbar m_scrollbar;//滚动条
    [HideInInspector] public UI_Fixed m_baseUI;
    [HideInInspector] public List<UI_FixedRow> m_listRow = new List<UI_FixedRow>();
    [HideInInspector] public int m_itemCount = 0;//列表项的数量
    [HideInInspector] public int m_scrollVal = 0;
    [HideInInspector] public int m_startIndex = 0;
    [HideInInspector] public int m_endIndex = 0;
    [HideInInspector] public int m_enterIndex = -1;//当前鼠标进入的行下标
    [HideInInspector] public int m_rowLimit = 1;//总个数
    [HideInInspector] public int m_oneRowNum = 1;//一行的个数，大于1时，一行有多个格，普通列表都是1
    [HideInInspector] public int m_tag = 0;//当前滚动表在UI中的下标，当UI中有多个时有用
    [HideInInspector] public int m_tableIndex = 0;//当前列表下标，主要是给背包这种切换列表和图标用的
    private bool m_enterContent = false;
    
    public void Init(int index)//手动在所属UI的Start里初始化，避免Start的执行顺序出问题
    {
        if (index < 0) index = 0;
        int maxRowNum = m_rowLimit / m_oneRowNum;//最大行数
        int rowNum = m_itemCount / m_oneRowNum;//当前行数
        if (m_itemCount % m_oneRowNum != 0) rowNum++;
        if (rowNum > maxRowNum)
        {
            m_scrollbar.gameObject.SetActive(true);
            for (int i = 0; i < m_listRow.Count; i++) m_listRow[i].SetActive(true);
            for (int i = m_listRow.Count; i < m_rowLimit; i++) m_baseUI.AddOneRow(m_tag, i);
            m_scrollbar.size = maxRowNum * 1f / (rowNum);
            if (m_scrollbar.size < 0.1f) m_scrollbar.size = 0.1f;
            float value = index * 1f / (rowNum - maxRowNum);
            if (m_scrollbar.value == value) RefreshListRow();//当点第一个时，值没改变，不会刷新
            else m_scrollbar.value = value;
        }
        else
        {
            m_scrollbar.gameObject.SetActive(false);
            if (m_listRow.Count > m_itemCount)
            {
                for (int i = 0; i < m_itemCount; i++) m_listRow[i].SetActive(true);
                for (int i = m_itemCount; i < m_listRow.Count; i++) m_listRow[i].SetActive(false);
            }
            else
            {
                for (int i = 0; i < m_listRow.Count; i++) m_listRow[i].SetActive(true);
                for (int i = m_listRow.Count; i < m_itemCount; i++) m_baseUI.AddOneRow(m_tag, i);
            }
            RefreshListRow();
        }
    }

    public void UpdateScroll()//在所属UI的Update里执行
    {
        int maxRowNum = m_rowLimit / m_oneRowNum;//最大行数
        int rowNum = m_itemCount / m_oneRowNum;//当前行数
        if (m_itemCount % m_oneRowNum != 0) rowNum++;
        if (rowNum > maxRowNum && m_enterContent)
        {
            float scrollVal = Input.GetAxis("Mouse ScrollWheel");
            if (scrollVal > 0)//这个是鼠标滚轮响应函数
            {
                if (m_scrollVal > 0)
                {
                    int tmpVal = m_scrollVal - 1;
                    m_scrollbar.value = tmpVal * 1f / (rowNum - maxRowNum);
                }
            }
            else if (scrollVal < 0)//这个是鼠标滚轮响应函数
            {
                if (m_scrollVal < rowNum - maxRowNum + 1)
                {
                    int tmpVal = m_scrollVal + 1;
                    m_scrollbar.value = tmpVal * 1f / (rowNum - maxRowNum);
                }
            }
        }
    }
    public void ScrollbarChange()
    {
        int maxRowNum = m_rowLimit / m_oneRowNum;//最大行数
        int rowNum = m_itemCount / m_oneRowNum;//当前行数
        if (m_itemCount % m_oneRowNum != 0) rowNum++;
        int lineNum = rowNum - maxRowNum;
        int scrollVal = (int)(m_scrollbar.value * lineNum + 0.1f);
        if (scrollVal < 0) scrollVal = 0;
        else if (scrollVal > lineNum) scrollVal = lineNum;
        //if (m_scrollVal == scrollVal) return;//scrollVal虽然不变，但是内容可能变了，比如UI_ExchangeUI玩家往中间拖动时
        m_scrollVal = scrollVal;
        RefreshListRow();
        if (m_enterIndex >= 0 && m_startIndex + m_enterIndex < m_itemCount) m_baseUI.RowEnter(m_tag, m_startIndex + m_enterIndex, m_enterIndex, m_listRow[m_enterIndex].GetPosY());
        else m_baseUI.RowExit(m_tag);
    }
    void RefreshListRow()
    {
        m_startIndex = 0;
        m_endIndex = m_itemCount;
        if (m_itemCount > m_rowLimit)
        {
            m_startIndex = m_scrollVal * m_oneRowNum;
            m_endIndex = m_startIndex + m_rowLimit;
        }
        for (int i = m_startIndex; i < m_endIndex; i++)
        {
            if (i >= m_itemCount) m_listRow[i - m_startIndex].SetActive(false);
            else
            {
                m_listRow[i - m_startIndex].SetActive(true);
            }
        }
        if (m_endIndex > m_itemCount) m_endIndex = m_itemCount;
        m_baseUI.RefreshListRow(m_tag, m_startIndex, m_endIndex);
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_enterContent = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        m_enterContent = false;
    }
    public void RowPress(int type, int index)
    {
        m_baseUI.RowPress(m_tag, type, m_startIndex + index, index);
    }
    public void RowEnter(int index, float posY)
    {
        m_enterIndex = index;
        m_baseUI.RowEnter(m_tag, m_startIndex + index, index, posY);
    }
    public void RowExit(int index)
    {
        m_enterIndex = -1;
        m_baseUI.RowExit(m_tag);
    }
}
