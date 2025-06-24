using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using LitJson;

public enum SoundType//内置的音效类型
{
    ButtonPress,//点击按钮
    ButtonPagePress,//点击页面按钮
    CancelPress,//点击取消按钮
    TogglePress,//点击勾选框
    Notify,//弹出通知
    StartGame,//开始新游戏

    None = 1000,
}
public class SoundManager : MonoBehaviour
{
    public AudioSource m_source;
    public static SoundManager m_instance = null;
    public static Dictionary<string, string> m_dicAudioClipPath = new Dictionary<string, string>();
    private static Dictionary<string, AudioClip> m_dicAudioClip = new Dictionary<string, AudioClip>();
    public static AudioClip[] m_builtinSound = new AudioClip[6];//内置音效
    // Start is called before the first frame update
    void Start()
    {
        m_instance = GetComponent<SoundManager>();
        InitAllAudioClip();
        InitBuiltinSound();
    }
    public static void InitAllAudioClip()
    {
        m_dicAudioClipPath.Clear();
        m_dicAudioClip.Clear();
    }
    public static AudioClip GetClip(string clipName)
    {
        if (m_dicAudioClip.ContainsKey(clipName)) return m_dicAudioClip[clipName];
        if (!m_dicAudioClipPath.ContainsKey(clipName)) return null;
        AudioClip audioClip = GameAssets.LoadAsset<AudioClip>(m_dicAudioClipPath[clipName]);
        if (audioClip == null)
        {
            GlobalAssist.ShowCenterTips("加载个人战音效<" + m_dicAudioClipPath[clipName] + ">失败", 50);
            return null;
        }
        m_dicAudioClip.Add(clipName, audioClip);
        return audioClip;
    }
    public static void InitBuiltinSound()//初始化内置音效
    {
        m_builtinSound[(int)SoundType.ButtonPress] = GameAssets.LoadAsset<AudioClip>("Assets/Resource/Sounds/ButtonPress.wav");
        m_builtinSound[(int)SoundType.ButtonPagePress] = GameAssets.LoadAsset<AudioClip>("Assets/Resource/Sounds/ButtonPagePress.wav");
        m_builtinSound[(int)SoundType.CancelPress] = GameAssets.LoadAsset<AudioClip>("Assets/Resource/Sounds/CancelPress.wav");
        m_builtinSound[(int)SoundType.TogglePress] = GameAssets.LoadAsset<AudioClip>("Assets/Resource/Sounds/TogglePress.wav");
        m_builtinSound[(int)SoundType.Notify] = GameAssets.LoadAsset<AudioClip>("Assets/Resource/Sounds/Notify.wav");
        m_builtinSound[(int)SoundType.StartGame] = GameAssets.LoadAsset<AudioClip>("Assets/Resource/Sounds/StartGame.wav");
    }

    public static void PlaySoundInWorld(string soundName, Vector3 pos, float volumeScale = 1)
    {
        if (!GameSetting.m_checkSound) return;
        if (!m_dicAudioClipPath.ContainsKey(soundName))
        {
            GlobalAssist.ShowCenterTips("错误：音效<" + soundName + ">不存在", 10);
            return;
        }
        AudioSource.PlayClipAtPoint(GetClip(soundName), pos, volumeScale * GameSetting.m_volumeSound);
    }
    public static void PlaySound(string soundName, float volumeScale = 1)
    {
        if (!GameSetting.m_checkSound) return;
        if (!m_dicAudioClipPath.ContainsKey(soundName))
        {
            GlobalAssist.ShowCenterTips("错误：音效<" + soundName + ">不存在", 10);
            return;
        }
        m_instance.m_source.PlayOneShot(GetClip(soundName), volumeScale * GameSetting.m_volumeSound);
    }
    public static void Play(SoundType type)//播放内置的UI音效
    {
        if (type == SoundType.None) return;
        if (!GameSetting.m_checkSoundUI) return;
        if (m_builtinSound[(int)type] == null) return;
        m_instance.m_source.PlayOneShot(m_builtinSound[(int)type], GameSetting.m_volumeSoundUI);
    }
}
