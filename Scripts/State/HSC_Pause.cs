using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//游戏暂停控制器
public class HSC_Pause : HeroStateControl
{
    public float m_duration = 0;//暂停时长
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Pause;
    }
    public override bool Execute(int index, bool byOperation)
    {
        UI_GameUI.m_pauseLeftTime = m_duration;
        Time.timeScale = 0;
        return true;
    }
    public override string GetDisplayString()
    {
        return "暂停" + m_duration + "s";
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_duration = br.ReadSingle();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_duration);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Pause hsc = (HSC_Pause)control;
        m_duration = hsc.m_duration;
    }
}
