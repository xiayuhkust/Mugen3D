using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class HSC_Effect : HeroStateControl
{
    public List<string> m_listEffectID = new List<string>();//特效ID列表，从中随机一个
    public float m_effectSpeed = 1;//特效速度
    public Vector3 m_effectPos = Vector3.zero;//特效坐标（相对于动画人物的相对坐标）
    public float[] m_randomPosX = new float[2] { 0, 0 };//坐标随机变化范围
    public float[] m_randomPosY = new float[2] { 0, 0 };//坐标随机变化范围
    public Vector3 m_effectRotation = Vector3.zero;//特效旋转，此处用EulerAngles
    public Vector3 m_effectScale = Vector3.one;//特效缩放
    public bool m_effectStatic = false;//特效不随人移动

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Effect;
    }
    public override bool Execute(int index, bool byOperation)
    {
        return true;
    }
    public override string GetDisplayString()
    {
        string str = "特效";
        foreach (string strID in m_listEffectID) str += "<" + strID + ">";
        str += "[x" + m_effectSpeed + "]";
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_listEffectID.Clear();
        int effectCount = br.ReadInt32();
        for (int i = 0; i < effectCount; i++) m_listEffectID.Add(br.ReadString());
        m_effectSpeed = br.ReadSingle();
        m_effectPos = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        m_randomPosX[0] = br.ReadSingle();
        m_randomPosX[1] = br.ReadSingle();
        m_randomPosY[0] = br.ReadSingle();
        m_randomPosY[1] = br.ReadSingle();
        m_effectRotation = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        m_effectScale = new Vector3(br.ReadSingle(), br.ReadSingle(), br.ReadSingle());
        m_effectStatic = br.ReadBoolean();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_listEffectID.Count);
        bw.Write(m_effectSpeed);
        bw.Write(m_effectPos[0]);
        bw.Write(m_effectPos[1]);
        bw.Write(m_effectPos[2]);
        bw.Write(m_randomPosX[0]);
        bw.Write(m_randomPosX[1]);
        bw.Write(m_randomPosY[0]);
        bw.Write(m_randomPosY[1]);
        bw.Write(m_effectRotation.x);
        bw.Write(m_effectRotation.y);
        bw.Write(m_effectRotation.z);
        bw.Write(m_effectScale.x);
        bw.Write(m_effectScale.y);
        bw.Write(m_effectScale.z);
        bw.Write(m_effectStatic);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Effect hsc = (HSC_Effect)control;
        m_listEffectID.Clear();
        m_listEffectID.AddRange(hsc.m_listEffectID);
        m_effectSpeed = hsc.m_effectSpeed;
        m_effectPos = hsc.m_effectPos;
        m_randomPosX[0] = hsc.m_randomPosX[0];
        m_randomPosX[1] = hsc.m_randomPosX[1];
        m_randomPosY[0] = hsc.m_randomPosY[0];
        m_randomPosY[1] = hsc.m_randomPosY[1];
        m_effectRotation = hsc.m_effectRotation;
        m_effectScale = hsc.m_effectScale;
        m_effectStatic = hsc.m_effectStatic;
    }
}
