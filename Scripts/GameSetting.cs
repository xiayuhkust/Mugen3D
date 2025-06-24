using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

//游戏中所有的设置参数
public static class GameSetting
{
    //画面
    public static int m_resolutionWidth = 1920;
    public static int m_resolutionHeight = 1080;
    public static bool m_isWondow = false;
    public static bool m_isVSync = true;
    //声音
    public static bool m_checkBGM = true;//是否播放背景音乐
    public static float m_volumeBGM = 1;//背景音乐音量
    public static bool m_checkSound = true;//是否播放音效
    public static float m_volumeSound = 1;//音效音量
    public static bool m_checkSoundUI = true;//是否播放UI音效
    public static float m_volumeSoundUI = 1;//UI音效音量
    public static bool m_checkSoundHuman = true;//是否播放人物语音
    public static float m_volumeSoundHuman = 1;//人物语音音量
    //调试
    public static bool m_showFPS = false;//是否显示帧率
    public static bool m_debug = false;//是否输出调试信息

    public static void SaveSetting()
    {
        string filePath = Application.streamingAssetsPath + "/SaveData/Setting";
        BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create));
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////版本号
        //bw.Write(SaveLoadManager.m_versionNo);
        //画面
        bw.Write(m_resolutionWidth);
        bw.Write(m_resolutionHeight);
        bw.Write(m_isWondow);
        bw.Write(m_isVSync);
        //声音
        bw.Write(m_checkBGM);
        bw.Write(m_volumeBGM);
        bw.Write(m_checkSound);
        bw.Write(m_volumeSound);
        bw.Write(m_checkSoundUI);
        bw.Write(m_volumeSoundUI);
        bw.Write(m_checkSoundHuman);
        bw.Write(m_volumeSoundHuman);
        //调试
        bw.Write(m_showFPS);
        bw.Write(m_debug);

        bw.Close();
    }
    public static void ReadSetting()
    {
        string filePath = Application.streamingAssetsPath + "/SaveData/Setting";
        if (!File.Exists(filePath)) return;
        BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
        if (br == null) return;
        ////////////////////////////////////////////////////////////////////////////////////////////////////版本号
        //int versionNo = br.ReadInt32();
        //if (versionNo > SaveLoadManager.m_versionNo)//只向前兼容，不向后兼容
        //{
        //    GlobalAssist.ShowCenterTips("设置文件版本号为" + versionNo + "，当前程序支持的最高版本号为" + SaveLoadManager.m_versionNo + "，无法加载设置文件");
        //    return;
        //}
        //画面
        m_resolutionWidth = br.ReadInt32();
        m_resolutionHeight = br.ReadInt32();
        m_isWondow = br.ReadBoolean();
        m_isVSync = br.ReadBoolean();
        //声音
        m_checkBGM = br.ReadBoolean();
        m_volumeBGM = br.ReadSingle();
        m_checkSound = br.ReadBoolean();
        m_volumeSound = br.ReadSingle();
        m_checkSoundUI = br.ReadBoolean();
        m_volumeSoundUI = br.ReadSingle();
        m_checkSoundHuman = br.ReadBoolean();
        m_volumeSoundHuman = br.ReadSingle();
        //调试
        m_showFPS = br.ReadBoolean();
        m_debug = br.ReadBoolean();
        br.Close();
        QualitySettings.vSyncCount = m_isVSync ? 1 : 0;
    }

}
