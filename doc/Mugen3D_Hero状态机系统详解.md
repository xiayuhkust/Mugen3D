# Mugen3D Hero状态机系统详解

## 📋 概述

Mugen3D的Hero状态机系统是整个格斗游戏的核心逻辑引擎，负责管理角色的所有行为状态、动画播放、攻击判定、移动控制等复杂游戏机制。本文档将深入分析状态机的架构设计和运作原理。

## 🏗️ 状态机架构总览

### 核心组件关系图
```
HeroCtrl (角色控制器)
    ├── m_curStateDef (当前状态定义)
    ├── SetStateDef() (状态切换方法)
    └── 状态执行循环

HeroStateDef (状态定义类)
    ├── 状态基础属性 (类型、物理、控制等)
    ├── m_listState (状态控制器列表)
    └── m_listAnimation (动画序列)

HeroState (状态控制器实例)
    ├── m_condition (执行条件)
    ├── m_control (具体控制器)
    └── 执行逻辑

HSC_* 控制器族 (13种专业控制器)
    ├── HSC_ChangeState (状态切换)
    ├── HSC_Hitdef (攻击判定)
    ├── HSC_Displacement (位移控制)
    └── ... (其他10种控制器)
```

## 🎯 核心类详解

### 1. HeroStateDef - 状态定义类

#### 基础属性系统
```csharp
public class HeroStateDef
{
    public int m_stateNo = 0;                    // 状态编号 (唯一标识)
    public StateDefType m_type;                  // 状态类型
    public StateDefMoveType m_moveType;          // 招式类型  
    public StateDefPhysicType m_physicType;      // 物理类型
    public StateDefControlType m_controlType;    // 控制类型
    
    // 物理属性
    public bool m_isVelset = false;              // 是否设置初始速度
    public float[] m_velset = new float[2];      // 初始速度 [x, y]
    public float m_gravity = 0;                  // 重力加速度
    
    // 游戏机制属性
    public int m_powerAdd = 0;                   // 能量条变化
    public int m_juggle = 0;                     // 连击点数
    public bool m_facep2 = false;                // 是否面向对手
    
    // 状态持久化属性
    public bool m_hitdefpersist = false;         // 保留攻击判定
    public bool m_movehitpersist = false;        // 保留击中信息
    public bool m_hitcountpersist = false;       // 保留连击数
    
    // 核心组件
    public List<HeroState> m_listState;          // 状态控制器列表
    public List<HeroAnimation> m_listAnimation;  // 动画序列
    public bool m_animRecycle = false;           // 动画循环播放
}
```

#### 状态类型枚举详解

**StateDefType (状态类型)**
- `Unchanged(0)`: 保持上一个状态类型
- `Stand(1)`: 站立状态
- `Crouch(2)`: 蹲下状态  
- `Air(3)`: 空中状态
- `Lay(4)`: 躺下状态

**StateDefMoveType (招式类型)**
- `Unchanged(0)`: 保持上一个招式类型
- `Attack(1)`: 攻击状态
- `Idle(2)`: 空闲状态
- `Hit(3)`: 受击状态

**StateDefPhysicType (物理类型)**
- `Unchanged(0)`: 不变
- `None(1)`: 无物理效果
- `Stand(2)`: 站立物理
- `Crouch(3)`: 蹲下物理
- `Air(4)`: 空中物理

**StateDefControlType (控制类型)**
- `Unchanged(0)`: 不变
- `UnControl(1)`: 不受玩家控制
- `Control(2)`: 受玩家控制

### 2. HeroState - 状态控制器实例

#### 核心属性
```csharp
public class HeroState
{
    // 基础信息
    public TextInfo m_remark;                    // 备注信息
    public string m_condition = "";              // 执行条件表达式
    public bool m_sameCondition = false;         // 与前一个相同条件
    
    // 执行控制
    public bool m_ignorehitpause = false;        // 忽略打击停顿
    public bool m_executeOnce = false;           // 只执行一次
    public HeroStateControl m_control;           // 具体控制器实例
    
    // 运行时状态
    public bool m_hasExecute = false;            // 已执行标记
    public float m_executeTime = -1;             // 首次执行时间
    public string m_tmpFormulaID = "";           // 临时公式ID
}
```

#### 条件执行机制
状态控制器通过条件表达式决定是否执行：
- **条件解析**: `m_condition`字符串包含复杂的逻辑表达式
- **条件复用**: `m_sameCondition`允许多个控制器共享条件
- **执行控制**: `m_executeOnce`确保某些操作只执行一次
- **时间管理**: `m_executeTime`记录执行时机

### 3. HeroStateControl - 控制器基类

#### 接口设计
```csharp
public class HeroStateControl
{
    public virtual ControlType GetControlType()           // 获取控制器类型
    public virtual bool Execute(int index, bool byOperation)  // 执行控制逻辑
    public virtual string GetDisplayString()              // 获取显示字符串
    public virtual void Read(BinaryReader br, int versionNo)  // 读取数据
    public virtual void Save(BinaryWriter bw)             // 保存数据
    public virtual void Copy(HeroStateControl control)   // 复制控制器
}
```

## 🎮 13种专业控制器详解

### 1. HSC_ChangeState - 状态切换控制器
**功能**: 实现状态间的跳转逻辑
```csharp
public class HSC_ChangeState : HeroStateControl
{
    public int m_target = 0;        // 目标: 0-自己, 1-对手
    public int m_changeStateNo = 0; // 目标状态号
    public int m_ctrl = 0;          // 控制权: 0-不变, 1-不受控制, 2-受控制
}
```

**执行逻辑**:
1. 清空输入记录 (非操控切换时)
2. 获取目标角色控制器
3. 查找目标状态定义
4. 调用`SetStateDef()`切换状态

### 2. HSC_Hitdef - 攻击判定控制器
**功能**: 处理攻击碰撞检测和伤害计算
```csharp
public class CollideInfo
{
    public int m_startAnimIndex = 1;           // 起始动画帧
    public float m_startAnimPer = 0;           // 起始动画百分比
    public int m_endAnimIndex = 1;             // 结束动画帧
    public float m_endAnimPer = 0;             // 结束动画百分比
    public bool[] m_attackCollider = new bool[17];  // 攻击碰撞器
    public bool[] m_hitCollider = new bool[17];     // 受击碰撞器
}
```

**碰撞检测机制**:
- 支持17个不同的碰撞器部位
- 基于动画帧和百分比的精确时间控制
- 攻击碰撞器与受击碰撞器的匹配算法

### 3. HSC_Displacement - 位移控制器
**功能**: 控制角色的位置和速度变化
```csharp
public class HSC_Displacement : HeroStateControl
{
    public int m_target = 0;                    // 目标: 0-自己, 1-对手
    public int m_type = 0;                      // 位移类型
    public int[] m_valueType = new int[3];      // 数值类型 [x,y,z]
    public Vector3 m_value = Vector3.zero;      // 位移数值
}
```

**位移类型**:
- `0`: 指定偏移量/秒
- `1`: 设置Y坐标
- `2`: 设置速度
- `3`: 设置速度变化

**数值类型** (支持13种预设速度):
- 向前走/向后走/向前跑速度
- 各种跳跃速度 (后小跳、垂直跳、前跳等)
- 空中移动速度

### 4. HSC_Value - 数值控制器
**功能**: 修改角色的各种数值属性
```csharp
public class HSC_Value : HeroStateControl
{
    public int m_target = 0;                    // 目标角色
    
    // 生命值控制
    public int m_lifeType = 0;                  // 0-不变, 1-设置, 2-变化
    public float m_life = 0;
    
    // 气槽控制
    public int m_powerType = 0;
    public float m_power = 0;
    
    // 重力控制
    public int m_gravityType = 0;
    public float m_gravity = 0;
    
    // 控制权
    public int m_ctrl = 0;                      // 0-不变, 1-可控制, 2-不可控制
    
    // 战斗系数
    public int m_defenceMulType = 0;            // 防御系数类型
    public float m_defenceMulSet = 1;           // 防御系数值
    public int m_attackMulType = 0;             // 攻击系数类型
    public float m_attackMulSet = 1;            // 攻击系数值
    
    // 动画速度
    public int m_animSpeedType = 0;
    public float m_animSpeed = 1;
}
```

### 5. HSC_Effect - 特效控制器
**功能**: 播放视觉特效
```csharp
public class HSC_Effect : HeroStateControl
{
    public List<string> m_listEffectID;         // 特效ID列表 (随机选择)
    public float m_effectSpeed = 1;             // 特效播放速度
    public Vector3 m_effectPos = Vector3.zero;  // 特效相对位置
    public float[] m_randomPosX = new float[2]; // X轴随机范围
    public float[] m_randomPosY = new float[2]; // Y轴随机范围
    public Vector3 m_effectRotation;            // 特效旋转
    public Vector3 m_effectScale = Vector3.one; // 特效缩放
    public bool m_effectStatic = false;         // 特效是否静态
}
```

### 6. HSC_Sound - 音效控制器
**功能**: 播放音效
```csharp
public class HSC_Sound : HeroStateControl
{
    public string m_soundID = "";               // 音效ID
    public float m_volume = 1;                  // 音量
}
```

### 7. HSC_Target - 目标控制器
**功能**: 处理角色间的绑定关系
```csharp
public class HSC_Target : HeroStateControl
{
    public int m_type = 0;                      // 0-绑定, 1-解绑
    public float m_duration = 0;                // 绑定时长
    public Vector3 m_pos;                       // 相对位置
}
```

### 8-13. 其他控制器
- **HSC_AfterImage**: 残影效果控制
- **HSC_AssertSpecial**: 特殊断言控制
- **HSC_Helper**: 援助任务控制
- **HSC_Pause**: 暂停控制
- **HSC_PlayerPush**: 碰撞检测开关
- **HSC_ScreenShake**: 屏幕震动控制
- **HSC_VarSet**: 变量设置控制

## 🔄 状态机运行机制

### 1. 状态切换流程

#### SetStateDef() 方法详解
```csharp
public void SetStateDef(HeroStateDef hsd, float gravity = -1)
{
    // 1. 防重复切换检查
    if (m_curStateDef == hsd) return;
    
    // 2. 防御状态特殊处理
    if (m_curStateDef != null)
    {
        if (m_curStateDef.m_stateNo >= 120 && m_curStateDef.m_stateNo <= 155 
            && hsd.m_stateNo == 120) return;
        m_prevState = m_curStateDef.m_stateNo;  // 记录前一状态
    }
    
    // 3. 启动动画系统
    StartAnimation(hsd);
    
    // 4. 设置当前状态
    m_curStateDef = hsd;
    
    // 5. 更新控制权
    if (hsd.m_controlType == StateDefControlType.UnControl) 
        m_canControl = false;
    else if (hsd.m_controlType == StateDefControlType.Control) 
        m_canControl = true;
    
    // 6. 更新状态类型
    if (hsd.m_type != StateDefType.Unchanged) 
        m_stateDefType = hsd.m_type;
    if (hsd.m_moveType != StateDefMoveType.Unchanged) 
        m_stateDefMoveType = hsd.m_moveType;
    if (hsd.m_physicType != StateDefPhysicType.Unchanged) 
        m_stateDefPhysicType = hsd.m_physicType;
    
    // 7. 重置时间和重力
    m_curStateTime = 0;
    m_curStateDef.m_gravity = gravity < 0 ? m_hero.m_baseInfo.m_gravity : gravity;
    
    // 8. 重置状态控制器执行标记
    foreach (HeroState state in hsd.m_listState)
    {
        state.m_hasExecute = false;
        state.m_executeTime = -1;
    }
}
```

### 2. 动画系统集成

#### StartAnimation() 方法
```csharp
public void StartAnimation(HeroStateDef hsd)
{
    if (hsd.m_listAnimation.Count == 0)
    {
        // 无动画定义时的处理
        return;
    }
    
    // 动画序列播放逻辑
    // 支持多段动画的连续播放
    // 支持循环播放模式
}
```

#### HeroAnimation 动画信息
```csharp
public class HeroAnimation
{
    public string m_name = "";          // 动画名称
    public float m_fadeTime = 0.1f;     // 过渡时间
    public float m_offTime = 0;         // 开始时间偏移
    public float m_endTime = 0.75f;     // 结束时间
}
```

### 3. 物理系统集成

#### 重力处理
```csharp
// 在Update循环中的重力计算
if (m_velocity.y > -100 && m_curStateDef != null) 
    m_velocity.y -= m_curStateDef.m_gravity * Time.deltaTime;
```

#### 摩擦力计算
```csharp
public float GetFriction()
{
    if (m_curStateDef == null) return 1;
    if (m_stateDefPhysicType == StateDefPhysicType.Stand) 
        return m_hero.m_baseInfo.m_standFriction;
    // ... 其他物理类型的摩擦力
}
```

### 4. 状态控制器执行循环

#### 执行条件检查
每个`HeroState`都有条件表达式`m_condition`，系统会：
1. 解析条件表达式
2. 检查`m_ignorehitpause`是否忽略打击停顿
3. 验证`m_executeOnce`的执行次数限制
4. 调用对应控制器的`Execute()`方法

#### 控制器多态执行
```csharp
// 在状态更新循环中
foreach (HeroState state in m_curStateDef.m_listState)
{
    if (CheckCondition(state.m_condition))  // 条件检查
    {
        if (!state.m_executeOnce || !state.m_hasExecute)  // 执行次数检查
        {
            state.m_control.Execute(playerIndex, false);  // 多态执行
            if (state.m_executeOnce) state.m_hasExecute = true;
        }
    }
}
```

## 🎯 实际应用场景

### 1. 攻击招式实现
```
状态定义: 攻击状态 (StateNo: 200)
├── 状态类型: Stand (站立)
├── 招式类型: Attack (攻击)
├── 物理类型: Stand (站立物理)
├── 控制类型: UnControl (不受控制)
├── 动画序列: ["attack_start", "attack_active", "attack_recovery"]
└── 状态控制器:
    ├── HSC_Hitdef (攻击判定) - 在active帧激活
    ├── HSC_Sound (音效) - 播放攻击音效
    ├── HSC_Effect (特效) - 显示攻击特效
    └── HSC_ChangeState (状态切换) - 攻击结束后回到待机
```

### 2. 受击反应实现
```
状态定义: 受击状态 (StateNo: 5000)
├── 状态类型: Stand (站立)
├── 招式类型: Hit (受击)
├── 物理类型: Stand (站立物理)
├── 控制类型: UnControl (不受控制)
├── 动画序列: ["hit_light"]
└── 状态控制器:
    ├── HSC_Value (数值修改) - 扣除生命值
    ├── HSC_Displacement (位移) - 击退效果
    ├── HSC_ScreenShake (屏幕震动) - 打击感
    └── HSC_ChangeState (状态切换) - 受击结束后恢复
```

### 3. 移动状态实现
```
状态定义: 向前走 (StateNo: 20)
├── 状态类型: Stand (站立)
├── 招式类型: Idle (空闲)
├── 物理类型: Stand (站立物理)
├── 控制类型: Control (受控制)
├── 动画序列: ["walk_forward"] (循环)
└── 状态控制器:
    ├── HSC_Displacement (位移) - 持续向前移动
    └── HSC_ChangeState (状态切换) - 根据输入切换到其他状态
```

## 🔧 高级特性

### 1. 条件表达式系统
状态机支持复杂的条件表达式，可以包含：
- 时间条件: `time > 10`
- 输入条件: `command = "forward"`
- 状态条件: `stateno = 200`
- 数值条件: `life < 50`
- 逻辑运算: `&&`, `||`, `!`

### 2. 状态持久化机制
- **hitdefpersist**: 攻击判定跨状态保持
- **movehitpersist**: 击中信息跨状态保持  
- **hitcountpersist**: 连击数跨状态保持

### 3. 多目标控制
大部分控制器支持`m_target`参数：
- `0`: 作用于自己
- `1`: 作用于对手

### 4. 数据序列化
所有状态机组件都支持二进制序列化：
- `Read()`: 从二进制流读取
- `Save()`: 保存到二进制流
- 支持版本兼容性检查

## 📊 性能优化设计

### 1. 对象池模式
- 控制器实例复用
- 减少GC压力

### 2. 条件缓存
- 相同条件的状态控制器共享计算结果
- `m_sameCondition`标记实现

### 3. 执行次数控制
- `m_executeOnce`避免重复执行
- `m_hasExecute`标记跟踪

### 4. 状态切换优化
- 防重复切换检查
- 特殊状态的切换限制

## 🎮 总结

Mugen3D的Hero状态机系统是一个设计精良的游戏逻辑框架，具有以下特点：

### 优势
1. **模块化设计**: 13种专业控制器各司其职
2. **高度可配置**: 丰富的参数和条件系统
3. **扩展性强**: 基于接口的多态设计
4. **性能优化**: 多种优化策略减少运行开销
5. **数据驱动**: 完整的序列化支持

### 应用价值
- **格斗游戏开发**: 完整的格斗游戏逻辑框架
- **状态机学习**: 优秀的状态机设计参考
- **Unity开发**: 专业的游戏架构实现

### 学习建议
1. 从`HeroState.cs`开始理解基础概念
2. 深入研究`HSC_ChangeState`和`HSC_Hitdef`核心控制器
3. 分析`HeroCtrl.SetStateDef()`的状态切换逻辑
4. 实践创建自定义状态和控制器

这个状态机系统展现了专业游戏开发的架构水准，是学习游戏编程和状态机设计的优秀案例。
