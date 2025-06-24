using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using LitJson;

public enum BGMType
{
    None = 0,//无
    StartMenu,//开始菜单
    CitySmall,//小城市
    CityMiddle,//中城市
    CityLarge,//大巨城市
    BigMap,//大地图
    War,//军团战
    Battle,//个人战
    Debate,//舌战
    SightRegion,//景区
}
public class BGM
{
    public string m_id;
    public BGMType m_type;
    public AudioClip m_audioClip = null;

    public static Dictionary<string, BGM> m_dicBGM = new Dictionary<string, BGM>();//key-id
    public static List<AudioClip> m_listClip = new List<AudioClip>();

    public static void InitAllBGM()
    {
        m_dicBGM.Clear();
        string filePath = Application.streamingAssetsPath + "/Json/BGM.json";
        StreamReader sr = new StreamReader(filePath);
        string textString = sr.ReadToEnd();
        sr.Close();
        JsonData jsonData = JsonMapper.ToObject(textString);
        foreach (JsonData item in jsonData)
        {
            BGM bgm = new BGM();
            bgm.m_id = item["id"].ToString();
            string fileName = item["fileName"].ToString();
            bgm.m_type = (BGMType)Convert.ToInt32(item["type"].ToString());
            bgm.m_audioClip = GameAssets.LoadAsset<AudioClip>("Assets/Resource/BGM/" + fileName);
            if (bgm.m_audioClip == null)
            {
                GlobalAssist.ShowCenterTips("错误：背景音乐<" + fileName + ">不存在", 50);
            }
            m_dicBGM.Add(bgm.m_id, bgm);//以编号作为key值
        }
    }
    public static AudioClip GetOneBGM(BGMType type)//根据BGM类型返回一个文件名称
    {
        m_listClip.Clear();
        foreach (BGM bgm in m_dicBGM.Values)
        {
            if (bgm.m_type == type) m_listClip.Add(bgm.m_audioClip);
        }
        if (m_listClip.Count == 0) return null;
        return m_listClip[UnityEngine.Random.Range(0, m_listClip.Count)];
    }
}
public class BGMManager : MonoBehaviour
{
    public AudioSource m_source;
    public static BGMManager m_instance = null;
    private bool m_isSwitching = false;//是否正在切换BGM
    private AudioClip m_newClip = null;//切换的新BGM

    private void Update()
    {
        if (m_isSwitching)
        {
            if (m_instance.m_source.volume <= 0)
            {
                m_source.clip = m_newClip;
                m_instance.m_source.volume = GameSetting.m_volumeBGM;
                m_isSwitching = false;
                UpdatePlayBGM();
            }
            else
            {
                m_instance.m_source.volume -= GameSetting.m_volumeBGM * Time.unscaledDeltaTime;
            }
        }
    }
    public static void SetBGM(BGMType type)
    {
        AudioClip clip = BGM.GetOneBGM(type);
        if (clip == null)
        {
            GlobalAssist.ShowCenterTips("错误：背景音乐类型<" + type + ">不存在", 10);
            return;
        }
        if (m_instance.m_source.clip == clip) return;
        m_instance.m_isSwitching = true;
        m_instance.m_newClip = clip;
    }
    public static void SetBGM(string bgmID)
    {
        if (!BGM.m_dicBGM.ContainsKey(bgmID) || BGM.m_dicBGM[bgmID].m_audioClip == null) 
        {
            GlobalAssist.ShowCenterTips("错误：背景音乐ID<" + bgmID + ">异常", 10);
            return;
        }
        if (m_instance.m_source.clip == BGM.m_dicBGM[bgmID].m_audioClip) return;
        m_instance.m_isSwitching = true;
        m_instance.m_newClip= BGM.m_dicBGM[bgmID].m_audioClip;
    }
    public static void UpdatePlayBGM()
    {
        if (GameSetting.m_checkBGM) m_instance.m_source.Play();
        else m_instance.m_source.Pause();
        m_instance.m_source.volume = GameSetting.m_volumeBGM;
    }
    public static void UpdateBGM()
    {
        
    }
}
