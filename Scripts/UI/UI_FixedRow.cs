using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface UI_FixedRow//由固定行列表的行继承实现
{
    void SetActive(bool active);
    float GetPosY();
}
