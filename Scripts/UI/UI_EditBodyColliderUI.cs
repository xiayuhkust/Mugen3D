using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_EditBodyColliderUI : MonoBehaviour, UI_Base
{
    public InputField[] m_inputFieldStart = new InputField[2];
    public InputField[] m_inputFieldEnd = new InputField[2];
    public Text[] m_textAttack = new Text[17];
    public Text[] m_textHit = new Text[17];
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    public static bool m_isFinish = false;
    public static CollideInfo m_ci = null;
    public static int m_openType = 0;//0-添加，1-修改
    public static bool[] m_attackCollider = new bool[17];
    public static bool[] m_hitCollider = new bool[17];
    private static UI_EditBodyColliderUI m_UI = null;
    public static void OpenUI(CollideInfo ci, int openType)
    {
        m_ci = ci;
        m_openType = openType;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/EditBodyColliderUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_EditBodyColliderUI>();
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
        m_isFinish = false;
        UpdateDataToUI();
    }

    // Update is called once per frame
    void Update()
    {
        if (GlobalAssist.m_curUI != gameObject) return;
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            SoundManager.Play(SoundType.CancelPress);
            ButtonCancelPress();
        }
    }
    void UpdateDataToUI()
    {
        for (int i = 0; i < 17; i++)
        {
            m_attackCollider[i] = m_ci.m_attackCollider[i];
            m_hitCollider[i] = m_ci.m_hitCollider[i];
            m_textAttack[i].color = m_attackCollider[i] ? Color.green : Color.white;
            m_textHit[i].color = m_hitCollider[i] ? Color.green : Color.white;
        }
        m_inputFieldStart[0].text = m_ci.m_startAnimIndex.ToString();
        m_inputFieldStart[1].text = m_ci.m_startAnimPer.ToString("0");
        m_inputFieldEnd[0].text = m_ci.m_endAnimIndex.ToString();
        m_inputFieldEnd[1].text = m_ci.m_endAnimPer.ToString("0");
    }
    void UpdateUIToData()
    {
        for (int i = 0; i < 17; i++)
        {
            m_ci.m_attackCollider[i] = m_attackCollider[i];
            m_ci.m_hitCollider[i] = m_hitCollider[i];
        }
        if (m_inputFieldStart[0].text == "") m_inputFieldStart[0].text = "0";
        m_ci.m_startAnimIndex = Convert.ToInt32(m_inputFieldStart[0].text);
        if (m_inputFieldStart[1].text == "") m_inputFieldStart[1].text = "0";
        m_ci.m_startAnimPer = Convert.ToSingle(m_inputFieldStart[1].text);
        if (m_inputFieldEnd[0].text == "") m_inputFieldEnd[0].text = "0";
        m_ci.m_endAnimIndex = Convert.ToInt32(m_inputFieldEnd[0].text);
        if (m_inputFieldEnd[1].text == "") m_inputFieldEnd[1].text = "0";
        m_ci.m_endAnimPer = Convert.ToSingle(m_inputFieldEnd[1].text);
    }
    public void ButtonAttackPress(int index)
    {
        m_attackCollider[index] = !m_attackCollider[index];
        m_textAttack[index].color = m_attackCollider[index] ? Color.green : Color.white;
    }
    public void ButtonHitPress(int index)
    {
        m_hitCollider[index] = !m_hitCollider[index];
        m_textHit[index].color = m_hitCollider[index] ? Color.green : Color.white;
    }
    public void ButtonOKPress()
    {
        bool hasAttack = false;
        bool hasHit = false;
        for (int i = 0; i < 17; i++)
        {
            if (m_attackCollider[i]) hasAttack = true;
            if (m_hitCollider[i]) hasHit = true;
        }
        if (!hasAttack || !hasHit)
        {
            GlobalAssist.ShowCenterTips("至少选中1个攻击碰撞体和1个受击碰撞体");
            return;
        }
        m_isFinish = true;
        UpdateUIToData();
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
