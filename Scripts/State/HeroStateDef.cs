using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class HeroAnimation//状态动画信息
{
    public string m_name = "";//动画名称
    public float m_fadeTime = 0.1f;//播放当前动画的过度时间
    public float m_offTime = 0;//播放当前动画的开始时间
    public float m_endTime = 0.75f;//当前动画的结束时间
    public void Copy(HeroAnimation ha)
    {
        m_name = ha.m_name;
        m_fadeTime = ha.m_fadeTime;
        m_offTime = ha.m_offTime;
        m_endTime = ha.m_endTime;
    }
}
public enum StateDefType
{
    Unchanged = 0,//保持上一个状态的类型
    Stand,//站
    Crouch,//蹲
    Air,//空中
    Lay,//躺下
}
public enum StateDefMoveType
{
    Unchanged = 0,//保持上一个状态的招式类型
    Attack,//攻击
    Idle,//空闲
    Hit,//受击
}
public enum StateDefPhysicType
{
    Unchanged = 0,//不变
    None,//无
    Stand,//站立
    Crouch,//蹲下
    Air,//空中
}
public enum StateDefControlType
{
    Unchanged = 0,//不变
    UnControl,//不受控制
    Control,//受控制
}
public class HeroStateDef//状态类
{
    public int m_stateNo = 0;//状态号
    public TextInfo m_name = new TextInfo();
    public StateDefType m_type = StateDefType.Stand;
    public StateDefMoveType m_moveType = StateDefMoveType.Attack;
    public StateDefPhysicType m_physicType = StateDefPhysicType.None;
    public bool m_isVelset = false;//是否设置状态起始时的速度
    public float[] m_velset = new float[2] { 0, 0 };
    public StateDefControlType m_controlType = StateDefControlType.Control;//是否受控制，0不变，1-不受控制，2-受控制
    public int m_powerAdd = 0;//能量条变化
    public int m_juggle = 0;//连击点数，攻击有用
    public bool m_facep2 = false;//是否在状态开始时转向对手
    public bool m_hitdefpersist = false;//是否保留上一个状态的HitDef的激活状态
    public bool m_movehitpersist = false;//是否保留上一个状态的招式击中信息
    public bool m_hitcountpersist = false;//是否保留上一个状态的连击数
    public List<HeroState> m_listState = new List<HeroState>();//状态控制器列表

    public List<HeroAnimation> m_listAnimation = new List<HeroAnimation>();//动画列表
    public bool m_animRecycle = false;//是否循环动画
    //运行时变量
    public float m_gravity = 0;//当前状态的重力加速度，切换状态时指定

    public void Copy(HeroStateDef hsd)
    {
        m_name.Copy(hsd.m_name);
        m_type = hsd.m_type;
        m_moveType = hsd.m_moveType;
        m_physicType = hsd.m_physicType;
        m_isVelset = hsd.m_isVelset;
        for (int i = 0; i < 2; i++) m_velset[i] = hsd.m_velset[i];
        m_controlType = hsd.m_controlType;
        m_powerAdd = hsd.m_powerAdd;
        m_juggle = hsd.m_juggle;
        m_facep2 = hsd.m_facep2;
        m_hitdefpersist = hsd.m_hitdefpersist;
        m_movehitpersist = hsd.m_movehitpersist;
        m_hitcountpersist = hsd.m_hitcountpersist;
        for (int i = 0; i < hsd.m_listState.Count; i++)
        {
            HeroState state = new HeroState();
            state.Copy(hsd.m_listState[i]);
            m_listState.Add(state);
        }
        for (int i = 0; i < hsd.m_listAnimation.Count; i++)
        {
            HeroAnimation ha = new HeroAnimation();
            ha.Copy(hsd.m_listAnimation[i]);
            m_listAnimation.Add(ha);
        }
        m_animRecycle = hsd.m_animRecycle;
    }
    public static List<string> GetListStateTypeStr()
    {
        List<string> listStr = new List<string>();
        listStr.Add("不变");
        listStr.Add("站");
        listStr.Add("蹲");
        listStr.Add("空中");
        listStr.Add("躺下");
        return listStr;
    }
    public static List<string> GetListMoveTypeStr()
    {
        List<string> listStr = new List<string>();
        listStr.Add("不变");
        listStr.Add("攻击");
        listStr.Add("空闲");
        listStr.Add("受击");
        return listStr;
    }
    public static List<string> GetListPhysicTypeStr()
    {
        List<string> listStr = new List<string>();
        listStr.Add("不变");
        listStr.Add("无");
        listStr.Add("站立");
        listStr.Add("蹲下");
        listStr.Add("空中");
        return listStr;
    }
    public static List<string> GetListControlTypeStr()
    {
        List<string> listStr = new List<string>();
        listStr.Add("不变");
        listStr.Add("不受控制");
        listStr.Add("受控制");
        return listStr;
    }

    public void Read(BinaryReader br, int versionNo)
    {
        m_stateNo = br.ReadInt32();
        m_name.Read(br, versionNo);
        m_type = (StateDefType)br.ReadInt32();
        m_moveType = (StateDefMoveType)br.ReadInt32();
        m_physicType = (StateDefPhysicType)br.ReadInt32();
        m_isVelset = br.ReadBoolean();
        m_velset[0] = br.ReadSingle();
        m_velset[1] = br.ReadSingle();
        m_controlType = (StateDefControlType)br.ReadInt32();
        m_powerAdd = br.ReadInt32();
        m_juggle = br.ReadInt32();
        m_facep2 = br.ReadBoolean();
        m_hitdefpersist = br.ReadBoolean();
        m_movehitpersist = br.ReadBoolean();
        m_hitcountpersist = br.ReadBoolean();
        int stateCount = br.ReadInt32();
        m_listState.Clear();
        for (int i = 0; i < stateCount; i++)
        {
            HeroState state = new HeroState();
            state.Read(br, versionNo);
            m_listState.Add(state);
        }
        float tmpVal = br.ReadSingle();
        int animCount = br.ReadInt32();
        m_listAnimation.Clear();
        for (int i = 0; i < animCount; i++)
        {
            HeroAnimation ha = new HeroAnimation();
            ha.m_name = br.ReadString();
            ha.m_fadeTime = br.ReadSingle();
            ha.m_offTime = br.ReadSingle();
            ha.m_endTime = br.ReadSingle();
            m_listAnimation.Add(ha);
        }
        m_animRecycle = br.ReadBoolean();
    }
    public void Save(BinaryWriter bw)
    {
        bw.Write(m_stateNo);
        m_name.Save(bw);
        bw.Write((int)m_type);
        bw.Write((int)m_moveType);
        bw.Write((int)m_physicType);
        bw.Write(m_isVelset);
        bw.Write(m_velset[0]);
        bw.Write(m_velset[1]);
        bw.Write((int)m_controlType);
        bw.Write(m_powerAdd);
        bw.Write(m_juggle);
        bw.Write(m_facep2);
        bw.Write(m_hitdefpersist);
        bw.Write(m_movehitpersist);
        bw.Write(m_hitcountpersist);
        bw.Write(m_listState.Count);
        foreach (HeroState state in m_listState) state.Save(bw);
        bw.Write(0f);
        bw.Write(m_listAnimation.Count);
        foreach (HeroAnimation ha in m_listAnimation)
        {
            bw.Write(ha.m_name);
            bw.Write(ha.m_fadeTime);
            bw.Write(ha.m_offTime);
            bw.Write(ha.m_endTime);
        }
        bw.Write(m_animRecycle);
    }
}
