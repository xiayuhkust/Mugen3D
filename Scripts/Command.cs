using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InputData
{
    public List<string> m_listKey = new List<string>();
    public int m_type = 0;//特殊类型，0-无，1-按住不放，2-按住多少秒后松开
    public float m_pressTime = 0;//当m_type=2时有用
    public bool m_allowOtherArrow = false;//允许和其它方向键一起按下，只有含方向键时才有用

    public static Dictionary<int, string>[] m_dicKeyToInput = new Dictionary<int, string>[2] { new Dictionary<int, string>(), new Dictionary<int, string>() };//玩家的key和输入的对应关系，可通过UI设置

    public void Copy(InputData data)
    {
        m_listKey.Clear();
        m_listKey.AddRange(data.m_listKey);
        m_type = data.m_type;
        m_pressTime = data.m_pressTime;
        m_allowOtherArrow = data.m_allowOtherArrow;
    }
    public bool MeetInput(InputRecord record, Dictionary<string, float> dicPressingKey)
    {
        if (m_allowOtherArrow)//方向键允许和其它方向键同时按
        {
            if (m_listKey.Count > record.m_listKey.Count) return false;
            foreach (string key in record.m_listKey)//不能有方向键以外的按键
            {
                if (key != "→" && key != "←" && key != "↑" && key != "↓") return false;
            }
            foreach (string key in m_listKey)
            {
                if (!record.m_listKey.Contains(key)) return false;
                if (m_type == 1 && !dicPressingKey.ContainsKey(key)) return false;
            }
        }
        else
        {
            if (m_listKey.Count != record.m_listKey.Count) return false;
            foreach (string key in record.m_listKey)
            {
                if (!m_listKey.Contains(key)) return false;
                if (m_type == 1 && !dicPressingKey.ContainsKey(key)) return false;
            }
        }
        if (m_type == 2)
        {
            if (record.m_upTime < 0 || record.m_upTime - record.m_downTime < m_pressTime) return false;
        }
        return true;
    }
    public string GetDisplay()
    {
        string str = "";
        foreach (string key in m_listKey)
        {
            switch(key)
            {
                case "J": str += "跳"; break;
                case "a": str += "轻拳"; break;
                case "b": str += "中拳"; break;
                case "c": str += "重拳"; break;
                case "x": str += "轻脚"; break;
                case "y": str += "中脚"; break;
                case "z": str += "重脚"; break;
                case "s": str += "开始"; break;
                default: str += key; break;
            }
        }
        if (m_allowOtherArrow) str = "$" + str;
        if (m_type == 1) str = "/" + str;
        else if (m_type == 2) str = "(~" + m_pressTime + ")" + str;
        return str;
    }
    public static void InitKeyToInput()
    {
        m_dicKeyToInput[0].Clear();
        m_dicKeyToInput[0].Add((int)KeyCode.D, "→");
        m_dicKeyToInput[0].Add((int)KeyCode.A, "←");
        m_dicKeyToInput[0].Add((int)KeyCode.W, "↑");
        m_dicKeyToInput[0].Add((int)KeyCode.S, "↓");
        m_dicKeyToInput[0].Add((int)KeyCode.H, "J");//跳
        m_dicKeyToInput[0].Add((int)KeyCode.J, "a");//轻拳
        m_dicKeyToInput[0].Add((int)KeyCode.K, "b");//中拳
        m_dicKeyToInput[0].Add((int)KeyCode.L, "c");//重拳
        m_dicKeyToInput[0].Add((int)KeyCode.U, "x");//轻脚
        m_dicKeyToInput[0].Add((int)KeyCode.I, "y");//中脚
        m_dicKeyToInput[0].Add((int)KeyCode.O, "z");//重脚
        m_dicKeyToInput[0].Add((int)KeyCode.P, "s");//开始

        m_dicKeyToInput[1].Clear();
        m_dicKeyToInput[1].Add((int)KeyCode.RightArrow, "→");
        m_dicKeyToInput[1].Add((int)KeyCode.LeftArrow, "←");
        m_dicKeyToInput[1].Add((int)KeyCode.UpArrow, "↑");
        m_dicKeyToInput[1].Add((int)KeyCode.DownArrow, "↓");
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad0, "J");//跳
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad1, "a");//轻拳
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad2, "b");//中拳
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad3, "c");//重拳
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad4, "x");//轻脚
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad5, "y");//中脚
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad6, "z");//重脚
        m_dicKeyToInput[1].Add((int)KeyCode.Keypad9, "s");//开始
    }
    public void Read(BinaryReader br, int versionNo)
    {
        m_listKey.Clear();
        int keyCount = br.ReadInt32();
        for (int i = 0; i < keyCount; i++) m_listKey.Add(br.ReadString());
        m_type = br.ReadInt32();
        m_pressTime = br.ReadSingle();
        m_allowOtherArrow = br.ReadBoolean();
    }
    public void Save(BinaryWriter bw)
    {
        bw.Write(m_listKey.Count);
        for (int i = 0; i < m_listKey.Count; i++) bw.Write(m_listKey[i]);
        bw.Write(m_type);
        bw.Write(m_pressTime);
        bw.Write(m_allowOtherArrow);
    }
}

public class Command
{
    public int m_commandNo = 0;//命令编号
    public TextInfo m_name = new TextInfo();
    public float m_time = 1;//在指定时间内完成才有效
    public float m_preTime = 0;//可提前输入的时间，用于不受控制时的输入
    public List<InputData> m_listInput = new List<InputData>();//输入列表

    public static List<InputRecord> m_tmpListRecord = new List<InputRecord>();
    
    public bool MeetCommand(List<InputRecord> listInputRecord, Dictionary<string, float> dicPressingKey)
    {
        if (listInputRecord.Count < m_listInput.Count) return false;//输入长度不够直接返回false
        m_tmpListRecord.Clear();//用这个列表，把间隔小于0.1s的松开排除
        for (int i = listInputRecord.Count - 1; i >= 0; i--)
        {
            InputRecord ir = listInputRecord[i];
            if (ir.IsQuickUp() && i < listInputRecord.Count - 1 && m_tmpListRecord.Count < m_listInput.Count - 1) continue;//除了第一个和最后一个抬起，中间的抬起跳过
            m_tmpListRecord.Add(ir);
            if (m_tmpListRecord.Count == m_listInput.Count) break;
        }
        if (m_tmpListRecord.Count < m_listInput.Count) return false;//输入长度不够直接返回false
        float inputLength = m_tmpListRecord[m_tmpListRecord.Count - 1].GetJudgeTime() - m_tmpListRecord[m_tmpListRecord.Count - m_listInput.Count].GetJudgeTime();
        if (inputLength > m_time) return false;//输入时间超过指定时长，返回false
        for (int i = 0; i < m_listInput.Count; i++)
        {
            InputData data = m_listInput[m_listInput.Count - 1 - i];
            InputRecord record = m_tmpListRecord[i];
            if (!data.MeetInput(record, dicPressingKey)) return false;
        }
        return true;
    }
    public string GetDisplayInput()
    {
        string str = "";
        for (int i = 0; i < m_listInput.Count; i++)
        {
            str += m_listInput[i].GetDisplay();
            if (i < m_listInput.Count - 1) str += ", ";
        }
        return str;
    }
    public void Read(BinaryReader br, int versionNo)
    {
        m_commandNo = br.ReadInt32();
        m_name.Read(br, versionNo);
        m_time = br.ReadSingle();
        m_preTime = br.ReadSingle();
        m_listInput.Clear();
        int inputCount = br.ReadInt32();
        for (int i = 0; i < inputCount; i++)
        {
            InputData data = new InputData();
            data.Read(br, versionNo);
            m_listInput.Add(data);
        }
    }
    public void Save(BinaryWriter bw)
    {
        bw.Write(m_commandNo);
        m_name.Save(bw);
        bw.Write(m_time);
        bw.Write(m_preTime);
        bw.Write(m_listInput.Count);
        for (int i = 0; i < m_listInput.Count; i++) m_listInput[i].Save(bw);
    }
}
