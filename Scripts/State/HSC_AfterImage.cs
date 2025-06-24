using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//人物残影控制器
public class HSC_AfterImage : HeroStateControl
{
    public float m_duration = 0;//持续时间
    public int m_num = 1;//数量，持续时间除以数量得到频率
    public float m_stayDuration = 0;//残影的残留时间
    public Color m_color = Color.white;//残影颜色

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.AfterImage;
    }
    public override bool Execute(int index, bool byOperation)
    {
        if (m_duration < 0.1 || m_num == 0) return true;
        HeroCtrl heroCtrl = UI_GameUI.m_heroCtrl[index];
        heroCtrl.StartAfterImage(m_num, m_duration / m_num, m_stayDuration, m_color);
        return true;
    }
    public override string GetDisplayString()
    {
        return "残影" + m_duration + "s";
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_duration = br.ReadSingle();
        m_num = br.ReadInt32();
        m_stayDuration = br.ReadSingle();
        m_color = new Color(br.ReadSingle(), br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_duration);
        bw.Write(m_num);
        bw.Write(m_stayDuration);
        bw.Write(m_color.r);
        bw.Write(m_color.g);
        bw.Write(m_color.b);
        bw.Write(m_color.a);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_AfterImage hsc = (HSC_AfterImage)control;
        m_duration = hsc.m_duration;
        m_num = hsc.m_num;
        m_stayDuration = hsc.m_stayDuration;
        m_color = hsc.m_color;
    }
}
