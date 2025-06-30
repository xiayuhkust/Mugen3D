# Mugen3D 3D资源导入系统详解

## 📋 概述

Mugen3D项目实现了一套完整的3D资源导入和管理系统，基于Unity的AssetBundle技术，支持模型、动画、特效、音频等多种资源类型的动态加载。该系统具有模块化设计、支持MOD扩展、资源预加载等特性，为3D格斗游戏提供了高效的资源管理解决方案。

## 🏗️ 核心架构

### 系统组件结构
```
3D资源导入系统
├── GameAssets.cs (核心资源加载器)
├── NPCModelManager.cs (模型管理器)
├── HeroModelPreviewCtrl.cs (模型预览控制器)
├── AnimationManager.cs (动画管理器)
└── 各种UI组件 (资源编辑界面)
```

## 🎯 核心组件详解

### 1. GameAssets.cs - 核心资源加载系统

**位置**: `/Scripts/GameAssets.cs`

这是整个3D资源导入系统的核心，负责管理多个AssetBundle的加载和资源获取。

```csharp
public static class GameAssets
{
    // 六大AssetBundle资源包
    public static AssetBundle m_assets;              // 主要资源包
    public static AssetBundle m_assetsAnimation;     // 动画资源包
    public static AssetBundle m_assetsEffect;        // 特效资源包
    public static AssetBundle m_assetsSoundEffect;   // 动画音效包
    public static AssetBundle m_assetsImage;         // 图像资源包
    public static AssetBundle m_assetsSound;         // 背景音乐和音效包
    public static AssetBundle m_assetsEquipment;     // 装备配件包
    public static List<AssetBundle> m_listAssetsMod; // MOD支持列表
}
```

**资源加载流程**:
```csharp
public static void LoadAssetBundle()
{
    // 1. 主要资源包
    string filePath = Application.streamingAssetsPath + "/AssetBundles/resource";
    m_assets = AssetBundle.LoadFromFile(filePath);
    
    // 2. 动画资源包 (AES加密)
    filePath = Application.streamingAssetsPath + "/AssetBundlesAES/animation";
    m_assetsAnimation = AssetBundle.LoadFromFile(filePath);
    
    // 3. 特效资源包 (可选)
    filePath = Application.streamingAssetsPath + "/AssetBundlesAES/effect";
    if (File.Exists(filePath)) m_assetsEffect = AssetBundle.LoadFromFile(filePath);
    
    // 4. 装备配件包
    filePath = Application.streamingAssetsPath + "/AssetBundlesEquipment/equipment";
    m_assetsEquipment = AssetBundle.LoadFromFile(filePath);
    
    // ... 其他资源包加载
}
```

**通用资源加载方法**:
```csharp
public static T LoadAsset<T>(string fileName) where T : Object
{
    // 优先级加载顺序：
    // 1. 主资源包 → 2. MOD资源包 → 3. 动画包 → 4. 特效包 → 
    // 5. 音效包 → 6. 图像包 → 7. 音频包 → 8. 装备包
    
    var obj = m_assets.LoadAsset<T>(fileName);
    if (obj == null) // 优先查找MOD资源
    {
        foreach (AssetBundle asset in m_listAssetsMod)
        {
            obj = asset.LoadAsset<T>(fileName);
            if (obj != null) break;
        }
    }
    // 按优先级依次查找其他资源包...
    return obj;
}
```

### 2. NPCModelManager.cs - 3D模型管理系统

**位置**: `/Scripts/NPCModelManager.cs`

负责管理角色模型、装备配件、武器等3D资源的加载和管理。

**核心数据结构**:
```csharp
public class ModelInfo
{
    public List<PartMatInfo> m_listBodyMatInfo;  // 身体材质信息
    public List<PartMatInfo> m_listEyeMatInfo;   // 眼睛材质信息
    public List<string> m_listPart;              // 配件列表
    public Dictionary<string, List<PartMatInfo>> m_dicPartListColor; // 配件颜色映射
    
    // 身体变形参数
    public float m_bodyWidthFactor;   // 身体宽度系数
    public float m_bodyHeightFactor;  // 身体高度系数
    public float m_neckWidthFactor;   // 脖子宽度系数
    public float[] m_eyePara = new float[3]; // 眼部参数[上下/左右/前后位移]
}

public class NPCModel
{
    // 配件预览相关静态数据
    public static List<string> m_listPartTitleName;  // 配件标题名称
    public static List<string> m_listPartFileName;   // 配件文件名称
    public static List<string>[,] m_listPartName;    // 配件名称二维数组[性别,部位]
    
    // 缓存字典
    private static Dictionary<string, GameObject> m_dicPartObjects; // 实例化的配件对象
    private static Dictionary<string, GameObject> m_dicPart;        // 未实例化的配件
}
```

**配件加载机制**:
```csharp
// 从.dat文件加载配件列表
public static void LoadPartName()
{
    for (int gender = 0; gender < 2; gender++) // 男性/女性
    {
        string folderName = m_genderFolder[gender]; // "ModelPartMale"/"ModelPartFemale"
        
        for (int i = 0; i < 26; i++) // 26个身体部位
        {
            string filePath = Application.streamingAssetsPath + 
                "/AssetBundlesEquipment/" + folderName + "/" + 
                m_listPartFileName[i] + ".dat";
                
            BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            int fileCount = br.ReadInt32();
            
            for (int k = 0; k < fileCount; k++)
            {
                string modelName = br.ReadString();
                modelName = modelName.Replace(".prefab", "");
                m_listPartName[gender, i].Add(modelName);
            }
        }
    }
}

// 获取配件预览对象 (实例化)
public static GameObject GetPartPreview(string partName)
{
    if (m_dicPartObjects.ContainsKey(partName))
        return m_dicPartObjects[partName];
        
    GameObject model = GameAssets.LoadAsset<GameObject>("Assets/Resource/" + partName + ".prefab");
    if (model == null) return null;
    
    GameObject modelPreview = GameObject.Instantiate(model);
    GameObject.DontDestroyOnLoad(modelPreview);
    m_dicPartObjects.Add(partName, modelPreview);
    return modelPreview;
}

// 获取配件原始对象 (不实例化)
public static GameObject GetPart(string partName)
{
    if (m_dicPart.ContainsKey(partName)) return m_dicPart[partName];
    
    GameObject model = GameAssets.LoadAsset<GameObject>("Assets/Resource/" + partName + ".prefab");
    if (model == null) return null;
    
    m_dicPart.Add(partName, model);
    return model;
}
```

### 3. HeroModelPreviewCtrl.cs - 模型预览系统

**位置**: `/Scripts/HeroModelPreviewCtrl.cs`

提供3D模型的实时预览功能，支持装备穿戴、动画播放、材质调整等。

**初始化流程**:
```csharp
public static void Init()
{
    // 加载预览场景预制体
    GameObject prefab = GameAssets.LoadAsset<GameObject>("Assets/Resource/MakeNPCModel.prefab");
    
    // 创建两个预览控制器 (左右对比)
    for (int i = 0; i < 2; i++)
    {
        GameObject obj = GameObject.Instantiate(prefab);
        obj.transform.position = new Vector3(i * 100, 0, 0);
        DontDestroyOnLoad(obj);
        m_modelCtrl[i] = obj.GetComponent<HeroModelPreviewCtrl>();
        m_modelCtrl[i].InitComponent(i);
    }
}
```

**模型刷新机制**:
```csharp
public void RefreshModel(Hero hero)
{
    // 1. 清理现有配件
    foreach (Eq_PartLink partLink in m_listWearingPart)
    {
        m_avatar[0].DetachAvatarParts(partLink.part);
        m_avatar[1].DetachAvatarParts(partLink.part);
    }
    m_listWearingPart.Clear();
    
    // 2. 按顺序加载新配件
    for (int index = hero.m_modelInfo.m_listPart.Count - 1; index >= 0; index--)
    {
        string partName = hero.m_modelInfo.m_listPart[index];
        
        // 获取配件材质信息
        List<PartMatInfo> listMatInfo = null;
        if (hero.m_modelInfo.m_dicPartListColor.ContainsKey(partName))
            listMatInfo = hero.m_modelInfo.m_dicPartListColor[partName];
        
        // 加载配件模型
        GameObject model = NPCModel.GetPart(partName);
        Eq_PartLink partLinkTmp = model.GetComponent<Eq_PartLink>();
        
        // 验证配件完整性
        if (partLinkTmp == null)
        {
            GlobalAssist.ShowCenterTips("错误:<" + partName + ">不存在组件Eq_PartLink", 10);
            continue;
        }
        
        // 应用材质和颜色
        ApplyMaterialAndColor(partLinkTmp, listMatInfo);
        
        // 附加到Avatar系统
        m_avatar[0].AttachAvatarParts(partLinkTmp.part);
        m_avatar[1].AttachAvatarParts(partLinkTmp.part);
        m_listWearingPart.Add(partLinkTmp);
    }
}
```

### 4. AnimationManager.cs - 动画资源管理

**位置**: `/Scripts/AnimationManager.cs`

管理动画资源的加载、缓存和播放控制。

**动画资源加载**:
```csharp
public static void LoadAnimationClipName()
{
    // 清理现有缓存
    m_dicAnimationClip.Clear();
    m_dicAnimationClipPath.Clear();
    
    // 1. 加载动画目录信息
    string filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationInfo/AnimationDirectory.dat";
    BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
    int fileCount = br.ReadInt32();
    
    List<string> listAnimationFileName = new List<string>();
    for (int k = 0; k < fileCount; k++)
    {
        string fileName = br.ReadString();
        listAnimationFileName.Add(fileName);
        UI_SelectAnimationUI.m_listAnimationDirectory.Add(fileName);
    }
    br.Close();
    
    // 2. 加载各类动画文件信息
    for (int i = 0; i < listAnimationFileName.Count; i++)
    {
        filePath = Application.streamingAssetsPath + "/AssetBundlesAES/AnimationInfo/" + 
                   listAnimationFileName[i] + ".dat";
        br = new BinaryReader(new FileStream(filePath, FileMode.Open));
        fileCount = br.ReadInt32();
        
        List<string> listFileName = new List<string>();
        for (int k = 0; k < fileCount; k++)
        {
            string animationName = br.ReadString();
            string animationPath = br.ReadString();
            listFileName.Add(animationName);
            m_dicAnimationClipPath.Add(animationName, animationPath);
        }
        UI_SelectAnimationUI.m_listAnimationClipFile.Add(listFileName);
        br.Close();
    }
}
```

## 📁 资源存储结构

### StreamingAssets目录结构
```
StreamingAssets/
├── AssetBundles/
│   └── resource                    # 主要资源包
├── AssetBundlesAES/               # 加密资源包
│   ├── animation                  # 动画资源
│   ├── effect                     # 特效资源
│   ├── sound                      # 音效资源
│   └── AnimationInfo/             # 动画信息文件
│       ├── AnimationDirectory.dat
│       └── *.dat                  # 各类动画列表
├── AssetBundlesImage/
│   └── image                      # 图像资源包
├── AssetBundlesSound/
│   └── bgmsound                   # 背景音乐包
├── AssetBundlesEquipment/         # 装备资源包
│   ├── equipment
│   ├── ModelPartMale/             # 男性配件信息
│   │   └── *.dat                  # 配件列表文件
│   ├── ModelPartFemale/           # 女性配件信息
│   │   └── *.dat
│   └── Weapon/                    # 武器信息
│       └── *.dat
└── SaveData/                      # 存档数据
    └── Setting                    # 游戏设置
```

### 资源路径规范
```csharp
// 角色预制体路径
"Assets/Resource/Hero_Prefab/MaleAvatar.prefab"
"Assets/Resource/Hero_Prefab/FemaleAvatar.prefab"

// 配件预制体路径
"Assets/Resource/{partName}.prefab"

// UI预制体路径
"Assets/Resource/UI_Prefab/{UIName}.prefab"
"Assets/Resource/MakeNPCModel.prefab"
"Assets/Resource/UI_Prefab/TipsCanvas.prefab"
```

## 🔧 材质和纹理系统

### 材质信息管理
```csharp
public class PartMatInfo // 配件材质颜色信息
{
    public string m_matName = "";     // 材质名称
    public Color m_color = Color.white; // 材质颜色
    public float m_roughness = -1;    // 粗糙度
    public float m_metalic = -1;      // 金属度
}
```

### 材质应用流程
```csharp
// HeroCtrl.cs中的材质应用
private void ApplyMaterialAndColor(Eq_PartLink partLink, List<PartMatInfo> listMatInfo)
{
    if (listMatInfo != null)
    {
        Material[] materials = partLink.smr.materials;
        
        // 应用材质
        for (int matIndex = 0; matIndex < materials.Length; matIndex++)
        {
            if (matIndex >= listMatInfo.Count) break;
            if (!string.IsNullOrEmpty(listMatInfo[matIndex].m_matName))
            {
                materials[matIndex] = GlobalAssist.GetMaterial(listMatInfo[matIndex].m_matName);
            }
        }
        partLink.smr.materials = materials;
        
        // 应用颜色和属性
        for (int matIndex = 0; matIndex < partLink.smr.materials.Length; matIndex++)
        {
            if (matIndex >= listMatInfo.Count) break;
            PartMatInfo matInfo = listMatInfo[matIndex];
            
            // 设置颜色
            partLink.smr.materials[matIndex].color = matInfo.m_color;
            
            // 设置物理属性
            if (matInfo.m_roughness >= 0 && partLink.smr.materials[matIndex].HasProperty("_Glossiness"))
                partLink.smr.materials[matIndex].SetFloat("_Glossiness", matInfo.m_roughness);
                
            if (matInfo.m_metalic >= 0 && partLink.smr.materials[matIndex].HasProperty("_Metallic"))
                partLink.smr.materials[matIndex].SetFloat("_Metallic", matInfo.m_metalic);
        }
    }
}
```

## 🎮 实际应用场景

### 1. 角色创建流程
```csharp
// HeroCtrl.cs - 角色生成
static HeroCtrl GenOne(Hero hero)
{
    GameObject obj;
    // 根据性别加载对应的Avatar预制体
    if (hero.m_gender == HeroGender.Male) 
        obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/MaleAvatar.prefab");
    else 
        obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/FemaleAvatar.prefab");
        
    obj = GameObject.Instantiate(obj);
    HeroCtrl heroCtrl = obj.GetComponent<HeroCtrl>();
    
    // 应用角色配件和材质
    heroCtrl.RefreshWearing(hero);
    return heroCtrl;
}
```

### 2. 装备系统集成
```csharp
// 装备穿戴流程
public void RefreshWearing(Hero hero)
{
    // 清理现有装备
    ClearWearing();
    
    // 按优先级穿戴装备
    for (int index = hero.m_modelInfo.m_listPart.Count - 1; index >= 0; index--)
    {
        string partName = hero.m_modelInfo.m_listPart[index];
        
        // 加载装备模型
        GameObject modelPart = GameAssets.LoadAsset<GameObject>("Assets/Resource/" + partName + ".prefab");
        if (modelPart == null) continue;
        
        // 实例化并配置
        GameObject obj = GameObject.Instantiate(modelPart);
        Eq_PartLink partLink = obj.GetComponent<Eq_PartLink>();
        
        // 应用材质和颜色
        ApplyMaterialAndColor(partLink, hero.m_modelInfo.m_dicPartListColor[partName]);
        
        // 附加到角色
        m_avatar.AttachAvatarParts(partLink.part);
        m_listWearingPart.Add(partLink);
    }
}
```

### 3. UI编辑器集成
```csharp
// UI_MakeNPCModelUI.cs - 模型编辑界面
public static void OpenUI(Hero hero)
{
    if (m_UI == null)
    {
        // 加载编辑器UI预制体
        GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/MakeNPCModelUI.prefab");
        obj = GlobalAssist.InstantiateUI(obj, true);
        DontDestroyOnLoad(obj);
        m_UI = obj.GetComponent<UI_MakeNPCModelUI>();
    }
    
    // 初始化模型预览
    HeroModelPreviewCtrl.Init();
    HeroModelPreviewCtrl.m_modelCtrl[0].RefreshModel(hero);
}
```

## 🚀 高级特性

### 1. MOD支持系统
```csharp
// GameAssets.cs中的MOD支持
public static List<AssetBundle> m_listAssetsMod = new List<AssetBundle>();

// 加载MOD资源时优先查找
if (obj == null) // 优先查找mod资源
{
    foreach (AssetBundle asset in m_listAssetsMod)
    {
        obj = asset.LoadAsset<T>(fileName);
        if (obj != null) break;
    }
}
```

### 2. 资源预加载机制
```csharp
public static void PreloadUI(string fileName)
{
    // 为了避免UI卡顿，每进入一个UI，就把当前UI涉及的子UI提前加载
#if !UNITY_EDITOR
    m_assets.LoadAssetAsync(fileName);
#endif
}
```

### 3. 编辑器模式支持
```csharp
// 编辑器模式下使用AssetDatabase，运行时使用AssetBundle
#if UNITY_EDITOR
    var obj = AssetDatabase.LoadAssetAtPath<T>(fileName);
    if (!Application.isPlaying) return obj;
#endif
```

### 4. 资源缓存优化
```csharp
// NPCModelManager.cs中的双重缓存机制
private static Dictionary<string, GameObject> m_dicPartObjects; // 实例化缓存
private static Dictionary<string, GameObject> m_dicPart;        // 原始对象缓存
```

## 📊 性能优化策略

### 1. 分层加载
- **主资源包**: 核心游戏资源
- **专用资源包**: 动画、特效、音频分离
- **装备资源包**: 可选装备内容
- **MOD资源包**: 用户自定义内容

### 2. 懒加载机制
- 资源按需加载，避免内存浪费
- 使用字典缓存已加载资源
- 支持异步预加载减少卡顿

### 3. 内存管理
- DontDestroyOnLoad保持关键对象
- 及时清理临时实例化对象
- 材质和纹理复用机制

## 💡 开发建议

### 1. 添加新3D资源
1. 将模型文件放入对应的AssetBundle构建目录
2. 更新相应的.dat文件列表
3. 确保预制体包含必要的组件 (Eq_PartLink等)
4. 设置正确的Layer (16:NPC)

### 2. 扩展资源类型
1. 在GameAssets.cs中添加新的AssetBundle变量
2. 在LoadAssetBundle()中添加加载逻辑
3. 在LoadAsset()中添加查找优先级
4. 创建对应的管理器类

### 3. 性能优化
1. 合理使用资源预加载
2. 避免重复实例化相同资源
3. 及时释放不需要的资源引用
4. 使用对象池管理频繁创建的对象

## 🎯 总结

Mugen3D的3D资源导入系统是一个功能完整、设计精良的资源管理解决方案，具有以下特点：

- **模块化设计**: 清晰的职责分离和组件化架构
- **高效加载**: 基于AssetBundle的分层加载机制
- **灵活扩展**: 支持MOD和自定义资源
- **编辑器集成**: 完整的可视化编辑工具
- **性能优化**: 缓存机制和懒加载策略
- **跨平台支持**: Unity标准的资源管理方案

该系统为3D格斗游戏提供了强大的资源管理基础，支持复杂的角色定制、装备系统和动态内容加载需求。
