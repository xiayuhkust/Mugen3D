using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_EditHeroUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public RectTransform m_parentTransform;//插入行时的父对象
    public GameObject m_rowPrefab;//行（格子）
    public UI_FixedScrollRect m_fixedRect;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    private List<UI_HeroBoxUI> m_listRow = new List<UI_HeroBoxUI>();//每一格
    private int m_curSel = 0;

    private static UI_EditHeroUI m_UI = null;
    public static void OpenUI()
    {
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditHeroUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditHeroUI>();
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
        HeroModelPreviewCtrl.m_modelCtrl[0].Show();

        m_fixedRect.m_baseUI = this;
        m_fixedRect.m_rowLimit = 30;
        m_fixedRect.m_oneRowNum = 6;
        m_fixedRect.m_itemCount = GlobalAssist.m_listHero.Count;
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
        if (UI_ConfirmUI.m_isFinish && UI_ConfirmUI.m_relateUI == gameObject)//确认删除
        {
            UI_ConfirmUI.m_isFinish = false;
            GlobalAssist.m_dicHero.Remove(GlobalAssist.m_listHero[m_curSel].m_id);
            GlobalAssist.m_listHero.RemoveAt(m_curSel);
            m_fixedRect.m_itemCount = GlobalAssist.m_listHero.Count;
            m_fixedRect.Init(0);
            RowPress(0, 0, 0, 0);
        }
        if (UI_EditOneHeroUI.m_closeType > 0)
        {
            if (UI_EditOneHeroUI.m_closeType == 1)//确认关闭
            {
                if (UI_EditOneHeroUI.m_openType == 0)
                {
                    m_fixedRect.m_itemCount = GlobalAssist.m_listHero.Count;
                    m_fixedRect.Init(GlobalAssist.m_listHero.Count - 1);
                    RowPress(0, 0, GlobalAssist.m_listHero.Count - 1, 0);
                }
                else RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
            }
            else//取消关闭
            {
                RowPress(0, 0, m_curSel, 0);
            }
            UI_EditOneHeroUI.m_closeType = 0;
        }
    }
    void RefreshContent(int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            UI_HeroBoxUI boxUI = m_listRow[index - startIndex];
            if (m_curSel == index) boxUI.m_imageBack.color = new Color(1, 1, 1, 0.8f);
            else boxUI.m_imageBack.color = new Color(1, 1, 1, 0.4f);
            boxUI.m_imageIcon.sprite = GlobalAssist.m_listHero[index].GetIcon();
            boxUI.m_textName.text = GlobalAssist.m_listHero[index].GetName();
        }
    }
    public void ButtonAddPress()
    {
        Hero hero = new Hero();
        hero.m_id = GlobalAssist.GetNewHeroID();
        GlobalAssist.m_listHero.Add(hero);
        GlobalAssist.m_dicHero.Add(hero.m_id, hero);
        UI_EditOneHeroUI.m_curHero = GlobalAssist.m_listHero[GlobalAssist.m_listHero.Count - 1];
        UI_EditOneHeroUI.OpenUI(0);
    }
    public void ButtonEditPress()
    {
        UI_EditOneHeroUI.m_curHero = GlobalAssist.m_listHero[m_curSel];
        UI_EditOneHeroUI.OpenUI(1);
    }
    public void ButtonDeletePress()
    {
        if (GlobalAssist.m_listHero.Count <= 1)
        {
            GlobalAssist.ShowCenterTips("无法删除最后一个角色");
            return;
        }
        UI_ConfirmUI.OpenUI(gameObject, "确认要删除<" + GlobalAssist.m_listHero[m_curSel].GetName() + ">吗?");
    }
    public void ButtonClosePress()
    {
        HeroModelPreviewCtrl.m_modelCtrl[0].Hide();
        GlobalAssist.HideUI(gameObject);
        Hero.SaveAll();
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
        UI_HeroBoxUI boxUI = obj.GetComponent<UI_HeroBoxUI>();
        boxUI.m_index = index;
        boxUI.m_rect = m_fixedRect;
        m_fixedRect.m_listRow.Add(boxUI);
        m_listRow.Add(boxUI);
    }
    public void RowPress(int tag, int type, int itemIndex, int rowIndex)
    {
        m_curSel = itemIndex;
        HeroModelPreviewCtrl.m_modelCtrl[0].RefreshModel(GlobalAssist.m_listHero[itemIndex]);
        RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
    }
    public void RowEnter(int tag, int itemIndex, int rowIndex, float posY)
    {
        
    }
    public void RowExit(int tag)
    {
        
    }
}
