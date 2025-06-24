using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
//援助人物控制器
public class HSC_Helper : HeroStateControl
{

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.Helper;
    }
    public override bool Execute(int index, bool byOperation)
    {
        return true;
    }
    public override string GetDisplayString()
    {
        return "援助人物";
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
