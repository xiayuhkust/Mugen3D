using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_EditStateUI : MonoBehaviour, UI_Base, UI_Fixed
{
    public InputField m_inputFieldRemark;
    public InputField m_inputFieldCondition;
    public Toggle m_togglePersist;
    public Toggle m_toggleExecuteOnce;
    public Toggle m_toggleSameCondition;
    public Button m_buttonEditCondition;
    public GameObject[] m_partControl = new GameObject[14];
    //切换状态
    public Toggle[] m_toggleChangeStateTarget = new Toggle[2];
    public InputField m_inputFieldChangeStateNo;
    public Dropdown m_dropdownNextStateControl;
    //断言
    public Dropdown m_dropdownAssert;
    //残影
    public InputField m_inputFieldAfterImageDuration;
    public InputField m_inputFieldAfterImageNum;
    public InputField m_inputFieldAfterImageStayDuration;
    public Image m_imageAfterImageColor;
    //位移
    public Toggle[] m_toggleDisplacementTarget = new Toggle[2];
    public Dropdown m_dropdownDisplacement;
    public Dropdown[] m_dropdownDisplacementXYZ = new Dropdown[3];
    public InputField[] m_inputFieldDisplacement = new InputField[3];
    //特效
    public Toggle m_toggleEffectNoMove;
    public InputField m_inputFieldEffectSpeed;
    public InputField[] m_inputFieldEffectPos = new InputField[3];
    public InputField[] m_inputFieldEffectRandomX = new InputField[2];
    public InputField[] m_inputFieldEffectRandomY = new InputField[2];
    public InputField[] m_inputFieldEffectRotate = new InputField[3];
    public InputField[] m_inputFieldEffectScale = new InputField[3];
    public RectTransform m_parentTransformEffect;
    public GameObject m_rowPrefabEffect;
    private List<string> m_listEffectID = new List<string>();
    //音效
    public InputField m_inputFieldSoundID;
    public Slider m_sliderVolume;
    public Text m_textVolume;
    //目标绑定/解绑
    public Toggle[] m_toggleTargetType = new Toggle[2];
    public InputField m_inputFieldTargetDuration;
    public InputField[] m_inputFieldTargetPos = new InputField[3];
    //攻击判定
    public Toggle[] m_toggleHitDefAttr = new Toggle[3];
    public Dropdown[] m_dropdownHitDefAttr = new Dropdown[2];
    public Toggle[] m_toggleHitFlag = new Toggle[6];
    public Toggle[] m_toggleGuardFlag = new Toggle[4];
    public Dropdown m_dropdownGroundType;
    public Dropdown m_dropdownAirType;
    public Dropdown m_dropdownHitAnim;
    public Dropdown m_dropdownHitAnimAir;
    public InputField[] m_inputFieldDamage = new InputField[2];
    public InputField[] m_inputFieldPauseTime = new InputField[2];
    public InputField m_inputFieldPauseDelay;
    public InputField[] m_inputFieldSparkNo = new InputField[2];
    public InputField[] m_inputFieldSparkPos = new InputField[3];
    public InputField[] m_inputFieldSparkScale = new InputField[3];
    public InputField[] m_inputFieldSoundNo = new InputField[2];
    public InputField m_inputFieldSlideTime;
    public InputField m_inputFieldHitTime;
    public InputField m_inputFieldHitTimeAir;
    public InputField[] m_inputFieldHitVelocity = new InputField[3];
    public InputField m_inputFieldGuardVelocityX;
    public InputField m_inputFieldGuardSlideTime;
    public InputField[] m_inputFieldHitVelocityAir = new InputField[3];
    public Toggle m_toggleYaccel;
    public InputField m_inputFieldYaccl;
    public Toggle m_toggleGuardDist;
    public InputField m_inputFieldGuardDist;
    public RectTransform m_parentTransformCollideInfo;
    public GameObject m_rowPrefabCollideInfo;
    public UI_FixedScrollRect m_fixedRectCollide;
    private List<CollideInfo> m_listCollideInfo = new List<CollideInfo>();//碰撞信息
    private List<UI_CollideInfoRowUI> m_listCollideRow = new List<UI_CollideInfoRowUI>();
    private int m_curSelCollideInfo = 0;
    //援助人物

    //暂停
    public InputField m_inputFieldPauseDuration;
    //屏幕震动
    public InputField m_inputFieldShakeDuration;
    public Dropdown m_dropdownShakeRange;
    public InputField m_inputFieldShakeRange;
    //参数修改
    public Toggle[] m_toggleValueTarget = new Toggle[2];
    public Dropdown[] m_dropdownValueType = new Dropdown[6];
    public InputField[] m_inputFieldValue = new InputField[6];
    public Dropdown m_dropdownValueControl;
    //变量设置
    public Toggle[] m_toggleVarSetType = new Toggle[2];
    public Toggle[] m_toggleVarSetValueType = new Toggle[2];
    public Dropdown m_dropdownVarSetIndex;
    public InputField m_inputFieldVarSetValue;

    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    private int m_curType = 0;
    public static bool m_isFinish = false;
    public static int m_openType = 0;//0-新建，1-修改
    public static HeroState m_state;

    private static UI_EditStateUI m_UI = null;
    public static void OpenUI(HeroState state, int openType)
    {
        m_state = state;
        m_openType = openType;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditStateUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditStateUI>();
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
        InitDropdown();
        if (m_state.m_control != null)
        {
            ButtonControlPress((int)m_state.m_control.GetControlType());
            UpdateInfoToUI(m_state.m_control.GetControlType());
        }
        else ButtonControlPress(0);
        RefreshItemState();
        m_fixedRectCollide.m_baseUI = this;
        m_fixedRectCollide.m_tag = 0;
        m_fixedRectCollide.m_rowLimit = 5;
        m_fixedRectCollide.m_oneRowNum = 1;
        m_fixedRectCollide.m_itemCount = m_listCollideInfo.Count;
        m_fixedRectCollide.Init(0);
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
        if (UI_EditFormulaUI.m_isFinish)
        {
            UI_EditFormulaUI.m_isFinish = false;
            m_inputFieldCondition.text = UI_EditFormulaUI.m_formula;
        }
        if (UI_SelectColorUI.m_isFinish)
        {
            UI_SelectColorUI.m_isFinish = false;
            m_imageAfterImageColor.color = UI_SelectColorUI.m_color;
        }
        if (UI_EditBodyColliderUI.m_isFinish)
        {
            UI_EditBodyColliderUI.m_isFinish = false;
            if (UI_EditBodyColliderUI.m_openType == 0) m_listCollideInfo.Add(UI_EditBodyColliderUI.m_ci);
            m_fixedRectCollide.m_itemCount = m_listCollideInfo.Count;
            m_fixedRectCollide.Init(m_fixedRectCollide.m_startIndex);
        }
    }
    void InitDropdown()
    {
        m_listEffectID.Clear();
        if (m_dropdownNextStateControl.options.Count > 0) return;
        List<Dropdown.OptionData> listData = new List<Dropdown.OptionData>();
        //切换状态
        List<string> listStr = HSC_ChangeState.GetListControlType();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownNextStateControl.ClearOptions();
        m_dropdownNextStateControl.AddOptions(listData);
        m_dropdownNextStateControl.value = 0;
        //断言
        listData = new List<Dropdown.OptionData>();
        listStr = HSC_AssertSpecial.GetListAssertStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownAssert.ClearOptions();
        m_dropdownAssert.AddOptions(listData);
        m_dropdownAssert.value = 0;
        //位移
        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Displacement.GetListTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownDisplacement.ClearOptions();
        m_dropdownDisplacement.AddOptions(listData);
        m_dropdownDisplacement.value = 0;

        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Displacement.GetListValueTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        for (int i = 0; i < 3; i++)
        {
            m_dropdownDisplacementXYZ[i].ClearOptions();
            m_dropdownDisplacementXYZ[i].AddOptions(listData);
            m_dropdownDisplacementXYZ[i].value = 0;
        }
        //攻击判定
        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Hitdef.GetListAttr1();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownHitDefAttr[0].ClearOptions();
        m_dropdownHitDefAttr[0].AddOptions(listData);
        m_dropdownHitDefAttr[0].value = 0;

        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Hitdef.GetListAttr2();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownHitDefAttr[1].ClearOptions();
        m_dropdownHitDefAttr[1].AddOptions(listData);
        m_dropdownHitDefAttr[1].value = 0;

        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Hitdef.GetListAnimType();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownHitAnim.ClearOptions();
        m_dropdownHitAnim.AddOptions(listData);
        m_dropdownHitAnim.value = 0;
        m_dropdownHitAnimAir.ClearOptions();
        m_dropdownHitAnimAir.AddOptions(listData);
        m_dropdownHitAnimAir.value = 0;

        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Hitdef.GetListGroundType();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownGroundType.ClearOptions();
        m_dropdownGroundType.AddOptions(listData);
        m_dropdownGroundType.value = 0;
        m_dropdownAirType.ClearOptions();
        m_dropdownAirType.AddOptions(listData);
        m_dropdownAirType.value = 0;
        //屏幕震动
        listData = new List<Dropdown.OptionData>();
        listStr = HSC_ScreenShake.GetListTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownShakeRange.ClearOptions();
        m_dropdownShakeRange.AddOptions(listData);
        m_dropdownShakeRange.value = 0;
        //参数修改
        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Value.GetListCtrlTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        m_dropdownValueControl.ClearOptions();
        m_dropdownValueControl.AddOptions(listData);
        m_dropdownValueControl.value = 0;

        listData = new List<Dropdown.OptionData>();
        listStr = HSC_Value.GetListValueTypeStr();
        for (int i = 0; i < listStr.Count; i++)
        {
            listData.Add(new Dropdown.OptionData(listStr[i]));
        }
        for (int i = 0; i < 6; i++)
        {
            m_dropdownValueType[i].ClearOptions();
            m_dropdownValueType[i].AddOptions(listData);
            m_dropdownValueType[i].value = 0;
        }
        //变量设置
        listData = new List<Dropdown.OptionData>();
        for (int i = 0; i < 50; i++)
        {
            listData.Add(new Dropdown.OptionData(i.ToString()));
        }
        m_dropdownVarSetIndex.ClearOptions();
        m_dropdownVarSetIndex.AddOptions(listData);
        m_dropdownVarSetIndex.value = 0;

    }
    void UpdateInfoToUI(ControlType type)
    {
        m_togglePersist.isOn = m_state.m_ignorehitpause;
        m_toggleExecuteOnce.isOn = m_state.m_executeOnce;
        m_inputFieldRemark.text = m_state.m_remark.GetStr();
        m_inputFieldCondition.text = m_state.m_condition;
        m_toggleSameCondition.isOn = m_state.m_sameCondition;
        if (type == ControlType.ChangeState)
        {
            HSC_ChangeState control = (HSC_ChangeState)m_state.m_control;
            m_toggleChangeStateTarget[control.m_target].isOn = true;
            m_inputFieldChangeStateNo.text = control.m_changeStateNo.ToString();
            m_dropdownNextStateControl.value = control.m_ctrl;
        }
        else if (type == ControlType.Assert)
        {
            HSC_AssertSpecial control = (HSC_AssertSpecial)m_state.m_control;
            m_dropdownAssert.value = control.m_assert;
        }
        else if (type == ControlType.AfterImage)
        {
            HSC_AfterImage control = (HSC_AfterImage)m_state.m_control;
            m_inputFieldAfterImageDuration.text = control.m_duration.ToString();
            m_inputFieldAfterImageNum.text = control.m_num.ToString();
            m_inputFieldAfterImageStayDuration.text = control.m_stayDuration.ToString();
            m_imageAfterImageColor.color = control.m_color;
        }
        else if (type == ControlType.Displacement)
        {
            HSC_Displacement control = (HSC_Displacement)m_state.m_control;
            m_toggleDisplacementTarget[control.m_target].isOn = true;
            m_dropdownDisplacement.value = control.m_type;
            for (int i = 0; i < 3; i++)
            {
                m_dropdownDisplacementXYZ[i].value = control.m_valueType[i];
                m_inputFieldDisplacement[i].text = control.m_value[i].ToString();
            }
        }
        else if (type == ControlType.Effect)
        {
            HSC_Effect control = (HSC_Effect)m_state.m_control;
            m_toggleEffectNoMove.isOn = control.m_effectStatic;
            m_inputFieldEffectSpeed.text = control.m_effectSpeed.ToString();
            for (int i = 0; i < 2; i++)
            {
                m_inputFieldEffectRandomX[i].text = control.m_randomPosX[i].ToString();
                m_inputFieldEffectRandomY[i].text = control.m_randomPosY[i].ToString();
            }
            for (int i = 0; i < 3; i++)
            {
                m_inputFieldEffectPos[i].text = control.m_effectPos[i].ToString();
                m_inputFieldEffectRotate[i].text = control.m_effectRotation[i].ToString();
                m_inputFieldEffectScale[i].text = control.m_effectScale[i].ToString();
            }
            m_listEffectID.Clear();
            m_listEffectID.AddRange(control.m_listEffectID);
        }
        else if (type == ControlType.Sound)
        {
            HSC_Sound control = (HSC_Sound)m_state.m_control;
            m_inputFieldSoundID.text = control.m_soundID;
            m_sliderVolume.value = control.m_volume;
        }
        else if (type == ControlType.Target)
        {
            HSC_Target control = (HSC_Target)m_state.m_control;
            m_toggleTargetType[control.m_type].isOn = true;
            m_inputFieldTargetDuration.text = control.m_duration.ToString();
            m_inputFieldTargetPos[0].text = control.m_pos[0].ToString();
            m_inputFieldTargetPos[1].text = control.m_pos[1].ToString();
            m_inputFieldTargetPos[2].text = control.m_pos[2].ToString();
        }
        else if (type == ControlType.HitDef)
        {
            HSC_Hitdef control = (HSC_Hitdef)m_state.m_control;
            for (int i = 0; i < 3; i++) m_toggleHitDefAttr[i].isOn = control.m_attrA[i];
            m_dropdownHitDefAttr[0].value = control.m_attr1;
            m_dropdownHitDefAttr[1].value = control.m_attr2;
            for (int i = 0; i < 6; i++) m_toggleHitFlag[i].isOn = control.m_hitFlag[i];
            for (int i = 0; i < 4; i++) m_toggleGuardFlag[i].isOn = control.m_guardFlag[i];
            m_dropdownGroundType.value = control.m_groundType;
            m_dropdownAirType.value = control.m_airType;
            m_dropdownHitAnim.value = control.m_animType;
            m_dropdownHitAnimAir.value = control.m_animTypeAir;
            for (int i = 0; i < 2; i++) m_inputFieldDamage[i].text = control.m_damage[i].ToString();
            for (int i = 0; i < 2; i++) m_inputFieldPauseTime[i].text = control.m_pauseTime[i].ToString();
            m_inputFieldPauseDelay.text = control.m_pauseDelay.ToString();
            for (int i = 0; i < 2; i++) m_inputFieldSparkNo[i].text = control.m_sparkNo[i].ToString();
            for (int i = 0; i < 3; i++) m_inputFieldSparkPos[i].text = control.m_sparkXYZ[i].ToString();
            for (int i = 0; i < 3; i++) m_inputFieldSparkScale[i].text = control.m_sparkScale[i].ToString();
            for (int i = 0; i < 2; i++) m_inputFieldSoundNo[i].text = control.m_soundID[i];
            m_inputFieldSlideTime.text = control.m_groundSlideTime.ToString();
            m_inputFieldHitTime.text = control.m_groundHitTime.ToString();
            m_inputFieldHitTimeAir.text = control.m_airHitTime.ToString();
            for (int i = 0; i < 3; i++) m_inputFieldHitVelocity[i].text = control.m_groundVelocity[i].ToString();
            m_inputFieldGuardVelocityX.text = control.m_guardVelocityX.ToString();
            m_inputFieldGuardSlideTime.text = control.m_guardSlideTime.ToString();
            for (int i = 0; i < 3; i++) m_inputFieldHitVelocityAir[i].text = control.m_airVelocity[i].ToString();
            m_toggleYaccel.isOn = control.m_isYAccel;
            m_inputFieldYaccl.text = control.m_yAccel.ToString();
            m_toggleGuardDist.isOn = control.m_isGuardDist;
            m_inputFieldGuardDist.text = control.m_guardDist.ToString();
            m_listCollideInfo.Clear();
            for (int i = 0; i < control.m_listCollideInfo.Count; i++)
            {
                CollideInfo ci = CollideInfo.GetOne();
                ci.Copy(control.m_listCollideInfo[i]);
                m_listCollideInfo.Add(ci);
            }
        }
        else if (type == ControlType.Helper)
        {
            HSC_Helper control = (HSC_Helper)m_state.m_control;

        }
        else if (type == ControlType.Pause)
        {
            HSC_Pause control = (HSC_Pause)m_state.m_control;
            m_inputFieldPauseDuration.text = control.m_duration.ToString();
        }
        else if (type == ControlType.PlayerPush)
        {

        }
        else if (type == ControlType.ScreenShake)
        {
            HSC_ScreenShake control = (HSC_ScreenShake)m_state.m_control;
            m_inputFieldShakeDuration.text = control.m_duration.ToString();
            m_dropdownShakeRange.value = control.m_shakeType;
            m_inputFieldShakeRange.text = control.m_shakeRange.ToString();
        }
        else if (type == ControlType.Value)
        {
            HSC_Value control = (HSC_Value)m_state.m_control;
            m_toggleValueTarget[control.m_target].isOn = true;
            m_inputFieldValue[0].text = control.m_life.ToString();
            m_inputFieldValue[1].text = control.m_power.ToString();
            m_inputFieldValue[2].text = control.m_gravity.ToString();
            m_inputFieldValue[3].text = control.m_defenceMulSet.ToString();
            m_inputFieldValue[4].text = control.m_attackMulSet.ToString();
            m_inputFieldValue[5].text = control.m_animSpeed.ToString();
            m_dropdownValueType[0].value = control.m_lifeType;
            m_dropdownValueType[1].value = control.m_powerType;
            m_dropdownValueType[2].value = control.m_gravityType;
            m_dropdownValueType[3].value = control.m_defenceMulType;
            m_dropdownValueType[4].value = control.m_attackMulType;
            m_dropdownValueType[5].value = control.m_animSpeedType;
            m_dropdownValueControl.value = control.m_ctrl;
        }
        else if (type == ControlType.VarSet)
        {
            HSC_VarSet control = (HSC_VarSet)m_state.m_control;
            m_toggleVarSetType[control.m_type].isOn = true;
            m_toggleVarSetValueType[control.m_valueType].isOn = true;
            m_dropdownVarSetIndex.value = control.m_index;
            if (control.m_valueType == 0) m_inputFieldVarSetValue.text = control.m_valInt.ToString();
            else m_inputFieldVarSetValue.text = control.m_valFloat.ToString();
        }
    }
    void UpdateUIToInfo()
    {
        m_state.m_ignorehitpause = m_togglePersist.isOn;
        m_state.m_executeOnce = m_toggleExecuteOnce.isOn;
        m_state.m_remark.SetStr(m_inputFieldRemark.text);
        m_state.m_condition = m_inputFieldCondition.text;
        m_state.m_sameCondition = m_toggleSameCondition.isOn;
        if (m_curType == 0)
        {
            HSC_ChangeState control = new HSC_ChangeState();
            control.m_target = m_toggleChangeStateTarget[0].isOn ? 0 : 1;
            if (m_inputFieldChangeStateNo.text == "") m_inputFieldChangeStateNo.text = "0";
            control.m_changeStateNo = Convert.ToInt32(m_inputFieldChangeStateNo.text);
            control.m_ctrl = m_dropdownNextStateControl.value;
            m_state.m_control = control;
        }
        else if (m_curType == 1)
        {
            HSC_AssertSpecial control = new HSC_AssertSpecial();
            control.m_assert = m_dropdownAssert.value;
            m_state.m_control = control;
        }
        else if (m_curType == 2)
        {
            HSC_AfterImage control = new HSC_AfterImage();
            if (m_inputFieldAfterImageDuration.text == "") m_inputFieldAfterImageDuration.text = "0";
            control.m_duration = Convert.ToSingle(m_inputFieldAfterImageDuration.text);
            if (m_inputFieldAfterImageNum.text == "") m_inputFieldAfterImageNum.text = "0";
            control.m_num = Convert.ToInt32(m_inputFieldAfterImageNum.text);
            if (m_inputFieldAfterImageStayDuration.text == "") m_inputFieldAfterImageStayDuration.text = "0";
            control.m_stayDuration = Convert.ToSingle(m_inputFieldAfterImageStayDuration.text);
            control.m_color = m_imageAfterImageColor.color;
            m_state.m_control = control;
        }
        else if (m_curType == 3)
        {
            HSC_Displacement control = new HSC_Displacement();
            control.m_target = m_toggleDisplacementTarget[0].isOn ? 0 : 1;
            control.m_type = m_dropdownDisplacement.value;
            for (int i = 0; i < 3; i++)
            {
                control.m_valueType[i] = m_dropdownDisplacementXYZ[i].value;
                if (m_inputFieldDisplacement[i].text == "") m_inputFieldDisplacement[i].text = "0";
                control.m_value[i] = Convert.ToSingle(m_inputFieldDisplacement[i].text);
            }
            m_state.m_control = control;
        }
        else if (m_curType == 4)
        {
            HSC_Effect control = new HSC_Effect();
            control.m_effectStatic = m_toggleEffectNoMove.isOn;
            if (m_inputFieldEffectSpeed.text == "") m_inputFieldEffectSpeed.text = "1";
            control.m_effectSpeed = Convert.ToSingle(m_inputFieldEffectSpeed.text);
            for (int i = 0; i < 2; i++)
            {
                if (m_inputFieldEffectRandomX[i].text == "") m_inputFieldEffectRandomX[i].text = "0";
                control.m_randomPosX[i] = Convert.ToSingle(m_inputFieldEffectRandomX[i].text);
                if (m_inputFieldEffectRandomY[i].text == "") m_inputFieldEffectRandomY[i].text = "0";
                control.m_randomPosY[i] = Convert.ToSingle(m_inputFieldEffectRandomY[i].text);
            }
            for (int i = 0; i < 3; i++)
            {
                if (m_inputFieldEffectPos[i].text == "") m_inputFieldEffectPos[i].text = "0";
                control.m_effectPos[i] = Convert.ToSingle(m_inputFieldEffectPos[i].text);
                if (m_inputFieldEffectRotate[i].text == "") m_inputFieldEffectRotate[i].text = "0";
                control.m_effectRotation[i] = Convert.ToSingle(m_inputFieldEffectRotate[i].text);
                if (m_inputFieldEffectScale[i].text == "") m_inputFieldEffectScale[i].text = "1";
                control.m_effectScale[i] = Convert.ToSingle(m_inputFieldEffectScale[i].text);
            }
            control.m_listEffectID.AddRange(m_listEffectID);
            m_state.m_control = control;
        }
        else if (m_curType == 5)
        {
            HSC_Sound control = new HSC_Sound();
            control.m_soundID = m_inputFieldSoundID.text;
            control.m_volume = m_sliderVolume.value;
            m_state.m_control = control;
        }
        else if (m_curType == 6)
        {
            HSC_Target control = new HSC_Target();
            control.m_type = m_toggleTargetType[0].isOn ? 0 : 1;
            if (m_inputFieldTargetDuration.text == "") m_inputFieldTargetDuration.text = "1";
            control.m_duration = Convert.ToSingle(m_inputFieldTargetDuration.text);
            if (m_inputFieldTargetPos[0].text == "") m_inputFieldTargetPos[0].text = "0";
            control.m_pos[0] = Convert.ToSingle(m_inputFieldTargetPos[0].text);
            if (m_inputFieldTargetPos[1].text == "") m_inputFieldTargetPos[1].text = "0";
            control.m_pos[1] = Convert.ToSingle(m_inputFieldTargetPos[1].text);
            if (m_inputFieldTargetPos[2].text == "") m_inputFieldTargetPos[2].text = "0";
            control.m_pos[2] = Convert.ToSingle(m_inputFieldTargetPos[2].text);
            m_state.m_control = control;
        }
        else if (m_curType == 7)
        {
            HSC_Hitdef control = new HSC_Hitdef();
            for (int i = 0; i < 3; i++) control.m_attrA[i] = m_toggleHitDefAttr[i].isOn;
            control.m_attr1 = m_dropdownHitDefAttr[0].value;
            control.m_attr2 = m_dropdownHitDefAttr[1].value;
            for (int i = 0; i < 6; i++) control.m_hitFlag[i] = m_toggleHitFlag[i].isOn;
            for (int i = 0; i < 4; i++) control.m_guardFlag[i] = m_toggleGuardFlag[i].isOn;
            control.m_groundType = m_dropdownGroundType.value;
            control.m_airType = m_dropdownAirType.value;
            control.m_animType = m_dropdownHitAnim.value;
            control.m_animTypeAir = m_dropdownHitAnimAir.value;
            for (int i = 0; i < 2; i++)
            {
                if (m_inputFieldDamage[i].text == "") m_inputFieldDamage[i].text = "0";
                control.m_damage[i] = Convert.ToInt32(m_inputFieldDamage[i].text);
            }
            for (int i = 0; i < 2; i++)
            {
                if (m_inputFieldPauseTime[i].text == "") m_inputFieldPauseTime[i].text = "0";
                control.m_pauseTime[i] = Convert.ToSingle(m_inputFieldPauseTime[i].text);
            }
            if (m_inputFieldPauseDelay.text == "") m_inputFieldPauseDelay.text = "0";
            control.m_pauseDelay = Convert.ToSingle(m_inputFieldPauseDelay.text);
            for (int i = 0; i < 2; i++)
            {
                if (m_inputFieldSparkNo[i].text == "") m_inputFieldSparkNo[i].text = "0";
                control.m_sparkNo[i] = Convert.ToInt32(m_inputFieldSparkNo[i].text);
            }
            for (int i = 0; i < 3; i++)
            {
                if (m_inputFieldSparkPos[i].text == "") m_inputFieldSparkPos[i].text = "0";
                control.m_sparkXYZ[i] = Convert.ToSingle(m_inputFieldSparkPos[i].text);
            }
            for (int i = 0; i < 3; i++)
            {
                if (m_inputFieldSparkScale[i].text == "") m_inputFieldSparkScale[i].text = "0";
                control.m_sparkScale[i] = Convert.ToSingle(m_inputFieldSparkScale[i].text);
            }
            for (int i = 0; i < 2; i++)
            {
                control.m_soundID[i] = m_inputFieldSoundNo[i].text;
            }
            if (m_inputFieldSlideTime.text == "") m_inputFieldSlideTime.text = "0";
            control.m_groundSlideTime = Convert.ToSingle(m_inputFieldSlideTime.text);
            if (m_inputFieldHitTime.text == "") m_inputFieldHitTime.text = "0";
            control.m_groundHitTime = Convert.ToSingle(m_inputFieldHitTime.text);
            if (m_inputFieldHitTimeAir.text == "") m_inputFieldHitTimeAir.text = "0";
            control.m_airHitTime = Convert.ToSingle(m_inputFieldHitTimeAir.text);
            for (int i = 0; i < 3; i++)
            {
                if (m_inputFieldHitVelocity[i].text == "") m_inputFieldHitVelocity[i].text = "0";
                control.m_groundVelocity[i] = Convert.ToSingle(m_inputFieldHitVelocity[i].text);
            }
            if (m_inputFieldGuardVelocityX.text == "") m_inputFieldGuardVelocityX.text = "0";
            control.m_guardVelocityX = Convert.ToSingle(m_inputFieldGuardVelocityX.text);
            if (m_inputFieldGuardSlideTime.text == "") m_inputFieldGuardSlideTime.text = "0";
            control.m_guardSlideTime = Convert.ToSingle(m_inputFieldGuardSlideTime.text);
            for (int i = 0; i < 3; i++)
            {
                if (m_inputFieldHitVelocityAir[i].text == "") m_inputFieldHitVelocityAir[i].text = "0";
                control.m_airVelocity[i] = Convert.ToSingle(m_inputFieldHitVelocityAir[i].text);
            }
            control.m_isYAccel = m_toggleYaccel.isOn;
            if (m_inputFieldYaccl.text == "") m_inputFieldYaccl.text = "0";
            control.m_yAccel = Convert.ToSingle(m_inputFieldYaccl.text);
            control.m_isGuardDist = m_toggleGuardDist.isOn;
            if (m_inputFieldGuardDist.text == "") m_inputFieldGuardDist.text = "1";
            control.m_guardDist = Convert.ToSingle(m_inputFieldGuardDist.text);
            control.m_listCollideInfo.Clear();
            control.m_listCollideInfo.AddRange(m_listCollideInfo);
            m_state.m_control = control;
        }
        else if (m_curType == 8)
        {

        }
        else if (m_curType == 9)
        {
            HSC_Pause control = new HSC_Pause();
            if (m_inputFieldPauseDuration.text == "") m_inputFieldPauseDuration.text = "1";
            control.m_duration = Convert.ToSingle(m_inputFieldPauseDuration.text);
            m_state.m_control = control;
        }
        else if (m_curType == 10)
        {
            HSC_PlayerPush control = new HSC_PlayerPush();
            m_state.m_control = control;
        }
        else if (m_curType == 11)
        {
            HSC_ScreenShake control = new HSC_ScreenShake();
            if (m_inputFieldShakeDuration.text == "") m_inputFieldShakeDuration.text = "1";
            control.m_duration = Convert.ToSingle(m_inputFieldShakeDuration.text);
            control.m_shakeType = m_dropdownShakeRange.value;
            if (m_inputFieldShakeRange.text == "") m_inputFieldShakeRange.text = "0";
            control.m_shakeRange = Convert.ToSingle(m_inputFieldShakeRange.text);
            m_state.m_control = control;
        }
        else if (m_curType == 12)
        {
            HSC_Value control = new HSC_Value();
            control.m_target = m_toggleValueTarget[0].isOn ? 0 : 1;
            for (int i = 0; i < 6; i++)
            {
                if (m_inputFieldValue[i].text == "") m_inputFieldValue[i].text = "0";
            }
            control.m_life = Convert.ToSingle(m_inputFieldValue[0].text);
            control.m_power = Convert.ToSingle(m_inputFieldValue[1].text);
            control.m_gravity = Convert.ToSingle(m_inputFieldValue[2].text);
            control.m_defenceMulSet = Convert.ToSingle(m_inputFieldValue[3].text);
            control.m_attackMulSet = Convert.ToSingle(m_inputFieldValue[4].text);
            control.m_animSpeed = Convert.ToSingle(m_inputFieldValue[5].text);
            control.m_lifeType = m_dropdownValueType[0].value;
            control.m_powerType = m_dropdownValueType[1].value;
            control.m_gravityType = m_dropdownValueType[2].value;
            control.m_defenceMulType = m_dropdownValueType[3].value;
            control.m_attackMulType = m_dropdownValueType[4].value;
            control.m_animSpeedType = m_dropdownValueType[5].value;
            m_state.m_control = control;
        }
        else if (m_curType == 13)
        {
            HSC_VarSet control = new HSC_VarSet();
            control.m_type = m_toggleVarSetType[0].isOn ? 0 : 1;
            control.m_valueType = m_toggleVarSetValueType[0].isOn ? 0 : 1;
            control.m_index = m_dropdownVarSetIndex.value;
            if (m_inputFieldVarSetValue.text == "") m_inputFieldVarSetValue.text = "0";
            control.m_valInt = Convert.ToInt32(m_inputFieldVarSetValue.text);
            control.m_valFloat = Convert.ToSingle(m_inputFieldVarSetValue.text);
            m_state.m_control = control;
        }
    }
    public void RefreshItemState()
    {
        m_inputFieldTargetDuration.interactable = m_toggleTargetType[0].isOn;
        m_inputFieldTargetPos[0].interactable = m_inputFieldTargetPos[1].interactable = m_inputFieldTargetPos[2].interactable = m_toggleTargetType[0].isOn;
        m_inputFieldDisplacement[0].interactable = m_dropdownDisplacement.value != 1 && m_dropdownDisplacementXYZ[0].value == 0;
        m_inputFieldDisplacement[2].interactable = m_dropdownDisplacement.value != 1 && m_dropdownDisplacementXYZ[2].value == 0;
        m_dropdownDisplacementXYZ[0].interactable = m_dropdownDisplacement.value != 1;
        m_dropdownDisplacementXYZ[2].interactable = m_dropdownDisplacement.value != 1;
        for (int i = 0; i < 6; i++) m_inputFieldValue[i].interactable = m_dropdownValueType[i].value > 0;
        m_buttonEditCondition.interactable = !m_toggleSameCondition.isOn;
        m_inputFieldYaccl.interactable = m_toggleYaccel.isOn;
        m_inputFieldGuardDist.interactable = m_toggleGuardDist.isOn;
    }
    void RefreshContentCollide(int startIndex, int endIndex)
    {
        for (int index = startIndex; index < endIndex; index++)
        {
            UI_CollideInfoRowUI boxUI = m_listCollideRow[index - startIndex];
            CollideInfo ci = m_listCollideInfo[index];
            if (m_curSelCollideInfo == index) boxUI.m_imageBack.color = new Color(1, 1, 1, 0.8f);
            else boxUI.m_imageBack.color = new Color(1, 1, 1, 0.4f);
            boxUI.m_textInfo[0].text = ci.GetDisplayStr();
            boxUI.m_textInfo[1].text = ci.m_startAnimIndex + "-" + ci.m_startAnimPer + "%";
            boxUI.m_textInfo[2].text = ci.m_endAnimIndex + "-" + ci.m_endAnimPer + "%";
        }
    }
    public void ButtonEditConditionPress()
    {
        UI_EditFormulaUI.OpenUI(m_inputFieldCondition.text);
    }
    public void ButtonEditEffectPress()
    {

    }
    public void ButtonControlPress(int index)
    {
        for (int i = 0; i < m_partControl.Length; i++) m_partControl[i].SetActive(false);
        m_partControl[index].SetActive(true);
        m_curType = index;
    }
    public void ButtonAfterImageColorPress()
    {
        UI_SelectColorUI.OpenUI(gameObject, m_imageAfterImageColor.color);
    }
    public void ButtonAddCollidePress()
    {
        CollideInfo ci = CollideInfo.GetOne();
        UI_EditBodyColliderUI.OpenUI(ci, 0);
    }
    public void ButtonEditCollidePress()
    {
        if (m_curSelCollideInfo >= m_listCollideInfo.Count) return;
        UI_EditBodyColliderUI.OpenUI(m_listCollideInfo[m_curSelCollideInfo], 1);
    }
    public void ButtonDelCollidePress()
    {
        if (m_curSelCollideInfo >= m_listCollideInfo.Count) return;
        m_listCollideInfo.RemoveAt(m_curSelCollideInfo);
        m_fixedRectCollide.m_itemCount = m_listCollideInfo.Count;
        m_fixedRectCollide.Init(m_fixedRectCollide.m_startIndex);
    }
    public void ButtonOKPress()
    {
        try
        {
            UpdateUIToInfo();
        }
        catch(Exception exc)
        {
            GlobalAssist.ShowCenterTips(exc.ToString());
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
        if (tag == 0) RefreshContentCollide(startIndex, endIndex);
    }
    public void AddOneRow(int tag, int index)
    {
        GameObject obj;
        if (tag == 0)
        {
            obj = Instantiate(m_rowPrefabCollideInfo, m_parentTransformCollideInfo);
            UI_CollideInfoRowUI boxUI = obj.GetComponent<UI_CollideInfoRowUI>();
            boxUI.m_index = index;
            boxUI.m_rect = m_fixedRectCollide;
            m_fixedRectCollide.m_listRow.Add(boxUI);
            m_listCollideRow.Add(boxUI);
        }
    }
    public void RowPress(int tag, int type, int itemIndex, int rowIndex)
    {
        if (tag == 0)
        {
            m_curSelCollideInfo = itemIndex;
            RefreshContentCollide(m_fixedRectCollide.m_startIndex, m_fixedRectCollide.m_endIndex);
        }
    }
    public void RowEnter(int tag, int itemIndex, int rowIndex, float posY)
    {

    }
    public void RowExit(int tag)
    {

    }
}
