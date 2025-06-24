using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Burst;
using Unity.Collections;
using UnityEngine.Jobs;
using System.IO;
using System;

//全局更新用，DontDestroyOnLoad
public class GlobalUpdateCtrl : MonoBehaviour
{
    public static int m_timeCount = 0;
    public static int m_gameDay = 0;
    public static bool m_isRefreshAI = false;
    public static int m_updateByEvent = 0;//由事件引起的天数更新，是的话不触发新的剧情任务，避免出问题
    // Start is called before the first frame update
    void Start()
    {
        GlobalAssist.m_globalUpdate = this;
    }
    public static void InitData(int day)
    {
        m_timeCount = 0;
        m_gameDay = day;
        m_isRefreshAI = false;
    }
    private void Update()
    {
        GlobalAssist.UpdateAllCanvasGroup();
        GlobalAssist.UpdateCenterTips();

    }
    public static void WriteToBinary(BinaryWriter bw)
    {
        bw.Write(m_timeCount);
        bw.Write(m_gameDay);
    }
    public static void ReadFromBinary(BinaryReader br, int versionNo)
    {
        m_timeCount = br.ReadInt32();
        m_gameDay = br.ReadInt32();
    }
}

