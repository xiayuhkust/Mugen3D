using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
public class CollideInfo
{
    public int m_startAnimIndex = 1;//序号从1开始
    public float m_startAnimPer = 0;//0-100
    public int m_endAnimIndex = 1;//
    public float m_endAnimPer = 0;
    public bool[] m_attackCollider = new bool[17];
    public bool[] m_hitCollider = new bool[17];

    public static List<string> m_tmpListStr = new List<string>();
    public static CollideInfo GetOne()
    {
        CollideInfo ci = new CollideInfo();
        for (int i = 0; i < 17; i++)
        {
            ci.m_attackCollider[i] = false;
            ci.m_hitCollider[i] = true;
        }
        return ci;
    }
    public void Copy(CollideInfo ci)
    {
        m_startAnimIndex = ci.m_startAnimIndex;
        m_startAnimPer = ci.m_startAnimPer;
        m_endAnimIndex = ci.m_endAnimIndex;
        m_endAnimPer = ci.m_endAnimPer;
        for (int i = 0; i < 17; i++) m_attackCollider[i] = ci.m_attackCollider[i];
        for (int i = 0; i < 17; i++) m_hitCollider[i] = ci.m_hitCollider[i];
    }
    public bool MeetCollider(List<int> listAttack, List<int> listHit)
    {
        bool hasAttack = false;
        bool hasHit = false;
        for (int i = 0; i < listAttack.Count; i++)
        {
            if (m_attackCollider[listAttack[i]])
            {
                hasAttack = true;
                break;
            }
        }
        for (int i = 0; i < listHit.Count; i++)
        {
            if (m_hitCollider[listHit[i]])
            {
                hasHit = true;
                break;
            }
        }
        return hasAttack && hasHit;
    }
    public string GetDisplayStr()
    {
        string str = "";
        int num = 0;
        List<string> listStr = GetListColliderStr();
        for (int i = 0; i < 17; i++)
        {
            if (m_attackCollider[i])
            {
                if (num >= 3)
                {
                    str += "...";
                    break;
                }
                if (num > 0) str += ", "; 
                str += listStr[i];
                num++;
            }
        }
        return str;
    }
    public static List<string> GetListColliderStr()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("头");
        m_tmpListStr.Add("脖子");
        m_tmpListStr.Add("胸");
        m_tmpListStr.Add("上腹");
        m_tmpListStr.Add("下腹");
        m_tmpListStr.Add("右上臂");
        m_tmpListStr.Add("左上臂");
        m_tmpListStr.Add("右前臂");
        m_tmpListStr.Add("左前臂");
        m_tmpListStr.Add("右手");
        m_tmpListStr.Add("左手");
        m_tmpListStr.Add("右大腿");
        m_tmpListStr.Add("左大腿");
        m_tmpListStr.Add("右小腿");
        m_tmpListStr.Add("左小腿");
        m_tmpListStr.Add("右脚");
        m_tmpListStr.Add("左脚");
        return m_tmpListStr;
    }
    public void Read(BinaryReader br, int versionNo)
    {
        m_startAnimIndex = br.ReadInt32();
        m_startAnimPer = br.ReadSingle();
        m_endAnimIndex = br.ReadInt32();
        m_endAnimPer = br.ReadSingle();
        for (int i = 0; i < 17; i++) m_attackCollider[i] = br.ReadBoolean();
        for (int i = 0; i < 17; i++) m_hitCollider[i] = br.ReadBoolean();
    }
    public void Write(BinaryWriter bw)
    {
        bw.Write(m_startAnimIndex);
        bw.Write(m_startAnimPer);
        bw.Write(m_endAnimIndex);
        bw.Write(m_endAnimPer);
        for (int i = 0; i < 17; i++) bw.Write(m_attackCollider[i]);
        for (int i = 0; i < 17; i++) bw.Write(m_hitCollider[i]);
    }
}
//受击控制器
public class HSC_Hitdef : HeroStateControl
{
    public bool[] m_attrA = new bool[3] { false, false, false };//分别为S(站立)、C(蹲下)、A(空中)，攻击属性，用来确定是否可以击中对手
    public int m_attr1 = 0;//0-N(普通)、1-S(特殊)、2-H(超级)
    public int m_attr2 = 0;//0-A(普通攻击)、1-T(投掷类攻击)、2-P(飞行道具攻击)
    public bool[] m_hitFlag = new bool[6] { false, false, false, false, false, false };//0-H(上段)1-L(下段)2-A(空中)3-M(中段)4-F(作用于下落状态)5-D(作用于躺下状态)
    public bool[] m_guardFlag = new bool[4] { false, false, false, false };//分别为H(上段)、L(下段)、A(空中)、M(中段)，决定对手如何防御住这次攻击
    public bool m_isGuardDist = false;//是否使用m_guardDist
    public float m_guardDist = 1f;//对手在这个距离内后退能进入防御状态
    public int m_groundType = 0;//对手在地面上被攻击的类型，0-High(攻击使对手头向后移)，1-Low(击中对手腹部)，2-Trip(摔倒)
    public int m_airType = 0;//同m_groundType
    public int m_animType = 0;//对手被击中后的动画类型，0-轻、1-中、2-重、3-打翻天、4-笔直向上(上勾拳)、5-打到空中头着地
    public int m_animTypeAir = 0;//对手在空中被击中后的动画类型，0-轻、1-中、2-重、3-打翻天、4-笔直向上(上勾拳)、5-打到空中头着地
    public int[] m_damage = new int[2] { 0, 0 };//击中和防御住时的伤害值
    public float[] m_pauseTime = new float[2] { 0, 0 };//自己和对手停顿的时长
    public float m_pauseDelay = 0;//停顿的延时
    public int[] m_sparkNo = new int[2] { 0, 0 };//击中和防御住时的火花号
    public float[] m_sparkXYZ = new float[3] { 0, 0, 0 };//火花位置
    public float[] m_sparkScale = new float[3] { 0, 0, 0 };//火花缩放
    public string[] m_soundID = new string[2] { "", "" };//击中和防御住时的音效
    public float m_groundSlideTime = 0;//对手被击中后向后滑动的时长
    public float m_groundHitTime = 0;//对手被击中后停留在受击状态的时长
    public float m_airHitTime = 0;//对手在空中被击中或击飞至空中后停留在受击状态的时长
    public Vector3 m_groundVelocity = Vector3.zero;//对手被击中后的初始速度
    public float m_guardSlideTime = 0;//对手防御住后的滑动时长
    public float m_guardVelocityX = 0;//对手防御住攻击后的X向初始速度
    public Vector3 m_airVelocity = Vector3.zero;//对手在空中被击中后的初始速度
    public bool m_isYAccel = false;//是否使用m_yAccel
    public float m_yAccel = 0;//对手被击中后给予y向的重力加速度
    public List<CollideInfo> m_listCollideInfo = new List<CollideInfo>();//碰撞信息

    private static List<string> m_tmpListStr = new List<string>();

    public static List<string> GetListAttr1()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("N(普通)");
        m_tmpListStr.Add("S(特殊)");
        m_tmpListStr.Add("H(超级)");
        return m_tmpListStr;
    }
    public static List<string> GetListAttr2()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("A(普通攻击)");
        m_tmpListStr.Add("T(投掷类攻击)");
        m_tmpListStr.Add("P(飞行道具攻击)");
        return m_tmpListStr;
    }
    public static List<string> GetListGroundType()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("High(攻击使对手头向后移)");
        m_tmpListStr.Add("Low(击中对手腹部)");
        m_tmpListStr.Add("Trip(摔倒)");
        return m_tmpListStr;
    }
    public static List<string> GetListAnimType()
    {
        m_tmpListStr.Clear();
        m_tmpListStr.Add("轻");
        m_tmpListStr.Add("中");
        m_tmpListStr.Add("重");
        m_tmpListStr.Add("打翻天");
        m_tmpListStr.Add("笔直向上(上勾拳)");
        m_tmpListStr.Add("打到空中头着地");
        return m_tmpListStr;
    }

    //HeroStateControl接口实现
    public override ControlType GetControlType()
    {
        return ControlType.HitDef;
    }
    public override bool Execute(int index, bool byOperation)
    {
        HeroCtrl heroCtrl = UI_GameUI.m_heroCtrl[index];
        HeroCtrl otherHeroCtrl = UI_GameUI.m_heroCtrl[1 - index];
        bool meetCollide = false;
        foreach (CollideInfo ci in m_listCollideInfo) 
        {
            if (heroCtrl.m_curAnimationIndex < ci.m_startAnimIndex-1 || (heroCtrl.m_curAnimationIndex == ci.m_startAnimIndex-1 && heroCtrl.m_curAnimationTime * 100 < ci.m_startAnimPer)) continue;
            if (heroCtrl.m_curAnimationIndex > ci.m_endAnimIndex - 1 || (heroCtrl.m_curAnimationIndex == ci.m_endAnimIndex - 1 && heroCtrl.m_curAnimationTime * 100 > ci.m_endAnimPer)) continue;
            if (ci.MeetCollider(heroCtrl.m_listMyCollider, heroCtrl.m_listOtherCollider))
            {
                meetCollide = true;
                break;
            }
        }
        if (!meetCollide) return false;
        int valueIndex = 0;//0-击中，1-防御住
        if (otherHeroCtrl.m_curStateDef != null && otherHeroCtrl.m_curStateDef.m_stateNo >= 120 && otherHeroCtrl.m_curStateDef.m_stateNo <= 155) //防御住
        {
            otherHeroCtrl.m_life -= m_damage[1] * heroCtrl.m_hero.m_baseInfo.m_attackVal / otherHeroCtrl.m_hero.m_baseInfo.m_defenceVal;
            otherHeroCtrl.m_velocity.x = m_guardVelocityX;
            otherHeroCtrl.m_slideLeftTime = m_guardSlideTime;
            otherHeroCtrl.m_isSliding = true;
            if (m_soundID[1] != "") SoundManager.PlaySound(m_soundID[1]);
            heroCtrl.m_guardCount++;
            valueIndex = 1;
        }
        else//击中
        {
            int stateNo = 0;
            if (otherHeroCtrl.m_stateDefType == StateDefType.Air)//空中
            {
                if (m_airType == 2) stateNo = 5070;
                else stateNo = 5020;
                otherHeroCtrl.m_velocity = m_airVelocity;
                otherHeroCtrl.m_hitLeftTime = m_airHitTime;
                heroCtrl.m_airtypeHitdef = m_airType;
                heroCtrl.m_animtypeHitdef = m_animTypeAir;
                heroCtrl.m_hittimeHitdef = m_airHitTime;
                heroCtrl.m_xvelHitdef = m_airVelocity.x;
                heroCtrl.m_yvelHitdef = m_airVelocity.y;
                heroCtrl.m_zvelHitdef = m_airVelocity.z;
            }
            else if (otherHeroCtrl.m_stateDefType == StateDefType.Lay)//躺下
            {
                stateNo = 5080;
            }
            else//地面
            {
                if (m_groundType == 2) stateNo = 5070;
                else stateNo = 5000;
                otherHeroCtrl.m_velocity = m_groundVelocity;
                otherHeroCtrl.m_hitLeftTime = m_groundHitTime;
                otherHeroCtrl.m_isSliding = true;
                otherHeroCtrl.m_slideLeftTime = m_groundSlideTime;
                heroCtrl.m_typeHitdef = m_groundType;
                heroCtrl.m_animtypeHitdef = m_animType;
                heroCtrl.m_hittimeHitdef = m_groundHitTime;
                heroCtrl.m_slidetimeHitdef = m_groundSlideTime;
                heroCtrl.m_xvelHitdef = m_groundVelocity.x;
                heroCtrl.m_yvelHitdef = m_groundVelocity.y;
                heroCtrl.m_zvelHitdef = m_groundVelocity.z;
            }
            if (otherHeroCtrl.m_hero.m_dicStateDef.TryGetValue(stateNo, out HeroStateDef hsd))
            {
                float gravity = m_isYAccel ? m_yAccel : -1;
                otherHeroCtrl.SetStateDef(hsd, gravity);
            }
            else GlobalAssist.ShowCenterTips("错误：角色<" + otherHeroCtrl.m_hero.m_name.GetStr() + ">不存在状态号<" + stateNo + ">", 50);
            otherHeroCtrl.m_life -= m_damage[0] * heroCtrl.m_hero.m_baseInfo.m_attackVal / otherHeroCtrl.m_hero.m_baseInfo.m_defenceVal;
            if (m_soundID[0] != "") SoundManager.PlaySound(m_soundID[0]);
            heroCtrl.m_hitCount++;
        }
        heroCtrl.m_damageHitdef = m_damage[valueIndex];
        heroCtrl.m_hitshaketimeHitdef = m_pauseTime[valueIndex];
        heroCtrl.m_slidetimeHitdef = m_groundSlideTime;
        heroCtrl.m_yaccelHitdef = m_yAccel;
        if (m_pauseTime[0] > 0.01f) heroCtrl.StartPause(m_pauseTime[0], m_pauseDelay);
        if (m_pauseTime[1] > 0.01f) otherHeroCtrl.StartPause(m_pauseTime[1], m_pauseDelay);
        return true;
    }
    public override string GetDisplayString()
    {
        return "攻击判定";
    }
    public override void Read(BinaryReader br, int versionNo)
    {
        for (int i = 0; i < 3; i++) m_attrA[i] = br.ReadBoolean();
        m_attr1 = br.ReadInt32();
        m_attr2 = br.ReadInt32();
        for (int i = 0; i < 6; i++) m_hitFlag[i] = br.ReadBoolean();
        for (int i = 0; i < 4; i++) m_guardFlag[i] = br.ReadBoolean();
        if (versionNo >= 202204070)
        {
            m_isGuardDist = br.ReadBoolean();
            m_guardDist = br.ReadSingle();
        }
        m_groundType = br.ReadInt32();
        m_airType = br.ReadInt32();
        m_animType = br.ReadInt32();
        m_animTypeAir = br.ReadInt32();
        for (int i = 0; i < 2; i++) m_damage[i] = br.ReadInt32();
        for (int i = 0; i < 2; i++) m_pauseTime[i] = br.ReadSingle();
        if (versionNo >= 202204200) m_pauseDelay = br.ReadSingle();
        for (int i = 0; i < 2; i++) m_sparkNo[i] = br.ReadInt32();
        for (int i = 0; i < 3; i++) m_sparkXYZ[i] = br.ReadSingle();
        for (int i = 0; i < 3; i++) m_sparkScale[i] = br.ReadSingle();
        for (int i = 0; i < 2; i++) m_soundID[i] = br.ReadString();
        m_groundSlideTime = br.ReadSingle();
        m_groundHitTime = br.ReadSingle();
        m_airHitTime = br.ReadSingle();
        for (int i = 0; i < 3; i++) m_groundVelocity[i] = br.ReadSingle();
        m_guardVelocityX = br.ReadSingle();
        for (int i = 0; i < 3; i++) m_airVelocity[i] = br.ReadSingle();
        if (versionNo >= 202204070) m_isYAccel = br.ReadBoolean();
        m_yAccel = br.ReadSingle();
        m_listCollideInfo.Clear();
        int collideNum = br.ReadInt32();
        for (int i = 0; i < collideNum; i++)
        {
            CollideInfo ci = CollideInfo.GetOne();
            ci.Read(br, versionNo);
            m_listCollideInfo.Add(ci);
        }
        if (versionNo >= 202204150) m_guardSlideTime = br.ReadSingle();
    }
    public override void Save(BinaryWriter bw)
    {
        for (int i = 0; i < 3; i++) bw.Write(m_attrA[i]);
        bw.Write(m_attr1);
        bw.Write(m_attr2);
        for (int i = 0; i < 6; i++) bw.Write(m_hitFlag[i]);
        for (int i = 0; i < 4; i++) bw.Write(m_guardFlag[i]);
        bw.Write(m_isGuardDist);
        bw.Write(m_guardDist);
        bw.Write(m_groundType);
        bw.Write(m_airType);
        bw.Write(m_animType);
        bw.Write(m_animTypeAir);
        for (int i = 0; i < 2; i++) bw.Write(m_damage[i]);
        for (int i = 0; i < 2; i++) bw.Write(m_pauseTime[i]);
        bw.Write(m_pauseDelay);
        for (int i = 0; i < 2; i++) bw.Write(m_sparkNo[i]);
        for (int i = 0; i < 3; i++) bw.Write(m_sparkXYZ[i]);
        for (int i = 0; i < 3; i++) bw.Write(m_sparkScale[i]);
        for (int i = 0; i < 2; i++) bw.Write(m_soundID[i]);
        bw.Write(m_groundSlideTime);
        bw.Write(m_groundHitTime);
        bw.Write(m_airHitTime);
        for (int i = 0; i < 3; i++) bw.Write(m_groundVelocity[i]);
        bw.Write(m_guardVelocityX);
        for (int i = 0; i < 3; i++) bw.Write(m_airVelocity[i]);
        bw.Write(m_isYAccel);
        bw.Write(m_yAccel);
        bw.Write(m_listCollideInfo.Count);
        for (int i = 0; i < m_listCollideInfo.Count; i++) m_listCollideInfo[i].Write(bw);
        bw.Write(m_guardSlideTime);
    }
    public override void Copy(HeroStateControl control)
    {
        HSC_Hitdef hsc = (HSC_Hitdef)control;
        for (int i = 0; i < 3; i++) m_attrA[i] = hsc.m_attrA[i];
        m_attr1 = hsc.m_attr1;
        m_attr2 = hsc.m_attr2;
        for (int i = 0; i < 6; i++) m_hitFlag[i] = hsc.m_hitFlag[i];
        for (int i = 0; i < 4; i++) m_guardFlag[i] = hsc.m_guardFlag[i];
        m_isGuardDist = hsc.m_isGuardDist;
        m_guardDist = hsc.m_guardDist;
        m_groundType = hsc.m_groundType;
        m_airType = hsc.m_airType;
        m_animType = hsc.m_animType;
        m_animTypeAir = hsc.m_animTypeAir;
        for (int i = 0; i < 2; i++) m_damage[i] = hsc.m_damage[i];
        for (int i = 0; i < 2; i++) m_pauseTime[i] = hsc.m_pauseTime[i];
        m_pauseDelay = hsc.m_pauseDelay;
        for (int i = 0; i < 2; i++) m_sparkNo[i] = hsc.m_sparkNo[i];
        for (int i = 0; i < 3; i++) m_sparkXYZ[i] = hsc.m_sparkXYZ[i];
        for (int i = 0; i < 3; i++) m_sparkScale[i] = hsc.m_sparkScale[i];
        for (int i = 0; i < 2; i++) m_soundID[i] = hsc.m_soundID[i];
        m_groundSlideTime = hsc.m_groundSlideTime;
        m_groundHitTime = hsc.m_groundHitTime;
        m_airHitTime = hsc.m_airHitTime;
        for (int i = 0; i < 3; i++) m_groundVelocity[i] = hsc.m_groundVelocity[i];
        m_guardSlideTime = hsc.m_guardSlideTime;
        m_guardVelocityX = hsc.m_guardVelocityX;
        for (int i = 0; i < 3; i++) m_airVelocity[i] = hsc.m_airVelocity[i];
        m_isYAccel = hsc.m_isYAccel;
        m_yAccel = hsc.m_yAccel;
        m_listCollideInfo.Clear();
        for (int i = 0; i < hsc.m_listCollideInfo.Count; i++)
        {
            CollideInfo ci = CollideInfo.GetOne();
            ci.Copy(hsc.m_listCollideInfo[i]);
            m_listCollideInfo.Add(ci);
        }
    }
}
