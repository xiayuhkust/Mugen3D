
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using LitJson;

public class UI_SelectColorUI : MonoBehaviour, UI_Base
{
    public Image Saturation;
    public Image Hue;
    public Image Paint;
    public Image[] m_imageQuickColor = new Image[10];

    public RectTransform Point_Stauration;
    public RectTransform Point_Hue;

    public Slider[] m_sliderColor = new Slider[3];
    public InputField[] m_colorField = new InputField[3];

    private Sprite Saturation_Sprite;
    private Sprite Hue_Sprite;

    private Color32 currentHue = Color.red;
    private GameObject m_parentUI = null;//父窗口
    private int m_sortOrder = 0;//Canvas 的sort order

    public static bool m_isFinish = false;
    public static GameObject m_relateUI = null;//关联的UI
    public static Color m_color;

    private static UI_SelectColorUI m_UI = null;
    public static void OpenUI(GameObject relateUI, Color color)
    {
        m_relateUI = relateUI;
        m_color = color;
        if (m_UI == null)
        {
            GameObject obj = GameAssets.LoadAsset<GameObject>("Assets/Resource/UI_Prefab/SelectColorUI.prefab");
            obj = GlobalAssist.InstantiateUI(obj, false);
            DontDestroyOnLoad(obj);
            m_UI = obj.GetComponent<UI_SelectColorUI>();
        }
        else
        {
            GlobalAssist.ShowUI(m_UI.gameObject, false);
            m_UI.Start();
        }
    }
    private void Start()
    {
        m_isFinish = false;
        try
        {
            UpdateCurrentHue();
            UpdateHue();
        }
        catch (Exception exc)
        {
            GlobalAssist.ShowCenterTips(exc.Message, 20);
        }
        Color color = m_color;
        m_sliderColor[0].value = color.r;
        m_sliderColor[1].value = color.g;
        m_sliderColor[2].value = color.b;
        m_colorField[0].text = (color.r * 255).ToString("0");
        m_colorField[1].text = (color.g * 255).ToString("0");
        m_colorField[2].text = (color.b * 255).ToString("0");
    }
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ButtonCancelPress();
        }
    }
    float sWidth = 200, sHeight = 200;
    //更新饱和度
    private void UpdateStauration()
    {
        if (Saturation_Sprite == null) Saturation_Sprite = Sprite.Create(new Texture2D((int)sWidth, (int)sHeight), new Rect(0, 0, sWidth, sHeight), new Vector2(0, 0));

        for (int y = 0; y <= sHeight; y++)
        {
            for (int x = 0; x < sWidth; x++)
            {
                var pixColor = GetSaturation(currentHue, x / sWidth, y / sHeight);
                Saturation_Sprite.texture.SetPixel(x, ((int)sHeight - y), pixColor);
            }
        }
        Saturation_Sprite.texture.Apply();

        Saturation.sprite = Saturation_Sprite;
    }
    private void UpdateCurrentHue()
    {
        Color color = m_color;
        int minIndex = 0;
        float minVal = 1;
        float maxVal = 0;
        for (int i = 0; i < 3; i++)
        {
            if (color[i] < minVal)
            {
                minIndex = i;
                minVal = color[i];
            }
            maxVal = Mathf.Max(maxVal, color[i]);
        }
        color[minIndex] = 0;
        if (maxVal > 0.01)
        {
            for (int i = 0; i < 3; i++) color[i] = color[i] / maxVal;
        }
        currentHue = color;
        UpdateStauration();
        SetSaturationPosition();
        SetHuePosition();
    }
    private void SetSaturationPosition()
    {
        Color colorHue = currentHue;
        float x = 0, y = 0;
        for (int i = 0; i < 3; i++)
        {
            if (colorHue[i] >= 0.99)
            {
                y = m_color[i] / colorHue[i];
                break;
            }
        }
        for (int i = 0; i < 3; i++)
        {
            if (colorHue[i] <= 0.01)
            {
                if (y <= 0.001) x = 0;
                else x = m_color[i] / y;
                break;
            }
        }
        var size2 = Saturation.rectTransform.sizeDelta;
        float posX = (0.5f - x) * size2.x;
        float posY = (y - 0.5f) * size2.y;
        Point_Stauration.anchoredPosition = clickPoint = new Vector2(posX, posY);
    }
    private void SetHuePosition()
    {
        float y = 0;
        if (currentHue.r == 0)//中间
        {
            if (currentHue.b == 255)
            {
                y = 0.3333f * (255 - currentHue.g) / 255f;
            }
            else
            {
                y = -0.3333f * (255 - currentHue.b) / 255f;
            }
        }
        else if (currentHue.g == 0)//上面
        {
            if (currentHue.r == 255)
            {
                y = 1 - 0.3333f * currentHue.b / 255f;
            }
            else
            {
                y = 0.3333f + 0.3333f * currentHue.r / 255f;
            }
        }
        else if (currentHue.b == 0)//下面
        {
            if (currentHue.r == 255)
            {
                y = -1 + 0.3333f * currentHue.g / 255f;
            }
            else
            {
                y = -0.3333f - 0.3333f * currentHue.r / 255f;
            }
        }
        var h = Hue.rectTransform.sizeDelta.y / 2.0f;
        Point_Hue.anchoredPosition = new Vector2(0, y * h);
    }

    //更新色泽度 
    private void UpdateHue()
    {
        float w = 50, h = 50;
        if (Hue_Sprite == null) Hue_Sprite = Sprite.Create(new Texture2D((int)w, (int)h), new Rect(0, 0, w, h), new Vector2(0, 0));
        for (int y = 0; y <= h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                var pixColor = GetHue(y / h);
                Hue_Sprite.texture.SetPixel(x, ((int)h - y), pixColor);
            }
        }
        Hue_Sprite.texture.Apply();

        Hue.sprite = Hue_Sprite;
    }

    private Vector2 clickPoint = Vector2.zero;
    public void OnStaurationClick(ColorPickClick sender)
    {
        var size2 = Saturation.rectTransform.sizeDelta / 2;
        var pos = Vector2.zero;
        pos.x = Mathf.Clamp(sender.ClickPoint.x, -size2.x, size2.x);
        pos.y = Mathf.Clamp(sender.ClickPoint.y, -size2.y, size2.y);
        Point_Stauration.anchoredPosition = clickPoint = pos;

        UpdateColor(0);
    }

    public void UpdateColor(int type)//type=0调色板，=1拉条，=2数值
    {
        if (type == 0)
        {
            var size2 = Saturation.rectTransform.sizeDelta / 2;
            var pos = clickPoint;
            pos += size2;

            Color color = GetSaturation(currentHue, pos.x / Saturation.rectTransform.sizeDelta.x, 1 - pos.y / Saturation.rectTransform.sizeDelta.y);
            m_color = Paint.color = color;
            m_sliderColor[0].SetValueWithoutNotify(color.r);
            m_sliderColor[1].SetValueWithoutNotify(color.g);
            m_sliderColor[2].SetValueWithoutNotify(color.b);
            m_colorField[0].SetTextWithoutNotify((color.r * 255).ToString("0"));
            m_colorField[1].SetTextWithoutNotify((color.g * 255).ToString("0"));
            m_colorField[2].SetTextWithoutNotify((color.b * 255).ToString("0"));
        }
        else if (type == 1)
        {
            m_color = Paint.color = new Color(m_sliderColor[0].value, m_sliderColor[1].value, m_sliderColor[2].value);
            string[] strs = new string[3];
            strs[0] = (m_color.r * 255).ToString("0");
            strs[1] = (m_color.g * 255).ToString("0");
            strs[2] = (m_color.b * 255).ToString("0");
            for (int i = 0; i < 3; i++)
            {
                m_colorField[i].SetTextWithoutNotify(strs[i]);
            }
            UpdateCurrentHue();
        }
        else if (type == 2)
        {
            float[] rgb = new float[3];
            for (int i = 0; i < 3; i++)
            {
                if (m_colorField[i].text == "-") m_colorField[i].SetTextWithoutNotify("0");
                else if (Convert.ToInt32(m_colorField[i].text) < 0) m_colorField[i].SetTextWithoutNotify("0");
                else if (Convert.ToInt32(m_colorField[i].text) > 255) m_colorField[i].SetTextWithoutNotify("255");
                rgb[i] = Convert.ToSingle(m_colorField[i].text) / 255;
            }
            m_color = Paint.color = new Color(rgb[0], rgb[1], rgb[2]);
            for (int i = 0; i < 3; i++)
            {
                m_sliderColor[i].SetValueWithoutNotify(rgb[i]);
            }
            UpdateCurrentHue();
        }
    }

    public void OnHueClick(ColorPickClick sender)
    {
        var h = Hue.rectTransform.sizeDelta.y / 2.0f;
        var y = Mathf.Clamp(sender.ClickPoint.y, -h, h);
        Point_Hue.anchoredPosition = new Vector2(0, y);

        y += h;
        currentHue = GetHue(1 - y / Hue.rectTransform.sizeDelta.y);
        UpdateStauration();
        
        UpdateColor(0);
    }

    private static Color GetSaturation(Color color, float x, float y)
    {
        Color newColor = Color.white;
        for (int i = 0; i < 3; i++)
        {
            if (color[i] != 1)
            {
                newColor[i] = (1 - color[i]) * (1 - x) + color[i];
            }
        }

        newColor *= (1 - y);
        newColor.a = 1;
        return newColor;
    }

    //B,r,G,b,R,g //大写是升，小写是降
    private readonly static int[] hues = new int[] { 2, 0, 1, 2, 0, 1 };

    private readonly static Color[] colors = new Color[] { Color.red, Color.blue, Color.blue, Color.green, Color.green, Color.red };

    private readonly static float c = 1.0f / hues.Length;

    private static Color GetHue(float y)
    {
        y = Mathf.Clamp01(y);

        var index = (int)(y / c);
        if (index > 5 || index < 0)
        {
            //GlobalAssist.ShowCenterTips("index=" + index + "数组越界，已做保护", 10);
            index = 5;
        }
        var h = hues[index];
        if (h > 2 || h < 0) GlobalAssist.ShowCenterTips("h=" + h + "数组越界", 10);
        var newColor = colors[index];

        float less = (y - index * c) / c;

        newColor[h] = index % 2 == 0 ? less : 1 - less;

        return newColor;
    }

    public void SliderChange()
    {
        UpdateColor(1);
    }
    public void InputFieldChange()
    {
        UpdateColor(2);
    }
    public void ButtonQuickColorPress(int index)
    {
        m_color = m_imageQuickColor[index].color;
        ButtonOKPress();
    }
    public void ButtonOKPress()
    {
        m_isFinish = true;
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
