using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class UI_EditCommandUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public RectTransform m_parentTransform;//插入行时的父对象
    public GameObject m_rowPrefab;//行（格子）
    public UI_FixedScrollRect m_fixedRect;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    private List<UI_EditCommandRowUI> m_listRow = new List<UI_EditCommandRowUI>();
    private int m_curSel = 0;

    private static Hero m_hero = null;
    private static UI_EditCommandUI m_UI = null;
    public static void OpenUI(Hero hero)
    {
        m_hero = hero;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditCommandUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditCommandUI>();
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
        m_fixedRect.m_baseUI = this;
        m_fixedRect.m_rowLimit = 16;
        m_fixedRect.m_oneRowNum = 1;
        m_fixedRect.m_itemCount = m_hero.m_listCommand.Count;
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
            ButtonClosePress();
        }
        m_fixedRect.UpdateScroll();
        if (UI_MakeCommandUI.m_isFinish)
        {
            UI_MakeCommandUI.m_isFinish = false;
            if (UI_MakeCommandUI.m_openType == 0)
            {
                m_hero.m_listCommand.Add(UI_MakeCommandUI.m_cmd);
                m_hero.m_dicCommand.Add(UI_MakeCommandUI.m_cmd.m_commandNo, UI_MakeCommandUI.m_cmd);
            }
            m_fixedRect.m_itemCount = m_hero.m_listCommand.Count;
            m_fixedRect.Init(m_fixedRect.m_startIndex);
        }
    }
    void RefreshContent(int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            UI_EditCommandRowUI boxUI = m_listRow[index - startIndex];
            Command cmd = m_hero.m_listCommand[index];
            if (m_curSel == index) boxUI.m_imageBack.color = new Color(1, 1, 1, 0.8f);
            else boxUI.m_imageBack.color = new Color(1, 1, 1, 0.4f);
            boxUI.m_textNo.text = cmd.m_commandNo.ToString();
            boxUI.m_textName.text = cmd.m_name.GetStr();
            boxUI.m_textDuration.text = cmd.m_time.ToString();
            boxUI.m_textInput.text = cmd.GetDisplayInput();
        }
    }
    public void ButtonAddPress()
    {
        Command cmd = new Command();
        cmd.m_commandNo= m_hero.GetNewComandNo();
        UI_MakeCommandUI.OpenUI(cmd, 0);
    }
    public void ButtonEditPress()
    {
        if (m_curSel >= m_hero.m_listCommand.Count) return;
        Command cmd = m_hero.m_listCommand[m_curSel];
        UI_MakeCommandUI.OpenUI(cmd, 1);
    }
    public void ButtonDeletePress()
    {
        if (m_curSel >= m_hero.m_listCommand.Count) return;
        m_hero.m_dicCommand.Remove(m_hero.m_listCommand[m_curSel].m_commandNo);
        m_hero.m_listCommand.RemoveAt(m_curSel);
        m_fixedRect.m_itemCount = m_hero.m_listCommand.Count;
        m_fixedRect.Init(m_fixedRect.m_startIndex);
    }
    public void ButtonUpPress()
    {
        if (m_curSel >= m_hero.m_listCommand.Count || m_hero.m_listCommand.Count <= 1 || m_curSel == 0) return;
        Command cmd = m_hero.m_listCommand[m_curSel];
        m_hero.m_listCommand[m_curSel] = m_hero.m_listCommand[m_curSel - 1];
        m_hero.m_listCommand[m_curSel - 1] = cmd;
        m_curSel--;
        RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
    }
    public void ButtonDownPress()
    {
        if (m_curSel >= m_hero.m_listCommand.Count - 1 || m_hero.m_listCommand.Count <= 1) return;
        Command cmd = m_hero.m_listCommand[m_curSel];
        m_hero.m_listCommand[m_curSel] = m_hero.m_listCommand[m_curSel + 1];
        m_hero.m_listCommand[m_curSel + 1] = cmd;
        m_curSel++;
        RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
    }
    public void ButtonClosePress()
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
        UI_EditCommandRowUI boxUI = obj.GetComponent<UI_EditCommandRowUI>();
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
