using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//断言控制器
public class HSC_AssertSpecial : HeroStateControl
{
    public int m_assert = 0;//断言，跟下面的GetListAssert下标对应
    public static List<string> m_tmpListStr = new List<string>();
    public static List<string> GetListAssertStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("0-表演开场姿势");
        m_tmpListStr.Add("1-表演胜利姿势");
        m_tmpListStr.Add("2-隐藏角色");
        m_tmpListStr.Add("3-隐藏UI");
        m_tmpListStr.Add("4-背景变黑");
        m_tmpListStr.Add("5-不能站立防御");
        m_tmpListStr.Add("6-不能蹲下防御");
        m_tmpListStr.Add("7-不能空中防御");
        m_tmpListStr.Add("8-无视juggle限制");
        m_tmpListStr.Add("9-暂停背景音乐");
        m_tmpListStr.Add("10-不能进入行走状态");
        m_tmpListStr.Add("11-暂停倒计时");
        m_tmpListStr.Add("12-不可防御");

        return m_tmpListStr;
    }
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Assert;
    }
    public override bool Execute(int index, bool byOperation)
    {
        return true;
    }
    public override string GetDisplayString()
    {
        return GetListAssertStr()[m_assert];
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_assert = br.ReadInt32();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_assert);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_AssertSpecial hsc = (HSC_AssertSpecial)control;
        m_assert = hsc.m_assert;
    }
}
