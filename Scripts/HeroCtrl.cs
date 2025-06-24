using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using MagicaCloth;
using System;
using System.IO;


//角色控制类，挂载在角色上
public class HeroCtrl : MonoBehaviour
{
    public int m_id;
    public Transform m_avatarTransform;
    public Transform m_headTransform = null;
    public Transform m_neckTransform;
    public Transform m_hipTransform;
    public Transform m_shoulderTransform;
    public Transform[] m_thighTransform = new Transform[2];
    public Transform[] m_shankTransform = new Transform[2];
    public Transform[] m_eyes = new Transform[2];
    public MagicaCapsuleCollider[] m_thighCollider = new MagicaCapsuleCollider[2];
    public MagicaCapsuleCollider[] m_shankCollider = new MagicaCapsuleCollider[2];
    public Transform m_planeColliderTransform;

    public List<ColliderComponent> m_upperColliderList = new List<ColliderComponent>();
    public List<ColliderComponent> m_lowerColliderList = new List<ColliderComponent>();
    public List<ColliderComponent> m_totalColliderList = new List<ColliderComponent>();
    public List<ColliderComponent> m_totalColliderList2 = new List<ColliderComponent>();
    public List<ColliderComponent> m_robeColliderList = new List<ColliderComponent>();//罩袍
    [HideInInspector] public float m_thighStartRadius = 1;
    [HideInInspector] public float m_thighEndRadius = 1;
    [HideInInspector] public float m_shankStartRadius = 1;
    [HideInInspector] public float m_shankEndRadius = 1;
    [HideInInspector] public Vector3[] m_baseEyePos = new Vector3[2];//眼睛的初始局部坐标
    [HideInInspector] public Hero m_hero;//该模型表示的武将
    [HideInInspector] public Transform m_transform = null;
    [HideInInspector] public Animator m_animator = null;
    [HideInInspector] public AnimatorOverrideController m_overrideController = null;
    [HideInInspector] public MagicaAvatar m_avatar = null;
    [HideInInspector] public Dictionary<string, Eq_PartLink> m_dicPart = new Dictionary<string, Eq_PartLink>();
    [HideInInspector] public List<SkinnedMeshRenderer> m_listMeshRenderer = new List<SkinnedMeshRenderer>();//用来将MeshRenderer传递给LOD，残影也用这个
    [HideInInspector] public int m_beHitIndex = 0;//用于判断受击动画用Attack1还是Attack2播放
    [HideInInspector] public MagicaBoneSpring m_boneSpring = null;
    [HideInInspector] public int m_prevState = 0;//前一个状态号
    [HideInInspector] public HeroStateDef m_curStateDef = null;//当前的statedef
    [HideInInspector] public int m_side = 0;//当前模型所属，0-左边，1-右边
    //一些运行时变量，给触发器等用
    [HideInInspector] public bool m_canControl = true;//当前是否可控制
    [HideInInspector] public StateDefType m_stateDefType = StateDefType.Stand;
    [HideInInspector] public StateDefMoveType m_stateDefMoveType = StateDefMoveType.Idle;
    [HideInInspector] public StateDefPhysicType m_stateDefPhysicType = StateDefPhysicType.None;
    [HideInInspector] public float m_life = 0;
    [HideInInspector] public float m_power = 0;
    [HideInInspector] public float m_curStateTime = 0;//在当前状态的时间，每次设置新StateDef时归零
    [HideInInspector] public Vector3 m_velocity = Vector3.zero;//当前速度
    [HideInInspector] public float m_defenceMulSet = 1;
    [HideInInspector] public float m_attackMulSet = 1;
    [HideInInspector] public float m_hitLeftTime = 0;//受击后停留在受击状态的剩余时间
    [HideInInspector] public bool m_isSliding = false;//是否收到攻击后在滑动
    [HideInInspector] public float m_slideLeftTime = 0;//剩余滑动时间
    [HideInInspector] public bool m_ignoreCollider = false;//是否关闭碰撞检测
    [HideInInspector] public int m_hitCount = 0;//当前状态已击中对方次数
    [HideInInspector] public int m_guardCount = 0;//当前状态已被对方防御住次数
    //传递HitDef的一些参数
    [HideInInspector] public int m_typeHitdef = 0;
    [HideInInspector] public int m_animtypeHitdef = 0;
    [HideInInspector] public int m_airtypeHitdef = 0;
    [HideInInspector] public int m_damageHitdef = 0;
    [HideInInspector] public float m_hitshaketimeHitdef = 0;
    [HideInInspector] public float m_hittimeHitdef = 0;
    [HideInInspector] public float m_slidetimeHitdef = 0;
    [HideInInspector] public float m_xvelHitdef = 0;
    [HideInInspector] public float m_yvelHitdef = 0;
    [HideInInspector] public float m_zvelHitdef = 0;
    [HideInInspector] public float m_yaccelHitdef = 0;
    ///////////////////////////////////////////////////////////////////////
    [HideInInspector] public int m_curAnimationIndex = 0;
    [HideInInspector] public float m_curAnimationTime = 0;
    [HideInInspector] public List<int> m_listMyCollider = new List<int>();//临时记录和对方相碰撞的我方碰撞体，将枚举值转为int，每帧判断完hitdef后清空列表
    [HideInInspector] public List<int> m_listOtherCollider = new List<int>();//临时记录和对方相碰撞的对手碰撞体，将枚举值转为int

    [HideInInspector] public int[] m_valueInt = new int[50];//自定义设置的整数变量，给控制器和触发器用
    [HideInInspector] public float[] m_valueFloat = new float[50];//自定义设置的浮点数变量，给控制器和触发器用
    private List<HeroAnimation> m_listAnimation = new List<HeroAnimation>();
    private float m_fadeTime = 0.1f;
    private bool m_animRecycle = false;
    private int m_curLayer = 0;
    private float m_afterImageNum = 0;//残影的剩余数量
    private Color m_afterImageColor;//残影颜色
    private float m_afterImageStayDuration = 0;//残影的残留时间
    private float m_afterImageCountTime = 0;//计时器，到时间生成残影
    private float m_afterImageInterval = 0;//残影的生成间隔时间
    [HideInInspector] public float m_pauseLeftTime = 0;//剩余停顿时间
    private Vector3 m_pausePosition;//停顿时的坐标，确保停顿过程不变
    private float m_pauseAnimSpeed = 1;//停顿前的动画速度，停顿结束时恢复
    private float m_tmpPauseDuration = 0;//当有延迟时，临时传递停顿时长
    [HideInInspector] public float m_boundLeftTime = 0;//被绑定的剩余时间
    private Vector3 m_boundPos;//绑定的相对坐标
    private Transform m_transformBound;//对手的Transform
    private static List<string> m_tmpListStr = new List<string>();

    public static Dictionary<int, List<HeroCtrl>> m_dicListHeroPool = new Dictionary<int, List<HeroCtrl>>();//对象池

    public static void RefreshModel(int heroID)//更新某个角色的模型
    {
        if (m_dicListHeroPool.ContainsKey(heroID))
        {
            foreach (HeroCtrl heroCtrl in m_dicListHeroPool[heroID]) heroCtrl.RefreshModel();
        }
    }

    public static HeroCtrl GetOne(int heroID)
    {
        if (!GlobalAssist.m_dicHero.ContainsKey(heroID))
        {
            GlobalAssist.ShowCenterTips("无法获取NPC模型，错误ID<" + heroID + ">", 50);
            return null;
        }
        Hero hero = GlobalAssist.m_dicHero[heroID];
        if (!m_dicListHeroPool.ContainsKey(heroID)) m_dicListHeroPool.Add(heroID, new List<HeroCtrl>());
        List<HeroCtrl> listHeroCtrl = m_dicListHeroPool[heroID];
        HeroCtrl npcCtrl = null;
        if (listHeroCtrl.Count > 0) 
        {
            npcCtrl = listHeroCtrl[0];
            npcCtrl.gameObject.SetActive(true);
            listHeroCtrl.RemoveAt(0);
        }
        else
        {
            npcCtrl = GenOne(hero);
        }
        npcCtrl.m_animator.speed = 1;
        npcCtrl.m_curStateDef = null;
        npcCtrl.InitValue(hero);
        return npcCtrl;
    }
    static HeroCtrl GenOne(Hero hero)
    {
        GameObject obj;
        if (hero.m_gender == HeroGender.Male) obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/MaleAvatar.prefab");
        else obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/Hero_Prefab/FemaleAvatar.prefab");
        obj = GameObject.Instantiate(obj);
        HeroCtrl npcCtrl = obj.GetComponent<HeroCtrl>();
        npcCtrl.m_avatar = obj.GetComponentInChildren<MagicaAvatar>();
        DontDestroyOnLoad(obj);
        npcCtrl.Init(hero);
        npcCtrl.RefreshModel();
        if (npcCtrl.m_boneSpring != null)
        {
            npcCtrl.m_boneSpring.Init();
            if (!npcCtrl.m_boneSpring.SetSpringPower(hero.m_modelInfo.m_springPower))
            {
                GlobalAssist.ShowCenterTips(hero.GetName() + "更新胸部弹力失败", 20);
            }
        }
        return npcCtrl;
    }
    public void Recycle()
    {
        if (!gameObject.activeSelf) return;//避免重复回收
        CancelInvoke();
        m_animator.WriteDefaultValues();
        SetPosition(new Vector3(0, -800, 0));//移动到看不到的地方，隐藏
        gameObject.SetActive(false);
        m_dicListHeroPool[m_id].Add(this);
    }
    void InitValue(Hero hero)
    {
        m_life = hero.m_baseInfo.m_lifeMax;
        m_power = 0;
        m_curLayer = 0;
        for (int i = 0; i < 50; i++)
        {
            m_valueInt[i] = 0;
            m_valueFloat[i] = 0;
        }
        m_typeHitdef = 0;
        m_animtypeHitdef = 0;
        m_airtypeHitdef = 0;
        m_damageHitdef = 0;
        m_hitshaketimeHitdef = 0;
        m_hittimeHitdef = 0;
        m_slidetimeHitdef = 0;
        m_xvelHitdef = 0;
        m_yvelHitdef = 0;
        m_zvelHitdef = 0;
        m_yaccelHitdef = 0;
    }
    // Start is called before the first frame update

    void OnEnable()
    {
        InitComponent();
        if (GlobalAssist.m_dicHero.ContainsKey(m_id))
        {
            m_hero = GlobalAssist.m_dicHero[m_id];
        }
    }
    public void InitComponent()//放在OnEnale执行而不放在Init执行，因为OnEnale先执行
    {
        if (m_animator == null)
        {
            m_animator = GetComponent<Animator>();
            if (m_animator == null) m_animator = GetComponentInChildren<Animator>();
            if (m_animator != null)
            {
                m_overrideController = new AnimatorOverrideController();
                m_overrideController.runtimeAnimatorController = m_animator.runtimeAnimatorController;
                m_animator.runtimeAnimatorController = m_overrideController;
            }
            m_transform = transform;
            m_boneSpring = GetComponentInChildren<MagicaBoneSpring>();
        }
    }
    public void Init(Hero hero)
    {
        m_id = hero.m_id;
        m_hero = hero;
        if (m_thighCollider[0] != null)
        {
            m_thighStartRadius = m_thighCollider[0].StartRadius;//记录初始的腿部碰撞体半径
            m_thighEndRadius = m_thighCollider[0].EndRadius;
            m_shankStartRadius = m_shankCollider[0].StartRadius;
            m_shankEndRadius = m_shankCollider[0].EndRadius;
        }
        for (int i = 0; i < 2; i++)
        {
            if (m_eyes[i] != null) m_baseEyePos[i] = m_eyes[i].localPosition;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_curStateTime += Time.deltaTime;
        if (m_hitLeftTime >= 0) m_hitLeftTime -= Time.deltaTime;
        if (m_pauseLeftTime > 0)//停顿中
        {
            m_pauseLeftTime -= Time.deltaTime;
            m_transform.position = m_pausePosition;
            if (m_pauseLeftTime <= 0)
            {
                m_animator.speed = m_pauseAnimSpeed;
            }
            return;//停顿过程中不执行下面代码
        }
        if (m_boundLeftTime > 0)//绑定中
        {
            m_boundLeftTime -= Time.deltaTime;
            m_transform.position = m_transformBound.position + m_transformBound.forward * m_boundPos.x + m_transformBound.right * m_boundPos.z + m_transformBound.up * m_boundPos.y;
            return;
        }
        if (m_isSliding)
        {
            m_slideLeftTime -= Time.deltaTime;
            if (m_slideLeftTime <= 0)
            {
                m_isSliding = false;
                m_velocity = Vector3.zero;
            }
        }
        if (m_transform.position.y > 0 || m_velocity.y > 0) 
        {
            float posY = m_transform.position.y + m_velocity.y * Time.deltaTime;
            if (m_velocity.y > -100 && m_curStateDef != null) m_velocity.y -= m_curStateDef.m_gravity * Time.deltaTime;
            if (posY > 3) posY = 3;
            if (posY <= 0)
            {
                posY = 0;
                m_velocity.y = 0;
            }
            m_transform.position = new Vector3(m_transform.position.x, posY, m_transform.position.z);
        }
        UpdateAnimation();
        if (m_afterImageNum > 0)
        {
            m_afterImageCountTime += Time.deltaTime;
            if (m_afterImageCountTime >= m_afterImageInterval)
            {
                GhostCtrl.GenGhost(m_listMeshRenderer, m_afterImageColor, m_afterImageStayDuration);
                m_afterImageCountTime = 0;
                m_afterImageNum--;
            }
        }
    }

    public void TakeOnBodyPart(GameObject modelPart, List<PartMatInfo> listMatInfo)
    {
        if (m_avatar == null) return;
        ModelInfo modelInfo = m_hero.m_modelInfo;
        GameObject modelWearing = m_avatar.AttachAvatarPartsCustom(modelPart);
        modelWearing.SetActive(true);
        Eq_PartLink partLink = modelWearing.GetComponent<Eq_PartLink>();
        m_dicPart.Add(modelPart.name, partLink);
        m_listMeshRenderer.Add(partLink.smr);
        if (partLink.partType == PartType.HeadBeardEyebrow || partLink.partType == PartType.Hair || partLink.partType == PartType.HairBall)
        {
            foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
            {
                int blendShapeIndex = partLink.smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    GlobalAssist.ShowCenterTips(partLink.gameObject.name + "找不到变形器<" + blendShapeName + ">");
                    continue;
                }
                partLink.smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
            }
        }
        else if (partLink.partType == PartType.Eye)
        {
            foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
            {
                if (NPCModel.m_listEyeExcludeBlendShape[(int)m_hero.m_gender].Contains(blendShapeName)) continue;
                int blendShapeIndex = partLink.smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    GlobalAssist.ShowCenterTips("眼睛找不到变形器<" + blendShapeName + ">");
                    continue;
                }
                partLink.smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
            }
        }
        else
        {
            foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
            {
                if (!NPCModel.m_listBodyBlendShape[(int)m_hero.m_gender].Contains(blendShapeName)) continue;
                int blendShapeIndex = partLink.smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    GlobalAssist.ShowCenterTips("找不到变形器<" + blendShapeName + ">");
                    continue;
                }
                float blendShapeFactor = modelInfo.m_dicBlendShapeFactor[blendShapeName];
                partLink.smr.SetBlendShapeWeight(blendShapeIndex, blendShapeFactor);
            }
        }
        if (listMatInfo != null)
        {
            Material[] materials = partLink.smr.materials;
            for (int matIndex = 0; matIndex < materials.Length; matIndex++)
            {
                if (matIndex >= listMatInfo.Count) break;
                if (GlobalAssist.m_dicMaterialPath.ContainsKey(listMatInfo[matIndex].m_matName))
                {
                    materials[matIndex] = GlobalAssist.GetMaterial(listMatInfo[matIndex].m_matName);
                }
            }
            partLink.smr.materials = materials;
            for (int matIndex = 0; matIndex < partLink.smr.materials.Length; matIndex++)//颜色在后面赋值，避免把材质球源文件的颜色改了
            {
                if (matIndex >= listMatInfo.Count) break;
                partLink.smr.materials[matIndex].color = listMatInfo[matIndex].m_color;
            }
        }
        if (partLink.physicType == PhysicType.NoPhysics) return;
        else if (partLink.physicType == PhysicType.Upper)
        {
            foreach (var tempCollider in m_upperColliderList)
            {
                partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
            }
        }
        else if (partLink.physicType == PhysicType.Lower)
        {
            foreach (var tempCollider in m_lowerColliderList)
            {
                partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
            }
        }
        else if (partLink.physicType == PhysicType.Cape)
        {
            foreach (var tempCollider in m_totalColliderList)
            {
                partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
            }
        }
        else if (partLink.physicType == PhysicType.Total)
        {
            foreach (var tempCollider in m_totalColliderList2)
            {
                partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
            }
        }
        else if (partLink.physicType == PhysicType.Robe)
        {
            foreach (var tempCollider in m_robeColliderList)
            {
                partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
            }
        }
    }
    public void TakeOnEquipmentPart(string partName, Dictionary<string, List<PartMatInfo>> dicPartListColor)
    {
        if (m_dicPart.ContainsKey(partName) || m_avatar == null) return;
        ModelInfo modelInfo = m_hero.m_modelInfo;
        GameObject modelPart = GameAssets.LoadAsset<GameObject>("Assets/Resource/" + partName + ".prefab");
        if (modelPart == null)
        {
            GlobalAssist.ShowCenterTips("模型组件不存在" + partName, 10);
            modelInfo.m_listPart.Remove(partName);
            return;
        }
        Eq_PartLink prefabPartLink = modelPart.GetComponent<Eq_PartLink>();
        if (prefabPartLink.meshCloth != null && prefabPartLink.meshCloth.VerifyData() != Define.Error.None)
        {
            GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">的物理组件有错误", 10);
            return;
        }
        if (prefabPartLink.smr == null || prefabPartLink.smr.sharedMesh == null)
        {
            GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">的smr有错误", 3);
            return;
        }
        GameObject modelWearing = m_avatar.AttachAvatarPartsCustom(modelPart);
        modelWearing.SetActive(true);
        Eq_PartLink partLink = modelWearing.GetComponent<Eq_PartLink>();
        m_dicPart.Add(partName, partLink);
        m_listMeshRenderer.Add(partLink.smr);
        List<PartMatInfo> listMatInfo = null;
        if (dicPartListColor.ContainsKey(partName)) listMatInfo = dicPartListColor[partName];
        if (partLink.partType == PartType.HeadBeardEyebrow || partLink.partType == PartType.Hair || partLink.partType == PartType.HairBall)
        {
            foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
            {
                int blendShapeIndex = partLink.smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    GlobalAssist.ShowCenterTips(partLink.gameObject.name + "找不到变形器<" + blendShapeName + ">");
                    continue;
                }
                partLink.smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
            }
        }
        else if (partLink.partType == PartType.Eyelash)
        {
            foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
            {
                if (NPCModel.m_listEyelashExcludeBlendShape[(int)m_hero.m_gender].Contains(blendShapeName)) continue;
                int blendShapeIndex = partLink.smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    GlobalAssist.ShowCenterTips("眼睫毛找不到变形器<" + blendShapeName + ">");
                    continue;
                }
                partLink.smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
            }
        }
        else
        {
            foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
            {
                if (!NPCModel.m_listBodyBlendShape[(int)m_hero.m_gender].Contains(blendShapeName)) continue;
                int blendShapeIndex = partLink.smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    GlobalAssist.ShowCenterTips("找不到变形器<" + blendShapeName + ">");
                    continue;
                }
                float blendShapeFactor = modelInfo.m_dicBlendShapeFactor[blendShapeName];
                partLink.smr.SetBlendShapeWeight(blendShapeIndex, blendShapeFactor);
            }
        }
        if (listMatInfo != null)
        {
            Material[] materials = partLink.smr.materials;
            Texture[] normalTex = new Texture[partLink.smr.materials.Length];
            Texture[] normalTexD = new Texture[partLink.smr.materials.Length];
            for (int matIndex = 0; matIndex < materials.Length; matIndex++)
            {
                if (materials[matIndex].HasProperty("_DetailNormalMap")) normalTexD[matIndex] = materials[matIndex].GetTexture("_DetailNormalMap");
                if (matIndex >= listMatInfo.Count) break;
                if (GlobalAssist.m_dicMaterialPath.ContainsKey(listMatInfo[matIndex].m_matName))
                {
                    materials[matIndex] = GlobalAssist.GetMaterial(listMatInfo[matIndex].m_matName);
                }
            }
            partLink.smr.materials = materials;
            for (int matIndex = 0; matIndex < partLink.smr.materials.Length; matIndex++)//颜色在后面赋值，避免把材质球源文件的颜色改了
            {
                if (matIndex >= listMatInfo.Count) break;
                PartMatInfo matInfo = listMatInfo[matIndex];
                partLink.smr.materials[matIndex].color = matInfo.m_color;
                if (matInfo.m_roughness >= 0 && partLink.smr.materials[matIndex].HasProperty("_Glossiness")) partLink.smr.materials[matIndex].SetFloat("_Glossiness", matInfo.m_roughness);
                if (matInfo.m_metalic >= 0 && partLink.smr.materials[matIndex].HasProperty("_Metallic")) partLink.smr.materials[matIndex].SetFloat("_Metallic", matInfo.m_metalic);
                if (matInfo.m_matName.Contains("BL_"))//带BL前缀的替换回原来的法线贴图
                {
                    if (partLink.smr.materials[matIndex].HasProperty("_DetailNormalMap")) partLink.smr.materials[matIndex].SetTexture("_DetailNormalMap", normalTexD[matIndex]);
                }
            }
        }
        if (partLink.meshCloth != null && partLink.meshCloth.TeamData.MergeAvatarCollider)
        {
            GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">勾选了MergeAvatarCollider", 10);
        }
        if (partLink.physicType == PhysicType.NoPhysics) return;
        else if (partLink.physicType == PhysicType.Upper)
        {
            if (partLink.meshCloth == null)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">没有物理组件，但是PhysicType却不是No Physics");
            }
            else
            {
                foreach (var tempCollider in m_upperColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
        }
        else if (partLink.physicType == PhysicType.Lower)
        {
            if (partLink.meshCloth == null)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">没有物理组件，但是PhysicType却不是No Physics");
            }
            else
            {
                foreach (var tempCollider in m_lowerColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
        }
        else if (partLink.physicType == PhysicType.Cape)
        {
            if (partLink.meshCloth == null)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">没有物理组件，但是PhysicType却不是No Physics");
            }
            else
            {
                foreach (var tempCollider in m_totalColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
        }
        else if (partLink.physicType == PhysicType.Total)
        {
            if (partLink.meshCloth == null)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">没有物理组件，但是PhysicType却不是No Physics");
            }
            else
            {
                foreach (var tempCollider in m_totalColliderList2)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
        }
        else if (partLink.physicType == PhysicType.Robe)
        {
            if (partLink.meshCloth == null)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">没有物理组件，但是PhysicType却不是No Physics");
            }
            else
            {
                foreach (var tempCollider in m_robeColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
        }
        partLink.meshCloth.DistanceDisable_Distance = 30;//默认20的距离太近，改成30米
        partLink.meshCloth.WorldInfluence_SetMovementInfluence(0.2f, 0, 0);//移动影响物理调小，不然穿模严重
        partLink.meshCloth.WorldInfluence_SetRotationInfluence(0.2f, 0, 0);//转动影响物理调小，不然可能穿模
    }

    public void TakeOffPart(string partName)
    {
        m_avatar.DetachAvatarParts(m_dicPart[partName].part);
    }
    public void TurnOnPhysic()
    {
        StartCoroutine(TurnOnPhysicCoroutine());
    }
    IEnumerator TurnOnPhysicCoroutine()
    {
        yield return null;
        foreach (Eq_PartLink partLink in m_dicPart.Values)
        {
            if (partLink.meshCloth != null) partLink.meshCloth.enabled = true;
        }
    }

    public void TurnOffPhysic(bool onlyLow)
    {
        StartCoroutine(TurnOffPhysicCoroutine(onlyLow));
    }
    IEnumerator TurnOffPhysicCoroutine(bool onlyLow)
    {
        yield return null;
        foreach (Eq_PartLink partLink in m_dicPart.Values)
        {
            if (onlyLow && (partLink.physicType == PhysicType.Upper || partLink.physicType == PhysicType.Cape)) continue;
            if (partLink.meshCloth != null) partLink.meshCloth.enabled = false;
        }
    }
    public void RefreshModel()
    {
        if (m_avatar == null) return;
//         try
//         {
            //清理原有的
            foreach (Eq_PartLink partLink in m_dicPart.Values) m_avatar.DetachAvatarParts(partLink.part);
            m_dicPart.Clear();
            m_listMeshRenderer.Clear();
            ModelInfo modelInfo = m_hero.m_modelInfo;
            //人物的配件
            for (int i = modelInfo.m_listPart.Count - 1; i >= 0; i--)
            {
                TakeOnEquipmentPart(modelInfo.m_listPart[i], modelInfo.m_dicPartListColor);
            }
            int bodyIndex = 0;
            //添加身体和眼睛
            TakeOnBodyPart(HeroBodyInfo.GetBody((int)m_hero.m_gender, bodyIndex), modelInfo.m_listBodyMatInfo);
            TakeOnBodyPart(HeroBodyInfo.GetEye((int)m_hero.m_gender), modelInfo.m_listEyeMatInfo);//大地图不加眼睛
            m_animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;//换完装备后动画不会更新，强制更新一下
            m_animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
//         }
//         catch (Exception exc)
//         {
//             GlobalAssist.ShowCenterTips("错误：武将<" + m_hero.GetName() + "><" + m_hero.m_id + ">执行RefreshModel()失败" + exc.Message, 50);
//         }
    }

    public void SetPosition(Vector3 pos, bool switchPhysic = true, bool moveToNavMesh = false)
    {
        if (switchPhysic)
        {
            foreach (Eq_PartLink partLink in m_dicPart.Values)
            {
                if (partLink.meshCloth != null) partLink.meshCloth.enabled = false;
            }
        }
        if (moveToNavMesh)
        {
            NavMeshHit navHit;
            if (NavMesh.SamplePosition(pos, out navHit, 10, NavMesh.AllAreas)) pos = navHit.position;
        }
        m_transform.position = pos;
        if (switchPhysic) TurnOnPhysic();
    }
    public void UpdateAnimatorSpeed(int layer)
    {
        if (m_animator.speed < 0.99f || m_animator.speed > 1.01f)
        {
            AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(layer);
            if (stateInfo.IsName("NullState") || stateInfo.IsName("Default")) m_animator.speed = 1;
        }
    }
    public void MoveToNavMesh()//有时候会被技能推到寻路网格外，此时应该强制移动回来
    {
        NavMeshHit navHit;
        if (NavMesh.SamplePosition(m_transform.position, out navHit, 3f, NavMesh.AllAreas))
        {
            if (Vector3.Distance(m_transform.position, navHit.position) > 0.01) SetPosition(navHit.position);//获取最近网格点
        }
    }
    public void StartAnimation(HeroStateDef hsd)
    {
        if (hsd.m_listAnimation.Count == 0)
        {
            //GlobalAssist.ShowCenterTips("错误：状态<" + hsd.m_name.GetStr() + ">的动画为空", 50);
            return;
        }
        m_listAnimation.Clear();
        m_listAnimation.AddRange(hsd.m_listAnimation);
        m_animRecycle = hsd.m_animRecycle;
        m_curAnimationIndex = 0;
        m_animator.speed = 1;
        //开始播放，此处只播放第一个攻击动画，后续的动画通过动画里的事件跳转
        HeroAnimation ba = m_listAnimation[0];
        if (!AnimationManager.m_dicAnimationClipPath.ContainsKey(ba.m_name))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + ba.m_name + ">不存在", 10);
            return;
        }
        AnimationClip animClip = AnimationManager.GetAnimationClip(ba.m_name);
        PlayAnimation(animClip, ba.m_fadeTime, ba.m_offTime);
    }
    void PlayAnimation(AnimationClip animClip, float fadeTime, float offTime)
    {
        m_curLayer++;
        if (m_curLayer > 5 || m_curLayer < 1) m_curLayer = 1;
        string animName = "Attack" + m_curLayer;
        //string layerName = "Layer" + m_curLayer;
        m_overrideController[animName] = animClip;
        //m_overrideController[layerName] = animClip;
        m_animator.CrossFade(animName, 0, m_curLayer, offTime);
        m_fadeTime = fadeTime * 0.5f;
        if (m_fadeTime < 0.05) m_fadeTime = 0.05f;
        else if (m_fadeTime > 1) m_fadeTime = 1;
    }
    void UpdateAnimation()
    {
        float curWeight = m_animator.GetLayerWeight(m_curLayer);
        if (curWeight < 1)
        {
            curWeight += Time.deltaTime / m_fadeTime;
            if (curWeight > 1) curWeight = 1;
            m_animator.SetLayerWeight(m_curLayer, curWeight);
        }
        else
        {
            for (int i = 1; i < 6; i++)
            {
                if (i == m_curLayer) continue;
                float weight = m_animator.GetLayerWeight(i);
                if (weight > 0)
                {
                    weight -= Time.deltaTime / m_fadeTime;
                    if (weight < 0) weight = 0;
                    m_animator.SetLayerWeight(i, weight);
                }
            }
        }
        AnimatorStateInfo stateInfo = m_animator.GetCurrentAnimatorStateInfo(m_curLayer);
        if ((m_animator.speed < 0.99 || m_animator.speed > 1.01) && (stateInfo.IsName("Default") || stateInfo.IsName("NullState"))) m_animator.speed = 1;
        m_curAnimationTime = stateInfo.normalizedTime - (int)stateInfo.normalizedTime;
        if (m_curAnimationIndex < m_listAnimation.Count && m_listAnimation[m_curAnimationIndex].m_endTime <= m_curAnimationTime)
        {
            if (m_curAnimationIndex + 1 < m_listAnimation.Count)
            {
                HeroAnimation ba = m_listAnimation[m_curAnimationIndex + 1];
                if (!AnimationManager.m_dicAnimationClipPath.ContainsKey(ba.m_name))
                {
                    GlobalAssist.ShowCenterTips("错误：动画<" + ba.m_name + ">不存在", 10);
                    return;
                }
                AnimationClip animClip = AnimationManager.GetAnimationClip(ba.m_name);
                animClip.events = null;
                PlayAnimation(animClip, ba.m_fadeTime, ba.m_offTime);
            }
            else
            {
                if (m_animRecycle)
                {
                    HeroAnimation ba = m_listAnimation[0];
                    AnimationClip animClip = AnimationManager.GetAnimationClip(ba.m_name);
                    PlayAnimation(animClip, ba.m_fadeTime, ba.m_offTime);
                    m_curAnimationIndex = -1;
                }
                else
                {
                    m_animator.Update(0);
                    m_animator.CrossFade("Default", m_fadeTime, m_curLayer);
                }
            }
            m_curAnimationIndex = m_curAnimationIndex + 1;
        }
    }
    public void SetStateDef(HeroStateDef hsd, float gravity = -1)
    {
        if (m_curStateDef == hsd) return;
        if (m_curStateDef != null)
        {
            if (m_curStateDef.m_stateNo >= 120 && m_curStateDef.m_stateNo <= 155 && hsd.m_stateNo == 120) return;//防御状态不重复跳转
            m_prevState = m_curStateDef.m_stateNo;
        }
        StartAnimation(hsd);
        m_curStateDef = hsd;
        if (hsd.m_controlType == StateDefControlType.UnControl) m_canControl = false;
        else if (hsd.m_controlType == StateDefControlType.Control) m_canControl = true;
        if (hsd.m_type != StateDefType.Unchanged) m_stateDefType = hsd.m_type;
        if (hsd.m_moveType != StateDefMoveType.Unchanged) m_stateDefMoveType = hsd.m_moveType;
        if (hsd.m_physicType != StateDefPhysicType.Unchanged) m_stateDefPhysicType = hsd.m_physicType;
        m_curStateTime = 0;
        m_curStateDef.m_gravity = gravity < 0 ? m_hero.m_baseInfo.m_gravity : gravity;
        foreach (HeroState state in hsd.m_listState)
        {
            state.m_hasExecute = false;//重置执行过的
            state.m_executeTime = -1;
        }
        m_hitCount = 0;
        m_guardCount = 0;
    }
    public float GetAttackDis()
    {
        if (m_curStateDef == null || m_curStateDef.m_moveType != StateDefMoveType.Attack) return 0;
        foreach (HeroState state in m_curStateDef.m_listState)
        {
            if (state.m_control.GetControlType() == ControlType.HitDef)
            {
                HSC_Hitdef control = (HSC_Hitdef)state.m_control;
                if (control.m_isGuardDist) return control.m_guardDist;
            }
        }
        return m_hero.m_baseInfo.m_attackDist;
    }
    public float GetFriction()
    {
        if (m_curStateDef == null) return 1;
        if (m_stateDefPhysicType == StateDefPhysicType.Stand) return m_hero.m_baseInfo.m_standFriction;
        else if (m_stateDefPhysicType == StateDefPhysicType.Crouch) return m_hero.m_baseInfo.m_crouchFriction;
        return 0;
    }
    public void StartAfterImage(int num, float interval, float stayDuration, Color color)
    {
        m_afterImageNum = num;
        m_afterImageColor = color;
        m_afterImageStayDuration = stayDuration;
        m_afterImageCountTime = m_afterImageInterval = interval;
    }
    public void StartPause(float duration, float delay)
    {
        m_tmpPauseDuration = duration;
        if (delay < 0.01f) ExecutePause();
        else Invoke("ExecutePause", delay);
    }
    void ExecutePause()
    {
        m_pauseLeftTime = m_tmpPauseDuration;
        m_pausePosition = m_transform.position;
        m_pauseAnimSpeed = m_animator.speed;
        m_animator.speed = 0;
    }
    public void StartBound(Transform transformBound, float duration, Vector3 boundPos)//开始被对方绑定
    {
        m_boundLeftTime = duration;
        m_boundPos = boundPos;
        m_transformBound = transformBound;
    }
}
