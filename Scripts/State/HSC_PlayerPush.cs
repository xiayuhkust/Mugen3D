using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//碰撞检测关闭控制器
public class HSC_PlayerPush : HeroStateControl
{
    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.PlayerPush;
    }
    public override bool Execute(int index, bool byOperation)
    {
        HeroCtrl heroCtrl = UI_GameUI.m_heroCtrl[index];
        heroCtrl.m_ignoreCollider = true;
        return true;
    }
    public override string GetDisplayString()
    {
        return "碰撞检测关闭";
    }
    public override void Read(BinaryReader br, int versionNo)
    {

    }
    public override void Save(BinaryWriter bw)
    {

    }
    public override void Copy(HeroStateControl control)
    {

    }
}
