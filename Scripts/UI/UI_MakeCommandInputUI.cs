using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UI_MakeCommandInputUI : MonoBehaviour, UI_Base
{
    public Text[] m_textInput = new Text[12];
    public Dropdown m_dropdownType;
    public InputField m_inputFieldTime;
    public Toggle m_toggleAllowOtherArrow;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    public static bool m_isFinish = false;
    public static InputData m_inputData = null;
    public static int m_openType = 0;
    private List<int> m_listIndex = new List<int>();
    private static UI_MakeCommandInputUI m_UI = null;
    public static void OpenUI(InputData inputData, int openType)
    {
        m_inputData = inputData;
        m_openType = openType;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/MakeCommandInputUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, true);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_MakeCommandInputUI>();
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
        List<Dropdown.OptionData> listData = new List<Dropdown.OptionData>();
        listData.Add(new Dropdown.OptionData("无"));
        listData.Add(new Dropdown.OptionData("按住不放"));
        listData.Add(new Dropdown.OptionData("按住多少秒后松开"));
        m_dropdownType.ClearOptions();
        m_dropdownType.AddOptions(listData);
        m_dropdownType.value = 0;
        UpdateDataToUI();
        RefreshItemState();
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
        m_listIndex.Clear();
        foreach (string key in m_inputData.m_listKey)
        {
            switch(key)
            {
                case "→": m_listIndex.Add(0); break;
                case "←": m_listIndex.Add(1); break;
                case "↑": m_listIndex.Add(2); break;
                case "↓": m_listIndex.Add(3); break;
                case "J": m_listIndex.Add(4); break;
                case "a": m_listIndex.Add(5); break;
                case "b": m_listIndex.Add(6); break;
                case "c": m_listIndex.Add(7); break;
                case "x": m_listIndex.Add(8); break;
                case "y": m_listIndex.Add(9); break;
                case "z": m_listIndex.Add(10); break;
                case "s": m_listIndex.Add(11); break;
            }
        }
        for (int i = 0; i < 12; i++)
        {
            if (m_listIndex.Contains(i)) m_textInput[i].color = Color.green;
            else m_textInput[i].color = Color.white;
        }
        m_dropdownType.value = m_inputData.m_type;
        m_inputFieldTime.text = m_inputData.m_pressTime.ToString();
        m_toggleAllowOtherArrow.isOn = m_inputData.m_allowOtherArrow;
    }
    void UpdateUIToData()
    {
        m_inputData.m_listKey.Clear();
        for (int i = 0; i < 12; i++)
        {
            if (m_listIndex.Contains(i))
            {
                switch(i)
                {
                    case 0: m_inputData.m_listKey.Add("→"); break;
                    case 1: m_inputData.m_listKey.Add("←"); break;
                    case 2: m_inputData.m_listKey.Add("↑"); break;
                    case 3: m_inputData.m_listKey.Add("↓"); break;
                    case 4: m_inputData.m_listKey.Add("J"); break;
                    case 5: m_inputData.m_listKey.Add("a"); break;
                    case 6: m_inputData.m_listKey.Add("b"); break;
                    case 7: m_inputData.m_listKey.Add("c"); break;
                    case 8: m_inputData.m_listKey.Add("x"); break;
                    case 9: m_inputData.m_listKey.Add("y"); break;
                    case 10: m_inputData.m_listKey.Add("z"); break;
                    case 11: m_inputData.m_listKey.Add("s"); break;
                }
            }
        }
        m_inputData.m_type = m_dropdownType.value;
        if (m_inputFieldTime.text == "") m_inputFieldTime.text = "0";
        m_inputData.m_pressTime = Convert.ToSingle(m_inputFieldTime.text);
        if (m_toggleAllowOtherArrow.interactable) m_inputData.m_allowOtherArrow = m_toggleAllowOtherArrow.isOn;
        else m_inputData.m_allowOtherArrow = false;
    }
    public void ButtonInputPress(int index)
    {
        if (m_listIndex.Contains(index))
        {
            m_listIndex.Remove(index);
            m_textInput[index].color = Color.white;
        }
        else
        {
            m_listIndex.Add(index);
            m_textInput[index].color = Color.green;
        }
        RefreshItemState();
    }
    public void RefreshItemState()
    {
        m_inputFieldTime.interactable = m_dropdownType.value == 2;
        bool hasArrow = false;
        for (int i = 0; i < 4; i++)
        {
            if (m_listIndex.Contains(i))
            {
                hasArrow = true;
                break;
            }
        }
        bool hasOtehr = false;
        for (int i = 4; i < 12; i++)
        {
            if (m_listIndex.Contains(i))
            {
                hasOtehr = true;
                break;
            }
        }
        m_toggleAllowOtherArrow.interactable = hasArrow && !hasOtehr;
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
