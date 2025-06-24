using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;

public static class GameAssets
{
    public static AssetBundle m_assets;
    public static AssetBundle m_assetsAnimation;
    public static AssetBundle m_assetsEffect;
    public static AssetBundle m_assetsSoundEffect;//动画音效
    public static AssetBundle m_assetsImage;
    public static AssetBundle m_assetsSound;//背景音乐和音效
    public static AssetBundle m_assetsEquipment;//装备配件
    public static List<AssetBundle> m_listAssetsMod = new List<AssetBundle>();//以后支持mod用

    public static void LoadAssetBundle()
    {
        string filePath = Application.streamingAssetsPath + "/AssetBundles/resource";
        m_assets = AssetBundle.LoadFromFile(filePath);
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/animation";
        m_assetsAnimation = AssetBundle.LoadFromFile(filePath);
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/effect";
        if (File.Exists(filePath)) m_assetsEffect = AssetBundle.LoadFromFile(filePath);
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/sound";
        if (File.Exists(filePath)) m_assetsSoundEffect = AssetBundle.LoadFromFile(filePath);
        filePath = Application.streamingAssetsPath + "/AssetBundlesImage/image";
        m_assetsImage = AssetBundle.LoadFromFile(filePath);
        filePath = Application.streamingAssetsPath + "/AssetBundlesSound/bgmsound";
        m_assetsSound = AssetBundle.LoadFromFile(filePath);
        filePath = Application.streamingAssetsPath + "/AssetBundlesEquipment/equipment";
        m_assetsEquipment = AssetBundle.LoadFromFile(filePath);
    }
    public static T LoadAsset<T>(string fileName) where T : Object
    {
#if !UNITY_EDITOR
        var obj = m_assets.LoadAsset<T>(fileName);
        if (obj == null)//优先查找mod资源
        {
            foreach (AssetBundle asset in m_listAssetsMod)
            {
                obj = asset.LoadAsset<T>(fileName);
                if (obj != null) break;
            }
        }
        if (obj == null) obj = m_assetsAnimation.LoadAsset<T>(fileName);
        if (obj == null && m_assetsEffect != null) obj = m_assetsEffect.LoadAsset<T>(fileName);
        if (obj == null && m_assetsSoundEffect != null) obj = m_assetsSoundEffect.LoadAsset<T>(fileName);
        if (obj == null) obj = m_assetsImage.LoadAsset<T>(fileName);
        if (obj == null) obj = m_assetsSound.LoadAsset<T>(fileName);
        if (obj == null) obj = m_assetsEquipment.LoadAsset<T>(fileName);
        return obj;
#else
        var obj = AssetDatabase.LoadAssetAtPath<T>(fileName);
        if (!Application.isPlaying) return obj;
        if (obj == null) obj = m_assets.LoadAsset<T>(fileName);
        if (obj == null)//优先查找mod资源
        {
            foreach (AssetBundle asset in m_listAssetsMod)
            {
                obj = asset.LoadAsset<T>(fileName);
                if (obj != null) break;
            }
        }
        if (obj == null) obj = m_assetsAnimation.LoadAsset<T>(fileName);
        if (obj == null && m_assetsEffect != null) obj = m_assetsEffect.LoadAsset<T>(fileName);
        if (obj == null && m_assetsSoundEffect != null) obj = m_assetsSoundEffect.LoadAsset<T>(fileName);
        if (obj == null) obj = m_assetsImage.LoadAsset<T>(fileName);
        if (obj == null) obj = m_assetsSound.LoadAsset<T>(fileName);
        if (obj == null) obj = m_assetsEquipment.LoadAsset<T>(fileName);
        return obj;
#endif
    }
    public static void PreloadUI(string fileName)//为了避免UI卡顿，每进入一个UI，就把当前UI涉及的子UI提前加载了
    {
#if !UNITY_EDITOR
        m_assets.LoadAssetAsync(fileName);
#endif
    }

}
