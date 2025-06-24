using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class PartMatInfo//配件材质颜色信息
{
    public string m_matName = "";//材质信息
    public Color m_color = Color.white;//材质颜色
    public float m_roughness = -1;//粗糙度
    public float m_metalic = -1;//金属度

    public static List<PartMatInfo> m_listPMI = new List<PartMatInfo>();
    public static PartMatInfo GetOne()
    {
        PartMatInfo pmi;
        if (m_listPMI.Count > 0)
        {
            pmi = m_listPMI[0];
            m_listPMI.RemoveAt(0);
        }
        else
        {
            pmi = new PartMatInfo();
        }
        return pmi;
    }
    public void Recycle()
    {
        if (m_listPMI.Contains(this)) return;
        m_listPMI.Add(this);
    }
    public void CopyFrom(PartMatInfo pmi)
    {
        m_matName = pmi.m_matName;
        m_color = pmi.m_color;
        m_roughness = pmi.m_roughness;
        m_metalic = pmi.m_metalic;
    }
    public void WriteToBinary(BinaryWriter bw)
    {
        bw.Write(m_matName);
        bw.Write(m_color.r);
        bw.Write(m_color.g);
        bw.Write(m_color.b);
        bw.Write(m_color.a);
        bw.Write(m_roughness);
        bw.Write(m_metalic);
    }
    public void ReadFromBinary(BinaryReader br, int versionNo)
    {
        m_matName = br.ReadString();
        float r = br.ReadSingle();
        float g = br.ReadSingle();
        float b = br.ReadSingle();
        float a = br.ReadSingle();
        m_color = new Color(r, g, b, a);
        m_roughness = br.ReadSingle();
        m_metalic = br.ReadSingle();
    }
}
public class ModelInfo
{
    public List<PartMatInfo> m_listBodyMatInfo = new List<PartMatInfo>();//身体材质信息
    public List<PartMatInfo> m_listEyeMatInfo = new List<PartMatInfo>();//眼睛材质信息
    public float m_heightFactor = 0;//0身高系数
    public bool m_isOnlyY = false;//身高系数是否只变化Y轴
    public float m_headFactor = 0;//头大小系数
    public float m_neckFactor = 0;//脖子长度系数
    public float m_headWidthFactor = 0;//头宽系数
    public float m_shoulderFactor = 0;//肩宽系数
    public float m_thighFactor = 0;//大腿长系数
    public float m_shankFactor = 0;//小腿长系数
    public float m_neckWidthFactor = 0;//脖子粗细系数
    public float[] m_eyePara = new float[3] { 0, 0, 0 };//眼睛系数，0-上下旋转角度，1左右旋转角度，2-前后位移
    public List<string> m_listPart = new List<string>();//配件列表
    public Dictionary<string, List<PartMatInfo>> m_dicPartListColor = new Dictionary<string, List<PartMatInfo>>();//配件材质列表颜色，key-配件名称
    public Dictionary<string, float> m_dicBlendShapeFactor = new Dictionary<string, float>();//变形器数值
    public float m_springPower = 0.02f;//胸部弹力，女性有用
    public void Clear()
    {
        m_listPart.Clear();
        m_dicPartListColor.Clear();
        m_dicBlendShapeFactor.Clear();
        m_listBodyMatInfo.Clear();
        m_listEyeMatInfo.Clear();
        m_heightFactor = 0;
        m_isOnlyY = false;
        m_headFactor = 0;
        m_neckFactor = 0;
        m_headWidthFactor = 0;
        m_shoulderFactor = 0;
        m_thighFactor = 0;
        m_shankFactor = 0;
        m_springPower = 0.02f;
        m_neckWidthFactor = 0;
        for (int i = 0; i < 3; i++) m_eyePara[i] = 0;
    }
    public void Copy(ModelInfo mi)
    {
        m_listBodyMatInfo.Clear();
        foreach (PartMatInfo pmi in mi.m_listBodyMatInfo)
        {
            PartMatInfo newPmi = new PartMatInfo();
            newPmi.m_matName = pmi.m_matName;
            newPmi.m_color = pmi.m_color;
            newPmi.m_roughness = pmi.m_roughness;
            newPmi.m_metalic = pmi.m_metalic;
            m_listBodyMatInfo.Add(newPmi);
        }
        m_listEyeMatInfo.Clear();
        foreach (PartMatInfo pmi in mi.m_listEyeMatInfo)
        {
            PartMatInfo newPmi = new PartMatInfo();
            newPmi.m_matName = pmi.m_matName;
            newPmi.m_color = pmi.m_color;
            newPmi.m_roughness = pmi.m_roughness;
            newPmi.m_metalic = pmi.m_metalic;
            m_listEyeMatInfo.Add(newPmi);
        }
        m_heightFactor = mi.m_heightFactor;
        m_isOnlyY = mi.m_isOnlyY;
        m_headFactor = mi.m_headFactor;
        m_neckFactor = mi.m_neckFactor;
        m_headWidthFactor = mi.m_headWidthFactor;
        m_shoulderFactor = mi.m_shoulderFactor;
        m_thighFactor = mi.m_thighFactor;
        m_shankFactor = mi.m_shankFactor;
        m_neckWidthFactor = mi.m_neckWidthFactor;
        for (int i = 0; i < 3; i++) m_eyePara[i] = mi.m_eyePara[i];
        m_listPart.Clear();
        m_listPart.AddRange(mi.m_listPart);
        m_dicPartListColor.Clear();
        foreach (string partName in mi.m_dicPartListColor.Keys)
        {
            List<PartMatInfo> listPmi = mi.m_dicPartListColor[partName];
            List<PartMatInfo> listNewPmi = new List<PartMatInfo>();
            foreach (PartMatInfo pmi in listPmi)
            {
                PartMatInfo newPmi = new PartMatInfo();
                newPmi.m_matName = pmi.m_matName;
                newPmi.m_color = pmi.m_color;
                newPmi.m_roughness = pmi.m_roughness;
                newPmi.m_metalic = pmi.m_metalic;
                listNewPmi.Add(newPmi);
            }
            m_dicPartListColor.Add(partName, listNewPmi);
        }
        m_dicBlendShapeFactor.Clear();
        foreach (string blendShape in mi.m_dicBlendShapeFactor.Keys) m_dicBlendShapeFactor.Add(blendShape, mi.m_dicBlendShapeFactor[blendShape]);
        m_springPower = mi.m_springPower;
    }
    public void WriteToBinary(BinaryWriter bw)
    {
        bw.Write(m_listBodyMatInfo.Count);
        foreach (PartMatInfo matInfo in m_listBodyMatInfo)
        {
            bw.Write(matInfo.m_matName);
            bw.Write(matInfo.m_color.r);
            bw.Write(matInfo.m_color.g);
            bw.Write(matInfo.m_color.b);
            bw.Write(matInfo.m_roughness);
            bw.Write(matInfo.m_metalic);
        }
        bw.Write(m_listEyeMatInfo.Count);
        foreach (PartMatInfo matInfo in m_listEyeMatInfo)
        {
            bw.Write(matInfo.m_matName);
            bw.Write(matInfo.m_color.r);
            bw.Write(matInfo.m_color.g);
            bw.Write(matInfo.m_color.b);
            bw.Write(matInfo.m_roughness);
            bw.Write(matInfo.m_metalic);
        }
        bw.Write(m_heightFactor);//身高系数
        bw.Write(m_isOnlyY);//身高知否只影响Y轴
        bw.Write(m_headFactor);//头大小系数
        bw.Write(m_neckFactor);//脖子长度系数
        bw.Write(m_listPart.Count);//写入配件数量
        foreach (string partName in m_listPart)
        {
            bw.Write(partName);//写入名称
        }
        bw.Write(m_dicPartListColor.Count);//写入配件颜色数量
        foreach (string partName in m_dicPartListColor.Keys)
        {
            bw.Write(partName);
            List<PartMatInfo> listMatInfo = m_dicPartListColor[partName];
            bw.Write(listMatInfo.Count);
            foreach (PartMatInfo matInfo in listMatInfo)
            {
                bw.Write(matInfo.m_matName);
                bw.Write(matInfo.m_color.r);
                bw.Write(matInfo.m_color.g);
                bw.Write(matInfo.m_color.b);
                bw.Write(matInfo.m_roughness);
                bw.Write(matInfo.m_metalic);
            }
        }
        bw.Write(m_dicBlendShapeFactor.Count);//写入变形器参数数量
        foreach (string blendShapeName in m_dicBlendShapeFactor.Keys)
        {
            bw.Write(blendShapeName);
            bw.Write(m_dicBlendShapeFactor[blendShapeName]);
        }
        bw.Write(m_headWidthFactor);//头宽系数
        bw.Write(m_shoulderFactor);//肩宽系数
        bw.Write(m_thighFactor);//大腿长系数
        bw.Write(m_shankFactor);//小腿长系数
        bw.Write(m_springPower);//胸部弹力
        bw.Write(m_neckWidthFactor);//脖子粗细系数
        bw.Write(m_eyePara[0]);//上下旋转角度
        bw.Write(m_eyePara[1]);//1左右旋转角度
        bw.Write(m_eyePara[2]);//前后位移
    }
    public void ReadFromBinary(BinaryReader br, int versionNo)
    {
        int bodyMatCount = br.ReadInt32();//身体材质数量
        m_listBodyMatInfo.Clear();
        for (int k = 0; k < bodyMatCount; k++)
        {
            PartMatInfo matInfo = new PartMatInfo();
            matInfo.m_matName = br.ReadString();
            matInfo.m_color.r = br.ReadSingle();
            matInfo.m_color.g = br.ReadSingle();
            matInfo.m_color.b = br.ReadSingle();
            matInfo.m_roughness = br.ReadSingle();
            matInfo.m_metalic = br.ReadSingle();
            m_listBodyMatInfo.Add(matInfo);
        }
        int eyeMatCount = br.ReadInt32();//眼睛材质数量
        m_listEyeMatInfo.Clear();
        for (int k = 0; k < eyeMatCount; k++)
        {
            PartMatInfo matInfo = new PartMatInfo();
            matInfo.m_matName = br.ReadString();
            matInfo.m_color.r = br.ReadSingle();
            matInfo.m_color.g = br.ReadSingle();
            matInfo.m_color.b = br.ReadSingle();
            matInfo.m_roughness = br.ReadSingle();
            matInfo.m_metalic = br.ReadSingle();
            m_listEyeMatInfo.Add(matInfo);
        }
        m_heightFactor = br.ReadSingle();
        m_isOnlyY = br.ReadBoolean();
        m_headFactor = br.ReadSingle();
        m_neckFactor = br.ReadSingle();
        int partCount = br.ReadInt32();
        m_listPart.Clear();
        for (int i = 0; i < partCount; i++)
        {
            string partName = br.ReadString();
            m_listPart.Add(partName);
        }
        int colorCount = br.ReadInt32();
        m_dicPartListColor.Clear();
        for (int i = 0; i < colorCount; i++)
        {
            string partName = br.ReadString();
            int matCount = br.ReadInt32();//材质数量
            List<PartMatInfo> listMatInfo = new List<PartMatInfo>();
            for (int k = 0; k < matCount; k++)
            {
                PartMatInfo matInfo = new PartMatInfo();
                matInfo.m_matName = br.ReadString();
                matInfo.m_color.r = br.ReadSingle();
                matInfo.m_color.g = br.ReadSingle();
                matInfo.m_color.b = br.ReadSingle();
                matInfo.m_roughness = br.ReadSingle();
                matInfo.m_metalic = br.ReadSingle();
                listMatInfo.Add(matInfo);
            }
            m_dicPartListColor.Add(partName, listMatInfo);
        }
        int blendShapeCount = br.ReadInt32();
        m_dicBlendShapeFactor.Clear();
        for (int i = 0; i < blendShapeCount; i++)
        {
            string blendShapeName = br.ReadString();
            float factor = br.ReadSingle();
            m_dicBlendShapeFactor.Add(blendShapeName, factor);
        }
        m_headWidthFactor = br.ReadSingle();
        m_shoulderFactor = br.ReadSingle();
        m_thighFactor = br.ReadSingle();
        m_shankFactor = br.ReadSingle();
        m_springPower = br.ReadSingle();
        m_neckWidthFactor = br.ReadSingle();
        m_eyePara[0] = br.ReadSingle();//上下旋转角度
        m_eyePara[1] = br.ReadSingle();//1左右旋转角度
        m_eyePara[2] = br.ReadSingle();//前后位移
    }
}
public class NPCModel
{
    public string m_id = "";
    public string m_name = "";

    //配件预览相关
    public static List<string> m_listPartTitleName = new List<string>();//用于在HeroModelPreviewCtrl、MakeEquipmentModelCtrl等预览，放这里方便统一调用
    public static List<string> m_listPartFileName = new List<string>();//用于在HeroModelPreviewCtrl、MakeEquipmentModelCtrl等预览，放这里方便统一调用
    public static List<string>[,] m_listPartName = new List<string>[2, 26];//非实例化的，用于在HeroModelPreviewCtrl、MakeEquipmentModelCtrl等预览，放这里方便统一调用
    public static List<string> m_listWeaponTitleName = new List<string>();//用于在MakeWeaponModelCtrl预览，放这里方便统一调用
    public static List<string> m_listWeaponFileName = new List<string>();//用于在MakeWeaponModelCtrl预览，放这里方便统一调用
    public static List<string>[] m_listWeaponName = new List<string>[8];//非实例化的，用于MakeWeaponModelCtrl预览，放这里方便统一调用
    private static Dictionary<string, GameObject> m_dicPartObjects = new Dictionary<string, GameObject>();//用Instantiate实例化的，用于在HeroModelPreviewCtrl、MakeEquipmentModelCtrl等预览，放这里方便统一调用
    private static Dictionary<string, GameObject> m_dicPart = new Dictionary<string, GameObject>();//没实例化的
    public static string[] m_genderFolder = new string[2] { "ModelPartMale", "ModelPartFemale" };
    //变形器相关
    public static List<string>[] m_listBlendShapeAll = new List<string>[2];//全部
    public static List<string>[] m_listBlendShapeFace = new List<string>[2];//脸
    public static List<string>[] m_listBlendShapeEyebrow = new List<string>[2];//眉毛
    public static List<string>[] m_listBlendShapeEye = new List<string>[2];//眼睛
    public static List<string>[] m_listBlendShapeNose = new List<string>[2];//鼻子
    public static List<string>[] m_listBlendShapeMouth = new List<string>[2];//嘴巴
    public static List<string>[] m_listBlendShapeBody = new List<string>[2];//身材

    public static Dictionary<string, string> m_dicBlendShapeDescription = new Dictionary<string, string>();//key-变形器真实名称，value-显示名称
    public static List<string>[] m_listEyelashExcludeBlendShape = new List<string>[2] { new List<string>(), new List<string>() };//眼睫毛排除的变形器
    public static List<string>[] m_listEyeExcludeBlendShape = new List<string>[2] { new List<string>(), new List<string>() };//眼睛排除的变形器
    public static List<string>[] m_listBodyBlendShape = new List<string>[2] { new List<string>(), new List<string>() };//全身通用的变形器

    public static void InitPartInfo()
    {
        if (m_listPartTitleName.Count > 0) return;//避免重复初始化
        m_listPartTitleName.Clear();
        m_listPartTitleName.Add("头发");
        m_listPartTitleName.Add("头饰");
        m_listPartTitleName.Add("帽子");
        m_listPartTitleName.Add("胡子");
        m_listPartTitleName.Add("项链");
        m_listPartTitleName.Add("人体模型");
        m_listPartTitleName.Add("内衣");
        m_listPartTitleName.Add("外衣");
        m_listPartTitleName.Add("袖子");
        m_listPartTitleName.Add("背心甲");
        m_listPartTitleName.Add("胸甲");
        m_listPartTitleName.Add("护腕");
        m_listPartTitleName.Add("肩甲");
        m_listPartTitleName.Add("肩带");
        m_listPartTitleName.Add("披肩");
        m_listPartTitleName.Add("抱肚");
        m_listPartTitleName.Add("腰带");
        m_listPartTitleName.Add("肚前皮甲");
        m_listPartTitleName.Add("前披");
        m_listPartTitleName.Add("腿甲");
        m_listPartTitleName.Add("腿后披甲");
        m_listPartTitleName.Add("裤子");
        m_listPartTitleName.Add("鞋");
        m_listPartTitleName.Add("披风");
        m_listPartTitleName.Add("武器");
        m_listPartTitleName.Add("其它");
        GetListPartFileName(ref m_listPartFileName);
        for (int gender = 0; gender < 2; gender++)
        {
            string folderName = m_genderFolder[gender];
            for (int i = 0; i < 26; i++)
            {
                m_listPartName[gender, i] = new List<string>();
                string filePath = Application.streamingAssetsPath + "/AssetBundlesEquipment/" + folderName + "/" + m_listPartFileName[i] + ".dat";
                BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
                if (br == null) continue;
                int fileCount = br.ReadInt32();
                for (int k = 0; k < fileCount; k++)
                {
                    string modelName = br.ReadString();
                    modelName = modelName.Replace(".prefab", "");
                    m_listPartName[gender, i].Add(modelName);
                }
                br.Close();
            }
        }
    }
    public static void InitWeaponInfo()
    {
        if (m_listWeaponTitleName.Count > 0) return;//避免重复初始化
        m_listWeaponTitleName.Clear();
        m_listWeaponTitleName.Add("长柄穿刺");
        m_listWeaponTitleName.Add("长柄挥砍");
        m_listWeaponTitleName.Add("短柄穿刺");
        m_listWeaponTitleName.Add("短柄挥砍");
        m_listWeaponTitleName.Add("远程");
        m_listWeaponTitleName.Add("文官");
        m_listWeaponTitleName.Add("艺术");
        m_listWeaponTitleName.Add("盾");
        GetListWeaponFileName(ref m_listWeaponFileName);
        for (int i = 0; i < 8; i++)
        {
            m_listWeaponName[i] = new List<string>();
            string filePath = Application.streamingAssetsPath + "/AssetBundlesEquipment/Weapon/" + m_listWeaponFileName[i] + ".dat";
            if (!File.Exists(filePath)) continue;
            BinaryReader br = new BinaryReader(new FileStream(filePath, FileMode.Open));
            if (br == null) continue;
            int fileCount = br.ReadInt32();
            for (int k = 0; k < fileCount; k++)
            {
                string modelName = br.ReadString();
                modelName = modelName.Replace(".prefab", "");
                m_listWeaponName[i].Add(modelName);
            }
            br.Close();
        }
    }
    public static GameObject GetPreviewPart(string partName)
    {
        if (m_dicPartObjects.ContainsKey(partName))
        {
            return m_dicPartObjects[partName];
        }
        GameObject model = GameAssets.LoadAsset<GameObject>("Assets/Resource/" + partName + ".prefab");
        if (model == null) return null;
        GameObject modelPreview = GameObject.Instantiate(model);
        GameObject.DontDestroyOnLoad(modelPreview);
        modelPreview.transform.position = new Vector3(-0.6f, 0, 0);
        m_dicPartObjects.Add(partName, modelPreview);
        return modelPreview;
    }
    public static GameObject GetPart(string partName)
    {
        if (m_dicPart.ContainsKey(partName)) return m_dicPart[partName];
        GameObject model = GameAssets.LoadAsset<GameObject>("Assets/Resource/" + partName + ".prefab");
        if (model == null) return null;
        m_dicPart.Add(partName, model);
        return model;
    }
    public static void GetListPartFileName(ref List<string> listPartFileName)
    {
        listPartFileName.Clear();
        listPartFileName.Add("01头发");
        listPartFileName.Add("02女性头饰");
        listPartFileName.Add("03帽子");
        listPartFileName.Add("04胡子");
        listPartFileName.Add("05项链");
        listPartFileName.Add("06人体模型");
        listPartFileName.Add("07内衣");
        listPartFileName.Add("08外衣");
        listPartFileName.Add("09袖子");
        listPartFileName.Add("10背心甲");
        listPartFileName.Add("11胸甲");
        listPartFileName.Add("12护腕");
        listPartFileName.Add("13肩甲");
        listPartFileName.Add("14肩带");
        listPartFileName.Add("15披肩");
        listPartFileName.Add("16抱肚");
        listPartFileName.Add("17腰带");
        listPartFileName.Add("18肚前皮甲");
        listPartFileName.Add("19前披");
        listPartFileName.Add("20腿甲");
        listPartFileName.Add("21腿后披甲");
        listPartFileName.Add("22裤子");
        listPartFileName.Add("23鞋");
        listPartFileName.Add("24披风");
        listPartFileName.Add("25武器");
        listPartFileName.Add("26其它");
    }
    public static void GetListWeaponFileName(ref List<string> listPartFileName)
    {
        listPartFileName.Clear();
        listPartFileName.Add("01长柄穿刺");
        listPartFileName.Add("02长柄挥砍");
        listPartFileName.Add("03短柄穿刺");
        listPartFileName.Add("04短柄挥砍");
        listPartFileName.Add("05远程");
        listPartFileName.Add("06文官");
        listPartFileName.Add("07艺术");
        listPartFileName.Add("08盾");
    }

    static void AddOneMaleBlendShape(int type, string name, string description)//type=1-脸，2-眉毛，3-眼睛，4-鼻子，5-嘴巴，6-身材
    {
        if (m_listBlendShapeAll[0].Contains(name))
        {
            GlobalAssist.ShowCenterTips("错误：变形器" + name + "重复添加", 50);
            return;
        }
        m_listBlendShapeAll[0].Add(name);
        switch (type)
        {
            case 1: m_listBlendShapeFace[0].Add(name); break;
            case 2: m_listBlendShapeEyebrow[0].Add(name); break;
            case 3: m_listBlendShapeEye[0].Add(name); break;
            case 4: m_listBlendShapeNose[0].Add(name); break;
            case 5: m_listBlendShapeMouth[0].Add(name); break;
            case 6: m_listBlendShapeBody[0].Add(name); break;
        }
        m_dicBlendShapeDescription.Add(name, description);
    }
    public static void InitMaleBlendShapeName()
    {
        if (m_listBlendShapeAll[0] != null) return;
        //变形器名称
        m_listBlendShapeAll[0] = new List<string>();
        m_listBlendShapeFace[0] = new List<string>();
        m_listBlendShapeEyebrow[0] = new List<string>();
        m_listBlendShapeEye[0] = new List<string>();
        m_listBlendShapeNose[0] = new List<string>();
        m_listBlendShapeMouth[0] = new List<string>();
        m_listBlendShapeBody[0] = new List<string>();
        //变形器中文描述
        AddOneMaleBlendShape(1, "Genesis8Male__ForeHead Style Swept", "额头");
        AddOneMaleBlendShape(1, "Genesis8Male__ForeHead Flatten", "额头后缩02");
        AddOneMaleBlendShape(1, "Genesis8Male__ForeHead Style Indented", "额头眉上凹陷01");
        AddOneMaleBlendShape(1, "Genesis8Male__ForeHead Indent", "额头眉上凹陷02");
        AddOneMaleBlendShape(1, "Genesis8Male__额头扁平-G", "额头扁平");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Full Depth", "眉毛深度");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Rotate 02", "眉毛整体旋转");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Inner Heights", "眉毛内部高度");
        AddOneMaleBlendShape(2, "Genesis8Male__Brows Distance", "眉弓外扩");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Thinner", "眉毛整体变薄");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Mid Heights", "眉毛中部高度");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Outer Heights", "眉毛外部高度");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Heights", "眉毛整体高度");
        AddOneMaleBlendShape(2, "Genesis8Male__EJFM Brows Center Down", "眉毛内侧");
        AddOneMaleBlendShape(2, "Genesis8Male__EJFM Brows Arched", "眉毛中部");
        AddOneMaleBlendShape(2, "Genesis8Male__EJFM Brows Arched Out", "眉毛外侧");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Inner Distance", "眉毛内部距离");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Mid Depth", "眉毛中部深度");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Inner Depth", "眉毛内部深度");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Outer Strengthen", "眉毛外部加强");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Outer Width", "眉毛外部加宽");
        AddOneMaleBlendShape(2, "Genesis8Male__RSM Brow Outer Depth", "眉毛外部深度");
        AddOneMaleBlendShape(2, "Genesis8Male__眉毛下降-G", "眉毛下降");

        AddOneMaleBlendShape(3, "Genesis8Male__EJRNDMOM_Eyes_Round_PART", "眼睛圆形");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Eyes Distance", "眼睛距离1");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Height A", "眼睛高度");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Slant In", "眼睛整体向内下旋转");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Depth (Alt)", "眼睛深度");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Distance", "眼睛距离2");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Scale", "眼睛比例");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Eyes Asian", "单眼皮");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Eyeslids Upper Rotate", "重睑线旋转");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Outer Height", "外眦点高度");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Inner Height", "内眦点高度");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Eyes Lower Curve 01", "眼下曲线");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Upper Eyes Curve 02", "眼上曲线");
        AddOneMaleBlendShape(3, "Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperInnerStyle1_PART", "上眼曲线内侧");
        AddOneMaleBlendShape(3, "Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperInnerStyle2_PART", "上眼曲线中部");
        AddOneMaleBlendShape(3, "Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperOuterStyle1_PART", "上眼曲线外侧");
        AddOneMaleBlendShape(3, "Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperInnerStyle3_PART", "上眼曲线内侧平移");
        AddOneMaleBlendShape(3, "Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperOuterStyle2_PART", "上眼曲线外侧平移");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Eyes Lower Puffy 01", "卧蚕");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Eyes Iris Size", "虹膜大小");
        AddOneMaleBlendShape(3, "Genesis8Male__Natural Eyes HD Iris", "瞳孔大小");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Lower Inner", "下眼曲线内侧");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Lower Middle A", "下眼曲线中外侧");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Lower Middle B", "下眼曲线外侧");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Lower Eyes Curve 02", "眼下曲线2");
        AddOneMaleBlendShape(3, "Genesis8Male__Eye Lids Depth", "眼睑深度");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Under Eye Bags", "眼袋");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Eyelids Lower Volume Wide", "眼袋2");
        AddOneMaleBlendShape(3, "Genesis8Male__Eye Lids Bottom Sag", "眼袋增大");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Inner Width", "内眦点向内推");
        AddOneMaleBlendShape(3, "Genesis8Male__Eyes Outer Width", "外眦点外扩");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Upper Eyes Curve 01", "曲线-中内成角");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Upper Eyes Inner ADJ 01", "内侧曲线 01");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Upper Eyes Inner ADJ 02", "内侧曲线 02");
        AddOneMaleBlendShape(3, "Genesis8Male__RSM Eyelids Upper Curve 02", "重睑线中段");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Eyes Corner Outer Down", "外眼角加重睑线高度");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Eyelids Upper Heavy", "外侧眉骨突出");
        AddOneMaleBlendShape(3, "Genesis8Male__EJFM Eyelids Upper Inner Style 01", "上眼曲线内侧高度2");
        AddOneMaleBlendShape(3, "Genesis8Male__卧蚕-G", "卧蚕");
        AddOneMaleBlendShape(3, "Genesis8Male__重睑线下降-G", "重睑线下降");

        AddOneMaleBlendShape(4, "Genesis8Male__Nose Lower Height", "鼻子长度");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Wing Height", "鼻翼高度");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Scale", "鼻子大小");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Depth (Alt)", "鼻子总体深度");
        AddOneMaleBlendShape(4, "Genesis8Male__RSM Nasolabial Stronger", "鼻唇沟");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Wing Width", "鼻翼宽度");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Angle Up", "鼻子上翻");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Angle Down", "鼻子下翻");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Width Center Line", "鼻梁宽度");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Bridge Width (Alt)", "鼻梁上部宽度");
        AddOneMaleBlendShape(4, "Genesis8Male__EJFM Nose Ridge Width Variation", "鼻梁骨形状");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Wing Upper Height", "鼻翼上部高度");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Wing Arc", "鼻翼旋转");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Bridge Depth (Alt)", "鼻根深度");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Curve Out", "鼻子曲线前突");
        AddOneMaleBlendShape(4, "Genesis8Male__Nose Curve In", "鼻子曲线后缩");
        AddOneMaleBlendShape(4, "Genesis8Male__RSM Nose Bump 01", "驼峰深度");
        AddOneMaleBlendShape(4, "Genesis8Male__鼻梁深度4", "鼻根深度");
        AddOneMaleBlendShape(4, "Genesis8Male__bigenhenji", "鼻根痕迹");
        AddOneMaleBlendShape(4, "Genesis8Male__笔尖翘起", "鼻尖深度");
        AddOneMaleBlendShape(4, "Genesis8Male__鼻根深度-G", "鼻根深度");
        AddOneMaleBlendShape(4, "Genesis8Male__鼻根侧回缩-G", "鼻根侧回缩");

        AddOneMaleBlendShape(1, "Genesis8Male__Ear Big", "耳朵增大");

        AddOneMaleBlendShape(5, "Genesis8Male__Mouth Width (Alt)", "嘴宽度");
        AddOneMaleBlendShape(5, "Genesis8Male__Mouth Height (Alt)", "嘴高度");
        AddOneMaleBlendShape(5, "Genesis8Male__Lips Thin (Alt)", "嘴唇厚薄");
        AddOneMaleBlendShape(5, "Genesis8Male__Lips Center Vertical 1", "嘴唇中部下降");
        AddOneMaleBlendShape(5, "Genesis8Male__Mouth Corners Vertical Adjust", "嘴角垂直调整");
        AddOneMaleBlendShape(5, "Genesis8Male__Mouth Corner Tips", "嘴角尖");
        AddOneMaleBlendShape(5, "Genesis8Male__Mouth Small", "嘴整体大小");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Upper Enlarge", "上唇厚度");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Upper Outer Curves A", "上唇曲线左右");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Upper Outer Curves B", "上唇曲线上下");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Lower Curves A", "下唇曲线");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Lower Curves B", "下唇曲线外侧");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Lower Curves C", "下唇曲线内侧");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Lower Height", "下唇厚度");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Lower Volume", "下唇饱满");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Upper Peaks Smooth", "人中");
        AddOneMaleBlendShape(5, "Genesis8Male__EJFM Lip Upper Inner Curves", "上唇中曲线高度");
        AddOneMaleBlendShape(5, "Genesis8Male__RSM Lips Upper Outer Fuller", "上唇下线外侧高度");
        AddOneMaleBlendShape(5, "Genesis8Male__Lip Upper Volume", "上唇厚度（向下）");
        AddOneMaleBlendShape(5, "Genesis8Male__EJFM Chin Lip Crease", "唇下深度");
        AddOneMaleBlendShape(5, "Genesis8Male__EJFM Chin Lip Height", "唇下高度");

        AddOneMaleBlendShape(1, "Genesis8Male__EJFM Jaw Strong", "下颌角外扩");
        AddOneMaleBlendShape(1, "Genesis8Male__EJFM Jaw Square", "下颌曲线");
        AddOneMaleBlendShape(1, "Genesis8Male__EJFM Jaw Round Small", "下巴回缩");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Chin Depth 03", "下巴厚度下");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Chin Depth 02", "下巴厚度中下");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Chin Depth 01", "下巴厚度全");
        AddOneMaleBlendShape(1, "Genesis8Male__Chin Depth", "下巴深度");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Chin Pointed", "下巴宽度");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Chin Heights", "下巴长度");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Width", "下颚宽度");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Corner Width (Alt)", "下颚角宽度");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Height (Alt)", "下颚高度");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Depth", "下颚深度");
        AddOneMaleBlendShape(1, "Genesis8Male__Face Lower Depth", "下面部深度");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Corner Height", "下颚角高度");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM JawLines Under Depth", "下颚底部回缩");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Middle Width", "下颌曲线（宽窄）");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Curve (Alt)", "下颌曲线（高低）");
        AddOneMaleBlendShape(1, "Genesis8Male__Jaw Shape Angular", "下颌角骨感");
        AddOneMaleBlendShape(1, "Genesis8Male__EJFM Jaw Angle High", "下颌高度");

        AddOneMaleBlendShape(1, "Genesis8Male__RSM Cheekbones Enlarge", "颧骨增大1");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Cheekbones Enlarge 02", "颧骨增大2");
        AddOneMaleBlendShape(1, "Genesis8Male__Cheeks Chubby Lower A", "面部肥胖");
        AddOneMaleBlendShape(1, "Genesis8Male__EJFM Neck Width", "脖子粗细");
        AddOneMaleBlendShape(1, "Genesis8Male__Face Outer Depth", "面部外侧深度");
        AddOneMaleBlendShape(1, "Genesis8Male__RSM Cheeks Front and Back", "面部内侧深度");
        AddOneMaleBlendShape(1, "Genesis8Male__Face Middle Depth", "中面部深度");
        AddOneMaleBlendShape(1, "Genesis8Male__Face Upper Depth", "上面部深度");

        AddOneMaleBlendShape(6, "Genesis8Male__50肥胖", "肥胖");
        AddOneMaleBlendShape(6, "Genesis8Male__51瘦削", "瘦削");
        AddOneMaleBlendShape(6, "Genesis8Male__52健美", "健美");
        AddOneMaleBlendShape(6, "Genesis8Male__53健壮", "健壮");
        AddOneMaleBlendShape(6, "Genesis8Male__60腰围", "腰围");

        AddOneMaleBlendShape(0, "Genesis8Male__22伤心", "###伤心");
        AddOneMaleBlendShape(0, "Genesis8Male__13沉思", "###沉思");
        AddOneMaleBlendShape(0, "Genesis8Male__12坏笑", "###坏笑");
        AddOneMaleBlendShape(0, "Genesis8Male__10微笑", "###微笑");
        AddOneMaleBlendShape(0, "Genesis8Male__01凝眉", "###凝眉");
        AddOneMaleBlendShape(0, "Genesis8Male__Faces of a Ninja - Surprised 05 Lee 8", "###惊恐1");
        AddOneMaleBlendShape(0, "Genesis8Male__eCTRLEyesClosedR", "###闭眼R");
        AddOneMaleBlendShape(0, "Genesis8Male__eCTRLEyesClosedL", "###闭眼L");
        AddOneMaleBlendShape(0, "Genesis8Male__Emotional Guy - Surprised 08 Owen 8", "###惊恐2");
        AddOneMaleBlendShape(0, "Genesis8Male__Emotional Guy - Happy 08 Owen 8", "###大笑");
        AddOneMaleBlendShape(0, "Genesis8Male__eCTRLMouthOpen", "###张嘴1");
        AddOneMaleBlendShape(0, "Genesis8Male__eCTRLEyesClosed", "###闭眼");
        AddOneMaleBlendShape(0, "Genesis8Male__邓艾脸", "###邓艾脸");
        AddOneMaleBlendShape(0, "Genesis8Male__1-姜维脸-zfr", "###姜维脸");
        AddOneMaleBlendShape(0, "Genesis8Male__2-新武将2脸-zfr", "###新武将2脸");
        AddOneMaleBlendShape(0, "Genesis8Male__3-新武将3脸-zfr", "###新武将3脸");
        AddOneMaleBlendShape(0, "Genesis8Male__4-72脸-zfr", "###72脸");
        AddOneMaleBlendShape(0, "Genesis8Male__周仓脸-G", "###周仓脸");
        AddOneMaleBlendShape(0, "Genesis8Male__Modern Man - Angry 02 Diego 8", "###愤怒1");
        AddOneMaleBlendShape(0, "Genesis8Male__Emotional Guy - Angry 05 Owen 8", "###愤怒2");
        AddOneMaleBlendShape(0, "Genesis8Male__FHM-FWAldo", "###变老");
        AddOneMaleBlendShape(0, "Genesis8Male__亚洲人脸-G zfr", "###亚洲人脸");
        AddOneMaleBlendShape(0, "Genesis8Male__Youth Morph", "###小孩");

        //眼睛排除的变形器
        m_listEyeExcludeBlendShape[0] = new List<string>();
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__22伤心");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__13沉思");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__12坏笑");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__10微笑");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__01凝眉");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__Faces of a Ninja - Surprised 05 Lee 8");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__eCTRLEyesClosedR");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__eCTRLEyesClosedL");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__Emotional Guy - Surprised 08 Owen 8");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__Emotional Guy - Happy 08 Owen 8");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__eCTRLMouthOpen");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__eCTRLEyesClosed");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__Eyes Outer Height");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__Eyes Inner Height");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperInnerStyle1_PART");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperInnerStyle2_PART");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperOuterStyle1_PART");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperInnerStyle3_PART");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__EJRNDMOM_Eyes_EyelidsUpperOuterStyle2_PART");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__鼻根深度-G");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__额头扁平-G");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__眉毛下降-G");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__卧蚕-G");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__重睑线下降-G");
        m_listEyeExcludeBlendShape[0].Add("Genesis8Male__鼻根侧回缩-G");
        //眼睫毛排除的变形器
        m_listEyelashExcludeBlendShape[0] = new List<string>();
        m_listEyelashExcludeBlendShape[0].Add("Genesis8Male__鼻根深度-G");
        m_listEyelashExcludeBlendShape[0].Add("Genesis8Male__额头扁平-G");
        m_listEyelashExcludeBlendShape[0].Add("Genesis8Male__眉毛下降-G");
        m_listEyelashExcludeBlendShape[0].Add("Genesis8Male__卧蚕-G");
        m_listEyelashExcludeBlendShape[0].Add("Genesis8Male__重睑线下降-G");
        m_listEyelashExcludeBlendShape[0].Add("Genesis8Male__鼻根侧回缩-G");
        //全身通用的变形器
        m_listBodyBlendShape[0] = new List<string>();
        m_listBodyBlendShape[0].Add("Genesis8Male__50肥胖");
        m_listBodyBlendShape[0].Add("Genesis8Male__51瘦削");
        m_listBodyBlendShape[0].Add("Genesis8Male__52健美");
        m_listBodyBlendShape[0].Add("Genesis8Male__53健壮");
        m_listBodyBlendShape[0].Add("Genesis8Male__60腰围");
        m_listBodyBlendShape[0].Add("Genesis8Male__EJFM Neck Width");
    }
    static void AddOneFemaleBlendShape(int type, string name, string description)//type=1-脸，2-眉毛，3-眼睛，4-鼻子，5-嘴巴，6-身材
    {
        if (m_listBlendShapeAll[1].Contains(name))
        {
            GlobalAssist.ShowCenterTips("错误：变形器" + name + "重复添加", 50);
            return;
        }
        m_listBlendShapeAll[1].Add(name);
        switch (type)
        {
            case 1: m_listBlendShapeFace[1].Add(name); break;
            case 2: m_listBlendShapeEyebrow[1].Add(name); break;
            case 3: m_listBlendShapeEye[1].Add(name); break;
            case 4: m_listBlendShapeNose[1].Add(name); break;
            case 5: m_listBlendShapeMouth[1].Add(name); break;
            case 6: m_listBlendShapeBody[1].Add(name); break;
        }
        m_dicBlendShapeDescription.Add(name, description);
    }
    public static void InitFemaleBlendShapeName()
    {
        if (m_listBlendShapeAll[1] != null) return;
        //变形器名称
        m_listBlendShapeAll[1] = new List<string>();
        m_listBlendShapeFace[1] = new List<string>();
        m_listBlendShapeEyebrow[1] = new List<string>();
        m_listBlendShapeEye[1] = new List<string>();
        m_listBlendShapeNose[1] = new List<string>();
        m_listBlendShapeMouth[1] = new List<string>();
        m_listBlendShapeBody[1] = new List<string>();
        //变形器中文描述
        AddOneFemaleBlendShape(1, "Genesis8Female__ForeHead Flatten", "额头后缩02");
        AddOneFemaleBlendShape(1, "Genesis8Female__ForeHead Depth", "额头深度 zetou");
        AddOneFemaleBlendShape(2, "Genesis8Female__Brow Inner Heights", "眉头高度");
        AddOneFemaleBlendShape(2, "Genesis8Female__Brow Middle Heights", "眉中高度");
        AddOneFemaleBlendShape(2, "Genesis8Female__Brows Outer Heights (Alt)", "眉尾高度");
        AddOneFemaleBlendShape(2, "Genesis8Female__Brows Heights", "眉弓高度");
        AddOneFemaleBlendShape(2, "Genesis8Female__Brows Depth 02", "眉弓深度");
        AddOneFemaleBlendShape(2, "Genesis8Female__Brows Inner Distance", "眉心宽度");
        AddOneFemaleBlendShape(2, "Genesis8Female__眉毛变粗 brow", "眉毛变粗 brow");
        AddOneFemaleBlendShape(2, "Genesis8Female__眉头下降 brow", "眉头下降 brow");
        AddOneFemaleBlendShape(2, "Genesis8Female__EJRNDMO_Brows_Arched Out_PART", "眉尾尖高度");
        AddOneFemaleBlendShape(2, "Genesis8Female__EJRNDMO_Brows_Arched_PART", "眉尾转折高度");
        AddOneFemaleBlendShape(2, "Genesis8Female__EJRNDMO_Brows_Center Down_PART", "眉头高度2");
        AddOneFemaleBlendShape(2, "Genesis8Female__眉弓高低-G", "眉弓高低-G");
        AddOneFemaleBlendShape(2, "Genesis8Female__CJ_Brow02", "眉毛旋转1");
        AddOneFemaleBlendShape(2, "Genesis8Female__CJ_Brow05", "眉毛旋转2");
        AddOneFemaleBlendShape(2, "Genesis8Female__EJRNDMO_Brows_Estilo4_STYLE", "眉毛变平");
        AddOneFemaleBlendShape(2, "Genesis8Female__EJRNDMO_Brows_Length In_PART", "眉间距离");
        AddOneFemaleBlendShape(2, "Genesis8Female__眉毛变薄-G", "眉毛变薄-G");

        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Round Eyes_PART", "眼睛变圆");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesAlmondInner", "内眼角左右");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesAngledInner", "内眼角高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesAlmondOuter", "外眼角左右");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesAngledOuter", "外眼角高度（带重睑线）");
        AddOneFemaleBlendShape(3, "Genesis8Female__Eyes Outer Corners Height", "外眼角向上");
        AddOneFemaleBlendShape(3, "Genesis8Female__Eyes Outer Corners Drop", "外眼角向下");
        AddOneFemaleBlendShape(3, "Genesis8Female__Eyes Inner Width", "内眼角距离");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesAngled", "眼睛角度");
        AddOneFemaleBlendShape(3, "Genesis8Female__Eyes Depth (Alt)", "眼睛深度");
        AddOneFemaleBlendShape(3, "Genesis8Female__yanpixiajiang", "上眼皮高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Upper Eyelids Inner Style 03_PART", "上眼曲线内侧平移");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Upper Eyelids Inner Style 02_PART", "上眼曲线内侧高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__Eyelids Upper Rotate", "上眼曲线旋转");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyelidsUpperHeight", "重睑线高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyelidsSmooth", "重睑线消失");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Lower Eyelids Inner Height_PART", "下眼皮内部高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Lower Eyelids Outer Height_PART", "下眼皮外部高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Lower Eyelids Center Height_PART", "下眼皮中部高度");
        AddOneFemaleBlendShape(3, "Genesis8Female__卧蚕-新", "卧蚕");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Upper Eyelids Outer Style 01_PART", "110 眼上曲线3（中外） zupeye");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Upper Eyelids Outer Style 03_PART", "110 眼上曲线4（外侧） zupeye");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesHeight", "眼部高度 zfr");
        AddOneFemaleBlendShape(3, "Genesis8Female__PHMEyesSize", "眼睛大小 zfr");
        AddOneFemaleBlendShape(3, "Genesis8Female__Eyes Distance", "眼睛距离");
        AddOneFemaleBlendShape(3, "Genesis8Female__CTRLEyesIrisSize", "瞳孔大小");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Sunken Lower Eyelid_PART", "卧蚕2");
        AddOneFemaleBlendShape(3, "Genesis8Female__Brows Lower Depth", "眼睑深度");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Eyelids Lower Volume Wide_PART", "卧蚕2");
        AddOneFemaleBlendShape(3, "Genesis8Female__重睑线旋转-G", "重睑线旋转-G");
        AddOneFemaleBlendShape(3, "Genesis8Female__上眼曲线内折角-G", "上眼曲线内折角-G");
        AddOneFemaleBlendShape(3, "Genesis8Female__EJRNDMO_Eyes_Ojos2_STYLE", "重睑线内折角");

        AddOneFemaleBlendShape(1, "Genesis8Female__PHMEarsSize", "耳朵尺寸");

        AddOneFemaleBlendShape(4, "Genesis8Female__PHMNoseFleshSize", "鼻子大小");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Depth (Alt)", "鼻子整体深度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Size Depth", "鼻头深度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Wing Width", "鼻翼宽度");
        AddOneFemaleBlendShape(4, "Genesis8Female__PHMNoseBridgeHeight", "鼻梁高度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Bridge Width (Alt)", "鼻梁宽度（上部）");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Width Center Line", "鼻背宽度");
        AddOneFemaleBlendShape(4, "Genesis8Female__PHMNoseBridgeDepth", "鼻梁深度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Curve In", "鼻子曲线向里");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Angle Up", "鼻头向上 znose");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Angle Down", "鼻头向下 znose");
        AddOneFemaleBlendShape(4, "Genesis8Female__PHMNoseSeptumHeight", "鼻中隔高度 znose");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Lower Height", "鼻子长短");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Wing Height", "鼻翼高度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Wing Arc", "鼻翼前曲线旋转");
        AddOneFemaleBlendShape(4, "Genesis8Female__Face Center Narrow", "鼻子嘴变窄");
        AddOneFemaleBlendShape(4, "Genesis8Female__NoseTip Details 03", "鼻尖宽度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nostrils Front Height", "鼻孔前鼻翼高度");
        AddOneFemaleBlendShape(4, "Genesis8Female__Nose Wing Volume", "鼻翼大小");
        AddOneFemaleBlendShape(4, "Genesis8Female__PHMNoseSeptumWidth", "鼻小柱宽度");

        AddOneFemaleBlendShape(5, "Genesis8Female__PHMMouthSize", "嘴大小");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMMouthHeight", "嘴高度");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMMouthWidth", "嘴宽度");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lips Thin (Alt)", "嘴薄厚");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMLipTopPeak", "唇峰显示");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lips Center Vertical 1", "唇中垂直高度");
        AddOneFemaleBlendShape(5, "Genesis8Female__Mouth Corner Tips", "嘴角高度");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMLipUpperSize", "上唇厚度");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lip Upper Volume", "上唇饱满");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMLipUpperCurves", "上唇外曲线");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lip Line Curves Inner", "唇中内曲线");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lip Line Curves Outer", "唇中外曲线");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lips Lower Shape 02", "下唇厚度");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lip Lower Curves B", "下唇外曲线");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lip Lower Curves A", "下唇内曲线");
        AddOneFemaleBlendShape(5, "Genesis8Female__eCTRLMouthOpen", "张嘴");
        AddOneFemaleBlendShape(5, "Genesis8Female__Mouth Angles", "嘴旋转");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMLipDepthUpper", "上唇深度");
        AddOneFemaleBlendShape(5, "Genesis8Female__PHMLipDepthLower", "下唇深度");
        AddOneFemaleBlendShape(5, "Genesis8Female__Lip Upper Peaks Distance", "人中距离");

        AddOneFemaleBlendShape(1, "Genesis8Female__Chin Scale", "下巴大小");
        AddOneFemaleBlendShape(1, "Genesis8Female__Chin Forward Back", "下巴深度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Chin In and Out", "下面部深度");
        AddOneFemaleBlendShape(1, "Genesis8Female__PHMJawlineDepth", "收下巴赘肉");
        AddOneFemaleBlendShape(1, "Genesis8Female__Chin Width (Alt)", "下巴宽度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jaw Corner Height", "下鄂角高度");
        AddOneFemaleBlendShape(1, "Genesis8Female__PHMJawHeight", "下巴下颚高度");
        AddOneFemaleBlendShape(1, "Genesis8Female__PHMJawAngle", "下颚深度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jaw Corner Width (Alt)", "下颚角宽度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Chin Length", "下巴");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jaw Middle Width", "下颌曲线（宽窄）");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jaw Curve (Alt)", "下颌曲线（高低）");
        AddOneFemaleBlendShape(1, "Genesis8Female__Chin Size", "下巴厚度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jaw Width", "下颚宽度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jaw Corners Width", "下颚角宽度");
        AddOneFemaleBlendShape(1, "Genesis8Female__Jawlines Angular 02", "下颚角清晰");
        AddOneFemaleBlendShape(1, "Genesis8Female__Face Center Adjustment", "中面部深度1");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_Face_Concave_STYLE", "中面部深度2");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_Face_Convex_STYLE", "中面部深度3");
        AddOneFemaleBlendShape(1, "Genesis8Female__Chin Area Shadows", "下面部深度小1");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_Face_Long_STYLE", "下面部长度");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_LowerFace_Jaw Style 03_PART", "下巴两侧回缩1");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_LowerFace_Chin Shape 03_PART", "下巴两侧回缩2");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_LowerFace_Jaw Middle Depression_PART", "下巴两侧回缩3");
        AddOneFemaleBlendShape(1, "Genesis8Female__EJRNDMO_LowerFace_Chin Lower Depression_PART", "下巴变尖");

        AddOneFemaleBlendShape(1, "Genesis8Female__PHMJawSize", "脸大小");
        AddOneFemaleBlendShape(1, "Genesis8Female__Cheeks Chubby Lower B", "面部肥胖");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMWaistWidth", "腰部粗细");
        AddOneFemaleBlendShape(6, "Genesis8Female__iVSM Contract Breast Left & Right Lay G8", "乳房内挤");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMBreastsSize", "乳房尺寸");
        AddOneFemaleBlendShape(6, "Genesis8Female__Breasts Very Large 01", "乳房尺寸(上提)");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMBreastsGone", "小乳房");
        AddOneFemaleBlendShape(6, "Genesis8Female__CTRLFitness", "强壮");
        AddOneFemaleBlendShape(6, "Genesis8Female__FBMVoluptuous", "丰满曲线");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMThighsSize", "大腿尺寸");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMShinsSize", "小腿尺寸");
        AddOneFemaleBlendShape(6, "Genesis8Female__CTRLWeight", "肥胖");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMGlutesSize", "臀部尺寸");
        AddOneFemaleBlendShape(1, "Genesis8Female__PHMCheeksSize", "脸颊胖点");
        AddOneFemaleBlendShape(6, "Genesis8Female__PBMShouldersSize", "肩膀尺寸");
        AddOneFemaleBlendShape(6, "Genesis8Female__Youth Head Width", "头部宽度");

        AddOneFemaleBlendShape(0, "Genesis8Female__貂蝉-G", "貂蝉-G");
        AddOneFemaleBlendShape(0, "Genesis8Female__14号脸型 zfr", "14号脸型 zfr");
        AddOneFemaleBlendShape(0, "Genesis8Female__17号脸型 zfr", "17号脸型 zfr");
        AddOneFemaleBlendShape(0, "Genesis8Female__23号脸型 zfr", "23号脸型 zfr");
        AddOneFemaleBlendShape(0, "Genesis8Female__脸型预设G2 zfr", "脸型预设G2 zfr");
        AddOneFemaleBlendShape(0, "Genesis8Female__脸型预设G3 zfr", "脸型预设G3 zfr");
        AddOneFemaleBlendShape(0, "Genesis8Female__15号眉毛 brow", "15号眉毛 brow");
        AddOneFemaleBlendShape(0, "Genesis8Female__20号眉毛 brow", "20号眉毛 brow");
        AddOneFemaleBlendShape(0, "Genesis8Female__23号眉毛 brow", "23号眉毛 brow");
        AddOneFemaleBlendShape(0, "Genesis8Female__25号眉毛 brow", "25号眉毛 brow");
        AddOneFemaleBlendShape(0, "Genesis8Female__眉毛预设5号 brow", "眉毛预设5号 brow");
        AddOneFemaleBlendShape(0, "Genesis8Female__眉毛预设G2 brow", "眉毛预设G2 brow");
        AddOneFemaleBlendShape(0, "Genesis8Female__29号眼睛 zeye", "29号眼睛 zeye");
        AddOneFemaleBlendShape(0, "Genesis8Female__26号眼睛 zeye", "26号眼睛 zeye");
        AddOneFemaleBlendShape(0, "Genesis8Female__24号眼睛 zeye", "24号眼睛 zeye");
        AddOneFemaleBlendShape(0, "Genesis8Female__21号眼睛 zeye", "21号眼睛 zeye");
        AddOneFemaleBlendShape(0, "Genesis8Female__15号眼睛 zeye", "15号眼睛 zeye");
        AddOneFemaleBlendShape(0, "Genesis8Female__30号鼻子 znose", "30号鼻子 znose");
        AddOneFemaleBlendShape(0, "Genesis8Female__27号鼻子 znose", "27号鼻子 znose");
        AddOneFemaleBlendShape(0, "Genesis8Female__17号鼻子 znose", "17号鼻子 znose");
        AddOneFemaleBlendShape(0, "Genesis8Female__30号嘴 zlip", "30号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__29号嘴 zlip", "29号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__26号嘴 zlip", "26号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__25号嘴 zlip", "25号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__24号嘴 zlip", "24号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__23号嘴 zlip", "23号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__22号嘴 zlip", "22号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__18号嘴 zlip", "18号嘴 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__嘴预设14号 zlip", "嘴预设14号 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__嘴预设2号 zlip", "嘴预设2号 zlip");
        AddOneFemaleBlendShape(0, "Genesis8Female__iVFL Animosity", "愤怒");
        AddOneFemaleBlendShape(0, "Genesis8Female__eCTRLSad_HD", "难过");
        AddOneFemaleBlendShape(0, "Genesis8Female__eCTRLShock_HD", "惊吓");
        AddOneFemaleBlendShape(0, "Genesis8Female__eCTRLFierce_HD", "沉思");
        AddOneFemaleBlendShape(0, "Genesis8Female__Kanade8 Smile", "微笑");
        AddOneFemaleBlendShape(0, "Genesis8Female__eCTRLPleased_HD", "微笑2");
        AddOneFemaleBlendShape(0, "Genesis8Female__Eva Mixable 29 Irritated", "凝眉");
        AddOneFemaleBlendShape(0, "Genesis8Female__eCTRLSurprised_HD", "惊恐2");
        AddOneFemaleBlendShape(0, "Genesis8Female__CRTLHSKeicySmileOpen", "张嘴笑");
        AddOneFemaleBlendShape(0, "Genesis8Female__eCTRLEyelidsUpperUp-Down", "闭眼");
        AddOneFemaleBlendShape(0, "Genesis8Female__Wisdom - Angry 01 Mrs Chow 8", "严肃");
        AddOneFemaleBlendShape(0, "Genesis8Female__Z PAP 20 Pleasure", "痛苦");
        AddOneFemaleBlendShape(0, "Genesis8Female__FHMMrsChow8", "变老");

        //眼睛排除的变形器
        m_listEyeExcludeBlendShape[1] = new List<string>();
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__EJRNDMO_Eyes_Round Eyes_PART");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyesAlmondInner");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyesAngledInner");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyesAlmondOuter");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyesAngledOuter");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__Eyes Outer Corners Height");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__Eyes Outer Corners Drop");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__Eyes Inner Width");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyesAngled");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__yanpixiajiang");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__EJRNDMO_Eyes_Upper Eyelids Inner Style 03_PART");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__EJRNDMO_Eyes_Upper Eyelids Inner Style 02_PART");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__Eyelids Upper Rotate");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyelidsUpperHeight");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__PHMEyelidsSmooth");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__EJRNDMO_Eyes_Lower Eyelids Inner Height_PART");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__EJRNDMO_Eyes_Lower Eyelids Outer Height_PART");
        m_listEyeExcludeBlendShape[1].Add("Genesis8Female__EJRNDMO_Eyes_Lower Eyelids Center Height_PART");
        //眼睫毛排除的变形器
        m_listEyelashExcludeBlendShape[1] = new List<string>();
        m_listEyelashExcludeBlendShape[1].Add("Genesis8Female__PHMEyelidsUpperHeight");
        //全身通用的变形器
        m_listBodyBlendShape[1] = new List<string>();
        m_listBodyBlendShape[1].Add("Genesis8Female__PBMWaistWidth");
        m_listBodyBlendShape[1].Add("Genesis8Female__iVSM Contract Breast Left & Right Lay G8");
        m_listBodyBlendShape[1].Add("Genesis8Female__PBMBreastsSize");
        m_listBodyBlendShape[1].Add("Genesis8Female__Breasts Very Large 01");
        m_listBodyBlendShape[1].Add("Genesis8Female__PBMBreastsGone");
        m_listBodyBlendShape[1].Add("Genesis8Female__CTRLFitness");
        m_listBodyBlendShape[1].Add("Genesis8Female__FBMVoluptuous");
        m_listBodyBlendShape[1].Add("Genesis8Female__PBMThighsSize");
        m_listBodyBlendShape[1].Add("Genesis8Female__PBMShinsSize");
        m_listBodyBlendShape[1].Add("Genesis8Female__CTRLWeight");
        m_listBodyBlendShape[1].Add("Genesis8Female__PBMGlutesSize");
    }
}

