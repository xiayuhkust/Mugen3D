using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//屏幕震动控制器
public class HSC_ScreenShake : HeroStateControl
{
    public float m_duration = 0;//持续时间
    public int m_shakeType = 0;//震动幅度，0-小，1-中，2-大，3-手动输入
    public float m_shakeRange = 0;//手动输入的震动幅度

    public static List<string> m_tmpListStr = new List<string>();
    public static List<string> GetListTypeStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("小(1)");
        m_tmpListStr.Add("中(3)");
        m_tmpListStr.Add("大(6)");
        m_tmpListStr.Add("手动输入(0.1~10)");
        return m_tmpListStr;
    }
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.ScreenShake;
    }
    public override bool Execute(int index, bool byOperation)
    {
        CameraCtrl.StartShake(m_duration, m_shakeType, m_shakeRange);
        return true;
    }
    public override string GetDisplayString()
    {
        string str = "屏幕震动<" + m_duration + "s>";
        str += GetListTypeStr()[m_shakeType];
        if (m_shakeType == 3) str += "<" + m_shakeRange + ">";
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_duration = br.ReadSingle();
        m_shakeType = br.ReadInt32();
        m_shakeRange = br.ReadSingle();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_duration);
        bw.Write(m_shakeType);
        bw.Write(m_shakeRange);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_ScreenShake hsc = (HSC_ScreenShake)control;
        m_duration = hsc.m_duration;
        m_shakeType = hsc.m_shakeType;
        m_shakeRange = hsc.m_shakeRange;
    }
}
