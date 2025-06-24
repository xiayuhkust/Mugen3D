using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

//描述类
public class TextInfo
{
    public string m_str = "";//简体中文
    public string m_strTc = "";//繁体
    public string m_strEn = "";//英文
    public string m_strJp = "";//日文

    public string GetStr()
    {
        switch (GlobalAssist.m_language)
        {
            case Language.Chinese:
                return m_str;
            case Language.ChineseTc:
                return m_strTc;
            case Language.English:
                return m_strEn;
            case Language.Japanese:
                return m_strJp;
        }
        return m_str;
    }
    public void SetStr(string str)
    {
        switch (GlobalAssist.m_language)
        {
            case Language.Chinese: m_str = str; break;
            case Language.ChineseTc: m_strTc = str; break;
            case Language.English: m_strEn = str; break;
            case Language.Japanese: m_strJp = str; break;
        }
    }
    public void Copy(TextInfo srcInfo)
    {
        m_str = srcInfo.m_str;
        m_strTc = srcInfo.m_strTc;
        m_strEn = srcInfo.m_strEn;
        m_strJp = srcInfo.m_strJp;
    }
    public void Save(BinaryWriter bw)
    {
        bw.Write(m_str);
        bw.Write(m_strTc);
        bw.Write(m_strEn);
        bw.Write(m_strJp);
    }
    public void Read(BinaryReader br, int versionNo)
    {
        m_str = br.ReadString();
        m_strTc = br.ReadString();
        m_strEn = br.ReadString();
        m_strJp = br.ReadString();
    }
}
