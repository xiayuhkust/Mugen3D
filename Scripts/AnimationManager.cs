using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public static class AnimationManager
{
    public static Dictionary<string, string> m_dicAnimationClipPath = new Dictionary<string, string>();//存储所有动画片段，key-动画名称
    private static Dictionary<string, AnimationClip> m_dicAnimationClip = new Dictionary<string, AnimationClip>();//存储所有动画片段，key-动画名称
    public static Dictionary<string, string> m_dicAnimationClipDisplayPath = new Dictionary<string, string>();//存储所有UI展示用的待机动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    private static Dictionary<string, AnimationClip> m_dicAnimationClipDisplay = new Dictionary<string, AnimationClip>();//存储所有UI展示用的待机动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    public static Dictionary<string, string> m_dicAnimationClipIdlePath = new Dictionary<string, string>();//存储所有待机动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    private static Dictionary<string, AnimationClip> m_dicAnimationClipIdle = new Dictionary<string, AnimationClip>();//存储所有待机动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    public static Dictionary<string, string> m_dicAnimationClipIdleOncePath = new Dictionary<string, string>();//存储所有待机开小差动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    private static Dictionary<string, AnimationClip> m_dicAnimationClipIdleOnce = new Dictionary<string, AnimationClip>();//存储所有待机开小差动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    public static Dictionary<string, string> m_dicAnimationClipMovePath = new Dictionary<string, string>();//存储所有移动动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    private static Dictionary<string, AnimationClip> m_dicAnimationClipMove = new Dictionary<string, AnimationClip>();//存储所有移动动画片段，key-动画名称，因为跟受击动画同名，所以另存一份
    public static Dictionary<string, string>[] m_dicAnimationHurtPath = new Dictionary<string, string>[2];//存储所有受击动画片段，key-动画名称
    private static Dictionary<string, AnimationClip>[] m_dicAnimationHurt = new Dictionary<string, AnimationClip>[2];//存储所有受击动画片段，key-动画名称
    public static Dictionary<string, string>[] m_dicAnimationDodgePath = new Dictionary<string, string>[3];//存储所有闪避动画片段，key-动画名称,0-开始，1-持续，2-结束
    private static Dictionary<string, AnimationClip>[] m_dicAnimationDodge = new Dictionary<string, AnimationClip>[3];//存储所有闪避动画片段，key-动画名称,0-开始，1-持续，2-结束
    public static Dictionary<string, AnimationClip>[] m_dicDebateAnimationIdleOnce = new Dictionary<string, AnimationClip>[2];//存储所有舌战开小差动画片段，每次从中随机一个播放，0-男性，1-女性
    public static List<string>[] m_listDebateAnimationIdleOncePath = new List<string>[2];//存储所有舌战开小差动画片段，0-男性，1-女性

    public static void ReloadAssetBundleAnimation()//重新载入动画的AssetBundle，有时候动画为empty，原因未知
    {
        //GameAssets.m_assetsAnimation.Unload(true);
        //string filePath = Application.streamingAssetsPath + "/AssetBundlesAES/animation";
        //GameAssets.m_assetsAnimation = AssetBundle.LoadFromFile(filePath);
        //m_dicAnimationClip.Clear();
        //m_dicAnimationClipDisplay.Clear();
        //m_dicAnimationClipIdle.Clear();
        //m_dicAnimationClipIdleOnce.Clear();
        //m_dicAnimationClipMove.Clear();
        //for (int i = 0; i < 2; i++) m_dicAnimationHurt[i].Clear();
        //for (int i = 0; i < 3; i++) m_dicAnimationDodge[i].Clear();
    }
    public static AnimationClip GetAnimationClip(string clipName, int curTime = 0)
    {
        if (m_dicAnimationClip.ContainsKey(clipName)) return m_dicAnimationClip[clipName];
        if (!m_dicAnimationClipPath.ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationClipPath[clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationClip(clipName, ++curTime);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationClip.Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetAnimationClipDisplay(string clipName, int curTime = 0)
    {
        if (m_dicAnimationClipDisplay.ContainsKey(clipName)) return m_dicAnimationClipDisplay[clipName];
        if (!m_dicAnimationClipDisplayPath.ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationClipDisplayPath[clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationClipDisplay(clipName, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationClipDisplay.Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetAnimationClipIdle(string clipName, int curTime = 0)
    {
        if (m_dicAnimationClipIdle.ContainsKey(clipName)) return m_dicAnimationClipIdle[clipName];
        if (!m_dicAnimationClipIdlePath.ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationClipIdlePath[clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationClipIdle(clipName, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationClipIdle.Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetAnimationClipIdleOnce(string clipName, int curTime = 0)
    {
        if (m_dicAnimationClipIdleOnce.ContainsKey(clipName)) return m_dicAnimationClipIdleOnce[clipName];
        if (!m_dicAnimationClipIdleOncePath.ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationClipIdleOncePath[clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationClipIdleOnce(clipName, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationClipIdleOnce.Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetAnimationClipMove(string clipName, int curTime = 0)
    {
        if (m_dicAnimationClipMove.ContainsKey(clipName)) return m_dicAnimationClipMove[clipName];
        if (!m_dicAnimationClipMovePath.ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationClipMovePath[clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationClipMove(clipName, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationClipMove.Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetAnimationHurt(string clipName, int index, int curTime = 0)
    {
        if (m_dicAnimationHurt[index].ContainsKey(clipName)) return m_dicAnimationHurt[index][clipName];
        if (!m_dicAnimationHurtPath[index].ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationHurtPath[index][clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationHurt(clipName, index, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationHurt[index].Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetAnimationDodge(string clipName, int index, int curTime = 0)
    {
        if (m_dicAnimationDodge[index].ContainsKey(clipName)) return m_dicAnimationDodge[index][clipName];
        if (!m_dicAnimationDodgePath[index].ContainsKey(clipName))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">不存在", 20);
            return null;
        }
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(m_dicAnimationDodgePath[index][clipName]);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetAnimationDodge(clipName, index, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicAnimationDodge[index].Add(clipName, animClip);
        return animClip;
    }
    public static AnimationClip GetDebateAnimationClipIdleOnce(int gender, int curTime = 0)
    {
        if (m_listDebateAnimationIdleOncePath[gender].Count == 0) return null;
        string clipName = m_listDebateAnimationIdleOncePath[gender][UnityEngine.Random.Range(0, m_listDebateAnimationIdleOncePath[gender].Count)];
        if (m_dicDebateAnimationIdleOnce[gender].ContainsKey(clipName)) return m_dicDebateAnimationIdleOnce[gender][clipName];
        AnimationClip animClip = GameAssets.LoadAsset<AnimationClip>(clipName);
        if (animClip == null)
        {
            GlobalAssist.ShowCenterTips("加载动画<" + clipName + ">失败", 50);
            return null;
        }
        if (animClip.empty)
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + clipName + ">为empty，已强行重新加载AssetBundleAnimation，尝试" + curTime + "次", 50);
            if (curTime >= 10) return null;
            ReloadAssetBundleAnimation();
            return GetDebateAnimationClipIdleOnce(gender, curTime + 1);
        }
        animClip.events = null;//清空所有动画事件
        m_dicDebateAnimationIdleOnce[gender].Add(clipName, animClip);
        return animClip;
    }
    public static void InitAllAnimationClip()
    {
        m_dicAnimationClip.Clear();
        m_dicAnimationClipPath.Clear();
        string filePath;
        BinaryReader br;
        int fileCount;
        List<string> listFileName;
        List<string> listAnimationFileName = new List<string>();
        UI_SelectAnimationUI.m_listAnimationClipFile.Clear();
        UI_SelectAnimationUI.m_listAnimationDirectory.Clear();
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationInfo/AnimationDirectory.dat";
        if (File.Exists(filePath))
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                listAnimationFileName.Add(fileName);
                UI_SelectAnimationUI.m_listAnimationDirectory.Add(fileName);
            }
            br.Close();
        }
        for (int i = 0; i < listAnimationFileName.Count; i++)
        {
            listFileName = new List<string>();
            filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationInfo/" + listAnimationFileName[i] + ".dat";
            if (!File.Exists(filePath))
            {
                GlobalAssist.ShowCenterTips("错误，文件不存在：" + filePath, 3);
                UI_SelectAnimationUI.m_listAnimationClipFile.Add(listFileName);
                continue;
            }
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();

                listFileName.Add(fileName);
                if (m_dicAnimationClipPath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，动画名重复(不同文件夹下文件应加前缀)：" + listAnimationFileName[i] + "/" + fileName, 50);
                    continue;
                }
                m_dicAnimationClipPath.Add(fileName, "Assets/Resource/Animation/" + listAnimationFileName[i] + "/" + fileName);
            }
            br.Close();
            UI_SelectAnimationUI.m_listAnimationClipFile.Add(listFileName);
        }
        //日常动画
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationDaily.dat";
        if (!File.Exists(filePath))
        {
            GlobalAssist.ShowCenterTips("错误，文件不存在：" + filePath, 3);
        }
        else
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                if (m_dicAnimationClipPath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，日常动画名重复：" + fileName, 50);
                    continue;
                }
                m_dicAnimationClipPath.Add(fileName, "Assets/Resource/AnimationDaily/" + fileName);
            }
            br.Close();
        }
        //个人战待机动画
        m_dicAnimationClipIdlePath.Clear();
        m_dicAnimationClipIdle.Clear();
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationIdle.dat";
        if (!File.Exists(filePath))
        {
            GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
        }
        else
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                if (m_dicAnimationClipIdlePath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，个人战待机动画名重复：" + fileName, 50);
                    continue;
                }
                m_dicAnimationClipIdlePath.Add(fileName, "Assets/Resource/AnimationIdle/" + fileName);
            }
            br.Close();
        }
        //个人战待机开小差动画
        m_dicAnimationClipIdleOncePath.Clear();
        m_dicAnimationClipIdleOnce.Clear();
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationIdleOnce.dat";
        if (!File.Exists(filePath))
        {
            GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
        }
        else
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                if (m_dicAnimationClipIdleOncePath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，待机开小差动画名重复：" + fileName, 50);
                    continue;
                }
                m_dicAnimationClipIdleOncePath.Add(fileName, "Assets/Resource/AnimationIdleOnce/" + fileName);
            }
            br.Close();
        }
        //个人战移动动画
        m_dicAnimationClipMovePath.Clear();
        m_dicAnimationClipMove.Clear();
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationMove.dat";
        if (!File.Exists(filePath))
        {
            GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
        }
        else
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                if (m_dicAnimationClipMovePath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，个人战移动动画名重复：" + fileName, 50);
                    continue;
                }
                m_dicAnimationClipMovePath.Add(fileName, "Assets/Resource/AnimationMove/" + fileName);
            }
            br.Close();
        }
        //个人战受击动画
        listFileName = new List<string>();
        for (int i = 0; i < 2; i++)
        {
            if (m_dicAnimationHurt[i] == null) m_dicAnimationHurt[i] = new Dictionary<string, AnimationClip>();
            else m_dicAnimationHurt[i].Clear();
            if (m_dicAnimationHurtPath[i] == null) m_dicAnimationHurtPath[i] = new Dictionary<string, string>();
            else m_dicAnimationHurtPath[i].Clear();
            string dirName = "AnimationHurt";
            if (i == 1) dirName = "AnimationHurt2";
            filePath = Application.streamingAssetsPath + "/AssetBundlesAES/" + dirName + ".dat";
            if (!File.Exists(filePath))
            {
                GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
            }
            else
            {
                br = new BinaryReader(new FileStream(filePath, FileMode.Open));
                fileCount = br.ReadInt32();
                for (int k = 0; k < fileCount; k++)
                {
                    string fileName = br.ReadString();
                    if (m_dicAnimationHurtPath[i].ContainsKey(fileName))
                    {
                        GlobalAssist.ShowCenterTips("错误，个人战受击动画名重复：" + fileName, 50);
                        continue;
                    }
                    if (i == 0) listFileName.Add(fileName);
                    m_dicAnimationHurtPath[i].Add(fileName, "Assets/Resource/" + dirName + "/" + fileName);
                }
                br.Close();
            }
        }
        //个人战闪避动画
        listFileName = new List<string>();
        for (int i = 0; i < 3; i++)
        {
            if (m_dicAnimationDodge[i] == null) m_dicAnimationDodge[i] = new Dictionary<string, AnimationClip>();
            else m_dicAnimationDodge[i].Clear();
            if (m_dicAnimationDodgePath[i] == null) m_dicAnimationDodgePath[i] = new Dictionary<string, string>();
            else m_dicAnimationDodgePath[i].Clear();
            string dirName = "AnimationDodgeStart";
            if (i == 1) dirName = "AnimationDodgeLoop";
            else if (i == 2) dirName = "AnimationDodgeEnd";
            filePath = Application.streamingAssetsPath + "/AssetBundlesAES/" + dirName + ".dat";
            if (!File.Exists(filePath))
            {
                GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
            }
            else
            {
                br = new BinaryReader(new FileStream(filePath, FileMode.Open));
                fileCount = br.ReadInt32();
                for (int k = 0; k < fileCount; k++)
                {
                    string fileName = br.ReadString();
                    if (m_dicAnimationDodgePath[i].ContainsKey(fileName))
                    {
                        GlobalAssist.ShowCenterTips("错误，个人战闪避动画名重复：" + fileName, 50);
                        continue;
                    }
                    if (i == 0) listFileName.Add(fileName);
                    m_dicAnimationDodgePath[i].Add(fileName, "Assets/Resource/" + dirName + "/" + fileName);
                }
                br.Close();
            }
        }
        //舌战待机开小差动画
        for (int i = 0; i < 2; i++)
        {
            if (m_listDebateAnimationIdleOncePath[i] == null) m_listDebateAnimationIdleOncePath[i] = new List<string>();
            else m_listDebateAnimationIdleOncePath[i].Clear();
            if (m_dicDebateAnimationIdleOnce[i] == null) m_dicDebateAnimationIdleOnce[i] = new Dictionary<string, AnimationClip>();
            else m_dicDebateAnimationIdleOnce[i].Clear();
            string strGender = i == 0 ? "Male" : "Female";
            filePath = Application.streamingAssetsPath + "/AssetBundlesAES/DebateAnimationIdleOnce" + strGender + ".dat";
            if (!File.Exists(filePath))
            {
                GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
            }
            else
            {
                br = new BinaryReader(new FileStream(filePath, FileMode.Open));
                fileCount = br.ReadInt32();
                for (int k = 0; k < fileCount; k++)
                {
                    string fileName = br.ReadString();
                    m_listDebateAnimationIdleOncePath[i].Add("Assets/Resource/DebateAnimationIdleOnce/" + strGender + "/" + fileName);
                }
                br.Close();
            }
        }
        //UI展示待机动画
        m_dicAnimationClipDisplayPath.Clear();
        m_dicAnimationClipDisplay.Clear();
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationDisplay.dat";
        if (!File.Exists(filePath))
        {
            GlobalAssist.ShowCenterTips("文件不存在：" + filePath, 3);
        }
        else
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                if (m_dicAnimationClipDisplayPath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，UI展示动画名重复：" + fileName, 50);
                    continue;
                }
                m_dicAnimationClipDisplayPath.Add(fileName, "Assets/Resource/AnimationDisplay/" + fileName);
            }
            br.Close();
        }
    }
}
