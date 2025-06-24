using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//音效控制器
public class HSC_Sound : HeroStateControl
{
    public string m_soundID = "";
    public float m_volume = 1;
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Sound;
    }
    public override bool Execute(int index, bool byOperation)
    {
        SoundManager.PlaySound(m_soundID, m_volume);
        return true;
    }
    public override string GetDisplayString()
    {
        return "音效<" + m_soundID + "><" + m_volume + ">";
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_soundID = br.ReadString();
        m_volume = br.ReadSingle();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_soundID);
        bw.Write(m_volume);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Sound hsc = (HSC_Sound)control;
        m_soundID = hsc.m_soundID;
        m_volume = hsc.m_volume;
    }
}
