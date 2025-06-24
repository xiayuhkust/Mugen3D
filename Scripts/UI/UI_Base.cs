using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public interface UI_Base//UI基础接口，用于实现UI间的层级切换
{
    bool IsRootUI();//是否为根UI
    void SetParent(GameObject parentUI);
    GameObject GetParent();
    int GetSortOrder();//返回Canvas的sort order
    void SetSortOrder(int order);//设置order
}
