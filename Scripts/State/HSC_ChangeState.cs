using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//切换状态控制器
public class HSC_ChangeState : HeroStateControl
{
    public int m_target = 0;//0-自己，1-对手
    public int m_changeStateNo = 0;//切换的目标状态号
    public int m_ctrl = 0;//下个状态能否控制，=0不变，=1不受控制，=2受控制

    private static List<string> m_tmpListStr = new List<string>();

    public static List<string> GetListControlType()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("-");
        m_tmpListStr.Add("不受控制");
        m_tmpListStr.Add("受控制");
        return m_tmpListStr;
    }
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.ChangeState;
    }
    public override bool Execute(int index, bool byOperation)
    {
        if (!byOperation)//非操控切换状态时，清空输入信息
        {
            UI_GameUI.m_listInputRecord[index].Clear();
            UI_GameUI.m_commandNo[index] = 0;
        }
        HeroCtrl heroCtrl = m_target == 0 ? UI_GameUI.m_heroCtrl[index] : UI_GameUI.m_heroCtrl[1 - index];
        if (heroCtrl.m_hero.m_dicStateDef.TryGetValue(m_changeStateNo, out HeroStateDef hsd))
        {
            heroCtrl.SetStateDef(hsd);
        }
        else GlobalAssist.ShowCenterTips("错误：角色<" + heroCtrl.m_hero.m_name.GetStr() + ">不存在状态号<" + m_changeStateNo + ">", 50);
        return true;
    }
    public override string GetDisplayString()
    {
        string str = "切换状态";
        str += m_target == 0 ? "<自己>" : "<对手>";
        str += "<" + m_changeStateNo + ">";
        if (m_ctrl == 1) str += "<不受控制>";
        else if (m_ctrl == 2) str += "<受控制>";
        return str;
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        m_target = br.ReadInt32();
        m_changeStateNo = br.ReadInt32();
        m_ctrl = br.ReadInt32();
    }
    public override void Save(BinaryWriter bw)
    {
        bw.Write(m_target);
        bw.Write(m_changeStateNo);
        bw.Write(m_ctrl);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_ChangeState hsc = (HSC_ChangeState)control;
        m_target = hsc.m_target;
        m_changeStateNo = hsc.m_changeStateNo;
        m_ctrl = hsc.m_ctrl;
    }
}
