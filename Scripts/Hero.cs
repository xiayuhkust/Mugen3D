using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
//角色信息类
public enum HeroGender
{
    Male,
    Female,
}
public class HeroBodyInfo//武将身体信息，用这个先把prefab存起来
{
    public static GameObject[,] m_bodys = new GameObject[2, 9] { { null, null, null, null, null, null, null, null, null }, { null, null, null, null, null, null, null, null, null } };
    public static GameObject[] m_eyes = new GameObject[2] { null, null };
    public static GameObject GetBody(int gender, int index)
    {
        if (m_bodys[gender, index] != null) return m_bodys[gender, index];
        if (gender == 0) m_bodys[gender, index] = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/Male/Body" + (index + 1) + ".prefab");
        else m_bodys[gender, index] = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/Female/Body" + (index + 1) + ".prefab");
        if (m_bodys[gender, index] == null) GlobalAssist.ShowCenterTips("错误：加载人物身体prefab失败", 50);
        return m_bodys[gender, index];
    }
    public static GameObject GetEye(int gender)
    {
        if (m_eyes[gender] != null) return m_eyes[gender];
        if (gender == 0) m_eyes[gender] = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/Male/Eyes.prefab");
        else m_eyes[gender] = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/Female/Eyes.prefab");
        if (m_eyes[gender] == null) GlobalAssist.ShowCenterTips("错误：加载人物眼睛prefab失败", 50);
        return m_eyes[gender];
    }
}
public class HeroBaseInfo
{
    public float m_lifeMax = 100;//生命上限
    public float m_powerMax = 50;//气槽上限
    public float m_attackVal = 10;//攻击力
    public float m_defenceVal = 5;//防御力
    public float m_attackDist = 1f;
    public int m_juggle = 15;//空中连击点数
    public int m_hitFireNo = 0;//打击火花号
    public int m_defenceFireNo = 0;//防御火花号
    public bool m_KOecho = true;//KO回音
    public float m_volume = 1;//角色音量
    public float m_walkFowardSpeed = 1;//向前走速度
    public float m_walkBackSpeed = 1;//向后走速度
    public float m_runForwardSpeed = 2;//向前跑速度
    public float[] m_dodgeBackSpeed = new float[3] { 1, 1, 0 };//后小跳速度
    public float[] m_standJumpSpeed = new float[3] { 1, 1, 0 };//站立垂直跳速度
    public float[] m_standJumpBackSpeed = new float[3] { 1, 1, 0 };//站立后跳速度
    public float[] m_standJumpForwardSpeed = new float[3] { 1, 1, 0 };//站立前跳速度
    public float[] m_runJumpBackSpeed = new float[3] { 1, 1, 0 };//跑步后跳速度
    public float[] m_runJumpForwardSpeed = new float[3] { 1, 1, 0 };//跑步前跳速度
    public bool m_airJump = true;//是否可二段跳
    public float[] m_airJumpSpeed = new float[3] { 1, 1, 0 };//空中垂直跳速度
    public float[] m_airJumpBackSpeed = new float[3] { 1, 1, 0 };//空中后跳速度
    public float[] m_airJumpForwardSpeed = new float[3] { 1, 1, 0 };//空中前跳速度
    public float m_gravity = 2;//重力加速度
    public float m_standFriction = 1;//站立摩擦力
    public float m_crouchFriction = 1;//蹲下摩擦力
    public void Read(BinaryReader br, int versionNo)
    {
        m_lifeMax = br.ReadSingle();
        m_powerMax = br.ReadSingle();
        m_attackVal = br.ReadSingle();
        m_defenceVal = br.ReadSingle();
        if (versionNo >= 202204070) m_attackDist = br.ReadSingle();
        m_juggle = br.ReadInt32();
        m_hitFireNo = br.ReadInt32();
        m_defenceFireNo = br.ReadInt32();
        m_KOecho = br.ReadBoolean();
        m_volume = br.ReadSingle();
        m_walkFowardSpeed = br.ReadSingle();
        m_walkBackSpeed = br.ReadSingle();
        m_runForwardSpeed = br.ReadSingle();
        m_dodgeBackSpeed[0] = br.ReadSingle();
        m_dodgeBackSpeed[1] = br.ReadSingle();
        m_standJumpSpeed[0] = br.ReadSingle();
        m_standJumpSpeed[1] = br.ReadSingle();
        m_standJumpBackSpeed[0] = br.ReadSingle();
        m_standJumpBackSpeed[1] = br.ReadSingle();
        m_standJumpForwardSpeed[0] = br.ReadSingle();
        m_standJumpForwardSpeed[1] = br.ReadSingle();
        m_runJumpBackSpeed[0] = br.ReadSingle();
        m_runJumpBackSpeed[1] = br.ReadSingle();
        m_runJumpForwardSpeed[0] = br.ReadSingle();
        m_runJumpForwardSpeed[1] = br.ReadSingle();
        m_airJump = br.ReadBoolean();
        m_airJumpSpeed[0] = br.ReadSingle();
        m_airJumpSpeed[1] = br.ReadSingle();
        m_airJumpBackSpeed[0] = br.ReadSingle();
        m_airJumpBackSpeed[1] = br.ReadSingle();
        m_airJumpForwardSpeed[0] = br.ReadSingle();
        m_airJumpForwardSpeed[1] = br.ReadSingle();
        m_gravity = br.ReadSingle();
        m_standFriction = br.ReadSingle();
        m_crouchFriction = br.ReadSingle();
    }
    public void Save(BinaryWriter bw)
    {
        bw.Write(m_lifeMax);
        bw.Write(m_powerMax);
        bw.Write(m_attackVal);
        bw.Write(m_defenceVal);
        bw.Write(m_attackDist);
        bw.Write(m_juggle);
        bw.Write(m_hitFireNo);
        bw.Write(m_defenceFireNo);
        bw.Write(m_KOecho);
        bw.Write(m_volume);
        bw.Write(m_walkFowardSpeed);
        bw.Write(m_walkBackSpeed);
        bw.Write(m_runForwardSpeed);
        bw.Write(m_dodgeBackSpeed[0]);
        bw.Write(m_dodgeBackSpeed[1]);
        bw.Write(m_standJumpSpeed[0]);
        bw.Write(m_standJumpSpeed[1]);
        bw.Write(m_standJumpBackSpeed[0]);
        bw.Write(m_standJumpBackSpeed[1]);
        bw.Write(m_standJumpForwardSpeed[0]);
        bw.Write(m_standJumpForwardSpeed[1]);
        bw.Write(m_runJumpBackSpeed[0]);
        bw.Write(m_runJumpBackSpeed[1]);
        bw.Write(m_runJumpForwardSpeed[0]);
        bw.Write(m_runJumpForwardSpeed[1]);
        bw.Write(m_airJump);
        bw.Write(m_airJumpSpeed[0]);
        bw.Write(m_airJumpSpeed[1]);
        bw.Write(m_airJumpBackSpeed[0]);
        bw.Write(m_airJumpBackSpeed[1]);
        bw.Write(m_airJumpForwardSpeed[0]);
        bw.Write(m_airJumpForwardSpeed[1]);
        bw.Write(m_gravity);
        bw.Write(m_standFriction);
        bw.Write(m_crouchFriction);
    }
}
public class Hero
{
    public int m_id;
    public TextInfo m_name = new TextInfo();//名
    public TextInfo m_description = new TextInfo();//描述
    public HeroBaseInfo m_baseInfo = new HeroBaseInfo();
    public ModelInfo m_modelInfo = new ModelInfo();
    public HeroGender m_gender;
    public Dictionary<int, HeroStateDef> m_dicStateDef = new Dictionary<int, HeroStateDef>();//存储角色所有状态
    public List<HeroStateDef> m_listBaseState = new List<HeroStateDef>();//基本状态列表
    public List<HeroStateDef> m_listBaseAttack = new List<HeroStateDef>();//基本招式列表
    public List<HeroStateDef> m_listAdvanceAttack = new List<HeroStateDef>();//必杀技列表
    public List<HeroStateDef> m_listSkill = new List<HeroStateDef>();//大招列表
    public List<HeroStateDef> m_listOtherState = new List<HeroStateDef>();//其它状态列表
    public List<HeroState> m_listOperationState = new List<HeroState>();//操控列表
    public List<HeroState> m_listAIState = new List<HeroState>();//AI列表
    public List<Command> m_listCommand = new List<Command>();//指令列表
    public Dictionary<int, Command> m_dicCommand = new Dictionary<int, Command>();//指令，方便查找

    public static void InitAll()
    {
        try
        {
            string filePath = Application.streamingAssetsPath + "/Hero.dat";
            if (File.Exists(filePath))
            {
                BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
                if (br == null) return;
                int versionNo = br.ReadInt32();
                int heroNum = br.ReadInt32();
                for (int i = 0; i < heroNum; i++)
                {
                    Hero hero = new Hero();
                    hero.Read(br, versionNo);
                    hero.InitFromModelData();
                    GlobalAssist.m_listHero.Add(hero);
                    GlobalAssist.m_dicHero.Add(hero.m_id, hero);
                }
                br.Close();
            }
        }
        catch(Exception exc)
        {
            GlobalAssist.ShowCenterTips(exc.ToString(), 50);
        }

        if (GlobalAssist.m_listHero.Count == 0)
        {
            Hero hero = new Hero();
            hero.m_id = GlobalAssist.GetNewHeroID();
            hero.InitFromModelData();
            GlobalAssist.m_listHero.Add(hero);
            GlobalAssist.m_dicHero.Add(hero.m_id, hero);
        }
    }
    public static void SaveAll()
    {
        string filePath = Application.streamingAssetsPath + "/Hero.dat";
        BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create));
        bw.Write(GlobalAssist.m_versionNo);//版本号
        bw.Write(GlobalAssist.m_listHero.Count);
        for (int i = 0; i < GlobalAssist.m_listHero.Count; i++) GlobalAssist.m_listHero[i].Save(bw);
        bw.Close();
    }
    public void Read(BinaryReader br, int versionNo)
    {
        m_id = br.ReadInt32();
        m_name.Read(br, versionNo);
        m_description.Read(br, versionNo);
        m_baseInfo.Read(br, versionNo);
        m_gender = (HeroGender)br.ReadInt32();
        m_dicStateDef.Clear();
        m_listBaseState.Clear();
        m_listOperationState.Clear();
        m_listBaseAttack.Clear();
        m_listAdvanceAttack.Clear();
        m_listSkill.Clear();
        m_listOtherState.Clear();
        m_listCommand.Clear();
        m_dicCommand.Clear();
        int baseStateCount = br.ReadInt32();
        for (int i = 0; i < baseStateCount; i++)
        {
            HeroStateDef hsd = new HeroStateDef();
            hsd.Read(br, versionNo);
            m_listBaseState.Add(hsd);
            m_dicStateDef.Add(hsd.m_stateNo, hsd);
        }
        int baseAttackCount = br.ReadInt32();
        for (int i = 0; i < baseAttackCount; i++)
        {
            HeroStateDef hsd = new HeroStateDef();
            hsd.Read(br, versionNo);
            m_listBaseAttack.Add(hsd);
            m_dicStateDef.Add(hsd.m_stateNo, hsd);
        }
        int advanceAttackCount = br.ReadInt32();
        for (int i = 0; i < advanceAttackCount; i++)
        {
            HeroStateDef hsd = new HeroStateDef();
            hsd.Read(br, versionNo);
            m_listAdvanceAttack.Add(hsd);
            m_dicStateDef.Add(hsd.m_stateNo, hsd);
        }
        int skillCount = br.ReadInt32();
        for (int i = 0; i < skillCount; i++)
        {
            HeroStateDef hsd = new HeroStateDef();
            hsd.Read(br, versionNo);
            m_listSkill.Add(hsd);
            m_dicStateDef.Add(hsd.m_stateNo, hsd);
        }
        int otherStateCount = br.ReadInt32();
        for (int i = 0; i < otherStateCount; i++)
        {
            HeroStateDef hsd = new HeroStateDef();
            hsd.Read(br, versionNo);
            m_listOtherState.Add(hsd);
            m_dicStateDef.Add(hsd.m_stateNo, hsd);
        }
        int controlStateCount = br.ReadInt32();
        for (int i = 0; i < controlStateCount; i++)
        {
            HeroState state = new HeroState();
            state.Read(br, versionNo);
            m_listOperationState.Add(state);
        }
        int aiStateCount = br.ReadInt32();
        for (int i = 0; i < aiStateCount; i++)
        {
            HeroState state = new HeroState();
            state.Read(br, versionNo);
            m_listAIState.Add(state);
        }
        int commandCount = br.ReadInt32();
        for (int i = 0; i < commandCount; i++)
        {
            Command cmd = new Command();
            cmd.Read(br, versionNo);
            m_listCommand.Add(cmd);
            m_dicCommand.Add(cmd.m_commandNo, cmd);
        }
    }
    public void Save(BinaryWriter bw)
    {
        bw.Write(m_id);
        m_name.Save(bw);
        m_description.Save(bw);
        m_baseInfo.Save(bw);
        bw.Write((int)m_gender);
        bw.Write(m_listBaseState.Count);
        for (int i = 0; i < m_listBaseState.Count; i++) m_listBaseState[i].Save(bw);
        bw.Write(m_listBaseAttack.Count);
        for (int i = 0; i < m_listBaseAttack.Count; i++) m_listBaseAttack[i].Save(bw);
        bw.Write(m_listAdvanceAttack.Count);
        for (int i = 0; i < m_listAdvanceAttack.Count; i++) m_listAdvanceAttack[i].Save(bw);
        bw.Write(m_listSkill.Count);
        for (int i = 0; i < m_listSkill.Count; i++) m_listSkill[i].Save(bw);
        bw.Write(m_listOtherState.Count);
        for (int i = 0; i < m_listOtherState.Count; i++) m_listOtherState[i].Save(bw);
        bw.Write(m_listOperationState.Count);
        for (int i = 0; i < m_listOperationState.Count; i++) m_listOperationState[i].Save(bw);
        bw.Write(m_listAIState.Count);
        for (int i = 0; i < m_listAIState.Count; i++) m_listAIState[i].Save(bw);
        bw.Write(m_listCommand.Count);
        for (int i = 0; i < m_listCommand.Count; i++) m_listCommand[i].Save(bw);
    }
    public void InitFromModelData()//初始化模型信息
    {
        //读取之前先清空所有信息，给编辑器的还原操作用
        m_modelInfo.Clear();
        string filePath = Application.streamingAssetsPath + "/NPCModel/" + m_id + ".dat";
        if (!File.Exists(filePath)) return;
        BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
        if (br == null) return;
        int versionNo = br.ReadInt32();
        m_modelInfo.ReadFromBinary(br, versionNo);
        br.Close();
    }
    public string GetName()
    {
        return m_name.GetStr();
    }
    public Sprite GetIcon()
    {
        string filePath = Application.streamingAssetsPath + "/HeroIcon/" + m_id + ".png";
        if (!File.Exists(filePath)) return null;
        FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        fs.Seek(0, SeekOrigin.Begin);
        byte[] bytes = new byte[fs.Length];
        fs.Read(bytes, 0, (int)fs.Length);
        fs.Close();
        fs.Dispose();
        Texture2D texture = new Texture2D(260, 340);
        texture.LoadImage(bytes);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        return sprite;
    }
    public int GetNewComandNo()
    {
        int id = 101;
        while (m_dicCommand.ContainsKey(id)) id++;
        return id;
    }
}
