using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public enum Language
{
    Chinese=0,
    ChineseTc,
    English,
    Japanese,
}
//全局类
public static class GlobalAssist
{
    public static Language m_language = Language.Chinese;
    public static int m_versionNo = 202204200;
    //数据
    public static Dictionary<int, Hero> m_dicHero = new Dictionary<int, Hero>();
    public static List<Hero> m_listHero = new List<Hero>();
    public static Dictionary<int, AnimatorClipInfo> m_dicAnimationInfo = new Dictionary<int, AnimatorClipInfo>();
    public static List<AnimatorClipInfo> m_listAnimationInfo = new List<AnimatorClipInfo>();

    private static Dictionary<string, Material> m_dicMaterial = new Dictionary<string, Material>();//存储全局的配件材质，为了避免加载材质占用内存，此处只存储用到的
    public static Dictionary<string, string> m_dicMaterialPath = new Dictionary<string, string>();//存储全局的配件材质，为了避免加载材质占用内存，此处只存储路径
    public static Dictionary<string, List<string>> m_dicListMatFile = new Dictionary<string, List<string>>();//材质文件名称信息

    //传递状态
    public static GlobalUpdateCtrl m_globalUpdate = null;//全局更新控制
    public static float m_screenScale = 1;//屏幕的缩放系数
    public static Canvas m_tipsCanvas = null;//提示条所在的画布
    public static GameObject m_curUI = null;//当前显示的UI
    public static bool m_isInUI = false;//是否打开了UI
    public static bool m_isOutOfUI = false;//是否刚从UI中退出
    private static List<CanvasGroup> m_listCanvasGroupShow = new List<CanvasGroup>();//正在显示中的UI
    private static List<CanvasGroup> m_listCanvasGroupHide = new List<CanvasGroup>();//正在隐藏中的UI

    public static int GetNewHeroID()
    {
        int id = 1001;
        while (m_dicHero.ContainsKey(id)) id++;
        return id;
    }
    public static int GetNewAnimInfoID()
    {
        int id = 3001;
        while (m_dicAnimationInfo.ContainsKey(id)) id++;
        return id;
    }
    public static void InitTipsCanvas()//初始化提示Canvas
    {
        if (m_tipsCanvas == null)//将TipsBar和TipsCenter改为DontDestroy且单独放一个Canvas
        {
            GameObject canvas = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/TipsCanvas.prefab");
            canvas = GameObject.Instantiate(canvas);
            GameObject.DontDestroyOnLoad(canvas);
            m_tipsCanvas = canvas.GetComponent<Canvas>();

            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/TipsCenter.prefab");
            obj = GameObject.Instantiate(obj, canvas.transform);
            UI_TipsCenterRowUI.InitAllRow(obj.transform);
        }
    }
    public static void ShowCenterTips(string str, float duration = 3)
    {
        for (int i = 0; i < UI_TipsCenterRowUI.m_listShowRow.Count; i++)//如果有相同提示，则不再重复弹出
        {
            if (UI_TipsCenterRowUI.m_listShowRow[i].m_textTips.text == str)
            {
                UI_TipsCenterRowUI.m_listShowRow[i].Show(str, duration);
                return;
            }
        }
        if (UI_TipsCenterRowUI.m_listHideRow.Count > 0)
        {
            UI_TipsCenterRowUI.m_listHideRow[0].Show(str, duration);
        }
        else
        {
            UI_TipsCenterRowUI.m_listShowRow[0].Show(str, duration);
        }
    }
    public static void UpdateCenterTips()
    {
        if (UI_TipsCenterRowUI.m_listShowRow.Count > 0 && Input.GetKeyUp(KeyCode.Escape))
        {
            for (int i = UI_TipsCenterRowUI.m_listShowRow.Count - 1; i >= 0; i--)
            {
                UI_TipsCenterRowUI.m_listShowRow[i].m_leftTime = 0;
                UI_TipsCenterRowUI.m_listShowRow[i].Hide();
            }
        }
        for (int i = UI_TipsCenterRowUI.m_listShowRow.Count - 1; i >= 0; i--)
        {
            UI_TipsCenterRowUI.m_listShowRow[i].m_leftTime -= Time.unscaledDeltaTime;
            if (UI_TipsCenterRowUI.m_listShowRow[i].m_leftTime <= 0) UI_TipsCenterRowUI.m_listShowRow[i].Hide();
            else UI_TipsCenterRowUI.m_listShowRow[i].m_textLeftTime.text = UI_TipsCenterRowUI.m_listShowRow[i].m_leftTime.ToString("0");
        }
    }
    public static Material GetMaterial(string matName)
    {
        if (m_dicMaterial.ContainsKey(matName)) return m_dicMaterial[matName];
        if (!m_dicMaterialPath.ContainsKey(matName))
        {
            ShowCenterTips("错误：配件材质<" + matName + ">不存在", 20);
            return null;
        }
        Material mat = GameAssets.LoadAsset<Material>(m_dicMaterialPath[matName]);
        if (mat == null)
        {
            ShowCenterTips("加载材质<" + matName + ">失败", 50);
            return null;
        }
        m_dicMaterial.Add(matName, mat);
        return mat;
    }

    public static void CloseAllUI()
    {
        while (true)
        {
            UI_Base m_baseUI = m_curUI.GetComponent<UI_Base>();
            if (m_baseUI.GetParent() == null) break;
            else
            {
                HideUI(m_curUI.gameObject);
            }
        }
    }
    public static GameObject InstantiateUI(GameObject ui, bool hideParent)
    {
        ui = GameObject.Instantiate(ui);
        CanvasGroup group = ui.GetComponent<CanvasGroup>();//渐变出现
        if (group != null) AddToShowUI(group);
        UI_Base baseUI = ui.GetComponent<UI_Base>();
        CanvasScaler canvasScaler = ui.GetComponent<CanvasScaler>();
        if (Screen.width * 1f / Screen.height < 1.7) canvasScaler.matchWidthOrHeight = 0;
        else canvasScaler.matchWidthOrHeight = 1;
        baseUI.SetParent(m_curUI);
        baseUI.SetSortOrder(m_curUI.GetComponent<UI_Base>().GetSortOrder() + 1);//确保在父UI之后绘制
        if (hideParent) m_curUI.SetActive(false);
        m_isInUI = true;
        m_curUI = ui;
        return ui;
    }
    public static void ShowUI(GameObject ui, bool hideParent)//对于DontDestroyOnLoad的UI，第二次打开时不用Instantiate
    {
        ui.SetActive(true);
        CanvasGroup group = ui.GetComponent<CanvasGroup>();//渐变出现
        if (group != null) AddToShowUI(group);
        if (ui != m_curUI)
        {
            UI_Base baseUI = ui.GetComponent<UI_Base>();
            CanvasScaler canvasScaler = ui.GetComponent<CanvasScaler>();
            if (Screen.width * 1f / Screen.height < 1.7) canvasScaler.matchWidthOrHeight = 0;
            else canvasScaler.matchWidthOrHeight = 1;
            baseUI.SetParent(m_curUI);
            baseUI.SetSortOrder(m_curUI.GetComponent<UI_Base>().GetSortOrder() + 1);//确保在父UI之后绘制
            if (hideParent) m_curUI.SetActive(false);
            m_isInUI = true;
            m_curUI = ui;
        }
    }
    public static void HideUI(GameObject ui)//对于DontDestroyOnLoad的UI，关闭时不Destroy
    {
        UI_Base baseUI = ui.GetComponent<UI_Base>();
        GameObject parentUI = baseUI.GetParent();
        baseUI = parentUI.GetComponent<UI_Base>();
        if (baseUI.IsRootUI())
        {
            m_isOutOfUI = true;
            m_isInUI = false;
        }
        m_curUI = parentUI;
        if (!m_curUI.activeSelf) m_curUI.SetActive(true);//如果隐藏了，则显示
        CanvasGroup group = ui.GetComponent<CanvasGroup>();//渐变消失
        if (group != null) AddToHideUI(group);
        else ui.SetActive(false);
    }
    static void AddToShowUI(CanvasGroup group)//不能用DoFade，因为个人战里面打开UI时TimeScale=0，导致DoFade暂停
    {
        m_listCanvasGroupHide.Remove(group);
        if (m_listCanvasGroupShow.Contains(group)) return;
        group.alpha = 0;
        m_listCanvasGroupShow.Add(group);
    }
    static void AddToHideUI(CanvasGroup group)
    {
        m_listCanvasGroupShow.Remove(group);
        if (m_listCanvasGroupHide.Contains(group)) return;
        m_listCanvasGroupHide.Add(group);
    }
    public static void UpdateAllCanvasGroup()
    {
        for (int i = m_listCanvasGroupShow.Count - 1; i >= 0; i--)
        {
            m_listCanvasGroupShow[i].alpha += Time.unscaledDeltaTime * 5;
            if (m_listCanvasGroupShow[i].alpha >= 1) m_listCanvasGroupShow.RemoveAt(i);
        }
        for (int i = m_listCanvasGroupHide.Count - 1; i >= 0; i--)
        {
            m_listCanvasGroupHide[i].alpha -= Time.unscaledDeltaTime * 5;
            if (m_listCanvasGroupHide[i].alpha <= 0)
            {
                m_listCanvasGroupHide[i].gameObject.SetActive(false);
                m_listCanvasGroupHide.RemoveAt(i);
            }
        }
    }
}
