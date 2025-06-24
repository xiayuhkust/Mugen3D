using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_MakeCommandUI : MonoBehaviour, UI_Base
{
    public RectTransform m_parentTransform;//插入行时的父对象
    public GameObject m_rowPrefab;
    public Text m_textNo;
    public InputField m_inputFieldName;
    public InputField m_inputFieldTime;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    private List<UI_CommandInputBoxUI> m_listBoxUI = new List<UI_CommandInputBoxUI>();
    private List<InputData> m_listInput = new List<InputData>();
    private int m_curIndex = 0;

    public static bool m_isFinish = false;
    public static int m_openType = 0;//0-新建，1-修改
    public static Command m_cmd = null;
    private static UI_MakeCommandUI m_UI = null;
    public static void OpenUI(Command cmd, int openType)
    {
        m_openType = openType;
        m_cmd = cmd;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/MakeCommandUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_MakeCommandUI>();
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
        if (UI_MakeCommandInputUI.m_isFinish)
        {
            UI_MakeCommandInputUI.m_isFinish = false;
            if (UI_MakeCommandInputUI.m_openType == 0)
            {
                if (m_curIndex < m_listInput.Count) m_listInput.Insert(m_curIndex, UI_MakeCommandInputUI.m_inputData);
                else m_listInput.Add(UI_MakeCommandInputUI.m_inputData);
            }
            for (int i = m_listInput.Count - 1; i >= 0; i--)
            {
                if (m_listInput[i].m_listKey.Count == 0) m_listInput.RemoveAt(i);
            }
            RefreshContent();
        }
    }
    void UpdateDataToUI()
    {
        m_textNo.text = m_cmd.m_commandNo.ToString();
        m_inputFieldName.text = m_cmd.m_name.GetStr();
        m_inputFieldTime.text = m_cmd.m_time.ToString();
        m_listInput.Clear();
        foreach (InputData data in m_cmd.m_listInput)
        {
            InputData newData = new InputData();
            newData.Copy(data);
            m_listInput.Add(newData);
        }
        RefreshContent();
    }
    void UpdateUIToData()
    {
        m_cmd.m_name.SetStr(m_inputFieldName.text);
        if (m_inputFieldTime.text == "") m_inputFieldTime.text = "0";
        m_cmd.m_time = float.Parse(m_inputFieldTime.text);
        m_cmd.m_listInput.Clear();
        m_cmd.m_listInput.AddRange(m_listInput);
    }
    void RefreshContent()
    {
        if (m_listBoxUI.Count < m_listInput.Count + 1)//已有行比所需的少，增加行
        {
            for (int i = 0; i < m_listBoxUI.Count; i++) m_listBoxUI[i].gameObject.SetActive(true);
            for (int i = m_listBoxUI.Count; i < m_listInput.Count + 1; i++) AddOneBox(i);
        }
        else//将多余的行隐藏
        {
            for (int i = 0; i < m_listInput.Count + 1; i++) m_listBoxUI[i].gameObject.SetActive(true);
            for (int i = m_listInput.Count + 1; i < m_listBoxUI.Count; i++) m_listBoxUI[i].gameObject.SetActive(false);
        }
        for (int i = 0; i < m_listInput.Count; i++)
        {
            UI_CommandInputBoxUI boxUI = m_listBoxUI[i];
            boxUI.m_buttonEdit.gameObject.SetActive(true);
            boxUI.m_textInput.text = m_listInput[i].GetDisplay();
        }
        m_listBoxUI[m_listInput.Count].m_buttonEdit.gameObject.SetActive(false);
    }
    public void AddOneBox(int index)
    {
        GameObject obj = Instantiate(m_rowPrefab, m_parentTransform);
        UI_CommandInputBoxUI boxUI = obj.GetComponent<UI_CommandInputBoxUI>();
        boxUI.m_index = index;
        boxUI.m_baseUI = this;
        m_listBoxUI.Add(boxUI);
    }
    public void ButtonAddPress(int index)
    {
        m_curIndex = index;
        InputData data = new InputData();
        UI_MakeCommandInputUI.OpenUI(data, 0);
    }
    public void ButtonEditPress(int index)
    {
        m_curIndex = index;
        UI_MakeCommandInputUI.OpenUI(m_listInput[index], 1);
    }
    public void ButtonOKPress()
    {
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
