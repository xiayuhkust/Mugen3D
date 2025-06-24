using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_ConfirmUI : MonoBehaviour, UI_Base
{
    public Text m_tipsText;
    public Toggle m_toggleNoMoreTips;//不再提示
    public GameObject m_partToggle;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order
    public static bool m_isFinish = false;
    public static GameObject m_relateUI = null;//关联的UI
    public static bool m_noMoreTips = false;//是否不再提示
    public static string m_tips = "";
    private static bool m_showToggle = false;

    private static UI_ConfirmUI m_UI = null;
    public static void OpenUI(GameObject relateUI, string tips = "", bool showToggle = false)
    {
        SoundManager.Play(SoundType.Notify);
        m_relateUI = relateUI;
        m_tips = tips;
        m_showToggle = showToggle;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/ConfirmUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, false);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_ConfirmUI>();
        }
        else
        {
            GlobalAssist.ShowUI(m_UI.gameObject, false);
            m_UI.Start();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        m_isFinish = false;
        m_tipsText.text = m_tips;
        m_toggleNoMoreTips.isOn = false;
        m_partToggle.SetActive(m_showToggle);
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalAssist.m_curUI != gameObject) return;
        if (Input.GetKeyUp(KeyCode.Escape) || (Input.GetMouseButtonUp(1) && !Input.GetMouseButton(0)))
        {
            SoundManager.Play(SoundType.CancelPress);
            ButtonCancelPress();
        }
    }
    public void ButtonOKPress()
    {
        m_isFinish = true;
        m_noMoreTips = m_toggleNoMoreTips.isOn;
        GlobalAssist.HideUI(gameObject);
    }
    public void ButtonCancelPress()
    {
        GlobalAssist.HideUI(gameObject);
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
