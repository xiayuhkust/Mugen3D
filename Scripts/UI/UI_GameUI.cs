using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class InputRecord//玩家的输入记录
{
    public List<string> m_listKey = new List<string>();
    public float m_downTime = 0;//按下的时刻
    public float m_upTime = 0;//松开的时刻
    public bool m_canControl = true;//当前输入是否处于可控制状态
    public static List<InputRecord> m_listInputRecordPool = new List<InputRecord>();
    public static InputRecord GetOne()
    {
        InputRecord record;
        if (m_listInputRecordPool.Count > 0)
        {
            record = m_listInputRecordPool[0];
            m_listInputRecordPool.RemoveAt(0);
        }
        else
        {
            record = new InputRecord();
        }
        record.m_listKey.Clear();
        return record;
    }
    public void Recycle()
    {
        m_listInputRecordPool.Add(this);
    }
    public string GetDisplay()
    {
        string str = "";
        foreach (string key in m_listKey)
        {
            switch (key)
            {
                case "J": str += "跳"; break;
                case "a": str += "轻拳"; break;
                case "b": str += "中拳"; break;
                case "c": str += "重拳"; break;
                case "x": str += "轻脚"; break;
                case "y": str += "中脚"; break;
                case "z": str += "重脚"; break;
                case "s": str += "开始"; break;
                default: str += key; break;
            }
        }
        return str;
    }
    public bool IsQuickUp()//是否短按的抬起
    {
        return m_upTime > 0 && m_upTime - m_downTime < 0.1f;
    }
    public float GetJudgeTime()
    {
        if (m_upTime > 0) return m_upTime;
        else return m_downTime;
    }
}

public class UI_GameUI : MonoBehaviour, UI_Base
{
    public Text m_textTime;
    public Slider[] m_sliderHP = new Slider[2];
    public Slider[] m_sliderMP = new Slider[2];
    public Text[] m_textInput = new Text[2];
    public Text[] m_textStateNo = new Text[2];
    public Text[] m_textCommandNo = new Text[2];
    public Text m_textAI;

    public static HeroCtrl[] m_heroCtrl = new HeroCtrl[2];
    public static bool[] m_isAI = new bool[2];//是否AI控制
    public static List<InputRecord>[] m_listInputRecord = new List<InputRecord>[2] { new List<InputRecord>(), new List<InputRecord>() };
    public static int[] m_commandNo = new int[2] { 0, 0 };//双方当前输入的指令
    public static Dictionary<string, float>[] m_dicPressingKey = new Dictionary<string, float>[2] { new Dictionary<string, float>(), new Dictionary<string, float>() };//正在按下的按键，value-按下的时刻
    public static float m_pauseLeftTime = 0;//剩余暂停时长
    
    public static UI_GameUI m_UI;
    public static void Show(Hero hero1, Hero hero2, bool canControl1, bool canControl2)
    {
        m_isAI[0] = !canControl1;
        m_isAI[1] = !canControl2;
        if (m_heroCtrl[0] != null) m_heroCtrl[0].Recycle();
        m_heroCtrl[0] = HeroCtrl.GetOne(hero1.m_id);
        m_heroCtrl[0].SetPosition(new Vector3(-2, 0, 0));
        m_heroCtrl[0].m_transform.forward = new Vector3(1, 0, 0);
        m_heroCtrl[0].m_side = 0;
        if (m_heroCtrl[1] != null) m_heroCtrl[1].Recycle();
        m_heroCtrl[1] = HeroCtrl.GetOne(hero2.m_id);
        m_heroCtrl[1].SetPosition(new Vector3(2, 0, 0));
        m_heroCtrl[1].m_transform.forward = new Vector3(-1, 0, 0);
        m_heroCtrl[1].m_side = 1;
        m_UI.gameObject.SetActive(true);
        GlobalAssist.m_curUI = m_UI.gameObject;
        CameraCtrl.m_cameraCtrl.gameObject.SetActive(true);
        m_UI.Start();
    }
    public static void Hide()
    {
        Time.timeScale = 1;
        m_UI.gameObject.SetActive(false);
        CameraCtrl.m_cameraCtrl.gameObject.SetActive(false);
        for (int i = 0; i < 2; i++)
        {
            if (m_heroCtrl[i] != null) m_heroCtrl[i].Recycle();
            m_heroCtrl[i] = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 2; i++) m_listInputRecord[i].Clear();
        for (int i = 0; i < 2; i++) m_dicPressingKey[i].Clear();
        m_commandNo[0] = m_commandNo[1] = 0;
        InitFormulaID();
        ScriptManager.Init();
        for (int i = 0; i < 2; i++) InitPlayerValue(i);
        RefreshUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalAssist.m_curUI != gameObject) return;
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Hide();
            SceneManager.LoadScene("StartMenu");
            return;
        }
        if (m_pauseLeftTime > 0)
        {
            m_pauseLeftTime -= Time.unscaledDeltaTime;
            if (m_pauseLeftTime <= 0) Time.timeScale = 1;
        }
        if (Time.frameCount % 10 == 0) RefreshUI();
        for (int i = 0; i < 2; i++)
        {
            if (!m_isAI[i])
            {
                if (!UpdatePlayerControl(i)) UpdatePressing(i);
            }
            UpdatePlayer(i);
            m_textStateNo[i].text = m_heroCtrl[i].m_curStateDef != null ? (m_heroCtrl[i].m_curStateDef.m_stateNo + m_heroCtrl[i].m_curStateDef.m_name.GetStr()) : "";
        }
        CameraCtrl.m_cameraCtrl.UpdateCamera(m_heroCtrl[0].m_transform.position, m_heroCtrl[1].m_transform.position);
    }
    bool UpdatePlayerControl(int index)
    {
        HeroCtrl heroCtrl = m_heroCtrl[index];
        HeroCtrl otherHeroCtrl = m_heroCtrl[1 - index];
        //Z向
        if (heroCtrl.m_velocity.z != 0)
        {
            float dis = Vector3.Distance(otherHeroCtrl.m_transform.position, heroCtrl.m_transform.position);
            Vector3 tmpPos = heroCtrl.m_transform.position - heroCtrl.transform.right * (heroCtrl.m_velocity.z * Time.deltaTime);
            Vector3 vec = tmpPos - otherHeroCtrl.m_transform.position;
            vec.Normalize();
            heroCtrl.m_transform.position = otherHeroCtrl.m_transform.position + vec * dis;
            vec= otherHeroCtrl.m_transform.position - heroCtrl.m_transform.position;
            vec.y = 0;
            heroCtrl.m_transform.forward = vec;
            otherHeroCtrl.m_transform.forward = -vec;
        }
        float friction = heroCtrl.GetFriction();
        if (heroCtrl.m_velocity.z > friction * 0.1f)
        {
            heroCtrl.m_velocity.z -= friction * Time.deltaTime;
            if (heroCtrl.m_velocity.z <= friction * 0.1f) heroCtrl.m_velocity.z = 0;
        }
        else if (heroCtrl.m_velocity.z < -friction * 0.1f)
        {
            heroCtrl.m_velocity.z += friction * Time.deltaTime;
            if (heroCtrl.m_velocity.z >= -friction * 0.1f) heroCtrl.m_velocity.z = 0;
        }
        //X向
        if (heroCtrl.m_velocity.x != 0) 
        {
            Vector3 forward = otherHeroCtrl.m_transform.position - heroCtrl.m_transform.position;
            forward.y = 0;
            forward.Normalize();
            heroCtrl.m_transform.position += forward * (heroCtrl.m_velocity.x * Time.deltaTime);
        }
        if (heroCtrl.m_velocity.x > friction * 0.1f)
        {
            heroCtrl.m_velocity.x -= friction * Time.deltaTime;
            if (heroCtrl.m_velocity.x <= friction * 0.1f) heroCtrl.m_velocity.x = 0;
        }
        else if (heroCtrl.m_velocity.x < -friction * 0.1f)
        {
            heroCtrl.m_velocity.x += friction * Time.deltaTime;
            if (heroCtrl.m_velocity.x >= -friction * 0.1f) heroCtrl.m_velocity.x = 0;
        }
        bool hasInput = false;
        if (index == 0)//左侧玩家按键
        {
            for (int i = 97; i <= 122; i++)
            {
                if (Input.GetKeyUp((KeyCode)i) && InputData.m_dicKeyToInput[index].TryGetValue(i, out string str))
                {
                    AddPlayerInput(index, str, false);
                    hasInput = true;
                }
            }
            for (int i = 97; i <= 122; i++)
            {
                if (Input.GetKeyDown((KeyCode)i) && InputData.m_dicKeyToInput[index].TryGetValue(i, out string str))
                {
                    AddPlayerInput(index, str, true);
                    hasInput = true;
                }
            }
        }
        else//右侧玩家按键
        {
            for (int i = 256; i <= 265; i++)
            {
                if (Input.GetKeyUp((KeyCode)i) && InputData.m_dicKeyToInput[index].TryGetValue(i, out string str))
                {
                    AddPlayerInput(index, str, false);
                    hasInput = true;
                }
            }
            for (int i = 273; i <= 276; i++)
            {
                if (Input.GetKeyUp((KeyCode)i) && InputData.m_dicKeyToInput[index].TryGetValue(i, out string str))
                {
                    AddPlayerInput(index, str, false);
                    hasInput = true;
                }
            }
            for (int i = 256; i <= 265; i++)
            {
                if (Input.GetKeyDown((KeyCode)i) && InputData.m_dicKeyToInput[index].TryGetValue(i, out string str))
                {
                    AddPlayerInput(index, str, true);
                    hasInput = true;
                }
            }
            for (int i = 273; i <= 276; i++)
            {
                if (Input.GetKeyDown((KeyCode)i) && InputData.m_dicKeyToInput[index].TryGetValue(i, out string str))
                {
                    AddPlayerInput(index, str, true);
                    hasInput = true;
                }
            }
        }
        return hasInput;
    }
    void AddPlayerInput(int index, string str, bool isDown)
    {
        if (index == 1)//P2的前后取反
        {
            if (str == "←") str = "→";
            else if (str == "→") str = "←";
        }
        List<InputRecord> listRecord = m_listInputRecord[index];
        if (listRecord.Count > 0 && Time.time - listRecord[listRecord.Count - 1].m_upTime > 0.3f && m_dicPressingKey[index].Count == 0) //暂定输入间隔0.3s，超过的话清空之前的输入记录
        {
            for (int i = 0; i < listRecord.Count; i++) listRecord[i].Recycle();
            listRecord.Clear();
        }
        if (listRecord.Count > 0 && Time.time - listRecord[listRecord.Count - 1].m_downTime < 0.03f)//间隔小于0.03s的认为是同时按下
        {
            listRecord[listRecord.Count - 1].m_listKey.Add(str);
            if (isDown)
            {
                if (m_dicPressingKey[index].ContainsKey(str))
                {
                    GlobalAssist.ShowCenterTips("程序错误：输入<" + str + ">当前为按下状态，识别出重复按下，请反馈给程序", 20);
                }
                else
                {
                    m_dicPressingKey[index].Add(str, Time.time);
                }
            }
            else
            {
                if (!m_dicPressingKey[index].ContainsKey(str))
                {
                    GlobalAssist.ShowCenterTips("程序错误：输入<" + str + ">当前为松开，但是没有识别出按下的状态，请反馈给程序", 20);
                }
                else
                {
                    m_dicPressingKey[index].Remove(str);
                }
            }
        }
        else
        {
            InputRecord record = InputRecord.GetOne();
            record.m_listKey.Add(str);
            if (isDown)
            {
                record.m_downTime = Time.time;
                record.m_upTime = -1;
                if (m_dicPressingKey[index].ContainsKey(str))
                {
                    GlobalAssist.ShowCenterTips("程序错误：输入<" + str + ">当前为按下状态，识别出重复按下，请反馈给程序", 20);
                }
                else
                {
                    m_dicPressingKey[index].Add(str, Time.time);
                }
            }
            else
            {
                record.m_upTime = Time.time;
                if (!m_dicPressingKey[index].ContainsKey(str))
                {
                    GlobalAssist.ShowCenterTips("程序错误：输入<" + str + ">当前为松开，但是没有识别出按下的状态，请反馈给程序", 20);
                }
                else
                {
                    record.m_downTime = m_dicPressingKey[index][str];
                    m_dicPressingKey[index].Remove(str);
                }
            }
            record.m_canControl = m_heroCtrl[index].m_canControl;
            listRecord.Add(record);
        }
        m_commandNo[index] = 0;
        for (int i = 0; i < m_heroCtrl[index].m_hero.m_listCommand.Count; i++)
        {
            if (m_heroCtrl[index].m_hero.m_listCommand[i].MeetCommand(listRecord, m_dicPressingKey[index]))
            {
                m_commandNo[index] = m_heroCtrl[index].m_hero.m_listCommand[i].m_commandNo;
                break;
            }
        }
        m_textCommandNo[index].text = m_commandNo[index].ToString();
        m_textInput[index].text = "";
        foreach (InputRecord ir in listRecord)
        {
            if (ir.m_upTime < 0) m_textInput[index].text += ir.GetDisplay() + "   ";//只显示按下的
        }
    }
    void UpdatePressing(int index)//按下的每帧循环
    {
        if (m_dicPressingKey[index].Count == 0) return;
        foreach (string str in m_dicPressingKey[index].Keys)
        {
            List<InputRecord> listRecord = m_listInputRecord[index];
            if (listRecord.Count == 0 || listRecord[0].m_listKey.Count != 1 || listRecord[0].m_listKey[0] != str)
            {
                InputRecord record = InputRecord.GetOne();
                record.m_listKey.Add(str);
                record.m_downTime = m_dicPressingKey[index][str];
                record.m_upTime = -1;
                record.m_canControl = m_heroCtrl[index].m_canControl;
                listRecord.Add(record);
            }
            for (int i = 0; i < m_heroCtrl[index].m_hero.m_listCommand.Count; i++)
            {
                if (m_heroCtrl[index].m_hero.m_listCommand[i].MeetCommand(listRecord, m_dicPressingKey[index]))
                {
                    m_commandNo[index] = m_heroCtrl[index].m_hero.m_listCommand[i].m_commandNo;
                    break;
                }
            }
            break;
        }
    }
    void InitFormulaID()//初始化所有公式ID
    {
        int formulaID = 10000;
        ScriptManager.m_dicFormulaBool.Clear();
        for (int i = 0; i < 2; i++)
        {
            foreach (HeroStateDef hsd in m_heroCtrl[i].m_hero.m_dicStateDef.Values)
            {
                foreach (HeroState state in hsd.m_listState)
                {
                    string formulaKey = "Func" + formulaID;
                    formulaID++;
                    state.m_tmpFormulaID = formulaKey;
                    ScriptManager.m_dicFormulaBool.Add(formulaKey, state.m_condition);
                }
            }
            foreach (HeroState state in m_heroCtrl[i].m_hero.m_listOperationState)
            {
                string formulaKey = "Func" + formulaID;
                formulaID++;
                state.m_tmpFormulaID = formulaKey;
                ScriptManager.m_dicFormulaBool.Add(formulaKey, state.m_condition);
            }
            foreach (HeroState state in m_heroCtrl[i].m_hero.m_listAIState)
            {
                string formulaKey = "Func" + formulaID;
                formulaID++;
                state.m_tmpFormulaID = formulaKey;
                ScriptManager.m_dicFormulaBool.Add(formulaKey, state.m_condition);
            }
        }
    }
    public void InitPlayerValue(int index)//初始化一些开局后不变的变量
    {
        if (ScriptManager.m_proxy[index] == null) return;
        int otherIndex = 1 - index;
        ScriptManager.m_proxy[index].Fields["teamSide"] = index == 0 ? 1 : 2;
        ScriptManager.m_proxy[index].Fields["name"] = m_heroCtrl[index].m_hero.m_id;
        ScriptManager.m_proxy[index].Fields["p2name"] = m_heroCtrl[otherIndex].m_hero.m_id ;
        ScriptManager.m_proxy[index].Fields["lifeMax"] = m_heroCtrl[index].m_hero.m_baseInfo.m_lifeMax;
        ScriptManager.m_proxy[index].Fields["p2lifeMax"] = m_heroCtrl[otherIndex].m_hero.m_baseInfo.m_lifeMax;
        ScriptManager.m_proxy[index].Fields["powerMax"] = m_heroCtrl[index].m_hero.m_baseInfo.m_powerMax;
        ScriptManager.m_proxy[index].Fields["p2powerMax"] = m_heroCtrl[otherIndex].m_hero.m_baseInfo.m_powerMax;
        //顺便在此初始化StateDef
        foreach (HeroStateDef hsd in m_heroCtrl[index].m_hero.m_listBaseState)
        {
            if (hsd.m_stateNo == 0)
            {
                m_heroCtrl[index].SetStateDef(hsd);
                break;
            }
        }
    }
    public void UpdatePlayer(int index)//更新玩家
    {
        if (ScriptManager.m_proxy[index] == null) return;
        HeroCtrl heroCtrl = m_heroCtrl[index];
        HeroCtrl otherHeroCtrl = m_heroCtrl[1 - index];
        //设置触发器用的变量
        ScriptManager.m_proxy[index].Fields["posY"] = heroCtrl.m_transform.position.y;
        ScriptManager.m_proxy[index].Fields["cmd"] = m_commandNo[index];
        ScriptManager.m_proxy[index].Fields["life"] = heroCtrl.m_life;
        ScriptManager.m_proxy[index].Fields["p2life"] = otherHeroCtrl.m_life;
        ScriptManager.m_proxy[index].Fields["power"] = heroCtrl.m_power;
        ScriptManager.m_proxy[index].Fields["p2power"] = otherHeroCtrl.m_power;
        ScriptManager.m_proxy[index].Fields["state"] = heroCtrl.m_curStateDef != null ? heroCtrl.m_curStateDef.m_stateNo : 0;
        ScriptManager.m_proxy[index].Fields["p2state"] = otherHeroCtrl.m_curStateDef != null ? otherHeroCtrl.m_curStateDef.m_stateNo : 0;
        ScriptManager.m_proxy[index].Fields["prevState"] = heroCtrl.m_prevState;
        ScriptManager.m_proxy[index].Fields["p2prevState"] = otherHeroCtrl.m_prevState;
        ScriptManager.m_proxy[index].Fields["moveType"] = (int)heroCtrl.m_stateDefMoveType;
        ScriptManager.m_proxy[index].Fields["p2moveType"] = (int)otherHeroCtrl.m_stateDefMoveType;
        ScriptManager.m_proxy[index].Fields["stateType"] = (int)heroCtrl.m_stateDefType;
        ScriptManager.m_proxy[index].Fields["p2stateType"] = (int)otherHeroCtrl.m_stateDefType;
        ScriptManager.m_proxy[index].Fields["ctrl"] = heroCtrl.m_canControl;
        ScriptManager.m_proxy[index].Fields["p2ctrl"] = otherHeroCtrl.m_canControl;
        ScriptManager.m_proxy[index].Fields["time"] = heroCtrl.m_curStateTime;
        ScriptManager.m_proxy[index].Fields["anim"] = heroCtrl.m_curAnimationIndex + 1;
        ScriptManager.m_proxy[index].Fields["per"] = heroCtrl.m_curAnimationTime * 100;
        ScriptManager.m_proxy[index].Fields["velX"] = heroCtrl.m_velocity.x;
        ScriptManager.m_proxy[index].Fields["velY"] = heroCtrl.m_velocity.y;
        ScriptManager.m_proxy[index].Fields["velZ"] = heroCtrl.m_velocity.z;
        ScriptManager.m_proxy[index].Fields["p2velX"] = otherHeroCtrl.m_velocity.x;
        ScriptManager.m_proxy[index].Fields["p2velY"] = otherHeroCtrl.m_velocity.y;
        ScriptManager.m_proxy[index].Fields["p2velZ"] = otherHeroCtrl.m_velocity.z;
        Vector3 vec = otherHeroCtrl.m_transform.position - heroCtrl.m_transform.position;
        float disY = vec.y;
        vec.y = 0;
        float disX = vec.magnitude;
        ScriptManager.m_proxy[index].Fields["p2distX"] = disX;
        ScriptManager.m_proxy[index].Fields["p2distY"] = disY;
        ScriptManager.m_proxy[index].Fields["ai"] = m_isAI[index];
        ScriptManager.m_proxy[index].Fields["varFloat"] = heroCtrl.m_valueFloat;
        ScriptManager.m_proxy[index].Fields["varInt"] = heroCtrl.m_valueInt;
        ScriptManager.m_proxy[index].Fields["inGuardDist"] = disX < otherHeroCtrl.GetAttackDis();
        ScriptManager.m_proxy[index].Fields["hit"] = heroCtrl.m_hitCount;
        ScriptManager.m_proxy[index].Fields["guard"] = heroCtrl.m_guardCount;
        ScriptManager.m_proxy[index].Fields["type"] = heroCtrl.m_typeHitdef;
        ScriptManager.m_proxy[index].Fields["animtype"] = heroCtrl.m_animtypeHitdef;
        ScriptManager.m_proxy[index].Fields["airtype"] = heroCtrl.m_airtypeHitdef;
        ScriptManager.m_proxy[index].Fields["damage"] = heroCtrl.m_damageHitdef;
        ScriptManager.m_proxy[index].Fields["hitshaketime"] = heroCtrl.m_hitshaketimeHitdef;
        ScriptManager.m_proxy[index].Fields["hittime"] = heroCtrl.m_hittimeHitdef;
        ScriptManager.m_proxy[index].Fields["slidetime"] = heroCtrl.m_slidetimeHitdef;
        ScriptManager.m_proxy[index].Fields["xvel"] = heroCtrl.m_xvelHitdef;
        ScriptManager.m_proxy[index].Fields["yvel"] = heroCtrl.m_yvelHitdef;
        ScriptManager.m_proxy[index].Fields["zvel"] = heroCtrl.m_zvelHitdef;
        ScriptManager.m_proxy[index].Fields["yaccel"] = heroCtrl.m_yaccelHitdef;
        ScriptManager.m_proxy[index].Fields["p2type"] = otherHeroCtrl.m_typeHitdef;
        ScriptManager.m_proxy[index].Fields["p2animtype"] = otherHeroCtrl.m_animtypeHitdef;
        ScriptManager.m_proxy[index].Fields["p2airtype"] = otherHeroCtrl.m_airtypeHitdef;
        ScriptManager.m_proxy[index].Fields["p2damage"] = otherHeroCtrl.m_damageHitdef;
        ScriptManager.m_proxy[index].Fields["p2hitshaketime"] = otherHeroCtrl.m_hitshaketimeHitdef;
        ScriptManager.m_proxy[index].Fields["p2hittime"] = otherHeroCtrl.m_hittimeHitdef;
        ScriptManager.m_proxy[index].Fields["p2slidetime"] = otherHeroCtrl.m_slidetimeHitdef;
        ScriptManager.m_proxy[index].Fields["p2xvel"] = otherHeroCtrl.m_xvelHitdef;
        ScriptManager.m_proxy[index].Fields["p2yvel"] = otherHeroCtrl.m_yvelHitdef;
        ScriptManager.m_proxy[index].Fields["p2zvel"] = otherHeroCtrl.m_zvelHitdef;
        ScriptManager.m_proxy[index].Fields["p2yaccel"] = otherHeroCtrl.m_yaccelHitdef;

        if (disX < otherHeroCtrl.GetAttackDis() && m_dicPressingKey[index].ContainsKey("←"))//进入防御状态
        {
            if (heroCtrl.m_hero.m_dicStateDef.TryGetValue(120, out HeroStateDef hsd))
            {
                heroCtrl.SetStateDef(hsd);
            }
            else GlobalAssist.ShowCenterTips("错误：角色<" + heroCtrl.m_hero.m_name.GetStr() + ">不存在状态号<120>", 50);
        }
        heroCtrl.m_ignoreCollider = false;
        //每帧执行判断
        bool conditionResult = false;
        if (m_isAI[index])
        {
            foreach (HeroState state in heroCtrl.m_hero.m_listAIState)
            {
                if (!state.m_sameCondition) conditionResult = ScriptManager.ExecuteBool(index, state.m_tmpFormulaID);
                if (conditionResult)
                {
                    state.m_control.Execute(index, true);
                }
            }
        }
        else if(heroCtrl.m_canControl)
        {
            foreach (HeroState state in heroCtrl.m_hero.m_listOperationState)
            {
                if (!state.m_sameCondition) conditionResult = ScriptManager.ExecuteBool(index, state.m_tmpFormulaID);
                if (conditionResult)
                {
                    state.m_control.Execute(index, true);
                }
            }
        }
        if (heroCtrl.m_curStateDef != null)
        {
            foreach (HeroState state in heroCtrl.m_curStateDef.m_listState)
            {
                if (state.m_executeOnce&& state.m_hasExecute) continue;
                if (!state.m_sameCondition)
                {
                    ScriptManager.m_proxy[index].Fields["triggerTime"] = state.m_hasExecute ? (Time.time - state.m_executeTime) : -1;
                    conditionResult = ScriptManager.ExecuteBool(index, state.m_tmpFormulaID);
                }
                if (conditionResult)
                {
                    if(state.m_control.Execute(index, false)) state.m_hasExecute = true;
                    if (!state.m_hasExecute) state.m_executeTime = Time.time;
                }
            }
        }
        //每帧判断完后清空碰撞信息
        heroCtrl.m_listMyCollider.Clear();
        heroCtrl.m_listOtherCollider.Clear();
    }
    void RefreshUI()
    {
        for (int i = 0; i < 2; i++)
        {
            HeroCtrl heroCtrl = m_heroCtrl[i];
            if (heroCtrl.m_life > heroCtrl.m_hero.m_baseInfo.m_lifeMax) heroCtrl.m_life = heroCtrl.m_hero.m_baseInfo.m_lifeMax;
            if (heroCtrl.m_power > heroCtrl.m_hero.m_baseInfo.m_powerMax) heroCtrl.m_power = heroCtrl.m_hero.m_baseInfo.m_powerMax;
            m_sliderHP[i].value = heroCtrl.m_life / heroCtrl.m_hero.m_baseInfo.m_lifeMax;
            m_sliderMP[i].value = heroCtrl.m_power / heroCtrl.m_hero.m_baseInfo.m_powerMax;
        }
        m_textAI.color = m_isAI[1] ? Color.green : Color.black;
    }
    public void ButtonAIPress()
    {
        m_isAI[1] = !m_isAI[1];
    }
    //UI_Base接口的实现
    public bool IsRootUI()
    {
        return true;
    }
    public void SetParent(GameObject parentUI)
    {
        
    }
    public GameObject GetParent()
    {
        return null;
    }
    public int GetSortOrder()//返回Canvas的sort order
    {
        return 10;
    }
    public void SetSortOrder(int order)//设置order
    {
        
    }
}
