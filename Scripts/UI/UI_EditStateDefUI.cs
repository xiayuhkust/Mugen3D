using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_EditStateDefUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public RectTransform m_parentTransform;//插入行时的父对象
    public RectTransform m_combAnimParentTransform;//插入行的父对象
    public GameObject m_rowPrefab;//行（格子）
    public GameObject m_combAnimRowPrefab;//行
    public UI_FixedScrollRect m_fixedRect;
    public InputField m_inputFieldStateNo;
    public InputField[] m_inputFieldInfo = new InputField[5];
    public Dropdown[] m_dropdownInfo = new Dropdown[4];
    public Toggle[] m_toggleInfo = new Toggle[5];
    public Toggle m_toggleRecycle;
    public Slider m_sliderProgress;
    public Text m_textProgress;
    public Text m_textSpeedState;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    private List<UI_StateRowUI> m_listRow = new List<UI_StateRowUI>();
    private List<HeroState> m_listState = new List<HeroState>();
    private List<UI_CombAnimationRowUI> m_listCardAnimRow = new List<UI_CombAnimationRowUI>();
    public List<HeroAnimation> m_listAnimation = new List<HeroAnimation>();//动画列表
    private int m_curSel = 0;
    public static bool m_isFinish = false;
    public static int m_openType = 0;//0-新建，1-修改
    public static HeroStateDef m_stateDef;
    public static Hero m_hero;

    private static UI_EditStateDefUI m_UI = null;
    public static void OpenUI(HeroStateDef stateDef, Hero hero, int openType)
    {
        m_stateDef = stateDef;
        m_openType = openType;
        m_hero = hero;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditStateDefUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditStateDefUI>();
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
        UpdateInfoToUI();
        RefreshItemState();
        m_fixedRect.m_baseUI = this;
        m_fixedRect.m_rowLimit = 11;
        m_fixedRect.m_oneRowNum = 1;
        m_fixedRect.m_itemCount = m_listState.Count;
        m_fixedRect.Init(0);
        RowPress(0, 0, 0, 0);
        RefreshCombAnimationList();
        HeroModelPreviewCtrl.m_modelCtrl[0].RefreshModel(m_hero);
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
        if (UI_SelectAnimationUI.m_isFinish)
        {
            UI_SelectAnimationUI.m_isFinish = false;
            HeroAnimation ha = new HeroAnimation();
            ha.m_name = UI_SelectAnimationUI.m_listAnimClipName[UI_SelectAnimationUI.m_curSel];
            m_listAnimation.Add(ha);
            RefreshCombAnimationList();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Animator animator = HeroModelPreviewCtrl.m_modelCtrl[0].m_animator[0];
            if (animator.speed < 0.05f)
            {
                animator.speed = 1;
            }
            else
            {
                animator.speed = 0;
                m_sliderProgress.interactable = true;
            }
        }
        if (HeroModelPreviewCtrl.m_modelCtrl[0].m_animator[0].speed < 0.05f)//暂停
        {
            m_sliderProgress.interactable = true;
            m_textSpeedState.text = "按空格继续";
        }
        else
        {
            m_sliderProgress.interactable = false;
            m_textProgress.text = (int)(HeroModelPreviewCtrl.m_curProgress * 100) + "%";
            m_sliderProgress.SetValueWithoutNotify(HeroModelPreviewCtrl.m_curProgress);
            m_textSpeedState.text = "按空格暂停";
        }
    }
    public void SliderProgressChange()
    {
        m_textProgress.text = (int)(m_sliderProgress.value * 100) + "%";
        int animatorLayer = HeroModelPreviewCtrl.m_curLayer[0];
        AnimatorStateInfo tmpStateInfo = HeroModelPreviewCtrl.m_modelCtrl[0].m_animator[0].GetCurrentAnimatorStateInfo(animatorLayer);
        HeroModelPreviewCtrl.m_modelCtrl[0].m_animator[0].Play(tmpStateInfo.fullPathHash, animatorLayer, m_sliderProgress.value);
    }
    void UpdateInfoToUI()
    {
        m_inputFieldStateNo.text = m_stateDef.m_stateNo.ToString();
        m_inputFieldInfo[0].text = m_stateDef.m_name.GetStr();
        m_toggleInfo[0].SetIsOnWithoutNotify(m_stateDef.m_isVelset);
        m_inputFieldInfo[1].text = m_stateDef.m_velset[0].ToString();
        m_inputFieldInfo[2].text = m_stateDef.m_velset[1].ToString();
        m_inputFieldInfo[3].text = m_stateDef.m_powerAdd.ToString();
        m_inputFieldInfo[4].text = m_stateDef.m_juggle.ToString();
        m_toggleInfo[1].isOn = m_stateDef.m_facep2;
        m_toggleInfo[2].isOn = m_stateDef.m_hitdefpersist;
        m_toggleInfo[3].isOn = m_stateDef.m_movehitpersist;
        m_toggleInfo[4].isOn = m_stateDef.m_hitcountpersist;

        List<Dropdown.OptionData> listData = new List<Dropdown.OptionData>();
        List<string> listStr = HeroStateDef.GetListStateTypeStr();
        for (int i = 0; i < listStr.Count; i++) 
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownInfo[0].ClearOptions();
        m_dropdownInfo[0].AddOptions(listData);
        m_dropdownInfo[0].value = (int)m_stateDef.m_type;

        listData = new List<Dropdown.OptionData>();
        listStr = HeroStateDef.GetListMoveTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownInfo[1].ClearOptions();
        m_dropdownInfo[1].AddOptions(listData);
        m_dropdownInfo[1].value = (int)m_stateDef.m_moveType;

        listData = new List<Dropdown.OptionData>();
        listStr = HeroStateDef.GetListPhysicTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownInfo[2].ClearOptions();
        m_dropdownInfo[2].AddOptions(listData);
        m_dropdownInfo[2].value = (int)m_stateDef.m_physicType;

        listData = new List<Dropdown.OptionData>();
        listStr = HeroStateDef.GetListControlTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownInfo[3].ClearOptions();
        m_dropdownInfo[3].AddOptions(listData);
        m_dropdownInfo[3].value = (int)m_stateDef.m_controlType;

        m_listState.Clear();
        for (int i = 0; i < m_stateDef.m_listState.Count; i++)
        {
            HeroState state = new HeroState();
            state.Copy(m_stateDef.m_listState[i]);
            m_listState.Add(state);
        }
        m_listAnimation.Clear();
        for (int i = 0; i < m_stateDef.m_listAnimation.Count; i++)
        {
            HeroAnimation ha = new HeroAnimation();
            ha.Copy(m_stateDef.m_listAnimation[i]);
            m_listAnimation.Add(ha);
        }
        m_toggleRecycle.isOn = m_stateDef.m_animRecycle;
    }
    void UpdateUIToInfo()
    {
        m_stateDef.m_stateNo = Convert.ToInt32(m_inputFieldStateNo.text);
        m_stateDef.m_name.SetStr(m_inputFieldInfo[0].text);
        m_stateDef.m_isVelset = m_toggleInfo[0].isOn;
        m_stateDef.m_velset[0] = Convert.ToSingle(m_inputFieldInfo[1].text);
        m_stateDef.m_velset[1] = Convert.ToSingle(m_inputFieldInfo[2].text);
        m_stateDef.m_powerAdd = Convert.ToInt32(m_inputFieldInfo[3].text);
        m_stateDef.m_juggle = Convert.ToInt32(m_inputFieldInfo[4].text);
        m_stateDef.m_facep2 = m_toggleInfo[1].isOn;
        m_stateDef.m_hitdefpersist = m_toggleInfo[2].isOn;
        m_stateDef.m_movehitpersist = m_toggleInfo[3].isOn;
        m_stateDef.m_hitcountpersist = m_toggleInfo[4].isOn;
        m_stateDef.m_type = (StateDefType)m_dropdownInfo[0].value;
        m_stateDef.m_moveType = (StateDefMoveType)m_dropdownInfo[1].value;
        m_stateDef.m_physicType = (StateDefPhysicType)m_dropdownInfo[2].value;
        m_stateDef.m_controlType = (StateDefControlType)m_dropdownInfo[3].value;
        
        m_stateDef.m_listState.Clear();
        m_stateDef.m_listState.AddRange(m_listState);
        m_stateDef.m_listAnimation.Clear();
        m_stateDef.m_listAnimation.AddRange(m_listAnimation);
        m_stateDef.m_animRecycle = m_toggleRecycle.isOn;
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
    void RefreshCombAnimationList()
    {
        int rowCount = m_listAnimation.Count;
        //设置容器大小
        int height = 34 * rowCount + 4;
        if (height < 160) SetContentSize(m_combAnimParentTransform, 160);
        else SetContentSize(m_combAnimParentTransform, height);
        //设置行
        if (m_listCardAnimRow.Count < rowCount)//已有行比所需的少，增加行
        {
            for (int i = 0; i < m_listCardAnimRow.Count; i++)
            {
                m_listCardAnimRow[i].gameObject.SetActive(true);
            }
            for (int i = m_listCardAnimRow.Count; i < rowCount; i++)
            {
                AddOneRow(i);
            }
        }
        else//将多余的行隐藏
        {
            for (int i = 0; i < rowCount; i++) m_listCardAnimRow[i].gameObject.SetActive(true);
            for (int i = rowCount; i < m_listCardAnimRow.Count; i++) m_listCardAnimRow[i].gameObject.SetActive(false);
        }
        //填列表内容
        for (int i = 0; i < m_listAnimation.Count; i++)
        {
            HeroAnimation ba = m_listAnimation[i];
            UI_CombAnimationRowUI rowUI = m_listCardAnimRow[i];
            rowUI.m_text[0].text = (i + 1).ToString();
            rowUI.m_text[1].text = ba.m_name;
            rowUI.m_inputFieldFadeTime.SetTextWithoutNotify(ba.m_fadeTime.ToString());
            rowUI.m_inputFieldOffTime.SetTextWithoutNotify(ba.m_offTime.ToString());
            rowUI.m_inputFieldEndTime.SetTextWithoutNotify(ba.m_endTime.ToString());
        }
    }
    public void AddOneRow(int i)//增加一行组合动画
    {
        GameObject obj = Instantiate(m_combAnimRowPrefab, m_combAnimParentTransform);
        UI_CombAnimationRowUI rowUI = obj.GetComponent<UI_CombAnimationRowUI>();
        rowUI.m_index = i;
        rowUI.m_baseUI = this;
        m_listCardAnimRow.Add(rowUI);
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
    public void CardAnimationUpPress(int index)
    {
        if (index == 0) return;
        HeroAnimation ba = m_listAnimation[index - 1];
        m_listAnimation[index - 1] = m_listAnimation[index];
        m_listAnimation[index] = ba;
        RefreshCombAnimationList();
    }
    public void CardAnimationDownPress(int index)
    {
        if (index >= m_listAnimation.Count - 1) return;
        HeroAnimation ba = m_listAnimation[index + 1];
        m_listAnimation[index + 1] = m_listAnimation[index];
        m_listAnimation[index] = ba;
        RefreshCombAnimationList();
    }
    public void DeleteCardAnimation(int index)
    {
        m_listAnimation.RemoveAt(index);
        RefreshCombAnimationList();
    }
    public void ButtonPlayPress()
    {
        if (m_listAnimation.Count == 0)
        {
            GlobalAssist.ShowCenterTips("未添加动画");
            return;
        }
        HeroModelPreviewCtrl.m_modelCtrl[0].StartAnimation(m_listAnimation, m_toggleRecycle.isOn, m_hero);
    }
    public void ButtonAddAnimationPress()
    {
        UI_SelectAnimationUI.OpenUI(m_hero);
    }
    public void RefreshItemState()
    {
        m_inputFieldInfo[1].interactable = m_inputFieldInfo[2].interactable = m_toggleInfo[0].isOn;
        m_inputFieldInfo[4].interactable = m_dropdownInfo[1].value == 1;
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
        m_listState.RemoveAt(m_curSel);
        m_fixedRect.m_itemCount = m_listState.Count;
        m_fixedRect.Init(m_fixedRect.m_startIndex);
    }
    public void ButtonOKPress()
    {
        m_isFinish = true;
        UpdateUIToInfo();
        if (m_openType == 0 && UI_EditOneHeroUI.m_dicStateDef.ContainsKey(m_stateDef.m_stateNo))
        {
            GlobalAssist.ShowCenterTips("State No 已存在，请修改");
            return;
        }
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
