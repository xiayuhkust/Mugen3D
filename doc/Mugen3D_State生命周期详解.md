# Mugen3D State生命周期详解

## 📋 概述

在Mugen3D格斗游戏引擎中，State（状态）的生命周期是游戏逻辑的核心。每个State从创建到销毁经历完整的生命周期管理，包括定义加载、初始化、执行循环、状态转换和清理回收等阶段。

## 🔄 State生命周期六大阶段

### 阶段一：定义阶段 (Definition Phase)
**位置**: `Hero.cs` 的 `Read()` 方法
**时机**: 游戏启动时加载角色数据

```csharp
// Hero.cs - 状态定义加载
public void Read(BinaryReader br, int versionNo)
{
    m_dicStateDef.Clear();  // 清空现有状态定义
    
    // 加载各类状态到字典
    int baseStateCount = br.ReadInt32();
    for (int i = 0; i < baseStateCount; i++)
    {
        HeroStateDef hsd = new HeroStateDef();
        hsd.Read(br, versionNo);                 // 从二进制流读取
        m_listBaseState.Add(hsd);
        m_dicStateDef.Add(hsd.m_stateNo, hsd);   // 注册到字典
    }
}
```

**关键数据结构**:
- `HeroStateDef`: 状态蓝图，包含状态号、类型、物理属性、控制器列表
- `HeroState`: 状态控制器，包含执行条件、控制逻辑、执行标记
- `m_dicStateDef`: 状态字典，以状态号为键快速查找

### 阶段二：注册阶段 (Registration Phase)
**位置**: `UI_GameUI.cs` 的 `InitFormula()` 方法
**时机**: 游戏开始前，注册所有状态条件到脚本系统

```csharp
// UI_GameUI.cs - 条件公式注册
public void InitFormula()
{
    int formulaID = 10000;
    ScriptManager.m_dicFormulaBool.Clear();
    
    for (int i = 0; i < 2; i++)  // 两个玩家
    {
        // 注册StateDef中的状态条件
        foreach (HeroStateDef hsd in m_heroCtrl[i].m_hero.m_dicStateDef.Values)
        {
            foreach (HeroState state in hsd.m_listState)
            {
                string formulaKey = "Func" + formulaID++;
                state.m_tmpFormulaID = formulaKey;  // 分配临时公式ID
                ScriptManager.m_dicFormulaBool.Add(formulaKey, state.m_condition);
            }
        }
    }
}
```

### 阶段三：初始化阶段 (Initialization Phase)
**位置**: `HeroCtrl.cs` 的 `SetStateDef()` 方法
**时机**: 状态切换时调用

```csharp
// HeroCtrl.cs - 状态初始化
public void SetStateDef(HeroStateDef hsd, float gravity = -1)
{
    // 1. 防重复切换检查
    if (m_curStateDef == hsd) return;
    
    // 2. 记录前一状态
    if (m_curStateDef != null)
    {
        m_prevState = m_curStateDef.m_stateNo;
    }
    
    // 3. 启动动画系统
    StartAnimation(hsd);
    
    // 4. 设置当前状态
    m_curStateDef = hsd;
    
    // 5. 应用状态属性
    if (hsd.m_controlType == StateDefControlType.UnControl) m_canControl = false;
    if (hsd.m_type != StateDefType.Unchanged) m_stateDefType = hsd.m_type;
    
    // 6. 重置时间和物理参数
    m_curStateTime = 0;  // 状态时间归零
    m_curStateDef.m_gravity = gravity < 0 ? m_hero.m_baseInfo.m_gravity : gravity;
    
    // 7. 重置所有状态控制器的执行状态
    foreach (HeroState state in hsd.m_listState)
    {
        state.m_hasExecute = false;    // 重置执行标记
        state.m_executeTime = -1;      // 重置执行时间
    }
    
    // 8. 重置战斗计数器
    m_hitCount = 0;
    m_guardCount = 0;
}
```

### 阶段四：执行阶段 (Execution Phase)
**位置**: `UI_GameUI.cs` 的 `UpdatePlayer()` 方法
**时机**: 每帧调用，持续执行状态逻辑

```csharp
// UI_GameUI.cs - 状态执行循环
public void UpdatePlayer(int index)
{
    HeroCtrl heroCtrl = m_heroCtrl[index];
    
    // 1. 更新状态时间 (在HeroCtrl.Update()中)
    // m_curStateTime += Time.deltaTime;
    
    // 2. 执行当前状态的控制器 (核心执行逻辑)
    if (heroCtrl.m_curStateDef != null)
    {
        foreach (HeroState state in heroCtrl.m_curStateDef.m_listState)
        {
            // 检查executeOnce限制
            if (state.m_executeOnce && state.m_hasExecute) continue;
            
            // 条件检查
            bool conditionResult = false;
            if (!state.m_sameCondition)
            {
                // 设置触发时间变量
                ScriptManager.m_proxy[index].Fields["triggerTime"] = 
                    state.m_hasExecute ? (Time.time - state.m_executeTime) : -1;
                conditionResult = ScriptManager.ExecuteBool(index, state.m_tmpFormulaID);
            }
            
            // 执行控制器
            if (conditionResult)
            {
                if(state.m_control.Execute(index, false)) 
                {
                    state.m_hasExecute = true;  // 标记已执行
                }
                if (!state.m_hasExecute) 
                {
                    state.m_executeTime = Time.time;  // 记录首次执行时间
                }
            }
        }
    }
    
    // 3. 清理每帧临时数据
    heroCtrl.m_listMyCollider.Clear();
    heroCtrl.m_listOtherCollider.Clear();
}
```

**执行机制详解**:
1. **条件检查**: 每帧评估状态控制器的执行条件
2. **executeOnce限制**: 防止某些控制器重复执行
3. **时间管理**: 记录执行时间，支持时间相关的条件判断
4. **执行标记**: 跟踪控制器的执行状态

### 阶段五：转换阶段 (Transition Phase)
**位置**: `HSC_ChangeState.cs` 的 `Execute()` 方法
**时机**: 状态切换条件满足时

```csharp
// HSC_ChangeState.cs - 状态转换
public override bool Execute(int index, bool byOperation)
{
    // 1. 清空输入记录 (非操控切换时)
    if (!byOperation)
    {
        UI_GameUI.m_listInputRecord[index].Clear();
        UI_GameUI.m_commandNo[index] = 0;
    }
    
    // 2. 获取目标角色控制器
    HeroCtrl heroCtrl = m_target == 0 ? UI_GameUI.m_heroCtrl[index] : UI_GameUI.m_heroCtrl[1 - index];
    
    // 3. 查找目标状态定义并切换
    if (heroCtrl.m_hero.m_dicStateDef.TryGetValue(m_changeStateNo, out HeroStateDef hsd))
    {
        heroCtrl.SetStateDef(hsd);  // 触发新状态初始化
    }
    
    return true;
}
```

**转换类型**:
- **主动转换**: 通过HSC_ChangeState控制器触发
- **自动转换**: 如防御状态的自动进入
- **条件转换**: 基于时间、输入、状态等条件

### 阶段六：清理阶段 (Cleanup Phase)
**时机**: 状态切换时、每帧结束时、游戏结束时

```csharp
// 1. 状态切换时的清理 (在SetStateDef中)
foreach (HeroState state in hsd.m_listState)
{
    state.m_hasExecute = false;    // 重置执行标记
    state.m_executeTime = -1;      // 重置执行时间
}
m_curStateTime = 0;               // 重置状态时间

// 2. 每帧临时数据清理 (在UpdatePlayer末尾)
heroCtrl.m_listMyCollider.Clear();      // 清空碰撞器列表
heroCtrl.m_listOtherCollider.Clear();

// 3. 游戏结束时的完全清理
public void Clear()
{
    m_dicStateDef.Clear();        // 清空状态定义字典
    m_listBaseState.Clear();      // 清空状态列表
    // ... 其他清理 ...
}
```

## 📊 生命周期时序图

```
游戏启动
    ↓
[定义阶段] Hero.Read() - 加载状态定义到m_dicStateDef
    ↓
[注册阶段] UI_GameUI.InitFormula() - 注册条件公式到脚本系统
    ↓
[初始化阶段] HeroCtrl.SetStateDef() - 设置初始状态(状态号0)
    ↓
[执行阶段] 每帧循环
    ├── HeroCtrl.Update() - 更新状态时间
    ├── UI_GameUI.UpdatePlayer() - 执行状态控制器
    ├── 条件检查 → 控制器执行 → 执行标记更新
    └── 临时数据清理
    ↓
[转换阶段] HSC_ChangeState.Execute() - 状态切换触发
    ├── 查找目标状态
    ├── 调用SetStateDef()
    └── 重新进入初始化阶段
    ↓
[清理阶段] 状态切换时/游戏结束时
    ├── 重置执行标记
    ├── 清理临时数据
    └── 释放资源
```

## 🎯 关键生命周期特性

### 1. 状态持久化
某些状态属性在转换时可以保持：
- `m_hitdefpersist`: 攻击判定持续
- `m_movehitpersist`: 击中信息持续
- `m_hitcountpersist`: 连击数持续

### 2. 执行控制机制
- **executeOnce**: 控制器只执行一次
- **m_hasExecute**: 执行标记，防止重复执行
- **m_executeTime**: 执行时间记录，支持时间条件

### 3. 内存管理
- 状态定义在游戏启动时一次性加载
- 执行时数据每帧更新和清理
- 状态切换时重置临时数据

### 4. 性能优化
- 使用字典快速查找状态定义
- 条件公式预编译和缓存
- 每帧清理临时数据避免内存泄漏

## 💡 实际应用示例

### 攻击状态的完整生命周期
```
1. 定义阶段: 加载攻击状态定义(如状态号200)
2. 注册阶段: 注册攻击条件"command = "attack_a""
3. 初始化阶段: 玩家按下攻击键，SetStateDef(攻击状态)
4. 执行阶段: 
   - 播放攻击动画
   - 执行HSC_Hitdef产生攻击判定
   - 执行HSC_Sound播放攻击音效
5. 转换阶段: 攻击结束，HSC_ChangeState切换回站立状态
6. 清理阶段: 重置攻击相关的临时数据
```

这个完整的生命周期确保了State在Mugen3D中的高效、稳定运行，为复杂的格斗游戏逻辑提供了坚实的基础架构。
