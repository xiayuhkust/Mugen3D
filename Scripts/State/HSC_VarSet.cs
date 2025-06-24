using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//设置变量控制器
public class HSC_VarSet : HeroStateControl
{
    public int m_type = 0;//0-设置，1-变化
    public int m_valueType = 0;//类型0-整数，1-浮点数
    public int m_index = 0;//变量下标
    public int m_valInt = 0;//当m_valueType=0时有用
    public float m_valFloat = 0;//当m_valueType=1时有用

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.VarSet;
    }
    public override bool Execute(int index, bool byOperation)
    {
        HeroCtrl heroCtrl = UI_GameUI.m_heroCtrl[index];
        if (m_type == 0)//设置
        {
            if (m_valueType == 0) heroCtrl.m_valueInt[m_index] = m_valInt;
            else heroCtrl.m_valueFloat[m_index] = m_valFloat;
        }
        else//变化
        {
            if (m_valueType == 0) heroCtrl.m_valueInt[m_index] += m_valInt;
            else heroCtrl.m_valueFloat[m_index] += m_valFloat;
        }
        return true;
    }
    public override string GetDisplayString()
    {
        string str;
        if (m_valueType == 0)//整数
        {
            if (m_type == 0) str = "整数[" + m_index + "] = " + m_valInt;
            else str = "整数[" + m_index + "] += " + m_valInt;
        }
        else//浮点数
        {
            if (m_type == 0) str = "浮点数[" + m_index + "] = " + m_valFloat;
            else str = "浮点数[" + m_index + "] += " + m_valFloat;
        }
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_type = br.ReadInt32();
        m_valueType = br.ReadInt32();
        m_index = br.ReadInt32();
        m_valInt = br.ReadInt32();
        m_valFloat = br.ReadSingle();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_type);
        bw.Write(m_valueType);
        bw.Write(m_index);
        bw.Write(m_valInt);
        bw.Write(m_valFloat);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_VarSet hsc = (HSC_VarSet)control;
        m_type = hsc.m_type;
        m_valueType = hsc.m_valueType;
        m_index = hsc.m_index;
        m_valInt = hsc.m_valInt;
        m_valFloat = hsc.m_valFloat;
    }
}
