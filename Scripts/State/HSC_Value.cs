using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//数值设置和变化控制器
public class HSC_Value : HeroStateControl
{
    public int m_target = 0;//目标，0-自己，1-对手
    public int m_lifeType = 0;//生命值类型0-不变，1-设置，2-变化
    public float m_life = 0;//生命值
    public int m_powerType = 0;//气类型0-不变，1-设置，2-变化
    public float m_power = 0;//气
    public int m_gravityType = 0;//重力类型0-不变，1-设置，2-变化
    public float m_gravity = 0;//重力
    public int m_ctrl = 0;//控制切换，0-不变，1-可控制，2-不可控制
    public int m_defenceMulType = 0;//防御系数类型0-不变，1-设置，2-变化
    public float m_defenceMulSet = 1;//角色防御系数，最终伤害为伤害x该系数
    public int m_attackMulType = 0;//攻击系数类型0-不变，1-设置，2-变化
    public float m_attackMulSet = 1;//角色攻击系数，最终造成伤害为伤害x该系数
    public int m_animSpeedType = 0;//动画播放速度，0-不变，1-设置，2-变化
    public float m_animSpeed = 1;//动画播放速度

    public static List<string> m_tmpListStr = new List<string>();
    public static List<string> GetListCtrlTypeStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("不变");
        m_tmpListStr.Add("可控制");
        m_tmpListStr.Add("不可控制");
        return m_tmpListStr;
    }
    public static List<string> GetListValueTypeStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("不变");
        m_tmpListStr.Add("设置");
        m_tmpListStr.Add("变化");
        return m_tmpListStr;
    }

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Value;
    }
    public override bool Execute(int index, bool byOperation)
    {
        HeroCtrl heroCtrl = m_target == 0 ? UI_GameUI.m_heroCtrl[index] : UI_GameUI.m_heroCtrl[1 - index];
        if (m_lifeType == 1) heroCtrl.m_life = m_life;
        else if (m_lifeType == 2) heroCtrl.m_life += m_life;
        if (m_powerType == 1) heroCtrl.m_power = m_power;
        else if (m_powerType == 2) heroCtrl.m_power += m_power;
        if (m_gravityType == 1) heroCtrl.m_curStateDef.m_gravity = m_gravity;
        else if (m_gravityType == 2) heroCtrl.m_curStateDef.m_gravity += m_gravity;
        if (m_ctrl == 1) heroCtrl.m_canControl = true;
        else if (m_ctrl == 2) heroCtrl.m_canControl = false;
        if (m_defenceMulType == 1) heroCtrl.m_defenceMulSet = m_defenceMulSet;
        else if (m_defenceMulSet == 2) heroCtrl.m_defenceMulSet += m_defenceMulSet;
        if (m_attackMulType == 1) heroCtrl.m_attackMulSet = m_attackMulSet;
        else if (m_attackMulSet == 2) heroCtrl.m_attackMulSet += m_attackMulSet;
        if (heroCtrl.m_pauseLeftTime <= 0)
        {
            if (m_animSpeedType == 1) heroCtrl.m_animator.speed = m_animSpeed;
            else if (m_animSpeedType == 2) heroCtrl.m_animator.speed += m_animSpeed;
        }
        return true;
    }
    public override string GetDisplayString()
    {
        string str = m_target == 0 ? "<自己>" : "<对手>";
        if (m_lifeType > 0)
        {
            if (m_lifeType == 1) str += "<生命值=" + m_life + ">";
            else if (m_life > 0) str += "<生命值+" + m_life + ">";
            else str += "<生命值" + m_life + ">";
        }
        if (m_powerType > 0)
        {
            if (m_powerType == 1) str += "<气=" + m_power + ">";
            else if (m_power > 0) str += "<气+" + m_power + ">";
            else str += "<气" + m_power + ">";
        }
        if (m_gravityType > 0)
        {
            if (m_gravityType == 1) str += "<重力=" + m_gravity + ">";
            else if (m_gravity > 0) str += "<重力+" + m_gravity + ">";
            else str += "<重力" + m_gravity + ">";
        }
        if (m_defenceMulType > 0)
        {
            if (m_defenceMulType == 1) str += "<防御系数=" + m_defenceMulSet + ">";
            else if (m_defenceMulSet > 0) str += "<防御系数+" + m_defenceMulSet + ">";
            else str += "<防御系数" + m_defenceMulSet + ">";
        }
        if (m_attackMulType > 0)
        {
            if (m_attackMulType == 1) str += "<攻击系数=" + m_attackMulSet + ">";
            else if (m_attackMulSet > 0) str += "<攻击系数+" + m_attackMulSet + ">";
            else str += "<攻击系数" + m_attackMulSet + ">";
        }
        if (m_animSpeedType > 0)
        {
            if (m_animSpeedType == 1) str += "<动画速度=" + m_animSpeed + ">";
            else if (m_animSpeedType > 0) str += "<动画速度+" + m_animSpeed + ">";
            else str += "<动画速度" + m_animSpeed + ">";
        }
        if (m_ctrl == 1) str += "<可控制>";
        else if (m_ctrl == 2) str += "<不可控制>";
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_target = br.ReadInt32();
        m_lifeType = br.ReadInt32();
        m_life = br.ReadSingle();
        m_powerType = br.ReadInt32();
        m_power = br.ReadSingle();
        m_gravityType = br.ReadInt32();
        m_gravity = br.ReadSingle();
        m_ctrl = br.ReadInt32();
        m_defenceMulType = br.ReadInt32();
        m_defenceMulSet = br.ReadSingle();
        m_attackMulType = br.ReadInt32();
        m_attackMulSet = br.ReadSingle();
        if (versionNo >= 202203290)
        {
            m_animSpeedType = br.ReadInt32();
            m_animSpeed = br.ReadSingle();
        }
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_target);
        bw.Write(m_lifeType);
        bw.Write(m_life);
        bw.Write(m_powerType);
        bw.Write(m_power);
        bw.Write(m_gravityType);
        bw.Write(m_gravity);
        bw.Write(m_ctrl);
        bw.Write(m_defenceMulType);
        bw.Write(m_defenceMulSet);
        bw.Write(m_attackMulType);
        bw.Write(m_attackMulSet);
        bw.Write(m_animSpeedType);
        bw.Write(m_animSpeed);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Value hsc = (HSC_Value)control;
        m_target = hsc.m_target;
        m_lifeType = hsc.m_lifeType;
        m_life = hsc.m_life;
        m_powerType = hsc.m_powerType;
        m_power = hsc.m_power;
        m_gravityType = hsc.m_gravityType;
        m_gravity = hsc.m_gravity;
        m_ctrl = hsc.m_ctrl;
        m_defenceMulType = hsc.m_defenceMulType;
        m_defenceMulSet = hsc.m_defenceMulSet;
        m_attackMulType = hsc.m_attackMulType;
        m_attackMulSet = hsc.m_attackMulSet;
        m_animSpeedType = hsc.m_animSpeedType;
        m_animSpeed = hsc.m_animSpeed;
    }
}
