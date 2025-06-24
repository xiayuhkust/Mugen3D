using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_TipsCenterRowUI : MonoBehaviour
{
    public Text m_textTips;
    public Text m_textLeftTime;
    public static List<UI_TipsCenterRowUI> m_listHideRow = new List<UI_TipsCenterRowUI>();//隐藏的提示行
    public static List<UI_TipsCenterRowUI> m_listShowRow = new List<UI_TipsCenterRowUI>();//显示的提示行
    [HideInInspector] public float m_leftTime = 0;

    public static void InitAllRow(Transform parentTransform)
    {
        m_listHideRow.Clear();
        m_listShowRow.Clear();
        GameObject prefab = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/TipsCenterRow.prefab");
        for (int i = 0; i < 20; i++)
        {
            GameObject obj = Instantiate(prefab, parentTransform);
            UI_TipsCenterRowUI rowUI = obj.GetComponent<UI_TipsCenterRowUI>();
            m_listHideRow.Add(rowUI);
            obj.SetActive(false);
        }
    }
    public void ButtonClosePress()
    {
        m_leftTime = 0;
    }
    public void Show(string str, float duration)
    {
        m_textTips.text = str;
        m_textLeftTime.text = duration.ToString("0");
        m_leftTime = duration;
        transform.SetSiblingIndex(transform.parent.childCount - 1);
        if (!gameObject.activeSelf)
        {
            gameObject.SetActive(true);
            m_listHideRow.Remove(this);
            m_listShowRow.Add(this);
        }
    }
    public void Hide()
    {
        gameObject.SetActive(false);
        m_listHideRow.Add(this);
        m_listShowRow.Remove(this);
    }
}
