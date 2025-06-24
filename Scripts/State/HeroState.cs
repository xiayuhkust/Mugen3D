using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum ControlType
{
    ChangeState = 0,//切换状态
    Assert,//断言
    AfterImage,//残影
    Displacement,//位移
    Effect,//特效
    Sound,//音效
    Target,//目标绑定/解绑
    HitDef,//攻击判定
    Helper,//援助任务
    Pause,//暂停
    PlayerPush,//碰撞检测开关
    ScreenShake,//屏幕震动
    Value,//参数修改
    VarSet,//变量设置
}

public class HeroStateControl//状态控制器类的通用接口，所有控制器都继承这个
{
    public virtual ControlType GetControlType() { return ControlType.ChangeState; }
    public virtual bool Execute(int index, bool byOperation) { return true; }
    public virtual string GetDisplayString() { return ""; }
    public virtual void Read(BinaryReader br, int versionNo) { }
    public virtual void Save(BinaryWriter bw) { }
    public virtual void Copy(HeroStateControl control) { }
}

public class HeroState
{
    public TextInfo m_remark = new TextInfo();//备注，用于列表显示预览
    public bool m_ignorehitpause = false;//是否在打击停顿时也检测控制器
    public bool m_sameCondition = false;//条件同前面
    public string m_condition = "";//条件
    public bool m_executeOnce = false;//只执行一遍
    public HeroStateControl m_control = new HSC_ChangeState();

    public string m_tmpFormulaID = "";//临时存储公式ID，开始战斗时初始化
    public bool m_hasExecute = false;//临时记录已经执行过的，跳过只执行一遍的
    public float m_executeTime = -1;//临时记录第一次执行的时间

    public void Copy(HeroState state)
    {
        m_remark.Copy(state.m_remark);
        m_ignorehitpause = state.m_ignorehitpause;
        m_sameCondition = state.m_sameCondition;
        m_condition = state.m_condition;
        m_executeOnce = state.m_executeOnce;
        switch (state.m_control.GetControlType())
        {
            case ControlType.ChangeState: m_control = new HSC_ChangeState(); break;
            case ControlType.Assert: m_control = new HSC_AssertSpecial(); break;
            case ControlType.AfterImage: m_control = new HSC_AfterImage(); break;
            case ControlType.Displacement: m_control = new HSC_Displacement(); break;
            case ControlType.Effect: m_control = new HSC_Effect(); break;
            case ControlType.Sound: m_control = new HSC_Sound(); break;
            case ControlType.Target: m_control = new HSC_Target(); break;
            case ControlType.HitDef: m_control = new HSC_Hitdef(); break;
            case ControlType.Helper: m_control = new HSC_Helper(); break;
            case ControlType.Pause: m_control = new HSC_Pause(); break;
            case ControlType.PlayerPush: m_control = new HSC_PlayerPush(); break;
            case ControlType.ScreenShake: m_control = new HSC_ScreenShake(); break;
            case ControlType.Value: m_control = new HSC_Value(); break;
            case ControlType.VarSet: m_control = new HSC_VarSet(); break;
        }
        m_control.Copy(state.m_control);
    }
    public string GetDisplayStr()
    {
        string str = m_remark.GetStr();
        if (str == "")
        {
            str = m_control.GetDisplayString();
        }
        if (m_sameCondition) str = "     " + str;
        return str;
    }
    public void Read(BinaryReader br, int versionNo)
    {
        m_remark.Read(br, versionNo);
        m_ignorehitpause = br.ReadBoolean();
        m_sameCondition = br.ReadBoolean();
        m_condition = br.ReadString();
        if (versionNo >= 202204130) m_executeOnce = br.ReadBoolean();
        ControlType type = (ControlType)br.ReadInt32();
        switch (type)
        {
            case ControlType.ChangeState: m_control = new HSC_ChangeState(); break;
            case ControlType.Assert: m_control = new HSC_AssertSpecial(); break;
            case ControlType.AfterImage: m_control = new HSC_AfterImage(); break;
            case ControlType.Displacement: m_control = new HSC_Displacement(); break;
            case ControlType.Effect: m_control = new HSC_Effect(); break;
            case ControlType.Sound: m_control = new HSC_Sound(); break;
            case ControlType.Target: m_control = new HSC_Target(); break;
            case ControlType.HitDef: m_control = new HSC_Hitdef(); break;
            case ControlType.Helper: m_control = new HSC_Helper(); break;
            case ControlType.Pause: m_control = new HSC_Pause(); break;
            case ControlType.PlayerPush: m_control = new HSC_PlayerPush(); break;
            case ControlType.ScreenShake: m_control = new HSC_ScreenShake(); break;
            case ControlType.Value: m_control = new HSC_Value(); break;
            case ControlType.VarSet: m_control = new HSC_VarSet(); break;
        }
        m_control.Read(br, versionNo);
    }
    public void Save(BinaryWriter bw)
    {
        m_remark.Save(bw);
        bw.Write(m_ignorehitpause);
        bw.Write(m_sameCondition);
        bw.Write(m_condition);
        bw.Write(m_executeOnce);
        bw.Write((int)m_control.GetControlType());
        m_control.Save(bw);
    }
}
