# Mugen3D 项目结构概览

## 项目简介
Mugen3D 是一个基于 Unity 的 3D 格斗游戏编辑器，使用 C# 开发。该项目提供了完整的格斗游戏制作工具链，包括角色编辑、动画管理、状态机控制、UI界面等核心功能模块。

## 整体架构

### 📁 核心目录结构
```
Scripts/
├── 核心系统文件 (20个主要C#脚本)
├── State/ (状态机系统，14个HSC控制器)
└── UI/ (用户界面系统，50+个UI组件)
```

## 🎯 核心系统模块

### 1. 角色管理系统
#### Hero.cs (14.5KB)
- **功能**: 角色信息核心类
- **主要组件**:
  - `HeroGender`: 角色性别枚举 (Male/Female)
  - `HeroBodyInfo`: 角色身体信息管理
    - 支持男女角色各9种身体类型
    - 眼部模型管理
    - Prefab资源加载系统
  - `HeroBaseInfo`: 角色基础属性
    - 生命值上限 (m_lifeMax)
    - 气槽上限 (m_powerMax)
    - 攻击力/防御力 (m_attackVal/m_defenceVal)
    - 移动速度配置 (前进/后退/跑步/跳跃)

#### HeroCtrl.cs (37.3KB) - 最大的脚本文件
- **功能**: 角色控制器，挂载在角色GameObject上
- **核心功能**:
  - 角色Transform管理 (头部、颈部、臀部、肩部等)
  - 碰撞体系统 (上半身/下半身/总体碰撞器列表)
  - MagicaCloth物理系统集成
  - 动画控制器管理
  - 状态机集成
  - 装备部件字典管理

### 2. 动画管理系统
#### AnimationManager.cs (25.2KB)
- **功能**: 全局动画资源管理器
- **核心特性**:
  - 多类型动画分类存储:
    - 普通动画片段 (m_dicAnimationClip)
    - UI展示动画 (m_dicAnimationClipDisplay)
    - 待机动画 (m_dicAnimationClipIdle)
    - 移动动画 (m_dicAnimationClipMove)
    - 受击动画 (m_dicAnimationHurt[2])
    - 闪避动画 (m_dicAnimationDodge[3])
  - AssetBundle动画加载系统
  - 动画资源路径管理
  - 舌战系统动画支持

### 3. 输入命令系统
#### Command.cs (7.9KB)
- **功能**: 格斗游戏输入命令处理
- **核心类**:
  - `InputData`: 输入数据结构
    - 按键列表管理
    - 特殊输入类型 (普通/按住/定时松开)
    - 方向键组合支持
  - 双玩家按键映射系统
  - 输入记录匹配算法

### 4. 全局辅助系统
#### GlobalAssist.cs (8.8KB)
- **功能**: 全局工具类和数据管理
- **主要功能**:
  - 多语言支持 (中文/繁体中文/英文/日文)
  - 版本管理 (当前版本: 202204200)
  - 全局数据字典:
    - 角色数据 (m_dicHero, m_listHero)
    - 动画信息 (m_dicAnimationInfo)
    - 材质资源管理
  - 屏幕缩放系统

### 5. 游戏设置系统
#### GameSetting.cs (3.5KB)
- **功能**: 游戏配置参数管理
- **设置类别**:
  - **画面设置**: 分辨率 (1920x1080)、窗口模式、垂直同步
  - **音频设置**: BGM/音效/UI音效/人物语音的开关和音量控制
  - **调试设置**: FPS显示、调试信息输出
- **数据持久化**: 支持设置保存到本地文件

## 🎮 状态机系统 (State目录)

### 状态控制器 (HSC_* 系列)
格斗游戏的核心逻辑通过状态控制器实现：

#### 核心状态控制器
- **HSC_Hitdef.cs** (17.3KB): 攻击判定系统
- **HSC_Displacement.cs** (5.8KB): 位移控制
- **HSC_Value.cs** (6.8KB): 数值计算
- **HSC_Effect.cs** (3.2KB): 特效系统
- **HSC_ChangeState.cs** (2.4KB): 状态切换
- **HSC_VarSet.cs** (2.3KB): 变量设置
- **HSC_Target.cs** (2.1KB): 目标系统
- **HSC_AfterImage.cs** (1.8KB): 残影效果
- **HSC_ScreenShake.cs** (1.8KB): 屏幕震动
- **HSC_AssertSpecial.cs** (1.8KB): 特殊断言

#### 状态定义
- **HeroStateDef.cs** (7.0KB): 状态定义类
- **HeroState.cs** (5.2KB): 状态实例类

## 🖥️ UI系统 (UI目录)

### 主要UI模块 (50+个组件)

#### 核心编辑器UI
- **UI_MakeNPCModelUI.cs** (53.1KB): NPC模型制作界面 - 最大的UI文件
- **UI_EditStateUI.cs** (37.9KB): 状态编辑界面
- **UI_GameUI.cs** (25.4KB): 游戏主界面
- **UI_EditOneHeroUI.cs** (17.6KB): 单个角色编辑界面
- **UI_EditStateDefUI.cs** (15.5KB): 状态定义编辑界面

#### 专业编辑工具
- **角色编辑**: UI_EditHeroUI.cs, UI_SelectHeroUI.cs
- **动画编辑**: UI_SelectAnimationUI.cs, UI_CombAnimationRowUI.cs
- **命令编辑**: UI_EditCommandUI.cs, UI_MakeCommandUI.cs
- **颜色编辑**: UI_SelectColorUI.cs, ColorPickClick.cs
- **碰撞体编辑**: UI_EditBodyColliderUI.cs

#### UI基础组件
- **自定义控件**: UI_MyButton.cs, UI_MyToggle.cs, UI_MyScrollRect.cs
- **滚动系统**: UI_FixedScrollRect.cs
- **行UI组件**: 各种RowUI类用于列表显示

## 🔧 辅助系统

### 资源管理
- **GameAssets.cs** (3.8KB): 游戏资源加载管理
- **NPCModelManager.cs** (59.7KB): NPC模型管理器 - 第二大文件

### 控制系统
- **CameraCtrl.cs** (3.1KB): 摄像机控制
- **RotateCtrl.cs** (0.6KB): 旋转控制
- **BodyColliderCtrl.cs** (1.1KB): 身体碰撞器控制

### 音频系统
- **BGMManager.cs** (3.9KB): 背景音乐管理
- **SoundManager.cs** (3.7KB): 音效管理

### 特效系统
- **InstantEffectCtrl.cs** (7.1KB): 即时特效控制
- **GhostCtrl.cs** (2.2KB): 幻影控制

### 其他工具
- **ScriptManager.cs** (6.5KB): 脚本管理器
- **TextInfo.cs** (1.6KB): 文本信息
- **GlobalUpdateCtrl.cs** (1.2KB): 全局更新控制

## 📊 代码规模统计

### 文件大小排行 (Top 10)
1. **NPCModelManager.cs** - 59.7KB (NPC模型管理)
2. **UI_MakeNPCModelUI.cs** - 53.1KB (NPC制作界面)
3. **UI_EditStateUI.cs** - 37.9KB (状态编辑界面)
4. **HeroCtrl.cs** - 37.3KB (角色控制器)
5. **HeroModelPreviewCtrl.cs** - 31.3KB (角色模型预览)
6. **UI_GameUI.cs** - 25.4KB (游戏主界面)
7. **AnimationManager.cs** - 25.2KB (动画管理)
8. **UI_EditOneHeroUI.cs** - 17.6KB (单角色编辑)
9. **HSC_Hitdef.cs** - 17.3KB (攻击判定)
10. **UI_EditStateDefUI.cs** - 15.5KB (状态定义编辑)

### 系统模块分布
- **核心系统**: ~20个主要脚本文件
- **状态机系统**: 14个HSC控制器 + 2个状态定义类
- **UI系统**: 50+个界面组件
- **总代码量**: 约368KB (未包含.meta文件)

## 🎯 技术特点

### 1. 模块化设计
- 清晰的功能模块划分
- 状态机模式的广泛应用
- UI组件的高度复用

### 2. 资源管理
- AssetBundle资源加载系统
- 材质和动画的缓存机制
- Prefab的动态加载

### 3. 扩展性
- 支持多语言国际化
- 可配置的游戏设置
- 模块化的状态控制器

### 4. 专业性
- 完整的格斗游戏编辑器功能
- 复杂的动画状态机
- 专业的碰撞检测系统

## 🚀 快速上手建议

### 1. 核心入口点
- 从 `GlobalAssist.cs` 开始了解全局数据结构
- 查看 `Hero.cs` 和 `HeroCtrl.cs` 理解角色系统
- 研究 `UI_GameUI.cs` 了解主界面逻辑

### 2. 关键系统理解
- **状态机**: 重点关注 `HeroStateDef.cs` 和各个 `HSC_*.cs`
- **动画**: 深入 `AnimationManager.cs` 的资源管理机制
- **UI**: 从 `UI_Base.cs` 开始理解UI架构

### 3. 开发流程
- 角色创建 → 动画配置 → 状态定义 → UI编辑 → 测试调试

## 📝 总结

Mugen3D 是一个功能完整、架构清晰的Unity 3D格斗游戏编辑器。项目采用了成熟的模块化设计，具有以下优势：

- **完整性**: 涵盖了格斗游戏制作的所有核心功能
- **专业性**: 实现了复杂的状态机和动画系统
- **可扩展性**: 良好的架构设计支持功能扩展
- **用户友好**: 丰富的UI编辑工具

该项目适合作为格斗游戏开发的学习案例，也可以作为实际游戏开发的基础框架使用。
