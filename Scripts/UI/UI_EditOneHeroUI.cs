using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;

public class UI_EditOneHeroUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public RectTransform[] m_parentTransform = new RectTransform[5];//插入行时的父对象
    public GameObject m_rowPrefab;//行（格子）
    public UI_FixedScrollRect[] m_fixedRect = new UI_FixedScrollRect[5];
    public InputField[] m_inputFieldBaseInfo = new InputField[33];
    public Toggle[] m_toggleBaseInfo = new Toggle[2];
    public Slider m_sliderBaseInfo;
    public Text m_textVolume;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    private List<UI_StateDefRowUI>[] m_listRow = new List<UI_StateDefRowUI>[5];
    private List<HeroStateDef>[] m_listState = new List<HeroStateDef>[5];
    public static Dictionary<int, HeroStateDef> m_dicStateDef = new Dictionary<int, HeroStateDef>();
    private int m_curType = 0;//当前编辑表格，0-基本状态，1-基本招式，2-必杀技，3-大招，4-其它
    private int[] m_curSel = new int[5];//以上5个表格的当前选中下标

    public static Hero m_curHero = null;
    public static int m_closeType = 0;//1-确认关闭，2-取消关闭
    public static int m_openType = 0;//0-新建，1-修改
    private static UI_EditOneHeroUI m_UI = null;
    public static void OpenUI(int openType)
    {
        m_openType = openType;
        m_closeType = 0;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditOneHeroUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditOneHeroUI>();
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
        if (m_listRow[0] == null)
        {
            for (int i = 0; i < 5; i++)
            {
                m_listRow[i] = new List<UI_StateDefRowUI>();
                m_listState[i] = new List<HeroStateDef>();
            }
        }
        UpdateBaseInfoToUI();
        for (int i = 0; i < 5; i++)
        {
            m_curSel[i] = 0;
            m_fixedRect[i].m_baseUI = this;
            m_fixedRect[i].m_tag = i;
            m_fixedRect[i].m_rowLimit = 8;
            m_fixedRect[i].m_oneRowNum = 1;
            m_fixedRect[i].m_itemCount = m_listState[i].Count;
            m_fixedRect[i].Init(0);
            RowPress(i, 0, 0, 0);
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
        for (int i = 0; i < 5; i++) m_fixedRect[i].UpdateScroll();
        if (UI_EditStateDefUI.m_isFinish)
        {
            UI_EditStateDefUI.m_isFinish = false;
            if (UI_EditStateDefUI.m_openType == 0)
            {
                m_listState[m_curType].Add(UI_EditStateDefUI.m_stateDef);
                m_dicStateDef.Add(UI_EditStateDefUI.m_stateDef.m_stateNo, UI_EditStateDefUI.m_stateDef);
            }
            m_fixedRect[m_curType].m_itemCount = m_listState[m_curType].Count;
            m_fixedRect[m_curType].Init(m_fixedRect[m_curType].m_startIndex);
        }
    }
    void UpdateBaseInfoToUI()
    {
        m_inputFieldBaseInfo[0].text = m_curHero.GetName();
        m_inputFieldBaseInfo[1].text = m_curHero.m_baseInfo.m_lifeMax.ToString();
        m_inputFieldBaseInfo[2].text = m_curHero.m_baseInfo.m_powerMax.ToString();
        m_inputFieldBaseInfo[3].text = m_curHero.m_baseInfo.m_attackVal.ToString();
        m_inputFieldBaseInfo[4].text = m_curHero.m_baseInfo.m_defenceVal.ToString();
        m_inputFieldBaseInfo[5].text = m_curHero.m_baseInfo.m_juggle.ToString();
        m_inputFieldBaseInfo[6].text = m_curHero.m_baseInfo.m_hitFireNo.ToString();
        m_inputFieldBaseInfo[7].text = m_curHero.m_baseInfo.m_defenceFireNo.ToString();
        m_inputFieldBaseInfo[8].text = m_curHero.m_baseInfo.m_walkFowardSpeed.ToString();
        m_inputFieldBaseInfo[9].text = m_curHero.m_baseInfo.m_walkBackSpeed.ToString();
        m_inputFieldBaseInfo[10].text = m_curHero.m_baseInfo.m_runForwardSpeed.ToString();
        m_inputFieldBaseInfo[11].text = m_curHero.m_baseInfo.m_dodgeBackSpeed[0].ToString();
        m_inputFieldBaseInfo[12].text = m_curHero.m_baseInfo.m_dodgeBackSpeed[1].ToString();
        m_inputFieldBaseInfo[13].text = m_curHero.m_baseInfo.m_standJumpSpeed[0].ToString();
        m_inputFieldBaseInfo[14].text = m_curHero.m_baseInfo.m_standJumpSpeed[1].ToString();
        m_inputFieldBaseInfo[15].text = m_curHero.m_baseInfo.m_standJumpBackSpeed[0].ToString();
        m_inputFieldBaseInfo[16].text = m_curHero.m_baseInfo.m_standJumpBackSpeed[1].ToString();
        m_inputFieldBaseInfo[17].text = m_curHero.m_baseInfo.m_standJumpForwardSpeed[0].ToString();
        m_inputFieldBaseInfo[18].text = m_curHero.m_baseInfo.m_standJumpForwardSpeed[1].ToString();
        m_inputFieldBaseInfo[19].text = m_curHero.m_baseInfo.m_runJumpBackSpeed[0].ToString();
        m_inputFieldBaseInfo[20].text = m_curHero.m_baseInfo.m_runJumpBackSpeed[1].ToString();
        m_inputFieldBaseInfo[21].text = m_curHero.m_baseInfo.m_runJumpForwardSpeed[0].ToString();
        m_inputFieldBaseInfo[22].text = m_curHero.m_baseInfo.m_runJumpForwardSpeed[1].ToString();
        m_inputFieldBaseInfo[23].text = m_curHero.m_baseInfo.m_airJumpSpeed[0].ToString();
        m_inputFieldBaseInfo[24].text = m_curHero.m_baseInfo.m_airJumpSpeed[1].ToString();
        m_inputFieldBaseInfo[25].text = m_curHero.m_baseInfo.m_airJumpBackSpeed[0].ToString();
        m_inputFieldBaseInfo[26].text = m_curHero.m_baseInfo.m_airJumpBackSpeed[1].ToString();
        m_inputFieldBaseInfo[27].text = m_curHero.m_baseInfo.m_airJumpForwardSpeed[0].ToString();
        m_inputFieldBaseInfo[28].text = m_curHero.m_baseInfo.m_airJumpForwardSpeed[1].ToString();
        m_inputFieldBaseInfo[29].text = m_curHero.m_baseInfo.m_gravity.ToString();
        m_inputFieldBaseInfo[30].text = m_curHero.m_baseInfo.m_standFriction.ToString();
        m_inputFieldBaseInfo[31].text = m_curHero.m_baseInfo.m_crouchFriction.ToString();
        m_inputFieldBaseInfo[32].text = m_curHero.m_baseInfo.m_attackDist.ToString();
        m_toggleBaseInfo[0].isOn = m_curHero.m_baseInfo.m_KOecho;
        m_toggleBaseInfo[1].isOn = m_curHero.m_baseInfo.m_airJump;
        m_sliderBaseInfo.value = m_curHero.m_baseInfo.m_volume;
        for (int i = 0; i < 5; i++) m_listState[i].Clear();
        m_listState[0].AddRange(m_curHero.m_listBaseState);
        m_listState[1].AddRange(m_curHero.m_listBaseAttack);
        m_listState[2].AddRange(m_curHero.m_listAdvanceAttack);
        m_listState[3].AddRange(m_curHero.m_listSkill);
        m_listState[4].AddRange(m_curHero.m_listOtherState);
        m_dicStateDef.Clear();
        foreach (HeroStateDef stateDef in m_curHero.m_dicStateDef.Values) m_dicStateDef.Add(stateDef.m_stateNo, stateDef);
    }
    void UpdateUIToBaseInfo()
    {
        m_curHero.m_name.SetStr(m_inputFieldBaseInfo[0].text);
        m_curHero.m_baseInfo.m_lifeMax = m_inputFieldBaseInfo[1].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[1].text);
        m_curHero.m_baseInfo.m_powerMax = m_inputFieldBaseInfo[2].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[2].text);
        m_curHero.m_baseInfo.m_attackVal = m_inputFieldBaseInfo[3].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[3].text);
        m_curHero.m_baseInfo.m_defenceVal = m_inputFieldBaseInfo[4].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[4].text);
        m_curHero.m_baseInfo.m_juggle = m_inputFieldBaseInfo[5].text == "" ? 0 : Convert.ToInt32(m_inputFieldBaseInfo[5].text);
        m_curHero.m_baseInfo.m_hitFireNo = m_inputFieldBaseInfo[6].text == "" ? 0 : Convert.ToInt32(m_inputFieldBaseInfo[6].text);
        m_curHero.m_baseInfo.m_defenceFireNo = m_inputFieldBaseInfo[7].text == "" ? 0 : Convert.ToInt32(m_inputFieldBaseInfo[7].text);
        m_curHero.m_baseInfo.m_walkFowardSpeed = m_inputFieldBaseInfo[8].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[8].text);
        m_curHero.m_baseInfo.m_walkBackSpeed = m_inputFieldBaseInfo[9].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[9].text);
        m_curHero.m_baseInfo.m_runForwardSpeed = m_inputFieldBaseInfo[10].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[10].text);
        m_curHero.m_baseInfo.m_dodgeBackSpeed[0] = m_inputFieldBaseInfo[11].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[11].text);
        m_curHero.m_baseInfo.m_dodgeBackSpeed[1] = m_inputFieldBaseInfo[12].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[12].text);
        m_curHero.m_baseInfo.m_standJumpSpeed[0] = m_inputFieldBaseInfo[13].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[13].text);
        m_curHero.m_baseInfo.m_standJumpSpeed[1] = m_inputFieldBaseInfo[14].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[14].text);
        m_curHero.m_baseInfo.m_standJumpBackSpeed[0] = m_inputFieldBaseInfo[15].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[15].text);
        m_curHero.m_baseInfo.m_standJumpBackSpeed[1] = m_inputFieldBaseInfo[16].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[16].text);
        m_curHero.m_baseInfo.m_standJumpForwardSpeed[0] = m_inputFieldBaseInfo[17].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[17].text);
        m_curHero.m_baseInfo.m_standJumpForwardSpeed[1] = m_inputFieldBaseInfo[18].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[18].text);
        m_curHero.m_baseInfo.m_runJumpBackSpeed[0] = m_inputFieldBaseInfo[19].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[19].text);
        m_curHero.m_baseInfo.m_runJumpBackSpeed[1] = m_inputFieldBaseInfo[20].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[20].text);
        m_curHero.m_baseInfo.m_runJumpForwardSpeed[0] = m_inputFieldBaseInfo[21].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[21].text);
        m_curHero.m_baseInfo.m_runJumpForwardSpeed[1] = m_inputFieldBaseInfo[22].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[22].text);
        m_curHero.m_baseInfo.m_airJumpSpeed[0] = m_inputFieldBaseInfo[23].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[23].text);
        m_curHero.m_baseInfo.m_airJumpSpeed[1] = m_inputFieldBaseInfo[24].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[24].text);
        m_curHero.m_baseInfo.m_airJumpBackSpeed[0] = m_inputFieldBaseInfo[25].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[25].text);
        m_curHero.m_baseInfo.m_airJumpBackSpeed[1] = m_inputFieldBaseInfo[26].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[26].text);
        m_curHero.m_baseInfo.m_airJumpForwardSpeed[0] = m_inputFieldBaseInfo[27].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[27].text);
        m_curHero.m_baseInfo.m_airJumpForwardSpeed[1] = m_inputFieldBaseInfo[28].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[28].text);
        m_curHero.m_baseInfo.m_gravity = m_inputFieldBaseInfo[29].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[29].text);
        m_curHero.m_baseInfo.m_standFriction = m_inputFieldBaseInfo[30].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[30].text);
        m_curHero.m_baseInfo.m_crouchFriction = m_inputFieldBaseInfo[31].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[31].text);
        m_curHero.m_baseInfo.m_attackDist = m_inputFieldBaseInfo[32].text == "" ? 0 : Convert.ToSingle(m_inputFieldBaseInfo[32].text);
        m_curHero.m_baseInfo.m_KOecho = m_toggleBaseInfo[0].isOn;
        m_curHero.m_baseInfo.m_airJump = m_toggleBaseInfo[1].isOn;
        m_curHero.m_baseInfo.m_volume = m_sliderBaseInfo.value;
        m_curHero.m_dicStateDef.Clear();
        m_curHero.m_listBaseState.Clear();
        m_curHero.m_listBaseState.AddRange(m_listState[0]);
        m_curHero.m_listBaseAttack.Clear();
        m_curHero.m_listBaseAttack.AddRange(m_listState[1]);
        m_curHero.m_listAdvanceAttack.Clear();
        m_curHero.m_listAdvanceAttack.AddRange(m_listState[2]);
        m_curHero.m_listSkill.Clear();
        m_curHero.m_listSkill.AddRange(m_listState[3]);
        m_curHero.m_listOtherState.Clear();
        m_curHero.m_listOtherState.AddRange(m_listState[4]);
        m_curHero.m_dicStateDef.Clear();
        foreach (HeroStateDef stateDef in m_dicStateDef.Values) m_curHero.m_dicStateDef.Add(stateDef.m_stateNo, stateDef);
    }
    int GetNewStateNo()
    {
        int stateNo = 0;
        while (m_dicStateDef.ContainsKey(stateNo)) stateNo++;
        return stateNo;
    }

    void RefreshContent(int type, int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            UI_StateDefRowUI boxUI = m_listRow[type][index - startIndex];
            if (m_curSel[type] == index) boxUI.m_imageBack.color = new Color(1, 1, 1, 0.8f);
            else boxUI.m_imageBack.color = new Color(1, 1, 1, 0.4f);
            boxUI.m_textID.text = m_listState[type][index].m_stateNo.ToString();
            boxUI.m_textName.text = m_listState[type][index].m_name.GetStr();
        }
    }
    public void SliderVolumeChange()
    {
        m_textVolume.text = (int)(m_sliderBaseInfo.value * 100) + "%";
    }
    public void ButtonTestPress()
    {
        UpdateUIToBaseInfo();
        Hero.SaveAll();
        GlobalAssist.CloseAllUI();
        HeroModelPreviewCtrl.m_modelCtrl[0].Hide();
        HeroModelPreviewCtrl.m_modelCtrl[1].Hide();
        SceneManager.LoadScene("BattleField1");
        UI_GameUI.Show(m_curHero, m_curHero, true, true);
    }
    public void ButtonEditModelPress()
    {
        UI_MakeNPCModelUI.OpenUI(m_curHero);
    }
    public void ButtonEditCommandPress()
    {
        UI_EditCommandUI.OpenUI(m_curHero);
    }
    public void ButtonEditOperationPress(int type)
    {
        UI_EditOperationUI.OpenUI(m_curHero, type);
    }
    public void ButtonAddStatePress(int type)
    {
        m_curType = type;
        HeroStateDef stateDef = new HeroStateDef();
        stateDef.m_stateNo = GetNewStateNo();
        UI_EditStateDefUI.OpenUI(stateDef, m_curHero, 0);
    }
    public void ButtonEditStatePress(int type)
    {
        if (m_curSel[type] >= m_listState[type].Count) return;
        m_curType = type;
        UI_EditStateDefUI.OpenUI(m_listState[type][m_curSel[type]], m_curHero, 1);
    }
    public void ButtonDeleteStatePress(int type)
    {
        if (m_curSel[type] >= m_listState[type].Count) return;
        m_curType = type;
        m_dicStateDef.Remove(m_listState[type][m_curSel[type]].m_stateNo);
        m_listState[type].RemoveAt(m_curSel[type]);
        m_fixedRect[m_curType].m_itemCount = m_listState[m_curType].Count;
        m_fixedRect[m_curType].Init(m_fixedRect[m_curType].m_startIndex);
    }
    public void ButtonCopyStatePress(int type)
    {
        if (m_curSel[type] >= m_listState[type].Count) return;
        m_curType = type;
        HeroStateDef stateDef = new HeroStateDef();
        stateDef.m_stateNo = GetNewStateNo();
        stateDef.Copy(m_listState[type][m_curSel[type]]);
        m_listState[m_curType].Add(stateDef);
        m_dicStateDef.Add(stateDef.m_stateNo, stateDef);
        m_fixedRect[m_curType].m_itemCount = m_listState[m_curType].Count;
        m_fixedRect[m_curType].Init(m_fixedRect[m_curType].m_startIndex);
    }
    public void ButtonOKPress()
    {
        UpdateUIToBaseInfo();
        m_closeType = 1;
        GlobalAssist.HideUI(gameObject);
    }
    public void ButtonCancelPress()
    {
        m_closeType = 2;
        if (m_openType == 0)//取消新建，删掉角色
        {
            GlobalAssist.m_listHero.Remove(m_curHero);
            GlobalAssist.m_dicHero.Remove(m_curHero.m_id);
        }
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
        RefreshContent(tag, startIndex, endIndex);
    }
    public void AddOneRow(int tag, int index)
    {
        GameObject obj = Instantiate(m_rowPrefab, m_parentTransform[tag]);
        UI_StateDefRowUI boxUI = obj.GetComponent<UI_StateDefRowUI>();
        boxUI.m_index = index;
        boxUI.m_rect = m_fixedRect[tag];
        m_fixedRect[tag].m_listRow.Add(boxUI);
        m_listRow[tag].Add(boxUI);
    }
    public void RowPress(int tag, int type, int itemIndex, int rowIndex)
    {
        m_curSel[tag] = itemIndex;
        RefreshContent(tag, m_fixedRect[tag].m_startIndex, m_fixedRect[tag].m_endIndex);
    }
    public void RowEnter(int tag, int itemIndex, int rowIndex, float posY)
    {

    }
    public void RowExit(int tag)
    {

    }
}
