using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_EditOperationUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public Text m_textTile;
    public RectTransform m_parentTransform;//插入行时的父对象
    public GameObject m_rowPrefab;//行（格子）
    public UI_FixedScrollRect m_fixedRect;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    private List<UI_StateRowUI> m_listRow = new List<UI_StateRowUI>();
    private List<HeroState> m_listState = new List<HeroState>();
    private int m_curSel = 0;
    public static Hero m_hero;
    public static int m_openType = 0;//0-操作，1-AI

    private static UI_EditOperationUI m_UI = null;
    public static void OpenUI(Hero hero, int openType)
    {
        m_hero = hero;
        m_openType = openType;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditOperationUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditOperationUI>();
        }
        else
        {
            GlobalAssist.ShowUI(m_UI.gameObject, true);
            m_UI.Start();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        UpdateInfoToUI();
        m_fixedRect.m_baseUI = this;
        m_fixedRect.m_rowLimit = 11;
        m_fixedRect.m_oneRowNum = 1;
        m_fixedRect.m_itemCount = m_listState.Count;
        m_fixedRect.Init(0);
        RowPress(0, 0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalAssist.m_curUI != gameObject) return;
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SoundManager.Play(SoundType.CancelPress);
            ButtonCancelPress();
        }
        m_fixedRect.UpdateScroll();
        if (UI_EditStateUI.m_isFinish)
        {
            UI_EditStateUI.m_isFinish = false;
            if (UI_EditStateUI.m_openType == 0) m_listState.Add(UI_EditStateUI.m_state);
            m_fixedRect.m_itemCount = m_listState.Count;
            m_fixedRect.Init(m_fixedRect.m_startIndex);
        }
    }
    void UpdateInfoToUI()
    {
        m_listState.Clear();
        if (m_openType == 0)
        {
            m_textTile.text = "操控";
            m_listState.AddRange(m_hero.m_listOperationState);
        }
        else
        {
            m_textTile.text = "AI";
            m_listState.AddRange(m_hero.m_listAIState);
        }
    }
    void UpdateUIToInfo()
    {
        if (m_openType == 0)
        {
            m_hero.m_listOperationState.Clear();
            m_hero.m_listOperationState.AddRange(m_listState);
        }
        else
        {
            m_hero.m_listAIState.Clear();
            m_hero.m_listAIState.AddRange(m_listState);
        }
    }
    void RefreshContent(int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            UI_StateRowUI boxUI = m_listRow[index - startIndex];
            if (m_curSel == index) boxUI.m_imageBack.color = new Color(1, 1, 1, 0.8f);
            else boxUI.m_imageBack.color = new Color(1, 1, 1, 0.4f);
            boxUI.m_textRemark.text = m_listState[index].GetDisplayStr();
        }
    }
    public void ButtonAddPress()
    {
        HeroState state = new HeroState();
        UI_EditStateUI.OpenUI(state, 0);
    }
    public void ButtonEditPress()
    {
        if (m_curSel >= m_listState.Count) return;
        UI_EditStateUI.OpenUI(m_listState[m_curSel], 1);
    }
    public void ButtonDeletePress()
    {
        if (m_curSel >= m_listState.Count) return;
        m_listState.RemoveAt(m_curSel);
        m_fixedRect.m_itemCount = m_listState.Count;
        m_fixedRect.Init(m_fixedRect.m_startIndex);
    }
    public void ButtonOKPress()
    {
        UpdateUIToInfo();
        GlobalAssist.HideUI(gameObject);
    }
    public void ButtonCancelPress()
    {
        GlobalAssist.HideUI(gameObject);
    }
    //UI_Base接口的实现
    public bool IsRootUI()
    {
        return false;
    }
    public void SetParent(GameObject parentUI)
    {
        m_parentUI = parentUI;
    }
    public GameObject GetParent()
    {
        return m_parentUI;
    }
    public int GetSortOrder()//返回Canvas的sort order
    {
        return m_sortOrder;
    }
    public void SetSortOrder(int order)//设置order
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.sortingOrder = order;
        m_sortOrder = order;
    }
    //UI_Fixed接口的实现
    public void RefreshListRow(int tag, int startIndex, int endIndex)
    {
        RefreshContent(startIndex, endIndex);
    }
    public void AddOneRow(int tag, int index)
    {
        GameObject obj = Instantiate(m_rowPrefab, m_parentTransform);
        UI_StateRowUI boxUI = obj.GetComponent<UI_StateRowUI>();
        boxUI.m_index = index;
        boxUI.m_rect = m_fixedRect;
        m_fixedRect.m_listRow.Add(boxUI);
        m_listRow.Add(boxUI);
    }
    public void RowPress(int tag, int type, int itemIndex, int rowIndex)
    {
        m_curSel = itemIndex;
        RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
    }
    public void RowEnter(int tag, int itemIndex, int rowIndex, float posY)
    {

    }
    public void RowExit(int tag)
    {

    }
}
