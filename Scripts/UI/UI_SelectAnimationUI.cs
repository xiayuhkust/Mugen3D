
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UI_SelectAnimationUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public RectTransform m_animClipParentTransform;//插入行的父对象
    public RectTransform m_buttonParentTransform;
    public GameObject m_animClipRowPrefab;//行
    public GameObject m_buttonRowPrefab;
    public UI_FixedScrollRect m_fixedRect;

    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    private List<UI_AnimClipRowUI> m_listAnimClipRow = new List<UI_AnimClipRowUI>();
    private List<UI_ButtonRowUI> m_listButtonRow = new List<UI_ButtonRowUI>();

    public static List<string> m_listAnimClipName = new List<string>();
    public static List<List<string>> m_listAnimationClipFile = new List<List<string>>();
    public static List<string> m_listAnimationDirectory = new List<string>();
    public static bool m_isFinish = false;
    public static int m_curSel = 0;
    public static Hero m_hero;

    private static UI_SelectAnimationUI m_UI = null;
    public static void OpenUI(Hero hero)
    {
        m_hero = hero;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/SelectAnimationUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_SelectAnimationUI>();
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
        m_isFinish = false;
        m_curSel = 0;
        InitButtonRow();
        ButtonAnimationClipTypePress(0);
    }
    void InitButtonRow()
    {
        List<string> listName = m_listAnimationDirectory;
        int rowCount = listName.Count;
        //设置容器大小
        int height = 34 * rowCount + 4;
        if (height < 860) SetContentSize(m_buttonParentTransform, 860);
        else SetContentSize(m_buttonParentTransform, height);
        //填列表内容
        for (int i = 0; i < listName.Count; i++)
        {
            GameObject obj = Instantiate(m_buttonRowPrefab, m_buttonParentTransform);
            UI_ButtonRowUI rowUI = obj.GetComponent<UI_ButtonRowUI>();
            rowUI.m_index = i;
            rowUI.m_baseUI = this;
            m_listButtonRow.Add(rowUI);
            rowUI.m_textName.text = listName[i];
            rowUI.m_selTag.SetActive(false);
        }
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
    }
    void RefreshContent(int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            UI_AnimClipRowUI rowUI = m_listAnimClipRow[index - startIndex];
            rowUI.m_text[0].text = (index + 1).ToString();
            rowUI.m_text[1].text = m_listAnimClipName[index];
            if (index == m_curSel) rowUI.m_imageBack.color = Color.green;
            else rowUI.m_imageBack.color = Color.white;
        }
    }
    public void SetContentSize(RectTransform parentTransform, int height)//设置容器大小
    {
        int contentHeight = height;
        if (parentTransform.sizeDelta.y != contentHeight)//尺寸改变才需刷新
        {
            parentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            parentTransform.anchoredPosition = new Vector3(0, 0, 0);
        }
    }
    public void ButtonAnimationClipTypePress(int index)
    {
        if (index >= m_listButtonRow.Count) return;
        m_curSel = 0;
        for (int i = 0; i < m_listButtonRow.Count; i++) m_listButtonRow[i].m_selTag.SetActive(false);
        m_listButtonRow[index].m_selTag.SetActive(true);
        m_listAnimClipName = m_listAnimationClipFile[index];
        m_fixedRect.m_baseUI = this;
        m_fixedRect.m_rowLimit = 25;
        m_fixedRect.m_oneRowNum = 1;
        m_fixedRect.m_itemCount = m_listAnimClipName.Count;
        m_fixedRect.Init(0);
    }
    public void ButtonOKPress()
    {
        if (m_curSel >= m_listAnimClipName.Count)
        {
            GlobalAssist.ShowCenterTips("错误：没有选中动画");
            return;
        }
        m_isFinish = true;
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
        GameObject obj = Instantiate(m_animClipRowPrefab, m_animClipParentTransform);
        UI_AnimClipRowUI rowUI = obj.GetComponent<UI_AnimClipRowUI>();
        rowUI.m_index = index;
        rowUI.m_rect = m_fixedRect;
        m_fixedRect.m_listRow.Add(rowUI);
        m_listAnimClipRow.Add(rowUI);
    }
    public void RowPress(int tag, int type, int itemIndex, int rowIndex)
    {
        m_curSel = itemIndex;
        RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
        HeroModelPreviewCtrl.m_modelCtrl[0].PlayOnceAnimation(m_hero, m_listAnimClipName[itemIndex]);
    }
    public void RowEnter(int tag, int itemIndex, int rowIndex, float posY)
    {

    }
    public void RowExit(int tag)
    {

    }
}
