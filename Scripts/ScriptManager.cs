using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RoslynCSharp;
using System;

public static class ScriptManager
{
    public static ScriptDomain m_domain;
    public static ScriptProxy[] m_proxy = new ScriptProxy[2];//P1和P2
    public static Dictionary<string, string> m_dicFormulaBool = new Dictionary<string, string>();//存储返回值为bool的公式
    public static void Init()
    {
        try
        {
            m_domain = ScriptDomain.CreateDomain("ScriptManager");
            ScriptType sType = m_domain.CompileAndLoadMainSource(GenSource(false));
            if (m_domain.OutputError(out string strError)) GlobalAssist.ShowCenterTips(strError, 10);
            if (sType != null)
            {
                m_proxy[0] = sType.CreateInstance();
                m_proxy[1] = sType.CreateInstance();
            }
            else
            {
                m_proxy[0] = m_proxy[1] = null;
            }
        }
        catch(Exception exc)
        {
            GlobalAssist.ShowCenterTips("脚本初始化出错: " + exc.ToString(), 50);
        }
    }
    public static string GenSource(bool isCheck, string checkFormula = "")
    {
        string str = "class RuntimeScript{";
        str += "public int teamSide = 0;";
        str += "public float posY = 0;";
        str += "public float p2distX = 0;";
        str += "public float p2distY = 0;";
        str += "public float velX = 0;";
        str += "public float velY = 0;";
        str += "public float velZ = 0;";
        str += "public float p2velX = 0;";
        str += "public float p2velY = 0;";
        str += "public float p2velZ = 0;";
        str += "public int name = 0;";
        str += "public int p2name = 0;";
        str += "public float life = 0;";
        str += "public float p2life = 0;";
        str += "public float lifeMax = 0;";
        str += "public float p2lifeMax = 0;";
        str += "public int moveType = 2;";
        str += "public int p2moveType = 2;";
        str += "public int state = 0;";
        str += "public int p2state = 0;";
        str += "public int prevState = 0;";
        str += "public int p2prevState = 0;";
        str += "public int stateType = 1;";
        str += "public int p2stateType = 1;";
        str += "public float power = 0;";
        str += "public float p2power = 0;";
        str += "public float powerMax = 0;";
        str += "public float p2powerMax = 0;";
        str += "public bool ctrl = true;";
        str += "public bool p2ctrl = true;";
        str += "public int cmd = 0;";
        str += "public float time = 0;";
        str += "public int anim = 0;";
        str += "public float per = 0;";
        str += "public bool ai = false;";
        str += "public float[] varFloat = new float[50];";
        str += "public int[] varInt = new int[50];";
        str += "public int Random(int val1, int val2){return UnityEngine.Random.Range(val1,val2+1); }";
        str += "public bool inGuardDist = false;";
        str += "public int hit = 0;";
        str += "public int guard = 0;";
        str += "public float triggerTime = -1;";
        str += "public int type = 0;";
        str += "public int animtype = 0;";
        str += "public int airtype = 0;";
        str += "public int damage = 0;";
        str += "public float hitshaketime = 0;";
        str += "public float hittime = 0;";
        str += "public float slidetime = 0;";
        str += "public float xvel = 0;";
        str += "public float yvel = 0;";
        str += "public float zvel = 0;";
        str += "public float yaccel = 0;";
        str += "public int p2type = 0;";
        str += "public int p2animtype = 0;";
        str += "public int p2airtype = 0;";
        str += "public int p2damage = 0;";
        str += "public float p2hitshaketime = 0;";
        str += "public float p2hittime = 0;";
        str += "public float p2slidetime = 0;";
        str += "public float p2xvel = 0;";
        str += "public float p2yvel = 0;";
        str += "public float p2zvel = 0;";
        str += "public float p2yaccel = 0;";
        str += "public int GetHitVar(int val){return val; }";
        str += "public float GetHitVar(float val){return val; }";
        if (isCheck)
        {
            if (checkFormula == "") str += "public bool Test(){ return true;}";
            else if (checkFormula.Contains("return")) str += "public bool Test(){ " + checkFormula + ";}";
            else str += "public bool Test(){ return " + checkFormula + ";}";
        }
        else
        {
            foreach (string strKey in m_dicFormulaBool.Keys)
            {
                if (m_dicFormulaBool[strKey] == "") str += "public bool " + strKey + "(){ return true;}";
                else if (m_dicFormulaBool[strKey].Contains("return")) str += "public bool " + strKey + "(){ " + m_dicFormulaBool[strKey] + ";}";
                else str += "public bool " + strKey + "(){ return " + m_dicFormulaBool[strKey] + ";}";
            }
        }
        str += "}";
        return str;
    }
    public static bool ExecuteBool(int index, string str)
    {
        if (m_proxy[index] == null) return false;
        object obj = m_proxy[index].SafeCall(str);
        if (obj != null) return (bool)obj;
        GlobalAssist.ShowCenterTips("错误：脚本返回null", 50);
        return false;
    }
    public static int ExecuteInt(int index, string str)
    {
        if (m_proxy[index] == null) return 0;
        object obj = m_proxy[index].SafeCall(str);
        if (obj != null) return (int)obj;
        GlobalAssist.ShowCenterTips("错误：脚本返回null", 50);
        return 0;
    }
    public static void CheckOneFormula(string formula)
    {
        try
        {
            ScriptDomain domain = ScriptDomain.CreateDomain("ScriptManager");
            ScriptType sType = domain.CompileAndLoadMainSource(GenSource(true, formula));
            if (domain.OutputError(out string strError)) GlobalAssist.ShowCenterTips(strError, 10);
            if (sType == null) return;
            ScriptProxy proxy = sType.CreateInstance();
            object obj = proxy.SafeCall("Test");
            if (obj == null)
            {
                GlobalAssist.ShowCenterTips("错误：公式返回null");
                return;
            }
        }
        catch (Exception exc)
        {
            GlobalAssist.ShowCenterTips("脚本初始化出错: " + exc.ToString(), 50);
            return;
        }
        GlobalAssist.ShowCenterTips("检查完成，没有错误");
    }
}
