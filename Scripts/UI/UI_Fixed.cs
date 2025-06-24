using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface UI_Fixed//由包含UI_FixedScrollRect的UI继承实现
{
    void RefreshListRow(int tag, int startIndex, int endIndex);
    void AddOneRow(int tag, int index);
    void RowPress(int tag, int type, int itemIndex, int rowIndex);//itemIndex-列表项的下标，itemRow-列表行的下标
    void RowEnter(int tag, int itemIndex, int rowIndex, float posY);
    void RowExit(int tag);
}
