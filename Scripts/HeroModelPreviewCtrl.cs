using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MagicaCloth;

public class HeroModelPreviewCtrl : MonoBehaviour
{
    public Camera m_camera;
    public Camera m_cameraHead;
    public RenderTexture[] m_renderTexture = new RenderTexture[2];
    public MagicaAvatar[] m_avatar = new MagicaAvatar[2];
    public Eq_Avatar[] m_eqAvatar = new Eq_Avatar[2];
    public Animator[] m_animator = new Animator[2];
    public GameObject[] m_planeCollider = new GameObject[2];
    public MagicaBoneSpring m_boneSpring;
    public Transform[] m_transform = new Transform[2];
    private int m_modelIndex = 0;//0-左边，1-右边
    [HideInInspector] public AnimatorOverrideController[] m_overrideController = new AnimatorOverrideController[2];
    private List<Eq_PartLink> m_listWearingPart = new List<Eq_PartLink>();
    private int m_curAnimationIndex = 0;
    private List<HeroAnimation> m_listAnimation = new List<HeroAnimation>();
    private float m_fadeTime = 0.1f;
    private bool m_animRecycle = false;
    public static int[] m_curLayer = new int[2] { 0, 0 };
    public static float m_curTime = 0;
    public static float m_curProgress = 0;

    public static HeroModelPreviewCtrl[] m_modelCtrl = new HeroModelPreviewCtrl[2];

    public static void Init()
    {
        GameObject prefab = GameAssets.LoadAsset<GameObject>("Assets/Resource/MakeNPCModel.prefab");
        GameObject obj = GameObject.Instantiate(prefab);
        obj.transform.position = Vector3.zero;
        DontDestroyOnLoad(obj);
        m_modelCtrl[0] = obj.GetComponent<HeroModelPreviewCtrl>();
        m_modelCtrl[0].InitComponent(0);

        obj = GameObject.Instantiate(prefab);
        obj.transform.position = new Vector3(100, 0, 0);
        DontDestroyOnLoad(obj);
        m_modelCtrl[1] = obj.GetComponent<HeroModelPreviewCtrl>();
        m_modelCtrl[1].InitComponent(1);
    }
    void InitComponent(int index)
    {
        m_camera.targetTexture = m_renderTexture[index];
        for (int i = 0; i < 2; i++)
        {
            m_overrideController[i] = new AnimatorOverrideController();
            m_overrideController[i].runtimeAnimatorController = m_animator[i].runtimeAnimatorController;
            m_animator[i].runtimeAnimatorController = m_overrideController[i];
        }
        m_modelIndex = index;
    }
    public void Show()
    {
        gameObject.SetActive(true);
    }
    public void Hide()
    {
        gameObject.SetActive(false);
    }
    private void Update()
    {
        UpdateAnimation(0);
        UpdateAnimation(1);
        Rotating();
    }
    void UpdateAnimation(int index)
    {
        Animator animator = m_animator[index];
        float curWeight = animator.GetLayerWeight(m_curLayer[index]);
        if (curWeight < 1)
        {
            curWeight += Time.deltaTime / m_fadeTime;
            if (curWeight > 1) curWeight = 1;
            animator.SetLayerWeight(m_curLayer[index], curWeight);
        }
        else
        {
            for (int i = 1; i < 6; i++)
            {
                if (i == m_curLayer[index]) continue;
                float weight = animator.GetLayerWeight(i);
                if (weight > 0)
                {
                    weight -= Time.deltaTime / m_fadeTime;
                    if (weight < 0) weight = 0;
                    animator.SetLayerWeight(i, weight);
                }
            }
        }
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(m_curLayer[index]);
        if ((animator.speed < 0.99 || animator.speed > 1.01) && (stateInfo.IsName("Default") || stateInfo.IsName("NullState"))) animator.speed = 1;
        m_curTime = stateInfo.normalizedTime - (int)stateInfo.normalizedTime;
        if (index == 0) m_curProgress = m_curLayer[0] == 0 ? 0 : m_curTime;
        if (m_curAnimationIndex < m_listAnimation.Count && m_listAnimation[m_curAnimationIndex].m_endTime <= m_curTime)
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
                PlayAnimation(animClip, ba.m_fadeTime, ba.m_offTime, index);
            }
            else
            {
                if (m_animRecycle)
                {
                    HeroAnimation ba = m_listAnimation[0];
                    AnimationClip animClip = AnimationManager.GetAnimationClip(ba.m_name);
                    PlayAnimation(animClip, ba.m_fadeTime, ba.m_offTime, index);
                    m_curAnimationIndex = -1;
                }
                else
                {
                    animator.Update(0);
                    animator.Play("Default");
                }
            }
            m_curAnimationIndex = m_curAnimationIndex + 1;
        }
    }
    void PlayAnimation(AnimationClip animClip, float fadeTime, float offTime, int index)
    {
        m_curLayer[index]++;
        if (m_curLayer[index] > 5 || m_curLayer[index] < 1) m_curLayer[index] = 1;
        string animName = "Attack" + m_curLayer[index];
        //string layerName = "Layer" + m_curLayer[index];
        m_overrideController[index][animName] = animClip;
        //m_overrideController[index][layerName] = null;
        m_animator[index].CrossFade(animName, 0, m_curLayer[index], offTime);
        m_fadeTime = fadeTime * 0.5f;
        if (m_fadeTime < 0.05) m_fadeTime = 0.05f;
        else if (m_fadeTime > 1) m_fadeTime = 1;
    }
    public void StartAnimation(List<HeroAnimation> listAnimation, bool recycle, Hero hero)
    {
        m_listAnimation.Clear();
        m_listAnimation.AddRange(listAnimation);
        m_animRecycle = recycle;
        m_curAnimationIndex = 0;
        Animator animator = GetAnimator(hero);
        AnimatorOverrideController overrideController = GetOverrideController(hero);
        if (animator.speed < 0.05f)//如果为暂停状态，则继续播放
        {
            animator.speed = 1;
            return;
        }
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Attack1") || stateInfo.IsName("Attack2") || stateInfo.IsName("Once"))
        {
            animator.Play("Default", 0);
            return;
        }
        //开始播放，此处只播放第一个攻击动画，后续的动画通过动画里的事件跳转
        HeroAnimation ba = m_listAnimation[0];
        if (!AnimationManager.m_dicAnimationClipPath.ContainsKey(ba.m_name))
        {
            GlobalAssist.ShowCenterTips("错误：动画<" + ba.m_name + ">不存在", 10);
            return;
        }
        AnimationClip animClip = AnimationManager.GetAnimationClip(ba.m_name);
        PlayAnimation(animClip, ba.m_fadeTime, ba.m_offTime, (int)hero.m_gender);
    }
    public void ClearModel()
    {
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            m_avatar[0].DetachAvatarParts(partLink.part);
            m_avatar[1].DetachAvatarParts(partLink.part);
        }
        m_listWearingPart.Clear();
    }
    public void RefreshModel(Hero hero)
    {
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            m_avatar[0].DetachAvatarParts(partLink.part);
            m_avatar[1].DetachAvatarParts(partLink.part);
        }
        m_listWearingPart.Clear();
        for (int index = hero.m_modelInfo.m_listPart.Count - 1; index >= 0; index--)
        {
            string partName = hero.m_modelInfo.m_listPart[index];
            List<PartMatInfo> listMatInfo = null;
            if (hero.m_modelInfo.m_dicPartListColor.ContainsKey(partName)) listMatInfo = hero.m_modelInfo.m_dicPartListColor[partName];

            //生成一套穿着的
            GameObject model = NPCModel.GetPart(partName);
            Eq_PartLink partLinkTmp = model.GetComponent<Eq_PartLink>();
            if (partLinkTmp == null)
            {
                GlobalAssist.ShowCenterTips("错误:<" + partName + ">不存在组件Eq_PartLink", 10);
                continue;
            }
            if (partLinkTmp.smr == null)
            {
                GlobalAssist.ShowCenterTips("错误:<" + partName + ">的Eq_PartLink没有设置Smr", 10);
                continue;
            }
            if (partLinkTmp.physicType == PhysicType.NoPhysics && partLinkTmp.smr.GetComponent<MagicaRenderDeformer>() != null)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">有物理组件，但是PhysicType却是No Physics", 10);
                continue;
            }
            if (partLinkTmp.meshCloth != null && partLinkTmp.meshCloth.VerifyData() != Define.Error.None)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">的物理组件有错误", 10);
                continue;
            }
            if (partName.Contains("04胡子") && partLinkTmp.partType != PartType.HeadBeardEyebrow)
            {
                GlobalAssist.ShowCenterTips("错误:<" + partName + ">的Eq_PartLink没有设置PartType，胡子要设置PartType，见使用说明", 10);
            }
            if (partName.Contains("01头发") && partLinkTmp.partType != PartType.Hair && partLinkTmp.partType != PartType.HairBall)
            {
                GlobalAssist.ShowCenterTips("错误:<" + partName + ">的Eq_PartLink没有设置PartType，头发要设置PartType，见使用说明", 10);
            }
            if (partName.Contains("眉毛") && partLinkTmp.partType != PartType.HeadBeardEyebrow)
            {
                GlobalAssist.ShowCenterTips("错误:<" + partName + ">的Eq_PartLink没有设置PartType，眉毛要设置PartType，见使用说明", 10);
            }
            if (partName.Contains("睫毛") && partLinkTmp.partType != PartType.Eyelash)
            {
                GlobalAssist.ShowCenterTips("错误:<" + partName + ">的Eq_PartLink没有设置PartType，睫毛要设置PartType，见使用说明", 10);
            }
            if (model.layer != 16)
            {
                GlobalAssist.ShowCenterTips(partName + "的Layer错误，确保所有配件的Layer设为16:NPC", 10);
            }
            Animator animator = model.GetComponent<Animator>();
            if (animator != null)
            {
                GlobalAssist.ShowCenterTips(partName + "包含Animator组件，确保所有配件不含Animator组件", 10);
            }
            MagicaAvatarParts maps = model.GetComponent<MagicaAvatarParts>();
            if (maps == null)
            {
                GlobalAssist.ShowCenterTips(partName + "不存在组件MagicaAvatarParts", 10);
                continue;
            }
            GameObject modelWearing = m_avatar[(int)hero.m_gender].AttachAvatarPartsCustom(model);
            modelWearing.SetActive(true);
            Eq_PartLink partLink = modelWearing.GetComponent<Eq_PartLink>();
            m_listWearingPart.Add(partLink);
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
                    partLink.smr.materials[matIndex].color = listMatInfo[matIndex].m_color;
                    if (listMatInfo[matIndex].m_roughness >= 0 && partLink.smr.materials[matIndex].HasProperty("_Glossiness")) partLink.smr.materials[matIndex].SetFloat("_Glossiness", listMatInfo[matIndex].m_roughness);
                    if (listMatInfo[matIndex].m_metalic >= 0 && partLink.smr.materials[matIndex].HasProperty("_Metallic")) partLink.smr.materials[matIndex].SetFloat("_Metallic", listMatInfo[matIndex].m_metalic);
                    if (listMatInfo[matIndex].m_matName.Contains("BL_"))//带BL前缀的替换回原来的法线贴图
                    {
                        if (partLink.smr.materials[matIndex].HasProperty("_DetailNormalMap")) partLink.smr.materials[matIndex].SetTexture("_DetailNormalMap", normalTexD[matIndex]);
                    }
                }
            }
            if (partLink.meshCloth != null && partLink.meshCloth.TeamData.MergeAvatarCollider)
            {
                GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">勾选了MergeAvatarCollider");
            }
            if (partLink.physicType == PhysicType.NoPhysics) continue;
            else if (partLink.physicType == PhysicType.Upper)
            {
                if (partLink.meshCloth == null)
                {
                    GlobalAssist.ShowCenterTips("错误：配件<" + partName + ">没有物理组件，但是PhysicType却不是No Physics");
                }
                else
                {
                    foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].upperColliderList)
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
                    foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].lowerColliderList)
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
                    foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].totalColliderList)
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
                    foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].m_totalColliderList2)
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
                    foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].m_robeColliderList)
                    {
                        partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                    }
                }
            }
        }
        //添加身体和眼睛模型
        for (int i = 0; i < 2; i++)
        {
            GameObject model = null;
            if (i == 0)
            {
                model = HeroBodyInfo.GetBody((int)hero.m_gender, 0/*m_bodyTypeDropdown.value*/);
            }
            else if (i == 1) model = HeroBodyInfo.GetEye((int)hero.m_gender);
            GameObject modelWearing = m_avatar[(int)hero.m_gender].AttachAvatarPartsCustom(model);
            modelWearing.SetActive(true);
            Eq_PartLink partLink = modelWearing.GetComponent<Eq_PartLink>();
            m_listWearingPart.Add(partLink);
            if (partLink.part == null)
            {
                GlobalAssist.ShowCenterTips("身体不存在组件MagicaAvatarParts", 10);
            }
            if (i == 0 && hero.m_modelInfo.m_listBodyMatInfo.Count > 0)
            {
                Material[] materials = partLink.smr.materials;
                Texture[] normalTex = new Texture[partLink.smr.materials.Length];
                Texture[] normalTexD = new Texture[partLink.smr.materials.Length];
                for (int matIndex = 0; matIndex < materials.Length; matIndex++)
                {
                    if (materials[matIndex].HasProperty("_DetailNormalMap")) normalTexD[matIndex] = materials[matIndex].GetTexture("_DetailNormalMap");
                    if (matIndex >= hero.m_modelInfo.m_listBodyMatInfo.Count) break;
                    if (GlobalAssist.m_dicMaterialPath.ContainsKey(hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_matName))
                    {
                        materials[matIndex] = GlobalAssist.GetMaterial(hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_matName);
                    }
                }
                partLink.smr.materials = materials;
                for (int matIndex = 0; matIndex < partLink.smr.materials.Length; matIndex++)//颜色在后面赋值，避免把材质球源文件的颜色改了
                {
                    if (matIndex >= hero.m_modelInfo.m_listBodyMatInfo.Count) break;
                    partLink.smr.materials[matIndex].color = hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_color;
                    if (hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_roughness >= 0 && partLink.smr.materials[matIndex].HasProperty("_Glossiness")) partLink.smr.materials[matIndex].SetFloat("_Glossiness", hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_roughness);
                    if (hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_metalic >= 0 && partLink.smr.materials[matIndex].HasProperty("_Metallic")) partLink.smr.materials[matIndex].SetFloat("_Metallic", hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_metalic);
                    if (hero.m_modelInfo.m_listBodyMatInfo[matIndex].m_matName.Contains("BL_"))//带BL前缀的替换回原来的法线贴图
                    {
                        if (partLink.smr.materials[matIndex].HasProperty("_DetailNormalMap")) partLink.smr.materials[matIndex].SetTexture("_DetailNormalMap", normalTexD[matIndex]);
                    }
                }
            }
            else if (i == 1 && hero.m_modelInfo.m_listEyeMatInfo.Count > 0)
            {
                Material[] materials = partLink.smr.materials;
                Texture[] normalTex = new Texture[partLink.smr.materials.Length];
                Texture[] normalTexD = new Texture[partLink.smr.materials.Length];
                for (int matIndex = 0; matIndex < materials.Length; matIndex++)
                {
                    if (materials[matIndex].HasProperty("_DetailNormalMap")) normalTexD[matIndex] = materials[matIndex].GetTexture("_DetailNormalMap");
                    if (matIndex >= hero.m_modelInfo.m_listEyeMatInfo.Count) break;
                    if (GlobalAssist.m_dicMaterialPath.ContainsKey(hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_matName))
                    {
                        materials[matIndex] = GlobalAssist.GetMaterial(hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_matName);
                    }
                }
                partLink.smr.materials = materials;
                for (int matIndex = 0; matIndex < partLink.smr.materials.Length; matIndex++)//颜色在后面赋值，避免把材质球源文件的颜色改了
                {
                    if (matIndex >= hero.m_modelInfo.m_listEyeMatInfo.Count) break;
                    partLink.smr.materials[matIndex].color = hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_color;
                    if (hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_roughness >= 0 && partLink.smr.materials[matIndex].HasProperty("_Glossiness")) partLink.smr.materials[matIndex].SetFloat("_Glossiness", hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_roughness);
                    if (hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_metalic >= 0 && partLink.smr.materials[matIndex].HasProperty("_Metallic")) partLink.smr.materials[matIndex].SetFloat("_Metallic", hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_metalic);
                    if (hero.m_modelInfo.m_listEyeMatInfo[matIndex].m_matName.Contains("BL_"))//带BL前缀的替换回原来的法线贴图
                    {
                        if (partLink.smr.materials[matIndex].HasProperty("_DetailNormalMap")) partLink.smr.materials[matIndex].SetTexture("_DetailNormalMap", normalTexD[matIndex]);
                    }
                }
            }
            if (partLink.physicType == PhysicType.Upper)
            {
                foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].upperColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
            else if (partLink.physicType == PhysicType.Lower)
            {
                foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].lowerColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
            else if (partLink.physicType == PhysicType.Cape)
            {
                foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].totalColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
            else if (partLink.physicType == PhysicType.Total)
            {
                foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].m_totalColliderList2)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
            else if (partLink.physicType == PhysicType.Robe)
            {
                foreach (var tempCollider in m_eqAvatar[(int)hero.m_gender].m_robeColliderList)
                {
                    partLink.meshCloth.TeamData.ColliderList.Add(tempCollider);
                }
            }
        }
    }
    public void RefreshBodyScale(Hero hero)//调整身体缩放
    {
        if (hero.m_modelInfo == null) return;
        float heightScale = 1 + hero.m_modelInfo.m_heightFactor * 0.01f;
        float neckScale = 1 + hero.m_modelInfo.m_neckFactor * 0.01f;
        float neckWidthScale = 1 + hero.m_modelInfo.m_neckWidthFactor * 0.01f;
        float headScale = 1 + hero.m_modelInfo.m_headFactor * 0.01f;
        float headWidthScale = 1 + hero.m_modelInfo.m_headWidthFactor * 0.01f;
        float shoulderScale = 1 + hero.m_modelInfo.m_shoulderFactor * 0.01f;
        float thighScale = 1 + hero.m_modelInfo.m_thighFactor * 0.01f;
        float shankScale = 1 + hero.m_modelInfo.m_shankFactor * 0.01f;
        float thighColliderScale = thighScale;
        float shankColliderScale = shankScale * thighScale;
        if (hero.m_modelInfo.m_isOnlyY)//身高只变化Y轴
        {
            m_eqAvatar[(int)hero.m_gender].m_hip.localScale = new Vector3(1, heightScale, 1);
            m_eqAvatar[(int)hero.m_gender].m_shoulder.localScale = new Vector3(shoulderScale, 1, 1);
            m_eqAvatar[(int)hero.m_gender].m_neck.localScale = new Vector3(neckWidthScale, neckScale, neckWidthScale);
            m_eqAvatar[(int)hero.m_gender].m_head.localScale = new Vector3(headScale * headWidthScale / (shoulderScale * neckWidthScale), headScale / (heightScale * neckScale), headScale / neckWidthScale);
            m_eqAvatar[(int)hero.m_gender].m_thigh[0].localScale = m_eqAvatar[(int)hero.m_gender].m_thigh[1].localScale = new Vector3(1, thighScale, 1);
            m_eqAvatar[(int)hero.m_gender].m_shank[0].localScale = m_eqAvatar[(int)hero.m_gender].m_shank[1].localScale = new Vector3(1, shankScale, 1);
            thighColliderScale *= heightScale;
            shankColliderScale *= heightScale;
        }
        else//三个方向都变
        {
            m_eqAvatar[(int)hero.m_gender].m_hip.localScale = new Vector3(heightScale, heightScale, heightScale);
            m_eqAvatar[(int)hero.m_gender].m_shoulder.localScale = new Vector3(shoulderScale, 1, 1);
            m_eqAvatar[(int)hero.m_gender].m_neck.localScale = new Vector3(neckWidthScale, neckScale, neckWidthScale);
            m_eqAvatar[(int)hero.m_gender].m_head.localScale = new Vector3(headScale * headWidthScale / (heightScale * shoulderScale * neckWidthScale), headScale / (heightScale * neckScale), headScale / (heightScale * neckWidthScale));
            m_eqAvatar[(int)hero.m_gender].m_thigh[0].localScale = m_eqAvatar[(int)hero.m_gender].m_thigh[1].localScale = new Vector3(1, thighScale, 1);
            m_eqAvatar[(int)hero.m_gender].m_shank[0].localScale = m_eqAvatar[(int)hero.m_gender].m_shank[1].localScale = new Vector3(1, shankScale, 1);
        }
        m_planeCollider[(int)hero.m_gender].transform.localPosition = new Vector3(0, -m_eqAvatar[(int)hero.m_gender].transform.position.y, 0);
        for (int i = 0; i < 2; i++)
        {
            m_eqAvatar[(int)hero.m_gender].m_thighCollider[i].StartRadius = m_eqAvatar[(int)hero.m_gender].m_thighStartRadius / thighColliderScale;
            m_eqAvatar[(int)hero.m_gender].m_thighCollider[i].EndRadius = m_eqAvatar[(int)hero.m_gender].m_thighEndRadius / thighColliderScale;
            m_eqAvatar[(int)hero.m_gender].m_shankCollider[i].StartRadius = m_eqAvatar[(int)hero.m_gender].m_shankStartRadius / shankColliderScale;
            m_eqAvatar[(int)hero.m_gender].m_shankCollider[i].EndRadius = m_eqAvatar[(int)hero.m_gender].m_shankEndRadius / shankColliderScale;
            m_eqAvatar[(int)hero.m_gender].m_thighCollider[i].OnValidate();
            m_eqAvatar[(int)hero.m_gender].m_shankCollider[i].OnValidate();
        }
        for (int i = 0; i < 2; i++)
        {
            m_eqAvatar[(int)hero.m_gender].m_eyes[i].localRotation = Quaternion.Euler(new Vector3(hero.m_modelInfo.m_eyePara[0], hero.m_modelInfo.m_eyePara[1], 0));
            m_eqAvatar[(int)hero.m_gender].m_eyes[i].localPosition = m_eqAvatar[(int)hero.m_gender].m_baseEyePos[i] + new Vector3(0, 0, hero.m_modelInfo.m_eyePara[2]);
        }
        m_boneSpring.SetSpringPower(hero.m_modelInfo.m_springPower);
    }
    IEnumerator TurnOnPhysic()
    {
        yield return null;
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            if (partLink.meshCloth != null)
            {
                partLink.meshCloth.enabled = true;
            }
        }
    }

    IEnumerator TurnOffPhysic()
    {
        yield return null;
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            if (partLink.physicType == PhysicType.Upper || partLink.physicType == PhysicType.Cape) continue;
            if (partLink.meshCloth != null)
            {
                partLink.meshCloth.enabled = false;
            }
        }
    }
    public List<SkinnedMeshRenderer> GetHeadRenderer()
    {
        List<SkinnedMeshRenderer> listSMR = new List<SkinnedMeshRenderer>();
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            if (partLink.partType == PartType.HeadBeardEyebrow || partLink.partType == PartType.Hair || partLink.partType == PartType.HairBall) listSMR.Add(partLink.smr);
        }
        return listSMR;
    }
    public List<SkinnedMeshRenderer> GetEyeRenderer()
    {
        List<SkinnedMeshRenderer> listSMR = new List<SkinnedMeshRenderer>();
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            if (partLink.partType == PartType.Eye) listSMR.Add(partLink.smr);
        }
        return listSMR;
    }
    public List<SkinnedMeshRenderer> GetEyelashRenderer()
    {
        List<SkinnedMeshRenderer> listSMR = new List<SkinnedMeshRenderer>();
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            if (partLink.partType == PartType.Eyelash) listSMR.Add(partLink.smr);
        }
        return listSMR;
    }
    public List<SkinnedMeshRenderer> GetOtherRenderer()
    {
        List<SkinnedMeshRenderer> listSMR = new List<SkinnedMeshRenderer>();
        foreach (Eq_PartLink partLink in m_listWearingPart)
        {
            if (partLink.partType == PartType.None) listSMR.Add(partLink.smr);
        }
        return listSMR;
    }
    public Animator GetAnimator(Hero hero)
    {
        return m_animator[(int)hero.m_gender];
    }
    public AnimatorOverrideController GetOverrideController(Hero hero)
    {
        return m_overrideController[(int)hero.m_gender];
    }
    public void PlayOnceAnimation(Hero hero, string animName, float speed = 1, float offset = 0, float transit = 0.1f)//默认初始速度都是1
    {
        m_curLayer[(int)hero.m_gender] = 0;
        for (int i = 1; i < 6; i++)
        {
            m_animator[(int)hero.m_gender].SetLayerWeight(i, 0);
        }
        m_animator[(int)hero.m_gender].speed = speed;
        AnimationClip animClip;
        if (!AnimationManager.m_dicAnimationClipPath.ContainsKey(animName))
        {
            GlobalAssist.ShowCenterTips("错误：找不到受击动画文件" + animName, 10);
            return;
        }
        animClip = AnimationManager.GetAnimationClip(animName);
        animClip.events = null;
        m_overrideController[(int)hero.m_gender]["Once"] = animClip; //一次性动画
        m_animator[(int)hero.m_gender].CrossFade("Once", transit, 0, offset);
    }
    void Rotating()
    {
        if (RotateCtrl.m_isRotating[m_modelIndex])
        {
            float rotate = -500 * Input.GetAxis("Mouse X") * Time.unscaledDeltaTime * 2;
            m_transform[0].RotateAround(m_transform[0].position, Vector3.up, rotate);
            m_transform[1].RotateAround(m_transform[1].position, Vector3.up, rotate);
        }
    }
}
