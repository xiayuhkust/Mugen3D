using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.IO;
using UnityEngine.AI;
using MagicaCloth;
using UnityEngine.SceneManagement;

public class UI_MakeNPCModelUI : MonoBehaviour, UI_Base
{
    public RectTransform m_partParentTransform;//插入配件行的父对象
    public RectTransform m_wearingParentTransform;//插入装备行的父对象
    public RectTransform m_blendShapeParentTransform;//插入变形器行的父对象
    public RectTransform m_matParentTransform;//插入材质行的父对象
    public GameObject m_partRowPrefab;//配件行
    public GameObject m_wearingRowPrefab;//装备行
    public GameObject m_blendShapeRowPrefab;//变形器行
    public GameObject m_matRowPrefab;//材质行
    public Slider m_sliderHeight;
    public Slider m_sliderNeck;
    public Slider m_sliderNeckWidth;
    public Slider m_sliderHead;
    public Slider m_sliderHeadWidth;
    public Slider m_sliderShoulder;
    public Slider m_sliderThigh;
    public Slider m_sliderShank;
    public Slider m_sliderSpringPower;
    public Slider[] m_sliderEye = new Slider[3];
    public Text m_textHeight;
    public Text m_textNeck;
    public Text m_textNeckWidth;
    public Text m_textHead;
    public Text m_textHeadWidth;
    public Text m_textShoulder;
    public Text m_textThigh;
    public Text m_textShank;
    public Text m_textSpringPower;
    public Text[] m_textEye = new Text[3];
    public Toggle m_toggleOnlyY;
    public Text m_textPartTitle;
    public GameObject m_partType;
    public GameObject m_partDetail;
    public GameObject m_partBlendShape;
    public GameObject m_partBody;
    public Button m_buttonPart;
    public Button m_buttonBlendShape;
    public Button m_buttonBody;
    public Slider m_sliderFieldOfView;
    public Text m_textFieldOfView;
    public Dropdown m_bodyTypeDropdown;
    public Toggle m_toggleTurnOffError;
    public Toggle m_toggleShowBlendShapeName;

    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    private List<UI_PartRowUI> m_listPartRow = new List<UI_PartRowUI>();
    private List<UI_WearingRowUI> m_listWearingRow = new List<UI_WearingRowUI>();
    private List<UI_BlendShapeRowUI> m_listBlendShapeRow = new List<UI_BlendShapeRowUI>();
    private List<UI_MaterialRowUI> m_listMaterialRow = new List<UI_MaterialRowUI>();
    private GameObject m_curShowObject = null;
    private int m_curPartIndex = 0;
    private int m_curWearingIndex = 0;
    private int m_curMatIndex = 0;
    private int m_editMatType = 0;//编辑材质类型，0-一般，1-身体，2-眼睛
    private bool m_isStand = true;

    public static Hero m_curHero = null;
    private static UI_MakeNPCModelUI m_UI = null;
    public static void OpenUI(Hero hero)
    {
        m_curHero = hero;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/MakeNPCModelUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_MakeNPCModelUI>();
        }
        else
        {
            GlobalAssist.ShowUI(m_UI.gameObject, true);
            m_UI.Start();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        NPCModel.InitPartInfo();
        NPCModel.InitMaleBlendShapeName();
        NPCModel.InitFemaleBlendShapeName();
        RefreshListBody();
        PartPress(0);
        RefreshCurModelInfo();
        ModelPartPress();
        RefreshMaterialList();
        m_sliderFieldOfView.value = Camera.main.fieldOfView;
        RefreshHeroIcon();
        //刷新女性站立动画
        //AnimatorOverrideController overrideController = new AnimatorOverrideController();
        //overrideController.runtimeAnimatorController = m_modelCtrl.m_animator[1].runtimeAnimatorController;
        //m_modelCtrl.m_animator[1].runtimeAnimatorController = overrideController;
        //overrideController["stand"] = GameAssets.LoadAsset<AnimationClip>("Assets/Resource/Animation/standFemale.anim");
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ClearPreview();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Time.timeScale < 0.01) Time.timeScale = 1;
            else Time.timeScale = 0;
        }
        //if (ColorPick.m_isFinish && ColorPick.m_relateUI == gameObject) //确认修改颜色
        //{
        //    ColorPick.m_isFinish = false;
        //    ColorPickConfirm();
        //}
        //if (MB_SelectMaterialUI.m_isFinish) //确认修改材质
        //{
        //    MB_SelectMaterialUI.m_isFinish = false;
        //    MaterialSelectConfirm();
        //}
    }
    void RefreshListBody()
    {
        List<Dropdown.OptionData> listData = new List<Dropdown.OptionData>();
        int bodyNum = 3;
        if (m_curHero.m_gender == HeroGender.Female) bodyNum = 8;
        for (int i = 1; i <= bodyNum; i++)
        {
            listData.Add(new Dropdown.OptionData("身体" + i));
        }
        m_bodyTypeDropdown.ClearOptions();
        m_bodyTypeDropdown.AddOptions(listData);
    }

    public static void GetListMaterialPageName(ref List<string> listPartFileName)
    {
        listPartFileName.Clear();
        listPartFileName.Add("01头盔");
        listPartFileName.Add("02布料");
        listPartFileName.Add("03斗笠");
        listPartFileName.Add("04甲片");
        listPartFileName.Add("05皮革");
        listPartFileName.Add("06绳子");
        listPartFileName.Add("07胸甲");
        listPartFileName.Add("08腕甲");
        listPartFileName.Add("09金属");
        listPartFileName.Add("10雕刻");
        listPartFileName.Add("11零件");
        listPartFileName.Add("12鞋子");
        listPartFileName.Add("13男眉毛");
        listPartFileName.Add("14女眉毛");
        listPartFileName.Add("15男皮肤");
        listPartFileName.Add("16女皮肤");
    }
    void RefreshPartList()
    {
        List<string> listPart = NPCModel.m_listPartName[(int) m_curHero.m_gender, m_curPartIndex];
        int rowCount = listPart.Count;
        //设置容器大小
        int height = 34 * rowCount + 4;
        if (height < 860) SetContentSize(m_partParentTransform, 860);
        else SetContentSize(m_partParentTransform, height);
        //设置行
        if (m_listPartRow.Count < rowCount)//已有行比所需的少，增加行
        {
            for (int i = 0; i < m_listPartRow.Count; i++)
            {
                m_listPartRow[i].gameObject.SetActive(true);
            }
            for (int i = m_listPartRow.Count; i < rowCount; i++)
            {
                AddOneRow(i, 0);
            }
        }
        else//将多余的行隐藏
        {
            for (int i = 0; i < rowCount; i++) m_listPartRow[i].gameObject.SetActive(true);
            for (int i = rowCount; i < m_listPartRow.Count; i++) m_listPartRow[i].gameObject.SetActive(false);
        }
        //填列表内容
        int index = 0;
        foreach (string partName in listPart)
        {
            m_listPartRow[index].m_text[0].text = (index + 1).ToString();
            m_listPartRow[index].m_text[1].text = partName;
            m_listPartRow[index].m_index = index;
            index++;
        }
    }
    void RefreshWearingList()
    {
        if (m_curHero.m_modelInfo == null) return;
        for (int index = m_curHero.m_modelInfo.m_listPart.Count - 1; index >= 0; index--)//单独提取出来剔除，放下面的循环里会导致m_listWearingRow序号错乱
        {
            string partName = m_curHero.m_modelInfo.m_listPart[index];
            if (NPCModel.GetPart(partName) == null)
            {
                GlobalAssist.ShowCenterTips("配件<" + partName + ">不存在");
                m_curHero.m_modelInfo.m_listPart.RemoveAt(index);
            }
        }
        int rowCount = m_curHero.m_modelInfo.m_listPart.Count;
        //设置容器大小
        int height = 34 * rowCount + 4;
        if (height < 300) SetContentSize(m_wearingParentTransform, 300);
        else SetContentSize(m_wearingParentTransform, height);
        //设置行
        if (m_listWearingRow.Count < rowCount)//已有行比所需的少，增加行
        {
            for (int i = 0; i < m_listWearingRow.Count; i++)
            {
                m_listWearingRow[i].gameObject.SetActive(true);
            }
            for (int i = m_listWearingRow.Count; i < rowCount; i++)
            {
                AddOneRow(i, 1);
            }
        }
        else//将多余的行隐藏
        {
            for (int i = 0; i < rowCount; i++) m_listWearingRow[i].gameObject.SetActive(true);
            for (int i = rowCount; i < m_listWearingRow.Count; i++) m_listWearingRow[i].gameObject.SetActive(false);
        }
        //填列表内容
        for (int index = m_curHero.m_modelInfo.m_listPart.Count - 1; index >= 0; index--)
        {
            string partName = m_curHero.m_modelInfo.m_listPart[index];
            m_listWearingRow[index].m_text[0].text = (index + 1).ToString();
            string[] names = partName.Split('/');
            m_listWearingRow[index].m_text[1].text = names[names.Length - 1];
            m_listWearingRow[index].m_index = index;
        }
#if !UNITY_EDITOR
        try
#endif
        {
            HeroModelPreviewCtrl.m_modelCtrl[0].RefreshModel(m_curHero);
            if (m_editMatType == 0) WearingRowPress(0);
            RefreshHeadBlendShape(m_curHero.m_modelInfo);
            HeroModelPreviewCtrl.m_modelCtrl[0].RefreshBodyScale(m_curHero);
        }
#if !UNITY_EDITOR
        catch (Exception exc)
        {
            GlobalAssist.ShowCenterTips(exc.Message, 10);
        }
#endif
    }
    void RefreshMaterialList()
    {
        if (m_curHero.m_modelInfo == null) return;
        List<Material> listMaterial = new List<Material>();
        List<PartMatInfo> listMatInfo;
        GameObject model;
        Eq_PartLink partLink;
        if (m_editMatType==1)//身体
        {
            model = HeroBodyInfo.GetBody((int)m_curHero.m_gender, 1);
            partLink = model.GetComponent<Eq_PartLink>();
            listMatInfo = m_curHero.m_modelInfo.m_listBodyMatInfo;
            if (m_curHero.m_modelInfo.m_listBodyMatInfo.Count != partLink.smr.sharedMaterials.Length)//在这里创建材质列表并初始化列表
            {
                listMatInfo.Clear();
                if (partLink.smr.sharedMaterials.Length == 0)
                {
                    GlobalAssist.ShowCenterTips("没有材质");
                    for (int i = 0; i < m_listMaterialRow.Count; i++) m_listMaterialRow[i].gameObject.SetActive(false);
                    return;
                }
                foreach (Material mat in partLink.smr.sharedMaterials)
                {
                    PartMatInfo matInfo = new PartMatInfo();
                    matInfo.m_color = mat.color;
                    if (mat.HasProperty("_Glossiness")) matInfo.m_roughness = mat.GetFloat("_Glossiness");
                    if (mat.HasProperty("_Metallic")) matInfo.m_metalic = mat.GetFloat("_Metallic");
                    listMatInfo.Add(matInfo);
                    listMaterial.Add(mat);
                }
            }
            else
            {
                foreach (Material mat in partLink.smr.sharedMaterials)
                {
                    listMaterial.Add(mat);
                }
            }
        }
        else if (m_editMatType == 2)//眼睛
        {
            model = HeroBodyInfo.GetEye((int)m_curHero.m_gender);
            partLink = model.GetComponent<Eq_PartLink>();
            listMatInfo = m_curHero.m_modelInfo.m_listEyeMatInfo;
            if (m_curHero.m_modelInfo.m_listEyeMatInfo.Count != partLink.smr.sharedMaterials.Length)//在这里创建材质列表并初始化列表
            {
                listMatInfo.Clear();
                if (partLink.smr.sharedMaterials.Length == 0)
                {
                    GlobalAssist.ShowCenterTips("没有材质");
                    for (int i = 0; i < m_listMaterialRow.Count; i++) m_listMaterialRow[i].gameObject.SetActive(false);
                    return;
                }
                foreach (Material mat in partLink.smr.sharedMaterials)
                {
                    PartMatInfo matInfo = new PartMatInfo();
                    matInfo.m_color = mat.color;
                    if (mat.HasProperty("_Glossiness")) matInfo.m_roughness = mat.GetFloat("_Glossiness");
                    if (mat.HasProperty("_Metallic")) matInfo.m_metalic = mat.GetFloat("_Metallic");
                    listMatInfo.Add(matInfo);
                    listMaterial.Add(mat);
                }
            }
            else
            {
                foreach (Material mat in partLink.smr.sharedMaterials)
                {
                    listMaterial.Add(mat);
                }
            }
        }
        else
        {
            string partName = m_curHero.m_modelInfo.m_listPart[m_curWearingIndex];
            model = NPCModel.GetPart(partName);
            partLink = model.GetComponent<Eq_PartLink>();
            if (!m_curHero.m_modelInfo.m_dicPartListColor.ContainsKey(partName) || m_curHero.m_modelInfo.m_dicPartListColor[partName].Count != partLink.smr.sharedMaterials.Length)//在这里创建材质列表并初始化列表
            {
                m_curHero.m_modelInfo.m_dicPartListColor.Remove(partName);
                if (partLink.smr.sharedMaterials.Length == 0)
                {
                    GlobalAssist.ShowCenterTips("没有材质");
                    for (int i = 0; i < m_listMaterialRow.Count; i++) m_listMaterialRow[i].gameObject.SetActive(false);
                    return;
                }
                listMatInfo = new List<PartMatInfo>();
                foreach (Material mat in partLink.smr.sharedMaterials)
                {
                    PartMatInfo matInfo = new PartMatInfo();
                    matInfo.m_color = mat.color;
                    if (mat.HasProperty("_Glossiness")) matInfo.m_roughness = mat.GetFloat("_Glossiness");
                    if (mat.HasProperty("_Metallic")) matInfo.m_metalic = mat.GetFloat("_Metallic");
                    listMatInfo.Add(matInfo);
                    listMaterial.Add(mat);
                }
                m_curHero.m_modelInfo.m_dicPartListColor.Add(partName, listMatInfo);
            }
            else
            {
                listMatInfo = m_curHero.m_modelInfo.m_dicPartListColor[partName];
                foreach (Material mat in partLink.smr.sharedMaterials)
                {
                    listMaterial.Add(mat);
                }
            }
        }
        
        int rowCount = listMaterial.Count;
        //设置容器大小
        int height = 64 * rowCount + 4;
        if (height < 200) SetContentSize(m_matParentTransform, 200);
        else SetContentSize(m_matParentTransform, height);
        //设置行
        if (m_listMaterialRow.Count < rowCount)//已有行比所需的少，增加行
        {
            for (int i = 0; i < m_listMaterialRow.Count; i++)
            {
                m_listMaterialRow[i].gameObject.SetActive(true);
            }
            for (int i = m_listMaterialRow.Count; i < rowCount; i++)
            {
                AddOneRow(i, 3);
            }
        }
        else//将多余的行隐藏
        {
            for (int i = 0; i < rowCount; i++) m_listMaterialRow[i].gameObject.SetActive(true);
            for (int i = rowCount; i < m_listMaterialRow.Count; i++) m_listMaterialRow[i].gameObject.SetActive(false);
        }
        //填列表内容
        for (int index = 0; index < listMaterial.Count; index++)
        {
            Material mat = listMaterial[index];
            UI_MaterialRowUI matRow = m_listMaterialRow[index];
            matRow.m_text[0].text = (index + 1).ToString();
            if (GlobalAssist.m_dicMaterialPath.ContainsKey(listMatInfo[index].m_matName))
            {
                matRow.m_text[1].text = listMatInfo[index].m_matName;
            }
            else matRow.m_text[1].text = mat.name;
            matRow.m_index = index;
            matRow.m_buttonColor.image.color = listMatInfo[index].m_color;
            if (listMatInfo[index].m_roughness < 0 && mat.HasProperty("_Glossiness")) listMatInfo[index].m_roughness = mat.GetFloat("_Glossiness");
            if (listMatInfo[index].m_metalic < 0 && mat.HasProperty("_Metallic")) listMatInfo[index].m_metalic = mat.GetFloat("_Metallic");
            matRow.m_sliderRough.SetValueWithoutNotify(listMatInfo[index].m_roughness);
            matRow.m_textRough.text = listMatInfo[index].m_roughness.ToString("0.00");
            matRow.m_sliderMetal.SetValueWithoutNotify(listMatInfo[index].m_metalic);
            matRow.m_textMetal.text = listMatInfo[index].m_metalic.ToString("0.00");
        }
    }
    public void AddOneRow(int i, int type)//增加一行，0-配件，1-装备，2-变形器,3-材质
    {
        GameObject obj = null;
        switch (type)
        {
            case 0:
                {
                    obj = Instantiate(m_partRowPrefab, m_partParentTransform);
                    UI_PartRowUI rowUI = obj.GetComponent<UI_PartRowUI>();
                    rowUI.m_baseUI = this;
                    m_listPartRow.Add(rowUI);
                }
                break;
            case 1:
                {
                    obj = Instantiate(m_wearingRowPrefab, m_wearingParentTransform);
                    UI_WearingRowUI rowUI = obj.GetComponent<UI_WearingRowUI>();
                    rowUI.m_index = i;
                    rowUI.m_baseUI = this;
                    m_listWearingRow.Add(rowUI);
                }
                break;
            case 2:
                {
                    obj = Instantiate(m_blendShapeRowPrefab, m_blendShapeParentTransform);
                    UI_BlendShapeRowUI rowUI = obj.GetComponent<UI_BlendShapeRowUI>();
                    rowUI.m_index = i;
                    rowUI.m_baseUI = this;
                    m_listBlendShapeRow.Add(rowUI);
                }
                break;
            case 3:
                {
                    obj = Instantiate(m_matRowPrefab, m_matParentTransform);
                    UI_MaterialRowUI rowUI = obj.GetComponent<UI_MaterialRowUI>();
                    rowUI.m_index = i;
                    rowUI.m_baseUI = this;
                    m_listMaterialRow.Add(rowUI);
                }
                break;
        }
    }
    public void SetContentSize(RectTransform parentTransform, int height)//设置容器大小
    {
        int contentHeight = height;
        if (parentTransform.sizeDelta.y != contentHeight)//尺寸改变才需刷新
        {
            parentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, contentHeight);
            parentTransform.anchoredPosition = new Vector3(0, 0, 0);
        }
    }
    public void ButtonPartPress(int index)
    {
        
        PartPress(index);
    }
    public void PartPress(int index)
    {
        m_curPartIndex = index;
        m_textPartTitle.text = NPCModel.m_listPartTitleName[index];
        RefreshPartList();
    }

    void RefreshHeroIcon()
    {
        //if (m_cgType == 0) m_imageHero.sprite = m_curHero.GetIcon();
        //else m_imageHero.sprite = m_curHero.GetCG();
    }
    void RefreshCurModelInfo()
    {
        if (m_curHero.m_modelInfo == null) return;
        m_editMatType = 1;
        m_sliderHeight.SetValueWithoutNotify(m_curHero.m_modelInfo.m_heightFactor);
        m_textHeight.text = m_sliderHeight.value + "%";
        m_sliderNeck.SetValueWithoutNotify(m_curHero.m_modelInfo.m_neckFactor);
        m_textNeck.text = m_sliderNeck.value + "%";
        m_sliderHead.SetValueWithoutNotify(m_curHero.m_modelInfo.m_headFactor);
        m_textHead.text = m_sliderHead.value + "%";
        m_toggleOnlyY.SetIsOnWithoutNotify(m_curHero.m_modelInfo.m_isOnlyY);

        m_sliderHeadWidth.SetValueWithoutNotify(m_curHero.m_modelInfo.m_headWidthFactor);
        m_textHeadWidth.text = m_sliderHeadWidth.value + "%";
        m_sliderShoulder.SetValueWithoutNotify(m_curHero.m_modelInfo.m_shoulderFactor);
        m_textShoulder.text = m_sliderShoulder.value + "%";
        m_sliderThigh.SetValueWithoutNotify(m_curHero.m_modelInfo.m_thighFactor);
        m_textThigh.text = m_sliderThigh.value + "%";
        m_sliderShank.SetValueWithoutNotify(m_curHero.m_modelInfo.m_shankFactor);
        m_textShank.text = m_sliderShank.value + "%";
        m_sliderSpringPower.SetValueWithoutNotify(m_curHero.m_modelInfo.m_springPower * 1000);
        m_textSpringPower.text = m_curHero.m_modelInfo.m_springPower.ToString();
        m_sliderNeckWidth.SetValueWithoutNotify(m_curHero.m_modelInfo.m_neckWidthFactor);
        m_textNeckWidth.text = m_sliderNeckWidth.value + "%";
        for (int i = 0; i < 3; i++)
        {
            m_sliderEye[i].SetValueWithoutNotify(m_curHero.m_modelInfo.m_eyePara[i]);
            if (i == 2) m_textEye[i].text = m_curHero.m_modelInfo.m_eyePara[i].ToString("0.0000");
            else m_textEye[i].text = m_curHero.m_modelInfo.m_eyePara[i].ToString();
        }
        RefreshWearingList();
        RefreshBlendShapeList();
        RefreshListBlendShapeRow(m_curHero.m_modelInfo); 
        RefreshMaterialList();
    }
    public void ButtonAnimationPress(int index)
    {
        
        //if (index == 0)//站
        //{
        //    m_modelCtrl.m_animator[(int)m_curHero.m_gender].Play("stand");
        //    StartCoroutine(TurnOnPhysic());
        //    m_isStand = true;
        //}
        //else if (index == 1)//坐
        //{
        //    m_modelCtrl.m_animator[(int)m_curHero.m_gender].Play("sit");
        //    //m_modelCtrl.m_animator.SetTrigger("standToSit");
        //    StartCoroutine(TurnOffPhysic());
        //    m_isStand = false;
        //}
        //else if (index == 2)//走路
        //{
        //    m_modelCtrl.m_animator[(int)m_curHero.m_gender].Play("Walk");
        //    StartCoroutine(TurnOnPhysic());
        //    m_isStand = false;
        //}
        //else if (index == 3)//跑步
        //{
        //    string strRun = (int)m_curHero.m_gender == 0 ? "RunMale" : "RunFemale";
        //    m_modelCtrl.m_animator[(int)m_curHero.m_gender].Play(strRun);
        //    StartCoroutine(TurnOnPhysic());
        //    m_isStand = false;
        //}
    }
    public void ColorPickConfirm()
    {
        if (m_curHero.m_modelInfo == null) return;
        //if (m_editMatType == 1)
        //{
        //    m_curHero.m_modelInfo.m_listBodyMatInfo[m_curMatIndex].m_color = ColorPick.m_color;
        //}
        //else if (m_editMatType == 2)
        //{
        //    m_curHero.m_modelInfo.m_listEyeMatInfo[m_curMatIndex].m_color = ColorPick.m_color;
        //}
        //else if (m_curWearingIndex < m_curHero.m_modelInfo.m_listPart.Count)
        //{
        //    string partName = m_curHero.m_modelInfo.m_listPart[m_curWearingIndex];
        //    if (m_curHero.m_modelInfo.m_dicPartListColor.ContainsKey(partName))
        //    {
        //        m_curHero.m_modelInfo.m_dicPartListColor[partName][m_curMatIndex].m_color = ColorPick.m_color;
        //    }
        //    else
        //    {
        //        GlobalAssist.ShowCenterTips("错误，没有材质列表");
        //    }
        //}
        int sel = m_curWearingIndex; 
        RefreshWearingList();
        WearingRowPress(sel);
    }
    public void MaterialSelectConfirm()
    {
        if (m_curHero.m_modelInfo == null) return;
        //if (m_editMatType == 1)
        //{
        //    m_curHero.m_modelInfo.m_listBodyMatInfo[m_curMatIndex].m_matName = MB_SelectMaterialUI.m_matName;
        //}
        //else if (m_editMatType == 2)
        //{
        //    m_curHero.m_modelInfo.m_listEyeMatInfo[m_curMatIndex].m_matName = MB_SelectMaterialUI.m_matName;
        //}
        //else if (m_curWearingIndex < m_curHero.m_modelInfo.m_listPart.Count)
        //{
        //    string partName = m_curHero.m_modelInfo.m_listPart[m_curWearingIndex];
        //    if (m_curHero.m_modelInfo.m_dicPartListColor.ContainsKey(partName))
        //    {
        //        m_curHero.m_modelInfo.m_dicPartListColor[partName][m_curMatIndex].m_matName = MB_SelectMaterialUI.m_matName;
        //    }
        //    else
        //    {
        //        GlobalAssist.ShowCenterTips("错误，没有材质列表");
        //    }
        //}
        int sel = m_curWearingIndex;
        RefreshWearingList();
        WearingRowPress(sel);
    }
    public void ButtonBodyMaterialPress()
    {
        
        if (m_curHero.m_modelInfo == null) return;
        m_editMatType = 1;
        RefreshMaterialList();
    }
    public void ButtonEyeMaterialPress()
    {
        
        if (m_curHero.m_modelInfo == null) return;
        m_editMatType = 2;
        RefreshMaterialList();
    }
    public void ButtonMatColorPress(int index)
    {
        
        if (m_curHero.m_modelInfo == null) return;
        //m_curMatIndex = index;
        //if (m_editMatType == 1)
        //{
        //    ColorPick.OpenUI(gameObject, m_curHero.m_modelInfo.m_listBodyMatInfo[m_curMatIndex].m_color);
        //}
        //else if (m_editMatType == 2)
        //{
        //    ColorPick.OpenUI(gameObject, m_curHero.m_modelInfo.m_listEyeMatInfo[m_curMatIndex].m_color);
        //}
        //else
        //{
        //    if (m_curWearingIndex >= m_curHero.m_modelInfo.m_listPart.Count) return;
        //    string partName = m_curHero.m_modelInfo.m_listPart[m_curWearingIndex];
        //    ColorPick.OpenUI(gameObject, m_curHero.m_modelInfo.m_dicPartListColor[partName][m_curMatIndex].m_color);
        //}
    }
    public void ButtonMaterialPress(int index)
    {
        if (m_curHero.m_modelInfo == null || (m_curWearingIndex >= m_curHero.m_modelInfo.m_listPart.Count && m_editMatType == 0)) return;
        m_curMatIndex = index;
        //MB_SelectMaterialUI.OpenUI();
    }
    public void ButtonMaterialResetPress()
    {
        if (m_curHero.m_modelInfo == null || m_curWearingIndex >= m_curHero.m_modelInfo.m_listPart.Count) return;
        int wearingIndex = m_curWearingIndex;
        string partName = m_curHero.m_modelInfo.m_listPart[wearingIndex];
        m_curHero.m_modelInfo.m_dicPartListColor.Remove(partName);
        RefreshWearingList();
        WearingRowPress(wearingIndex);
    }
    public void SliderRoughChange(int index, float val)
    {
        if (m_curHero.m_modelInfo == null) return;
        if (m_editMatType == 1)
        {
            m_curHero.m_modelInfo.m_listBodyMatInfo[index].m_roughness = val;
            RefreshWearingList();
        }
        else if (m_editMatType == 2)
        {
            m_curHero.m_modelInfo.m_listEyeMatInfo[index].m_roughness = val;
            RefreshWearingList();
        }
        else
        {
            if (m_curWearingIndex >= m_curHero.m_modelInfo.m_listPart.Count) return;
            int wearingIndex = m_curWearingIndex;
            string partName = m_curHero.m_modelInfo.m_listPart[wearingIndex];
            m_curHero.m_modelInfo.m_dicPartListColor[partName][index].m_roughness = val;
            RefreshWearingList();
            WearingRowPress(wearingIndex);
        }
    }
    public void SliderMetalChange(int index, float val)
    {
        if (m_curHero.m_modelInfo == null) return;
        if (m_editMatType == 1)
        {
            m_curHero.m_modelInfo.m_listBodyMatInfo[index].m_metalic = val;
            RefreshWearingList();
        }
        else if (m_editMatType == 2)
        {
            m_curHero.m_modelInfo.m_listEyeMatInfo[index].m_metalic = val;
            RefreshWearingList();
        }
        else
        {
            if (m_curWearingIndex >= m_curHero.m_modelInfo.m_listPart.Count) return;
            int wearingIndex = m_curWearingIndex;
            string partName = m_curHero.m_modelInfo.m_listPart[wearingIndex];
            m_curHero.m_modelInfo.m_dicPartListColor[partName][index].m_metalic = val;
            RefreshWearingList();
            WearingRowPress(wearingIndex);
        }
    }

    public void ButtonRecoveryPress()
    {
        if (m_curHero.m_modelInfo == null) return;
        m_curHero.InitFromModelData();
        RefreshCurModelInfo();
        GlobalAssist.ShowCenterTips("已还原所有操作");
    }
    public void ButtonOKPress()
    {
        CaptureCamera();
        SaveModelData();
        GlobalAssist.HideUI(gameObject);
    }
    public void ButtonCancelPress()
    {
        m_curHero.InitFromModelData();
        GlobalAssist.HideUI(gameObject);
    }
    public void TakeOnPart(int index)
    {
        if (m_curHero.m_modelInfo == null) return;
        string partName = NPCModel.m_genderFolder[(int)m_curHero.m_gender] + "/" + NPCModel.m_listPartFileName[m_curPartIndex] + "/" + NPCModel.m_listPartName[(int)m_curHero.m_gender, m_curPartIndex][index];
        if (m_curHero.m_modelInfo.m_listPart.Contains(partName))
        {
            GlobalAssist.ShowCenterTips("不能重复装备相同配件");
            return;
        }
        m_curHero.m_modelInfo.m_listPart.Add(partName);
        RefreshWearingList();
    }
    public void TakeOffPart(int index)
    {
        if (m_curHero.m_modelInfo == null) return;
        m_curHero.m_modelInfo.m_dicPartListColor.Remove(m_curHero.m_modelInfo.m_listPart[index]);
        m_curHero.m_modelInfo.m_listPart.RemoveAt(index);
        RefreshWearingList();
    }
    public void WearingRowPress(int index)
    {
        if (m_curHero.m_modelInfo == null || index >= m_curHero.m_modelInfo.m_listPart.Count)
        {
            m_editMatType = 1;
            RefreshMaterialList();
            return;
        }
        for (int i = 0; i < m_curHero.m_modelInfo.m_listPart.Count; i++)
        {
            m_listWearingRow[i].m_image.color = Color.white;
        }
        m_listWearingRow[index].m_image.color = Color.green;
        m_curWearingIndex = index;
        m_editMatType = 0;
        RefreshMaterialList();
    }
    public void PreviewBlendShapeName(int index)
    {
        if (!m_toggleShowBlendShapeName.isOn) return;
    }
    public void PreviewPart(int index)
    {
        if (m_curShowObject != null) m_curShowObject.SetActive(false);
        string partName = NPCModel.m_genderFolder[(int)m_curHero.m_gender] + "/" + NPCModel.m_listPartFileName[m_curPartIndex] + "/" + NPCModel.m_listPartName[(int)m_curHero.m_gender, m_curPartIndex][index];
        m_curShowObject = NPCModel.GetPreviewPart(partName);
        if (m_curShowObject != null) m_curShowObject.SetActive(true);
    }
    public void PreviewWearing(int index)
    {
        if (m_curHero.m_modelInfo == null) return;
        if (m_curShowObject != null) m_curShowObject.SetActive(false);
        m_curShowObject = null;
        string partName = m_curHero.m_modelInfo.m_listPart[index];
        m_curShowObject = NPCModel.GetPreviewPart(partName);
        if (m_curShowObject != null) m_curShowObject.SetActive(true);
    }
    public void ClearPreview()
    {
        if (m_curShowObject != null) m_curShowObject.SetActive(false);
        m_curShowObject = null;
    }

    void SaveModelData()
    {
        if (m_curHero.m_modelInfo == null) return;
        if (m_curHero == null) return;
        string filePath = Application.streamingAssetsPath + "/NPCModel/" + m_curHero.m_id + ".dat";
        SaveModelData(filePath);
        filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/3DMugen/NPCModel";
        if (!Directory.Exists(filePath)) Directory.CreateDirectory(filePath);
        filePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/3DMugen/NPCModel/" + m_curHero.m_id + ".dat";
        SaveModelData(filePath);//备份一份到我的文档
    }

    void SaveModelData(string filePath)
    {
        BinaryWriter bw = new BinaryWriter(new FileStream(filePath, FileMode.Create));
        bw.Write(GlobalAssist.m_versionNo);//版本号
        m_curHero.m_modelInfo.WriteToBinary(bw);
        bw.Close();
    }
    public void ButtonModelPartPress()
    {
        ModelPartPress();
    }
    public void ModelPartPress()
    {
        if (m_curHero.m_modelInfo == null) return;
        m_buttonPart.image.color = Color.yellow;
        m_partType.SetActive(true);
        m_partDetail.SetActive(true);
        m_buttonBlendShape.image.color = Color.white;
        m_partBlendShape.SetActive(false);
        m_buttonBody.image.color = Color.white;
        m_partBody.SetActive(false);
    }
    public void ButtonBlendShapePress()
    {
        if (m_curHero.m_modelInfo == null) return;
        m_buttonPart.image.color = Color.white;
        m_partType.SetActive(false);
        m_partDetail.SetActive(false);
        m_buttonBlendShape.image.color = Color.yellow;
        m_partBlendShape.SetActive(true);
        m_buttonBody.image.color = Color.white;
        m_partBody.SetActive(false);
    }
    public void ButtonBodyPress()
    {
        if (m_curHero.m_modelInfo == null) return;
        m_buttonPart.image.color = Color.white;
        m_partType.SetActive(false);
        m_partDetail.SetActive(false);
        m_buttonBlendShape.image.color = Color.white;
        m_partBlendShape.SetActive(false);
        m_buttonBody.image.color = Color.yellow;
        m_partBody.SetActive(true);
    }
    void RefreshBlendShapeList()
    {
        int rowCount = NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender].Count;
        //设置容器大小
        int height = 34 * rowCount + 4;
        if (height < 860) SetContentSize(m_blendShapeParentTransform, 860);
        else SetContentSize(m_blendShapeParentTransform, height);
        //设置行
        if (m_listBlendShapeRow.Count < rowCount)//已有行比所需的少，增加行
        {
            for (int i = 0; i < m_listBlendShapeRow.Count; i++)
            {
                m_listBlendShapeRow[i].gameObject.SetActive(true);
            }
            for (int i = m_listBlendShapeRow.Count; i < rowCount; i++)
            {
                AddOneRow(i, 2);
            }
        }
        else//将多余的行隐藏
        {
            for (int i = 0; i < rowCount; i++) m_listBlendShapeRow[i].gameObject.SetActive(true);
            for (int i = rowCount; i < m_listBlendShapeRow.Count; i++) m_listBlendShapeRow[i].gameObject.SetActive(false);
        }
        //填列表内容
        int index = 0;
        foreach (string blendShapeName in NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender])
        {
            UI_BlendShapeRowUI rowUI = m_listBlendShapeRow[index];

            rowUI.m_text[0].text = (index + 1).ToString();
            rowUI.m_text[1].text = NPCModel.m_dicBlendShapeDescription[blendShapeName];
            index++;
            if (m_curHero.m_gender == HeroGender.Male)
            {
                if (index > 140) rowUI.m_backImage.color = Color.white;
                else if (index > 135) rowUI.m_backImage.color = Color.cyan;
                else if (index > 127) rowUI.m_backImage.color = Color.green;
                else if (index > 107) rowUI.m_backImage.color = Color.blue;
                else if (index > 86) rowUI.m_backImage.color = Color.cyan;
                else if (index > 85) rowUI.m_backImage.color = Color.gray;
                else if (index > 66) rowUI.m_backImage.color = Color.yellow;
                else if (index > 23) rowUI.m_backImage.color = Color.cyan;
            }
            else
            {
                if (index > 133) rowUI.m_backImage.color = Color.white;
                else if (index > 117) rowUI.m_backImage.color = Color.green;
                else if (index > 92) rowUI.m_backImage.color = Color.blue;
                else if (index > 72) rowUI.m_backImage.color = Color.gray;
                else if (index > 52) rowUI.m_backImage.color = Color.green;
                else if (index > 51) rowUI.m_backImage.color = Color.yellow;
                else if (index > 19) rowUI.m_backImage.color = Color.cyan;
            }
        }
    }
    void RefreshListBlendShapeRow(ModelInfo modelInfo)
    {
        for (int i = 0; i < NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender].Count; i++)
        {
            UI_BlendShapeRowUI row = m_listBlendShapeRow[i];
            if (modelInfo.m_dicBlendShapeFactor.ContainsKey(NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender][i]))
            {
                if (modelInfo.m_dicBlendShapeFactor[NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender][i]] > 101)
                {
                    row.m_slider.SetValueWithoutNotify(modelInfo.m_dicBlendShapeFactor[NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender][i]] / 5f);
                    row.m_text[2].text = row.m_slider.value.ToString("0");
                    row.m_toggle.SetIsOnWithoutNotify(true);
                }
                else
                {
                    row.m_slider.SetValueWithoutNotify(modelInfo.m_dicBlendShapeFactor[NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender][i]]);
                    row.m_text[2].text = row.m_slider.value.ToString("0");
                    row.m_toggle.SetIsOnWithoutNotify(false);
                }
            }
            else
            {
                row.m_slider.SetValueWithoutNotify(0);
                row.m_text[2].text = "0";
                row.m_toggle.SetIsOnWithoutNotify(false);
            }
        }
        RefreshHeadBlendShape(modelInfo);
    }
    void RefreshHeadBlendShape(ModelInfo modelInfo)
    {
        List<SkinnedMeshRenderer> listSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetHeadRenderer();
        if (listSMR.Count > 0)
        {
            foreach (SkinnedMeshRenderer smr in listSMR)
            {
                Transform parentTransform = smr.transform.parent.parent;
                string partName = "脸部或头发或头发丸子";
                if (parentTransform != null) partName = parentTransform.gameObject.name;
                if (smr == null || smr.sharedMesh == null)
                {
                    if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("错误：" + partName + "的smr为null", 10);
                    continue;
                }
                foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
                {
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips(partName + "找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
                }
            }
        }
        List<SkinnedMeshRenderer> listEyeSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetEyeRenderer();
        if (listEyeSMR.Count > 0)
        {
            foreach (SkinnedMeshRenderer smr in listEyeSMR)
            {
                if (smr == null || smr.sharedMesh == null)
                {
                    GlobalAssist.ShowCenterTips("错误：眼睛smr为null", 10);
                    continue;
                }
                foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
                {
                    if (NPCModel.m_listEyeExcludeBlendShape[(int)m_curHero.m_gender].Contains(blendShapeName)) continue;
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("眼睛找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
                }
            }
        }
        List<SkinnedMeshRenderer> listEyelashSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetEyelashRenderer();
        if (listEyelashSMR.Count > 0)
        {
            foreach (SkinnedMeshRenderer smr in listEyelashSMR)
            {
                if (smr == null || smr.sharedMesh == null)
                {
                    GlobalAssist.ShowCenterTips("错误：眼睫毛smr为null", 10);
                    continue;
                }
                foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
                {
                    if (NPCModel.m_listEyelashExcludeBlendShape[(int)m_curHero.m_gender].Contains(blendShapeName)) continue;
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("眼睫毛找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
                }
            }
        }
        List<SkinnedMeshRenderer> listOtherSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetOtherRenderer();
        if (listOtherSMR.Count > 0)
        {
            foreach (SkinnedMeshRenderer smr in listOtherSMR)
            {
                if (smr == null || smr.sharedMesh == null)
                {
                    GlobalAssist.ShowCenterTips("错误：装备smr为null", 10);
                    continue;
                }
                foreach (string blendShapeName in modelInfo.m_dicBlendShapeFactor.Keys)
                {
                    if (!NPCModel.m_listBodyBlendShape[(int)m_curHero.m_gender].Contains(blendShapeName)) continue;
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("装备找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, modelInfo.m_dicBlendShapeFactor[blendShapeName]);
                }
            }
        }
    }
    void RefreshHeroBlendShape()//将变形器参数更新到NPC
    {
        if (m_curHero.m_modelInfo == null) return;
        m_curHero.m_modelInfo.m_dicBlendShapeFactor.Clear();
        for (int i = 0; i < NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender].Count; i++)
        {
            UI_BlendShapeRowUI row = m_listBlendShapeRow[i];
            if (Mathf.Abs(row.m_slider.value) > 1) m_curHero.m_modelInfo.m_dicBlendShapeFactor.Add(NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender][i], row.m_slider.value* (row.m_toggle.isOn ? 5f : 1f));
        }
    }
    public void ChangeBlendShape(int index, float val)
    {
        string blendShapeName = NPCModel.m_listBlendShapeAll[(int)m_curHero.m_gender][index];
        List<SkinnedMeshRenderer> listSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetHeadRenderer();
        if (listSMR.Count > 0) 
        {
            foreach (SkinnedMeshRenderer smr in listSMR)
            {
                if (smr == null || smr.sharedMesh == null)
                {
                    GlobalAssist.ShowCenterTips("错误：脸部或头发或头发丸子的smr为null", 10);
                    continue;
                }
                int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                if (blendShapeIndex < 0)
                {
                    if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("脸部或胡子或头发或眉毛找不到变形器<" + blendShapeName + ">", 10);
                    continue;
                }
                smr.SetBlendShapeWeight(blendShapeIndex, val);
            }
        }
        if (!NPCModel.m_listEyeExcludeBlendShape[(int)m_curHero.m_gender].Contains(blendShapeName))
        {
            List<SkinnedMeshRenderer> listEyeSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetEyeRenderer();
            if (listEyeSMR.Count > 0)
            {
                foreach (SkinnedMeshRenderer smr in listEyeSMR)
                {
                    if (smr == null || smr.sharedMesh == null)
                    {
                        GlobalAssist.ShowCenterTips("错误：眼睛smr为null", 10);
                        continue;
                    }
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("眼睛找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, val);
                }
            }
        }
        if (!NPCModel.m_listEyelashExcludeBlendShape[(int)m_curHero.m_gender].Contains(blendShapeName))
        {
            List<SkinnedMeshRenderer> listEyeSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetEyelashRenderer();
            if (listEyeSMR.Count > 0)
            {
                foreach (SkinnedMeshRenderer smr in listEyeSMR)
                {
                    if (smr == null || smr.sharedMesh == null)
                    {
                        GlobalAssist.ShowCenterTips("错误：眼睫毛smr为null", 10);
                        continue;
                    }
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("眼睫毛找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, val);
                }
            }
        }
        if (NPCModel.m_listBodyBlendShape[(int)m_curHero.m_gender].Contains(blendShapeName))
        {
            List<SkinnedMeshRenderer> listOtherSMR = HeroModelPreviewCtrl.m_modelCtrl[0].GetOtherRenderer();
            if (listOtherSMR.Count > 0)
            {
                foreach (SkinnedMeshRenderer smr in listOtherSMR)
                {
                    if (smr == null || smr.sharedMesh == null)
                    {
                        GlobalAssist.ShowCenterTips("错误：装备smr为null", 10);
                        continue;
                    }
                    int blendShapeIndex = smr.sharedMesh.GetBlendShapeIndex(blendShapeName);
                    if (blendShapeIndex < 0)
                    {
                        if (!m_toggleTurnOffError.isOn) GlobalAssist.ShowCenterTips("装备找不到变形器<" + blendShapeName + ">", 10);
                        continue;
                    }
                    smr.SetBlendShapeWeight(blendShapeIndex, val);
                }
            }
        }
        RefreshHeroBlendShape();//将变形器参数更新到NPC
    }

    public void SliderBodyChange()
    {
        m_textHeight.text = m_sliderHeight.value + "%";
        m_textNeck.text = m_sliderNeck.value + "%";
        m_textHead.text = m_sliderHead.value + "%";
        m_textHeadWidth.text = m_sliderHeadWidth.value + "%";
        m_textShoulder.text = m_sliderShoulder.value + "%";
        m_textThigh.text = m_sliderThigh.value + "%";
        m_textShank.text = m_sliderShank.value + "%";
        m_textSpringPower.text = (m_sliderSpringPower.value * 0.001f).ToString();
        m_textNeckWidth.text = m_sliderNeckWidth.value + "%";
        for (int i = 0; i < 3; i++)
        {
            if (i == 2) m_textEye[i].text = m_sliderEye[i].value.ToString("0.0000");
            else m_textEye[i].text = m_sliderEye[i].value.ToString();
        }
        if (m_curHero.m_modelInfo == null) return;
        m_curHero.m_modelInfo.m_heightFactor = m_sliderHeight.value;
        m_curHero.m_modelInfo.m_neckFactor = m_sliderNeck.value;
        m_curHero.m_modelInfo.m_headFactor = m_sliderHead.value;
        m_curHero.m_modelInfo.m_headWidthFactor = m_sliderHeadWidth.value;
        m_curHero.m_modelInfo.m_shoulderFactor = m_sliderShoulder.value;
        m_curHero.m_modelInfo.m_thighFactor = m_sliderThigh.value;
        m_curHero.m_modelInfo.m_shankFactor = m_sliderShank.value;
        m_curHero.m_modelInfo.m_springPower = m_sliderSpringPower.value * 0.001f;
        m_curHero.m_modelInfo.m_neckWidthFactor = m_sliderNeckWidth.value;
        for (int i = 0; i < 3; i++) m_curHero.m_modelInfo.m_eyePara[i] = m_sliderEye[i].value;
        HeroModelPreviewCtrl.m_modelCtrl[0].RefreshBodyScale(m_curHero);
        //RefreshWearingList();
    }
    public void ButtonHeightResetPress()
    {
        m_sliderHeight.value = 0;
    }
    public void ButtonNeckResetPress()
    {
        m_sliderNeck.value = 0;
    }
    public void ButtonNeckWidthResetPress()
    {
        m_sliderNeckWidth.value = 0;
    }
    public void ButtonHeadResetPress()
    {
        m_sliderHead.value = 0;
    }
    public void ButtonHeadWidthResetPress()
    {
        m_sliderHeadWidth.value = 0;
    }
    public void ButtonShoulderResetPress()
    {
        m_sliderShoulder.value = 0;
    }
    public void ButtonThighResetPress()
    {
        m_sliderThigh.value = 0;
    }
    public void ButtonShankResetPress()
    {
        m_sliderShank.value = 0;
    }
    public void ButtonSpringPowerResetPress()
    {
        m_sliderSpringPower.value = 20;
    }
    public void ButtonEyeResetPress(int index)
    {
        m_sliderEye[index].value = 0;
    }
    public void ToggleOnlyYChange()
    {
        if (m_curHero.m_modelInfo == null) return;
        m_curHero.m_modelInfo.m_isOnlyY = m_toggleOnlyY.isOn;
        HeroModelPreviewCtrl.m_modelCtrl[0].RefreshBodyScale(m_curHero);
    }
    public void SliderFieldOfViewChange()
    {
        m_textFieldOfView.text = m_sliderFieldOfView.value.ToString();
        Camera.main.fieldOfView = m_sliderFieldOfView.value;
    }
    public void ButtonFrontLookPress()
    {
        
    }
    public void ButtonSideLookPress()
    {
        
    }
    public void BodyTypeDropdownChange()
    {
//         NPCModel.m_dicModelInfo[(int)m_curHero.m_gender][NPCModel.m_listNPCModel[m_npcDropdown[0].value].m_id].m_bodyType = m_bodyTypeDropdown.value;
//         RefreshCurModelInfo();
    }
    Texture2D CaptureCamera()
    {
        Camera camera = HeroModelPreviewCtrl.m_modelCtrl[0].m_cameraHead;
        int width = 260;
        int height = 340;
        // 创建一个RenderTexture对象  
        RenderTexture rt = new RenderTexture(width, height, 0);
        // 临时设置相关相机的targetTexture为rt, 并手动渲染相关相机
        camera.targetTexture = rt;
        camera.Render();
        //ps: --- 如果这样加上第二个相机，可以实现只截图某几个指定的相机一起看到的图像
        //ps: camera2.targetTexture = rt;  
        //ps: camera2.Render();  
        //ps: -------------------------------------------------------------------  

        // 激活这个rt, 并从中中读取像素。  
        RenderTexture.active = rt;
        Texture2D screenShot = new Texture2D(width, height, TextureFormat.RGB24, false);
        screenShot.ReadPixels(new Rect(0, 0,width,height), 0, 0);// 注：这个时候，它是从RenderTexture.active中读取像素  
        screenShot.Apply();

        // 重置相关参数，以使用camera继续在屏幕上显示  
        camera.targetTexture = null;
        //ps: camera2.targetTexture = null;  
        RenderTexture.active = null; // JC: added to avoid errors  
        GameObject.Destroy(rt);
        // 最后将这些纹理数据，成一个png图片文件  
        byte[] bytes = screenShot.EncodeToPNG();
        string filename = Application.streamingAssetsPath + "/HeroIcon/" + m_curHero.m_id + ".png";
        System.IO.File.WriteAllBytes(filename, bytes);
        Debug.Log(string.Format("截屏了一张照片: {0}", filename));

        return screenShot;
    }
    //UI_Base接口的实现
    public bool IsRootUI()
    {
        return false;
    }
    public void SetParent(GameObject parentUI)
    {
        m_parentUI = parentUI;
    }
    public GameObject GetParent()
    {
        return m_parentUI;
    }
    public int GetSortOrder()//返回Canvas的sort order
    {
        return m_sortOrder;
    }
    public void SetSortOrder(int order)//设置order
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null) canvas.sortingOrder = order;
        m_sortOrder = order;
    }
}
