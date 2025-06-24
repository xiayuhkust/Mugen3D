using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//目标控制器
public class HSC_Target : HeroStateControl
{
    public int m_type = 0;//0-绑定，1-解绑
    public float m_duration = 0;//绑定时长
    public Vector3 m_pos;//相对于自身的坐标

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Target;
    }
    public override bool Execute(int index, bool byOperation)
    {
        HeroCtrl heroCtrl = UI_GameUI.m_heroCtrl[index];
        HeroCtrl otherHeroCtrl = UI_GameUI.m_heroCtrl[1 - index];
        if (m_type == 0)
        {
            if (heroCtrl.m_boundLeftTime > 0)
            {
                GlobalAssist.ShowCenterTips("错误：被对方绑定中，无法绑定对方", 50);
                return false;
            }
            otherHeroCtrl.StartBound(heroCtrl.m_transform, m_duration, m_pos);
        }
        else
        {
            otherHeroCtrl.m_boundLeftTime = 0;
        }
        return true;
    }
    public override string GetDisplayString()
    {
        string str;
        if (m_type == 0)
        {
            str = "目标绑定" + m_duration + "s [" + m_pos.x + ", " + m_pos.y + ", " + m_pos.z + "]";
        }
        else
        {
            str = "目标解绑";
        }
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_type = br.ReadInt32();
        m_duration = br.ReadSingle();
        m_pos[0] = br.ReadSingle();
        m_pos[1] = br.ReadSingle();
        m_pos[2] = br.ReadSingle();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_type);
        bw.Write(m_duration);
        bw.Write(m_pos[0]);
        bw.Write(m_pos[1]);
        bw.Write(m_pos[2]);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Target hsc = (HSC_Target)control;
        m_type = hsc.m_type;
        m_duration = hsc.m_duration;
        m_pos[0] = hsc.m_pos[0];
        m_pos[1] = hsc.m_pos[1];
        m_pos[2] = hsc.m_pos[2];
    }
}
