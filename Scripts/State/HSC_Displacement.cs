using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//位移控制器
public class HSC_Displacement : HeroStateControl
{
    public int m_target = 0;//目标，0-自己，1-对手
    public int m_type = 0;//0-指定偏移量/秒，1-设置y坐标，2-设置速度，3-设置速度变化
    public int[] m_valueType = new int[3] { 0, 0, 0 };//数值类型，0-指定，1-...
    public Vector3 m_value = Vector3.zero;//x、y、z方向

    public static List<string> m_tmpListStr = new List<string>();
    public static List<string> GetListTypeStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("指定偏移量/秒");
        m_tmpListStr.Add("设置Y坐标");
        m_tmpListStr.Add("设置速度");
        m_tmpListStr.Add("设置速度变化");
        return m_tmpListStr;
    }
    public static List<string> GetListValueTypeStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("指定");
        m_tmpListStr.Add("向前走速度");
        m_tmpListStr.Add("向后走速度");
        m_tmpListStr.Add("向前跑速度");
        m_tmpListStr.Add("后小跳速度");
        m_tmpListStr.Add("站立垂直跳速度");
        m_tmpListStr.Add("站立后跳速度");
        m_tmpListStr.Add("站立前跳速度");
        m_tmpListStr.Add("跑步后跳速度");
        m_tmpListStr.Add("跑步前跳速度");
        m_tmpListStr.Add("空中垂直跳速度");
        m_tmpListStr.Add("空中后跳速度");
        m_tmpListStr.Add("空中前跳速度");
        return m_tmpListStr;
    }
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Displacement;
    }
    public override bool Execute(int index, bool byOperation)
    {
        HeroCtrl heroCtrl = UI_GameUI.m_heroCtrl[index];
        if (m_target == 1) heroCtrl = UI_GameUI.m_heroCtrl[1 - index];
        Vector3 value = m_value;
        for (int i = 0; i < 3; i++)//默认指定值，如果指定了其它数值类型，特殊处理
        {
            switch(m_valueType[i])
            {
                case 1: value[i] = heroCtrl.m_hero.m_baseInfo.m_walkFowardSpeed; break;
                case 2: value[i] = heroCtrl.m_hero.m_baseInfo.m_walkBackSpeed; break;
                case 3: value[i] = heroCtrl.m_hero.m_baseInfo.m_runForwardSpeed; break;
                case 4: value[i] = heroCtrl.m_hero.m_baseInfo.m_dodgeBackSpeed[i]; break;
                case 5: value[i] = heroCtrl.m_hero.m_baseInfo.m_standJumpSpeed[i]; break;
                case 6: value[i] = heroCtrl.m_hero.m_baseInfo.m_standJumpBackSpeed[i]; break;
                case 7: value[i] = heroCtrl.m_hero.m_baseInfo.m_standJumpForwardSpeed[i]; break;
                case 8: value[i] = heroCtrl.m_hero.m_baseInfo.m_runJumpBackSpeed[i]; break;
                case 9: value[i] = heroCtrl.m_hero.m_baseInfo.m_runJumpForwardSpeed[i]; break;
                case 10: value[i] = heroCtrl.m_hero.m_baseInfo.m_airJumpSpeed[i]; break;
                case 11: value[i] = heroCtrl.m_hero.m_baseInfo.m_airJumpBackSpeed[i]; break;
                case 12: value[i] = heroCtrl.m_hero.m_baseInfo.m_airJumpForwardSpeed[i]; break;
            }
        }
        if (m_type == 0)//指定偏移/秒
        {
            Vector3 newPos= heroCtrl.m_transform.position + (new Vector3(0, value.y, 0) + heroCtrl.m_transform.forward * value.x - heroCtrl.m_transform.right * value.z) * Time.deltaTime;
            if (newPos.y > 3) newPos.y = 3;
            heroCtrl.m_transform.position = newPos;
            Vector3 vec= UI_GameUI.m_heroCtrl[1].m_transform.position - UI_GameUI.m_heroCtrl[0].m_transform.position;
            vec.y = 0;
            UI_GameUI.m_heroCtrl[0].m_transform.forward = vec;
            UI_GameUI.m_heroCtrl[1].m_transform.forward = -vec;
        }
        else if (m_type == 1)//设置y坐标
        {
            float posY = value.y;
            if (posY > 3) posY = 3;
            heroCtrl.m_transform.position = new Vector3(heroCtrl.m_transform.position.x, posY, heroCtrl.m_transform.position.z);
        }
        else if (m_type == 2)//设置速度
        {
            heroCtrl.m_velocity = value;
        }
        else if (m_type == 3)//设置速度变化
        {
            heroCtrl.m_velocity += value;
        }
        return true;
    }
    public override string GetDisplayString()
    {
        string str = m_target == 0 ? "[自己]" : "[对手]";
        str += GetListTypeStr()[m_type] + "[";
        if (m_valueType[0] == 0) str += m_value[0] + ", ";
        else str += GetListValueTypeStr()[m_valueType[0]] + ", ";
        if (m_valueType[1] == 0) str += m_value[1] + ", ";
        else str += GetListValueTypeStr()[m_valueType[1]] + ", ";
        if (m_valueType[2] == 0) str += m_value[2] + "]";
        else str += GetListValueTypeStr()[m_valueType[2]] + "]";
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_type = br.ReadInt32();
        for (int i = 0; i < 3; i++) m_valueType[i] = br.ReadInt32();
        for (int i = 0; i < 3; i++) m_value[i] = br.ReadSingle();
        if (versionNo >= 202204150) m_target = br.ReadInt32();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_type);
        for (int i = 0; i < 3; i++) bw.Write(m_valueType[i]);
        for (int i = 0; i < 3; i++) bw.Write(m_value[i]);
        bw.Write(m_target);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Displacement hsc = (HSC_Displacement)control;
        m_target = hsc.m_target;
        m_type = hsc.m_type;
        for (int i = 0; i < 3; i++)
        {
            m_valueType[i] = hsc.m_valueType[i];
        }
        m_value = hsc.m_value;
    }
}
