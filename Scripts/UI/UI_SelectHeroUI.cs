using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UI_SelectHeroUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public RectTransform m_parentTransform;//插入行时的父对象
    public GameObject m_rowPrefab;//行（格子）
    public UI_FixedScrollRect m_fixedRect;

    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    private List<UI_HeroBoxUI> m_listRow = new List<UI_HeroBoxUI>();//每一格
    private int m_curSel = 0;
    private int m_curState = 0;//当前状态，0-选择左边角色，1-选择右边角色
    private Hero m_leftHero = null;//当m_curState切换为1时，临时保存左边角色信息

    private static UI_SelectHeroUI m_UI = null;
    public static void OpenUI()
    {
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/SelectHeroUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_SelectHeroUI>();
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
        m_curState = 0;
        HeroModelPreviewCtrl.m_modelCtrl[0].Show();
        HeroModelPreviewCtrl.m_modelCtrl[1].Show();
        HeroModelPreviewCtrl.m_modelCtrl[1].ClearModel();

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
            ButtonCancelPress();
        }
        m_fixedRect.UpdateScroll();

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
    public void ButtonOKPress()
    {
        if (m_curState == 0)
        {
            m_curState = 1;
            m_leftHero = GlobalAssist.m_listHero[m_curSel];
            int newSel = m_curSel + 1;
            if (newSel >= GlobalAssist.m_listHero.Count) newSel = 0;
            m_curSel = newSel;
            RowPress(0, 0, m_curSel, 0);
        }
        else
        {
            HeroModelPreviewCtrl.m_modelCtrl[0].Hide();
            HeroModelPreviewCtrl.m_modelCtrl[1].Hide();
            GlobalAssist.HideUI(gameObject);
            SceneManager.LoadScene("BattleField1");
            UI_GameUI.Show(m_leftHero, GlobalAssist.m_listHero[m_curSel], true, true);
        }
    }
    public void ButtonCancelPress()
    {
        if (m_curState == 0)
        {
            HeroModelPreviewCtrl.m_modelCtrl[0].Hide();
            HeroModelPreviewCtrl.m_modelCtrl[1].Hide();
            GlobalAssist.HideUI(gameObject);
        }
        else
        {
            m_curState = 0;
            HeroModelPreviewCtrl.m_modelCtrl[1].ClearModel();
        }
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
        HeroModelPreviewCtrl.m_modelCtrl[m_curState].RefreshModel(GlobalAssist.m_listHero[itemIndex]);
        RefreshContent(m_fixedRect.m_startIndex, m_fixedRect.m_endIndex);
    }
    public void RowEnter(int tag, int itemIndex, int rowIndex, float posY)
    {

    }
    public void RowExit(int tag)
    {

    }
}
