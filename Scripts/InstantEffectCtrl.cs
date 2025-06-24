using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
//瞬发特效类，挂在特效上，持续性特效不在此控制（由DurativeEffectCtrl控制）
public class InstantEffectCtrl : MonoBehaviour
{
    private ParticleSystem m_particle = null;
    private List<ParticleSystem> m_listSonParticle = new List<ParticleSystem>();//该特效下的子特效
    private string m_effectName;

    public static List<List<string>> m_listEffectFile = new List<List<string>>();
    public static List<string> m_listEffectDirectory = new List<string>();
    public static Dictionary<string, string> m_dicInstantEffectPath = new Dictionary<string, string>();
    public static Dictionary<string, List<InstantEffectCtrl>> m_dicListEffect = new Dictionary<string, List<InstantEffectCtrl>>();
    private static float m_duration = 0;//临时传递持续时长

    public static InstantEffectCtrl GetOne(string effectName, Transform parentTransform, Vector3 pos, Quaternion rotation, Vector3 scale, float speed = 1, float duration = 0, bool isStatic = false)
    {
        m_duration = duration;
        if (!m_dicInstantEffectPath.ContainsKey(effectName))
        {
            GlobalAssist.ShowCenterTips("错误：找不到特效<" + effectName + ">");
            return null;
        }
        if (!m_dicListEffect.ContainsKey(effectName))
        {
            List<InstantEffectCtrl> tmpListEffect = new List<InstantEffectCtrl>();
            m_dicListEffect.Add(effectName, tmpListEffect);
        }
        List<InstantEffectCtrl> listEffect = m_dicListEffect[effectName];
        InstantEffectCtrl effectCtrl;
        if (listEffect.Count == 0)
        {
            GameObject effect = GameAssets.LoadAsset<GameObject>(m_dicInstantEffectPath[effectName]);
            effect = Instantiate(effect, parentTransform);
            effectCtrl = effect.AddComponent<InstantEffectCtrl>();
            effectCtrl.m_effectName = effectName;
        }
        else
        {
            effectCtrl = listEffect[0];
            effectCtrl.transform.SetParent(parentTransform);
            effectCtrl.gameObject.SetActive(true);
            listEffect.RemoveAt(0);
        }

        effectCtrl.transform.localPosition = pos;
        effectCtrl.transform.localRotation = rotation;
        effectCtrl.transform.localScale = scale;
        if (effectCtrl.m_particle != null)
        {
            var main = effectCtrl.m_particle.main;
            main.simulationSpeed = speed;
            foreach (ParticleSystem ps in effectCtrl.m_listSonParticle)
            {
                main = ps.main;
                main.simulationSpeed = speed;
            }
        }
        if (isStatic) effectCtrl.transform.SetParent(null);
        effectCtrl.StartPlay();
        return effectCtrl;
    }
    public void StartPlay()
    {
        if (m_particle == null)
        {
            m_particle = GetComponent<ParticleSystem>();
            if (m_particle == null)
            {
                GlobalAssist.ShowCenterTips("错误：特效<" + gameObject.name + ">不存在Particle");
                Invoke("Recycle", 1);
                return;
            }
            GetParticleComponent(transform);
            //修改例子设置
            var main = m_particle.main;
            main.loop = false;
            main.playOnAwake = false;
            foreach (ParticleSystem ps in m_listSonParticle)
            {
                main = ps.main;
                main.loop = false;
                main.playOnAwake = false;
            }
        }
        m_particle.Play(true);//激活后立刻播放
        float duration = m_particle.main.duration;
        foreach (ParticleSystem ps in m_listSonParticle)//取时长的最大值
        {
            if (duration < ps.main.duration) duration = ps.main.duration;
        }
        if (m_duration > 0) duration = m_duration;
        else duration += 5f;//多给5s才消失，因为有些特效不是同时触发的，不能单纯取最大的时长
        Invoke("Recycle", duration);//播放完后回收
    }
    public void GetParticleComponent(Transform selTransform)
    {
        int childCount = selTransform.childCount;
        if (childCount == 0) return;
        for (int i = 0; i < childCount; i++)
        {
            Transform childTransform = selTransform.GetChild(i);
            ParticleSystem ps = childTransform.GetComponent<ParticleSystem>();
            if (ps != null) m_listSonParticle.Add(ps);
            GetParticleComponent(childTransform);
        }
    }
    void Recycle()
    {
        if (!gameObject.activeSelf) return;//避免重复回收
        gameObject.SetActive(false);
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);//回收后设为DontDestroyOnLoad
        List<InstantEffectCtrl> listEffect = m_dicListEffect[m_effectName];
        listEffect.Add(this);
    }
    public static void InitAllInstantEffect()
    {
        m_dicInstantEffectPath.Clear();
        string filePath;
        BinaryReader br;
        List<string> listFileName;
        int fileCount;
        m_listEffectDirectory.Clear();
        m_listEffectFile.Clear();
        List<string> listEffectFileName = new List<string>();
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/EffectInfo/Instant/EffectDirectory.dat";
        if (File.Exists(filePath))
        {
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                listEffectFileName.Add(fileName);
                m_listEffectDirectory.Add(fileName);
            }
            br.Close();
        }
        for (int i = 0; i < listEffectFileName.Count; i++)
        {
            filePath = Application.streamingAssetsPath + "/AssetBundlesAES/EffectInfo/Instant/" + listEffectFileName[i] + ".dat";
            if (!File.Exists(filePath))
            {
                m_listEffectFile.Add(new List<string>());
                continue;
            }
            br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            if (br == null)
            {
                GlobalAssist.ShowCenterTips("错误，文件不存在：" + filePath, 3);
                continue;
            }
            listFileName = new List<string>();
            fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string fileName = br.ReadString();
                if (m_dicInstantEffectPath.ContainsKey(fileName))
                {
                    GlobalAssist.ShowCenterTips("错误，特效名重复：" + fileName, 50);
                    continue;
                }
                m_dicInstantEffectPath.Add(fileName, "Assets/Resource/Effect/Instant/" + listEffectFileName[i] + "/" + fileName);
                listFileName.Add(fileName);
            }
            m_listEffectFile.Add(listFileName);
            br.Close();
        }
    }
}
